//#define MEMORY_BUFFER

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace ServerCoreTCP
{
    public abstract class Session
    {
        const int HeaderSize = sizeof(ushort);
        const int RecvBufferSize = 1024;

        public EndPoint EndPoint
        {
            get
            {
                return _socket?.RemoteEndPoint;
            }
        }

        Socket _socket;
#if MEMORY_BUFFER
        readonly MRecvBuffer _mRecvBuffer = new(RecvBufferSize);
        readonly Queue<Memory<byte>> _sendQueue = new();
        readonly List<Memory<byte>> _sendPendingList = new();
        readonly List<ArraySegment<byte>> _sendBufferList = new();
#else
        readonly RecvBuffer _recvBuffer = new(RecvBufferSize);
        readonly Queue<ArraySegment<byte>> _sendQueue = new();
        readonly List<ArraySegment<byte>> _sendPendingList = new();
#endif

        /// <summary>
        /// Each session reuses one SendEventArgs.
        /// </summary>
        readonly SocketAsyncEventArgs _sendEvent = new();
        /// <summary>
        /// Each session reuses one RecvEventArgs.
        /// </summary>
        readonly SocketAsyncEventArgs _recvEvent = new();

        /// <summary>
        /// Note: Send/Recv can be occurred in multiple threads
        /// </summary>
        readonly object _lock = new();

        /// <summary>
        /// [Temp] The byte segment to store send buffer temporarily.
        /// </summary>
        //ArraySegment<byte> _sendSegment;
        //Memory<byte> _sendMemory;

        #region Abstract Methods
        /// <summary>
        /// Called when the socket is connected.
        /// </summary>
        /// <param name="endPoint">The endpoint of connected socket</param>
        public abstract void OnConnected(EndPoint endPoint);
        /// <summary>
        /// Called when the socket received data(buffer)
        /// </summary>
        /// <param name="buffer">The data received</param>
        public abstract void OnRecv(ArraySegment<byte> buffer);
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
        public abstract void OnRecv(Memory<byte> buffer);
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
        /// Send data to endpoint of the socket.
        /// </summary>
        /// <param name="sendBuffer">A byte buffer which contains data</param>
        public void Send(ArraySegment<byte> sendBuffer)
        {
#if MEMORY_BUFFER
            throw new Exception("The session uses Memory<byte> buffer now.");
#else
            lock (_lock)
            {
                //_sendSegment = sendBuffer;
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
#endif
        }


        public void Send(Memory<byte> sendBuffer)
        {
#if MEMORY_BUFFER
            lock (_lock)
            {
                // _sendMemory = sendBuffer;
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
        void RegisterSend()
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
        void RegisterRecv()
        {
            if (_disconnected == 1) return;

#if MEMORY_BUFFER
            _mRecvBuffer.CleanUp();
            _recvEvent.SetBuffer(_mRecvBuffer.WriteMemory);
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
                        _sendPendingList.Clear();
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
            // Check the length of bytes transferred and SocketError==Success
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                try
                {
#if MEMORY_BUFFER
                    if (_mRecvBuffer.OnWrite(e.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    int processLength = OnRecvProcess(_mRecvBuffer.DataMemory);
                    if (processLength <= 0)
                    {
                        Console.WriteLine("Not enough buffer size");
                        Disconnect();
                        return;
                    }

                    if (_mRecvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    OnRecv(_mRecvBuffer.DataMemory);

                    if (_mRecvBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }

#else
                    // Check if there is enough size to write on RecvBuffer.
                    if (_recvBuffer.OnWrite(e.BytesTransferred) == false)
                    {
                        Disconnect();
                        return;
                    }

                    // Check that the data has been processed normally from OnRecv.
                    int processLength = OnRecvProcess(_recvBuffer.DataSegment);
                    if (processLength <= 0)
                    {
                        Console.WriteLine("Not enough buffer size");
                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.DataSize < processLength)
                    {
                        Disconnect();
                        return;
                    }

                    OnRecv(_recvBuffer.DataSegment);

                    // Check the written data was readable normally => notice 'the data is read'
                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        Disconnect();
                        return;
                    }
#endif

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
        /// Check the received buffer is totally complete.
        /// Note: The header is ushort type and presents the whole size of the data.
        /// </summary>
        /// <param name="buffer">The length of processed bytes in buffer.</param>
        /// <returns></returns>
        int OnRecvProcess(Memory<byte> buffer) 
        {
            int processed = 0;

            // If the size of received buffer is shorter than the header size, it is not the whole data.
            if (buffer.Length < HeaderSize) return 0;

            // Check the whole data is received.
            ushort fullSize = BitConverter.ToUInt16(buffer.Span.Slice(0, HeaderSize));
            // If the size of recieved buffer is hosrter than the whole size of the data, it is not the whole data.
            if (buffer.Length < fullSize) return 0;

            processed += fullSize;
            // buffer = buffer.Slice(fullSize);

            return processed;
        }

        /// <summary>
        /// Check the received buffer is totally complete.
        /// Note: The header is ushort type and presents the whole size of the data.
        /// </summary>
        /// <param name="buffer">The length of processed bytes in buffer.</param>
        /// <returns></returns>
        int OnRecvProcess(ArraySegment<byte> buffer)
        {
            int processed = 0;

            // If the size of received buffer is shorter than the header size, it is not the whole data.
            if (buffer.Count < HeaderSize) return 0;

            // Check the whole data is received.
            ushort fullSize = BitConverter.ToUInt16(buffer.Array , buffer.Offset);
            // If the size of recieved buffer is hosrter than the whole size of the data, it is not the whole data.
            if (buffer.Count < fullSize) return 0;

            processed += fullSize;
            // buffer = buffer.Slice(fullSize);

            return processed;
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
            }
        }
    }
}
