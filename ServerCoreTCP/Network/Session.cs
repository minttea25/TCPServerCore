﻿using ServerCoreTCP.LoggerDebug;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace ServerCoreTCP
{
    public abstract class Session : SocketObject
    {
        public const int RecvBufferSize = 65535;

        public uint SessionId => m_sessionId;
        public EndPoint ConnectedEndPoint => m_socket?.RemoteEndPoint;



        protected Socket m_socket;


        /// <summary>
        /// The value to check the session connected. (Used with Interlocked)
        /// </summary>
        int m_connected = 0;
        uint m_sessionId;
        SocketAsyncEventArgs m_recvEventArgs = null;
        SocketAsyncEventArgs m_sendEventArgs = null;
        /// <summary>
        /// Note: Send/Recv can be occurred in multiple threads
        /// </summary>
        readonly object _lock = new object();


        internal void SetService(Service service)
        {
            m_service = service;
        }

        internal void SetSessionId(uint id)
        {
            m_sessionId = id;
        }

        internal sealed override void Dispatch(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is SocketEventToken)) throw new InvalidCastException();

            switch (eventArgs.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    throw new InvalidOperationException();
                case SocketAsyncOperation.Disconnect:
                    OnDisconnectedCompleted(eventArgs);
                    break;
                case SocketAsyncOperation.Accept:
                    throw new InvalidOperationException();
                case SocketAsyncOperation.Receive:
                    OnRecvCompleted(eventArgs);
                    break;
                case SocketAsyncOperation.Send:
                    OnSendCompleted(eventArgs);
                    break;
                default:
                    break;
            }
        }


#if MEMORY_BUFFER
        protected readonly MRecvBuffer _recvBuffer = new MRecvBuffer(RecvBufferSize);
        protected readonly Queue<Memory<byte>> _sendQueue = new Queue<Memory<byte>>();
        protected readonly List<Memory<byte>> _sendPendingList = new List<Memory<byte>>();
        protected readonly List<ArraySegment<byte>> _sendBufferList = new List<ArraySegment<byte>>();
#else
        protected readonly RecvBuffer m_recvBuffer = new RecvBuffer(RecvBufferSize);
        protected readonly Queue<ArraySegment<byte>> m_sendQueue = new Queue<ArraySegment<byte>>();
        protected readonly List<ArraySegment<byte>> m_sendPendingList = new List<ArraySegment<byte>>();
#endif

        #region Abstract Methods
        /// <summary>
        /// Called when the socket is connected. Initialize values here.
        /// </summary>
        public abstract void InitSession();
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
        /// A session must be initialized with this method with socket.
        /// </summary>
        /// <param name="socket">The socket to be connected to the session.</param>
        public void Init(Socket socket)
        {
            m_socket = socket;

            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("A new session is created. [EndPoint: {ConnectedEndPoint}]", ConnectedEndPoint);

            m_connected = 1;

            m_recvEventArgs = m_service.m_saeaPool.Pop();
            m_sendEventArgs = m_service.m_saeaPool.Pop();

            m_recvEventArgs.UserToken = new RecvEventToken(this);
            m_sendEventArgs.UserToken = new SendEventToken(this);

            InitSession();

            RegisterRecv();
        }

        ~Session()
        {
            ;
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
            if (sendBuffer.Count == 0) throw new Exception("The count of 'sendBuffer' was 0.");

            lock (_lock)
            {
                m_sendQueue.Enqueue(sendBuffer);

                if (m_sendPendingList.Count == 0) RegisterSend();
            }
        }

        /// <summary>
        /// Send a list of data to endpoint of the socket. [ArraySegment]
        /// </summary>
        /// <param name="sendBufferList">A list of serialized data to send</param>
        public void SendRaw(List<ArraySegment<byte>> sendBufferList)
        {
            if (sendBufferList.Count == 0) throw new Exception("The count of 'sendBufferList' was 0.");

            lock (_lock)
            {
                foreach (var buffer in sendBufferList)
                {
                    m_sendQueue.Enqueue(buffer);
                }

                if (m_sendPendingList.Count == 0) RegisterSend();
            }
        }
