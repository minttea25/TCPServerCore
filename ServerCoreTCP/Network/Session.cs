#nullable enable

using NetCore.CLogger;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace NetCore
{
    /// <summary>
    /// Session object of the core.
    /// </summary>
    public abstract class Session : SocketObject
    {
        /// <summary>
        /// Unique id of Session.
        /// </summary>
        public uint SessionId
        {
            get { return m_sessionId; }
            internal set { m_sessionId = value; }
        }

        /// <summary>
        /// [Nullable] Connected Endpoint.<br/>(Null if the socket is null or not connected)
        /// </summary>
        public EndPoint? ConnectedEndPoint => m_socket?.RemoteEndPoint;
        /// <summary>
        /// The socket of the session.
        /// </summary>
        public Socket? Socket => m_socket;

        protected Socket? m_socket = null;


        /// <summary>
        /// The value to check the session connected;<br/>0: disconnected<br/>1: connected
        /// </summary>
        int _connected = 0;
        uint m_sessionId;

        SocketAsyncEventArgs? _recvEventArgs = null;
        SocketAsyncEventArgs? _sendEventArgs = null;

        /// <summary>
        /// Note: Send/Recv can be occurred in multiple threads
        /// </summary>
        readonly object _lock = new object();

        /// <summary>
        /// Dispatch the socket event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        internal sealed override void Dispatch(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is SocketEventToken)) throw new InvalidCastException("The UserToken was not SocketEventToken"); ;

            switch (eventArgs.LastOperation)
            {
                case SocketAsyncOperation.Disconnect:
                    OnDisconnectedCompleted(eventArgs);
                    break;
                case SocketAsyncOperation.Receive:
                    OnRecvCompleted(eventArgs);
                    break;
                case SocketAsyncOperation.Send:
                    OnSendCompleted(eventArgs);
                    break;
                case SocketAsyncOperation.Connect:
                    throw new InvalidOperationException("The UserToken was ConnectEventToken at Session");
                case SocketAsyncOperation.Accept:
                    throw new InvalidOperationException("The UserToken was AcceptEventToken at Session");
                default:
                    throw new InvalidOperationException("The UserToken was unknown at Session");
            }
        }

        readonly RecvBuffer _recvBuffer;
        readonly Queue<ArraySegment<byte>> _sendQueue;
        readonly List<ArraySegment<byte>> _sendPendingList;

        #region Abstract Methods
        /// <summary>
        /// Called when the socket is connected.
        /// </summary>
        /// <param name="endPoint">The endpoint of connected socket</param>
        public abstract void OnConnected(EndPoint endPoint);
#if PROTOBUF
        /// <summary>
        /// Called when the socket received data.
        /// </summary>
        /// <param name="buffer">The buffer of unit packet received.</param>
        public abstract void OnRecv(ReadOnlySpan<byte> buffer);
#endif
#if FLATBUFFERS
        /// <summary>
        /// Called when the socket received data.
        /// </summary>
        /// <param name="buffer">Received buffer</param>
        /// <param name="offset">offset of the data</param>
        /// <param name="count">length of the data</param>
        public abstract void OnRecv(ArraySegment<byte> buffer, int offset, int count);
#endif
        /// <summary>
        /// Called when the socket sent data.
        /// </summary>
        /// <param name="numOfBytes">The length of bytes transferred.</param>
        public abstract void OnSend(int numOfBytes);
        /// <summary>
        /// Called when the socket is disconnected.
        /// </summary>
        /// <param name="endPoint">The end point of the socket.</param>
        /// <param name="error">The additional object of error</param>
        public abstract void OnDisconnected(EndPoint endPoint, object? error = null);

        /// <summary>
        /// Override this method to process the received buffer.
        /// </summary>
        /// <param name="buffer">The buffer received on socket.</param>
        /// <returns>The length of processed bytes.</returns>
        protected abstract int OnRecvProcess(ArraySegment<byte> buffer);
        #endregion

        /// <summary>
        /// Constructor of Session.
        /// </summary>
        public Session()
        {
            _recvBuffer = new RecvBuffer(Defines.RecvBufferSize);
            _sendQueue = new Queue<ArraySegment<byte>>();
            _sendPendingList = new List<ArraySegment<byte>>();
        }


        /// <summary>
        /// A session must be initialized with this method with socket.
        /// </summary>
        /// <param name="socket">The socket to be connected to the session.</param>
        internal void Init(Socket socket)
        {
            m_socket = socket;

            _connected = 1;

            _recvEventArgs = m_service.m_saeaPool.Pop();
            _sendEventArgs = m_service.m_saeaPool.Pop();

            _recvEventArgs.UserToken = new RecvEventToken(this);
            _sendEventArgs.UserToken = new SendEventToken(this);

            RegisterRecv();
        }

        ~Session()
        {
            ;
        }

        /// <summary>
        /// [lock] Send data to endpoint.
        /// </summary>
        /// <param name="sendBuffer">buffer of data</param>
        /// <exception cref="Exception">if the session is not connected or the length of buffer is 0</exception>
        /// <exception cref="ArgumentNullException">if buffer is null</exception>
        public void Send(ArraySegment<byte> sendBuffer)
        {
            if (_connected == 0) throw new Exception("This session is not connected.");
            if (sendBuffer == null) throw new ArgumentNullException("Failed to serialize the message.");
            if (sendBuffer.Count == 0) throw new Exception("Parameter sendBuffer is empty.");

            lock (_lock)
            {
                _sendQueue.Enqueue(sendBuffer);

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }

        /// <summary>
        /// [lock] Send a list of data to endpoint of the socket.
        /// </summary>
        /// <param name="sendBufferList">A list of serialized data to send</param>
        /// <exception cref="Exception">if sendBufferList is empty.</exception>
        protected void Send(List<ArraySegment<byte>> sendBufferList)
        {
            if (sendBufferList.Count == 0) return;

            if (sendBufferList.Count == 0) throw new Exception("The count of 'sendBufferList' was 0.");

            lock (_lock)
            {
                foreach (var buffer in sendBufferList)
                {
                    _sendQueue.Enqueue(buffer);
                }

                if (_sendPendingList.Count == 0) RegisterSend();
            }
        }

        #region Network IO

        /// <summary>
        /// Send asynchrously all buffer in sendPendingList.
        /// </summary>
        protected void RegisterSend()
        {
            // If it is already disconnected, return
            if (_connected == 0) return;

            // NOTE:
            // DO NOT ADD ITEMS to eventArgs.BufferList through Add method.
            // It does not work well and causes SocketError.InvalidArguments.
            // USE EventArgs.BufferList = list instead.
            // WHY? https://stackoverflow.com/questions/11820677/how-use-bufferlist-with-socketasynceventargs-and-not-get-socketerror-invalidargu

            while (_sendQueue.Count > 0)
            {
                _sendPendingList.Add(_sendQueue.Dequeue());
            }

            _sendEventArgs!.BufferList = _sendPendingList;

            try
            {
                bool pending = m_socket!.SendAsync(_sendEventArgs);
                if (pending == false) OnSendCompleted(_sendEventArgs);
            }
            catch (Exception ex)
            {
                CoreLogger.LogError("Session.RegisterSend", ex, "Exception");
            }
        }

        /// <summary>
        /// Receive asynchrously.
        /// </summary>
        protected void RegisterRecv()
        {
            if (_connected == 0) return;


            _recvBuffer.CleanUp(); // expensive
            var segment = _recvBuffer.WriteSegment;
            _recvEventArgs!.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = m_socket!.ReceiveAsync(_recvEventArgs);
                if (pending == false) OnRecvCompleted(_recvEventArgs);
            }
            catch (Exception ex)
            {
                CoreLogger.LogError("Session.RegisterSend", ex, "Exception");
            }
        }

        /// <summary>
        /// Callback that is called when send-operation is completed.
        /// </summary>
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

                        _sendEventArgs!.BufferList = null;
                        _sendPendingList.Clear();

                        if (_sendQueue.Count > 0) RegisterSend();
                    }
                    catch (Exception ex)
                    {
                        CoreLogger.LogError("Session.OnSendCompleted", ex, "Exception");
                    }
                }
                else if (eventArgs.BytesTransferred == 0)
                {
                    CoreLogger.LogError("Session.OnSendCompleted", "BytesTransferred was 0 at id={0}", SessionId);
                }
                else if (eventArgs.SocketError != SocketError.Success)
                {
                    CoreLogger.LogError("Session.OnSendCompleted", "SocketError was {0}", eventArgs.SocketError);
                }
                else
                {
                    CoreLogger.LogError("Session.OnSendCompleted", "Other error");
                    Disconnect("Other error");
                }
            }
        }

        /// <summary>
        /// Callback that is called when recv-operation is completed.
        /// </summary>
        /// <param name="eventArgs">An object that contains the socket-async-recv-event data</param>
        void OnRecvCompleted(SocketAsyncEventArgs eventArgs)
        {
            // Check the length of bytes transferred and SocketError==Success
            if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success)
            {
                try
                {
                    if (_recvBuffer.OnWrite(eventArgs.BytesTransferred) == false)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted", "The numOfBytes is larger than current data size at {0}", Socket?.RemoteEndPoint);

                        Disconnect("Failed to write recvBuffer.");
                        return;
                    }

                    int processLength = OnRecvProcess(_recvBuffer.DataSegment);

                    if (processLength <= 0)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted at {0}", "processLength <= 0", Socket?.RemoteEndPoint);
                        Disconnect("Processed length is 0 or less.");
                        return;
                    }

                    if (_recvBuffer.DataSize < processLength)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted", "The datasize of recvBuffer[{0}] was larger than processLength[{1}] at {3}", _recvBuffer.DataSize, processLength, Socket?.RemoteEndPoint);
                        Disconnect("RecvBuffer Overflow");
                        return;
                    }

                    if (_recvBuffer.OnRead(processLength) == false)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted", "The numOfBytes was larger than current data size at {0}", Socket?.RemoteEndPoint);
                        Disconnect("RecvBuffer Overflow");
                        return;
                    }

                    // Wait to receive again.
                    RegisterRecv();
                }
                catch (Exception ex)
                {
                    CoreLogger.LogError("Session.OnRecvCompleted", ex, "Exception");
                }
            }
            else if (eventArgs.SocketError != SocketError.Success)
            {
                CoreLogger.LogError("Session.OnRecvCompleted", "SocketError was {0} at {1}", eventArgs.SocketError, Socket?.RemoteEndPoint);
                Disconnect("SocketError was not Success.");
            }
            else if (eventArgs.BytesTransferred == 0)
            {
                CoreLogger.LogError("Session.OnRecvCompleted", "BytesTransferred was 0 at {0}", Socket?.RemoteEndPoint);
                Disconnect("BytesTransferred was 0.");
            }
            else
            {
                CoreLogger.LogError("Session.OnRecvCompleted", "Other error at {0}", Socket?.RemoteEndPoint);
                Disconnect("Other error");
            }
        }

        #endregion

        /// <summary>
        /// Close the connection and the socket and clear the session.
        /// </summary>
        public void Disconnect(string? msg)
        {
            // Check that it is already disconnected
            if (Interlocked.Exchange(ref _connected, 0) == 0) return;

            OnDisconnected(m_socket!.RemoteEndPoint, msg);

            // Shutdown send/recv both
            m_socket.Shutdown(SocketShutdown.Both);
            m_socket.Close();

            if (m_service.ServiceType == Service.ServiceTypes.Server)
            {
                (m_service as ServerService)?.m_sessionPool.Push(this);
                (m_service as ServerService)?.NotifySessionDisconnected();
            }
            else Clear();
        }

        /// <summary>
        /// Callback of disconnect operation.
        /// </summary>
        /// <param name="eventArgs"></param>
        public virtual void OnDisconnectedCompleted(SocketAsyncEventArgs eventArgs)
        {
            ;
        }

        internal void Clear()
        {
            _connected = 0;
            m_socket = null;

            m_service.m_saeaPool.Push(_recvEventArgs);
            m_service.m_saeaPool.Push(_sendEventArgs);

            _recvEventArgs = null;
            _sendEventArgs = null;

            _recvBuffer.ClearBuffer();

            lock (_lock)
            {
                _sendQueue.Clear();
                _sendPendingList.Clear();
            }
        }
    }
}
