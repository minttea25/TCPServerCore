using ServerCoreTCP.CLogger;
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
        public const int RecvBufferSize = 65535 * 10;

        public uint SessionId => m_sessionId;
        public EndPoint ConnectedEndPoint => m_socket?.RemoteEndPoint;



        protected Socket m_socket;


        /// <summary>
        /// The value to check the session connected; 0: disconnected, 1: connected (Used with Interlocked)
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

        protected readonly RecvBuffer m_recvBuffer;
        protected readonly Queue<ArraySegment<byte>> m_sendQueue;
        protected readonly List<ArraySegment<byte>> m_sendPendingList;

        #region Abstract Methods
        /// <summary>
        /// Called when the socket is connected. Initialize values here.
        /// </summary>
        public abstract void InitSession();
        /// <summary>
        /// Called before the session is cleaned up.
        /// </summary>
        public abstract void PreSessionCleanup();
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
        protected abstract int OnRecvProcess(ArraySegment<byte> buffer);


        #endregion

        public Session()
        {

            m_recvBuffer = new RecvBuffer(RecvBufferSize);
            m_sendQueue = new Queue<ArraySegment<byte>>();
            m_sendPendingList = new List<ArraySegment<byte>>();
        }


        /// <summary>
        /// A session must be initialized with this method with socket.
        /// </summary>
        /// <param name="socket">The socket to be connected to the session.</param>
        public void Init(Socket socket)
        {
            m_socket = socket;

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

            while (m_sendQueue.Count > 0)
            {
                m_sendPendingList.Add(m_sendQueue.Dequeue());
            }

            m_sendEventArgs.BufferList = m_sendPendingList;

            try
            {
                bool pending = m_socket.SendAsync(m_sendEventArgs);
                if (pending == false) OnSendCompleted(m_sendEventArgs);
            }
            catch (Exception ex)
            {
                CoreLogger.LogError("Session.RegisterSend", ex, "Exception");
            }
        }

        /// <summary>
        /// Reserve 'Receive' for async-receive
        /// </summary>
        protected void RegisterRecv()
        {
            if (m_connected == 0) return;


            m_recvBuffer.CleanUp(); // expensive
            var segment = m_recvBuffer.WriteSegment;
            m_recvEventArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = m_socket.ReceiveAsync(m_recvEventArgs);
                if (pending == false) OnRecvCompleted(m_recvEventArgs);
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

                        m_sendEventArgs.BufferList = null;
                        m_sendPendingList.Clear();
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
            // Check the length of bytes transferred and SocketError==Success
            if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success)
            {
                try
                {
                    if (m_recvBuffer.OnWrite(eventArgs.BytesTransferred) == false)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted", "The numOfBytes is larger than current data size");

                        Disconnect();
                        return;
                    }

                    int processLength = OnRecvProcess(m_recvBuffer.DataSegment);

                    if (processLength <= 0)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted", "processLength <= 0");
                        Disconnect();
                        return;
                    }

                    if (m_recvBuffer.DataSize < processLength)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted", "The datasize of recvBuffer[{0}] was larger than processLength[{1}]", m_recvBuffer.DataSize, processLength);
                        Disconnect();
                        return;
                    }

                    if (m_recvBuffer.OnRead(processLength) == false)
                    {
                        CoreLogger.LogError("Session.OnRecvCompleted", "The numOfBytes was larger than current data size");
                        Disconnect();
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
                CoreLogger.LogError("Session.OnRecvCompleted", "SocketError was {0}", eventArgs.SocketError);
                Disconnect();
            }
            else if (eventArgs.BytesTransferred == 0)
            {
                CoreLogger.LogError("Session.OnRecvCompleted", "BytesTransferred was 0");
                Disconnect();
            }
            else
            {
                CoreLogger.LogError("Session.OnRecvCompleted", "Other error");
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

            if (m_service.ServiceType == Service.ServiceTypes.Server)
            {
                (m_service as ServerService).m_sessionPool.Push(this);
            }
            else Clear();
        }

        public void OnDisconnectedCompleted(SocketAsyncEventArgs eventArgs)
        {
            ;
        }

        internal void Clear()
        {
            PreSessionCleanup();

            m_connected = 0;
            m_socket = null;

            m_service.m_saeaPool.Push(m_recvEventArgs);
            m_service.m_saeaPool.Push(m_sendEventArgs);

            m_recvEventArgs = null;
            m_sendEventArgs = null;


            m_recvBuffer.ClearBuffer();


            lock (_lock)
            {
                m_sendQueue.Clear();
                m_sendPendingList.Clear();
            }
        }
    }
}