#endif


        /// <summary>
        /// Reserve 'Send' for async-send (Note: It needs to be protected for race-condition.)
        /// </summary>
        protected void RegisterSend()
        {
            // If it is already disconnected, return
            if (m_connected == 0) return;

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
            while (m_sendQueue.Count > 0)
            {
                m_sendPendingList.Add(m_sendQueue.Dequeue());
            }

            m_sendEventArgs.BufferList = m_sendPendingList;
#endif

            try
            {
                bool pending = m_socket.SendAsync(m_sendEventArgs);
                if (pending == false) OnSendCompleted(m_sendEventArgs);
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
            if (m_connected == 0) return;

#if MEMORY_BUFFER
            _recvBuffer.CleanUp();
            _recvEvent.SetBuffer(_recvBuffer.WriteMemory);
#else
            m_recvBuffer.CleanUp(); // expensive
            var segment = m_recvBuffer.WriteSegment;
            m_recvEventArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);
#endif

            try
            {
                bool pending = m_socket.ReceiveAsync(m_recvEventArgs);
                if (pending == false) OnRecvCompleted(m_recvEventArgs);
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
        /// <param name="eventArgs">An object that contains the socket-async-send-event data</param>
        void OnSendCompleted(SocketAsyncEventArgs eventArgs)
        {
            lock (_lock)
            {
                // Check the length of bytes transferred and SocketError==Success
                if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success)
                {
                    try
                    {
                        OnSend(eventArgs.BytesTransferred);
#if MEMORY_BUFFER
                        _sendBufferList.Clear();
                        //e.SetBuffer(null);
                        _sendEvent.BufferList = null;
#else
                        m_sendEventArgs.BufferList = null;
                        m_sendPendingList.Clear();
#endif
                    }
                    catch (Exception ex)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error(ex, "OnSendCompleted - Exception");
                    }
                }
                else if (eventArgs.BytesTransferred == 0)
                {
                    if (CoreLogger.Logger != null)
                        CoreLogger.Logger.Error("OnSendCompleted - The length of sent data is 0.");
                }
                else if (eventArgs.SocketError != SocketError.Success)
                {
                    if (CoreLogger.Logger != null)
                        CoreLogger.Logger.Error("OnSendCompleted - SocketError: {SocketError}", eventArgs.SocketError);
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
        /// <param name="eventArgs">An object that contains the socket-async-recv-event data</param>
        void OnRecvCompleted(SocketAsyncEventArgs eventArgs)
        {
#if DEBUG
            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("Received: {BytesTransferred} bytes", e.BytesTransferred);
#endif

            // Check the length of bytes transferred and SocketError==Success
            if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success)
            {
                try
                {
                    if (m_recvBuffer.OnWrite(eventArgs.BytesTransferred) == false)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error("OnRecvCompleted - RecvBuffer.OnWrite() was false");

                        Disconnect();
                        return;
                    }
#if MEMORY_BUFFER
                    int processLength = OnRecvProcess(_recvBuffer.DataMemory);
#else
                    int processLength = OnRecvProcess(m_recvBuffer.DataSegment);
#endif
                    if (processLength <= 0)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error("OnRecvCompleted - processLength <= 0");

                        Disconnect();
                        return;
                    }

                    if (m_recvBuffer.DataSize < processLength)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error("OnRecvCompleted - RecvBuffer.DataSize < processLength");

                        Disconnect();
                        return;
                    }

                    if (m_recvBuffer.OnRead(processLength) == false)
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
            else if (eventArgs.SocketError != SocketError.Success)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("OnRecvCompleted - SocketError: {SocketError}", eventArgs.SocketError);

                Disconnect();
            }
            else if (eventArgs.BytesTransferred == 0)
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
            if (Interlocked.Exchange(ref m_connected, 0) == 0) return;

            OnDisconnected(m_socket.RemoteEndPoint);

            // Shutdown send/recv both
            m_socket.Shutdown(SocketShutdown.Both);
            m_socket.Close();

            Clear();
        }

        public void OnDisconnectedCompleted(SocketAsyncEventArgs eventArgs)
        {

        }

        internal void Clear()
        {
            m_connected = 0;
            m_socket = null;

            m_service.m_saeaPool.Push(m_recvEventArgs);
            m_service.m_saeaPool.Push(m_sendEventArgs);

            m_recvEventArgs = null;
            m_sendEventArgs = null;

            lock (_lock)
            {
                m_sendQueue.Clear();
                m_sendPendingList.Clear();
#if MEMORY_BUFFER
                _sendBufferList.Clear();
#endif
            }
        }
    }
}