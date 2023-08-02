#define MEMORY_BUFFER

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace ServerCoreTCP
{
    public abstract class Session : ISession
    {
        protected const int RecvBufferSize = 65535;

        public EndPoint ConnectedEndPoint
        {
            get
            {
                return _socket?.RemoteEndPoint;
            }
        }

        protected Socket _socket;
#if MEMORY_BUFFER
        protected readonly MRecvBuffer _recvBuffer = new(RecvBufferSize);
        protected readonly Queue<Memory<byte>> _sendQueue = new();
        protected readonly List<Memory<byte>> _sendPendingList = new();
        protected readonly List<ArraySegment<byte>> _sendBufferList = new();
#else
        protected readonly RecvBuffer _recvBuffer = new(RecvBufferSize);
        protected readonly Queue<ArraySegment<byte>> _sendQueue = new();
        protected readonly List<ArraySegment<byte>> _sendPendingList = new();
#endif

        /// <summary>
        /// Each session reuses one SendEventArgs.
        /// </summary>
        protected readonly SocketAsyncEventArgs _sendEvent = new();
        /// <summary>
        /// Each session reuses one RecvEventArgs.
        /// </summary>
        protected readonly SocketAsyncEventArgs _recvEvent = new();

        /// <summary>
        /// Note: Send/Recv can be occurred in multiple threads
        /// </summary>
        protected readonly object _lock = new();

#region Abstract Methods
        /// <summary>
        /// Called when the socket is connected.
        /// </summary>
        /// <param name="endPoint">The endpoint of connected socket</param>
        public abstract void OnConnected(EndPoint endPoint);
        /// <summary>
        /// Called when the socket received data(buffer)
        /// </summary>
        /// <param name="buffer">The buffer of unit packet received.</param>
        public abstract void OnRecv(ReadOnlySpan<byte> buffer);
        /// <summary>
        /// Called when the socket sent data.
        /// </summary>
        /// <param name="numOfBytes">The length of bytes transferred.</param>
        public abstract void OnSend(int numOfBytes);
        /// <summary>
        /// Called when the socket is disconnected
        /// </summary>
        /// <param name="endPoint">The end point of the socket.</param>
        /// <param name="error">The additional object of error</param>
        public abstract void OnDisconnected(EndPoint endPoint, object error = null);

        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        protected abstract int OnRecvProcess(Memory<byte> buffer);
        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        protected abstract int OnRecvProcess(ArraySegment<byte> buffer);

        #endregion

        /// <summary>
        /// The value to check the session disconnected. (Used with Interlocked)
        /// </summary>
        int _disconnected = 0;

        /// <summary>
        /// A session must be initialized with this method with socket.
        /// </summary>
        /// <param name="socket">The socket to be connected to the session.</param>
        public void Init(Socket socket)
        {
            _socket = socket;

            _sendEvent.Completed += new(OnSendCompleted);
            _recvEvent.Completed += new(OnRecvCompleted);

            RegisterRecv();
        }

        /// <summary>
        /// Send data to endpoint of the socket. [ArraySegment]
        /// </summary>
        /// <param name="sendBuffer">Serialized data to send</param>
        public void SendRaw(ArraySegment<byte> sendBuffer)
        {
#if MEMORY_BUFFER
            throw new Exception("The session uses Memory<byte> buffer now.");
#else
            int len = sendBuffer.Count;
            var b = SendBufferTLS.Reserve(len);
            sendBuffer.CopyTo(b);
            sendBuffer = SendBufferTLS.Return(len);

            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
#endif
        }

        /// <summary>
        /// Send data to endpoint of the socket. [Memory]
        /// </summary>
        /// <param name="sendBuffer">Serialized data to send</param>
        public void SendRaw(Memory<byte> sendBuffer)
        {
#if MEMORY_BUFFER
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
#else
            throw new Exception("The session uses ArraySegment<byte> buffer now.");
#endif
        }

        /// <summary>
        /// Reserve 'Send' for async-send (Note: It needs to be protected for race-condition.)
        /// </summary>
        protected void RegisterSend()
        {
            // If it is already disconnected, return
            if (_disconnected == 1) return;

            // NOTE:
            // DO NOT ADD ITEMS to eventArgs.BufferList through Add method.
            // It does not work well and causes SocketError.InvalidArguments.
            // USE EventArgs.BufferList = list instead.
            // WHY? https://stackoverflow.com/questions/11820677/how-use-bufferlist-with-socketasynceventargs-and-not-get-socketerror-invalidargu
#if MEMORY_BUFFER
            //sendEvent.SetBuffer(_sendMemory);
            while (_sendQueue.Count > 0)
            {
                _sendPendingList.Add(_sendQueue.Dequeue());
            }

            foreach (Memory<byte> buff in _sendPendingList)
            {
                if (MemoryMarshal.TryGetArray<byte>(buff, out var segment))
                {
                    _sendBufferList.Add(segment);
                }
                else
                {
                    Console.WriteLine($"Error: ReisterSend"); ;
                }
            }
            _sendEvent.BufferList = _sendBufferList;
            _sendPendingList.Clear();
#else
            //sendEvent.SetBuffer(_sendSegment.Array, 0, _sendSegment.Count);
            while (_sendQueue.Count > 0)
            {
                _sendPendingList.Add(_sendQueue.Dequeue());
            }

            _sendEvent.BufferList = _sendPendingList;
#endif

            try
            {
                bool pending = _socket.SendAsync(_sendEvent);
                if (pending == false) OnSendCompleted(null, _sendEvent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: RegisterSend - {0}", e);
            }
        }

        /// <summary>
        /// Reserve 'Receive' for async-receive
        /// </summary>
        protected void RegisterRecv()
        {
            if (_disconnected == 1) return;

#if MEMORY_BUFFER
            _recvBuffer.CleanUp();
            _recvEvent.SetBuffer(_recvBuffer.WriteMemory);
#else
            _recvBuffer.CleanUp(); // expensive
            var segment = _recvBuffer.WriteSegment;
            _recvEvent.SetBuffer(segment.Array, segment.Offset, segment.Count);
#endif

            try
            {
                bool pending = _socket.ReceiveAsync(_recvEvent);
                if (pending == false) OnRecvCompleted(null, _recvEvent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: RegisterRecv - {0}", ex);
            }
        }

        /// <summary>
        /// Callback that is called when send-operation is completed.
        /// </summary>
        /// <param name="sender">[Ignored] The source of the event</param>
        /// <param name="e">An object that contains the socket-async-send-event data</param>
        void OnSendCompleted(object sender, SocketAsyncEventArgs e)
        {
            lock (_lock)
            {
                // Check the length of bytes transferred and SocketError==Success
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    try
                    {
                        OnSend(e.BytesTransferred);
#if MEMORY_BUFFER
                        _sendBufferList.Clear();
                        //e.SetBuffer(null);
                        _sendEvent.BufferList = null;
#else
                        _sendEvent.BufferList = null;
                        _sendPendingList.Clear();
#endif
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: OnSendCompleted - {0}", ex);
                    }
                }
                else if (e.BytesTransferred == 0)
                {
                    Console.WriteLine($"The length of sent data was 0.");
                }
                else if (e.SocketError != SocketError.Success)
                {
                    Console.WriteLine($"SocketError: {e.SocketError}");
                }
                else
                {
                    Disconnect();
                }
            }
        }

        /// <summary>
        /// Callback that is called when recv-operation is completed.
        /// </summary>
        /// <param name="sender">[Ignored] The source of the event</param>
        /// <param name="e">An object that contains the socket-async-recv-event data</param>
        void OnRecvCompleted(object sender, SocketAsyncEventArgs e)
        {
#if DEBUG
            Console.WriteLine($"Received: {e.BytesTransferred} bytes");
#endif

            // Check the length of bytes transferred and SocketError==Success
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                try
                {
                    if (_recvBuffer.OnWrite(e.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }
#if MEMORY_BUFFER
                    int processLength = OnRecvProcess(_recvBuffer.DataMemory);
#else
                    int processLength = OnRecvProcess(_recvBuffer.DataSegment);
#endif
                    if (processLength <= 0)
                    {
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // Wait to receive again.
                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: OnRecvCompleted - {0}", ex);
                }
            }
            else if (e.BytesTransferred == 0)
            {
                Console.WriteLine($"The length of received data is 0.");
            }
            else if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine($"SocketError: {e.SocketError}");
            }
            else
            {
                Disconnect();
            }

        }

        /// <summary>
        /// Close the socket and clear the session.
        /// </summary>
        void Disconnect()
        {
            // Check that it is already disconnected
            if (Interlocked.Exchange(ref _disconnected, 1) == 1) return;

            OnDisconnected(_socket.RemoteEndPoint);

            // Shutdown send/recv both
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();

            Clear();
        }

        void Clear()
        {
            lock (_lock)
            {
                _sendQueue.Clear();
                _sendPendingList.Clear();
#if MEMORY_BUFFER
                _sendBufferList.Clear();
#endif
            }
        }
    }
}
