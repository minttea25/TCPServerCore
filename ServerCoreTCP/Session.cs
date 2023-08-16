﻿using ServerCoreTCP.LoggerDebug;
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
        protected readonly MRecvBuffer _recvBuffer = new MRecvBuffer(RecvBufferSize);
        protected readonly Queue<Memory<byte>> _sendQueue = new Queue<Memory<byte>>();
        protected readonly List<Memory<byte>> _sendPendingList = new List<Memory<byte>>();
        protected readonly List<ArraySegment<byte>> _sendBufferList = new List<ArraySegment<byte>>();
#else
        protected readonly RecvBuffer _recvBuffer = new RecvBuffer(RecvBufferSize);
        protected readonly Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        protected readonly List<ArraySegment<byte>> _sendPendingList = new List<ArraySegment<byte>>();
#endif

        /// <summary>
        /// Each session reuses one SendEventArgs.
        /// </summary>
        protected readonly SocketAsyncEventArgs _sendEvent = new SocketAsyncEventArgs();
        /// <summary>
        /// Each session reuses one RecvEventArgs.
        /// </summary>
        protected readonly SocketAsyncEventArgs _recvEvent = new SocketAsyncEventArgs();

        /// <summary>
        /// Note: Send/Recv can be occurred in multiple threads
        /// </summary>
        protected readonly object _lock = new object();

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
#if MEMORY_BUFFER
        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        protected abstract int OnRecvProcess(Memory<byte> buffer);
#else
        /// <summary>
        /// Check the received buffer. If there are multiple packet data on the buffer, each data is processed separately. OnRecv will be called here.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        protected abstract int OnRecvProcess(ArraySegment<byte> buffer);
#endif

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

            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("A new session is created. [EndPoint: {ConnectedEndPoint}]", ConnectedEndPoint);

            _sendEvent.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);
            _recvEvent.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

            RegisterRecv();
        }

        ~Session()
        {
            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("Session [endpoint={ConnectedEndPoint}] is removed.", ConnectedEndPoint);
        }

#if MEMORY_BUFFER
        /// <summary>
        /// Send data to endpoint of the socket. [Memory]
        /// </summary>
        /// <param name="sendBuffer">Serialized data to send</param>
        public void SendRaw(Memory<byte> sendBuffer)
        {
            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }

        /// <summary>
        /// Send a list of data to endpoint of the socket. [Memory]
        /// </summary>
        /// <param name="sendBufferList">A list of serialized data to send</param>
        public void SendRaw(List<Memory<byte>> sendBufferList)
        {
            if (sendBufferList.Count == 0) return;

            lock (_lock)
            {
                foreach (var buffer in sendBufferList)
                {
                    _sendQueue.Enqueue(buffer);
                }

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }
#else
        /// <summary>
        /// Send data to endpoint of the socket. [ArraySegment]
        /// </summary>
        /// <param name="sendBuffer">Serialized data to send</param>
        public void SendRaw(ArraySegment<byte> sendBuffer)
        {
            int len = sendBuffer.Count;
            var b = SendBufferTLS.Reserve(len);
            sendBuffer.CopyTo(b);
            sendBuffer = SendBufferTLS.Return(len);

            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }

        /// <summary>
        /// Send a list of data to endpoint of the socket. [ArraySegment]
        /// </summary>
        /// <param name="sendBufferList">A list of serialized data to send</param>
        public void SendRaw(List<ArraySegment<byte>> sendBufferList)
        {
            if (sendBufferList.Count == 0) return;

            lock (_lock)
            {
                foreach (var buffer in sendBufferList)
                {
                    _sendQueue.Enqueue(buffer);
                }

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }
#endif


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
                    if (CoreLogger.Logger != null)
                        CoreLogger.Logger.Error("An error occured at RegisterSend: MemoryMarshal.TryGetArray<byte>");
                }
            }
            _sendEvent.BufferList = _sendBufferList;
            _sendPendingList.Clear();
#else
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
            catch (Exception ex)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error(ex, "RegisterSend - Exception");
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
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error(ex, "RegisterRecv - Exception");
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
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error(ex, "OnSendCompleted - Exception");
                    }
                }
                else if (e.BytesTransferred == 0)
                {
                    if (CoreLogger.Logger != null)
                        CoreLogger.Logger.Error("OnSendCompleted - The length of sent data is 0.");
                }
                else if (e.SocketError != SocketError.Success)
                {
                    if (CoreLogger.Logger != null)
                        CoreLogger.Logger.Error("OnSendCompleted - SocketError: {SocketError}", e.SocketError);
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
            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("Received: {BytesTransferred} bytes", e.BytesTransferred);
#endif

            // Check the length of bytes transferred and SocketError==Success
            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                try
                {
                    if (_recvBuffer.OnWrite(e.BytesTransferred) == false)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error("OnRecvCompleted - RecvBuffer.OnWrite() was false");

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
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error("OnRecvCompleted - processLength <= 0");

                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.DataSize < processLength)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error("OnRecvCompleted - RecvBuffer.DataSize < processLength");

                        Disconnect();
                        return;
                    }

                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error("OnRecvCompleted - RecvBuffer.OnRead() was false");

                        Disconnect();
                        return;
                    }

                    // Wait to receive again.
                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    if (CoreLogger.Logger != null)
                        CoreLogger.Logger.Error(ex, "OnRecvCompleted - Exception");
                }
            }
            else if (e.SocketError != SocketError.Success)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("OnRecvCompleted - SocketError: {SocketError}", e.SocketError);

                Disconnect();
            }
            else if (e.BytesTransferred == 0)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("OnRecvCompleted - The length of received data is 0.");

                Disconnect();
            }
            else
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("OnRecvCompleted - Unknown.");

                Disconnect();
            }
        }

        /// <summary>
        /// Close the socket and clear the session.
        /// </summary>
        public void Disconnect()
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
