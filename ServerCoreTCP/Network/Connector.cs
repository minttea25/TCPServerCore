using ServerCoreTCP.CLogger;
using ServerCoreTCP.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCoreTCP
{
    /// <summary>
    /// The connector object to connect to other endpoint. The default connected count is 1, and it also provided the multiple connections for doing test.
    /// </summary>
    internal class Connector : SocketObject
    {
        readonly IPEndPoint _endPoint;
        readonly int m_connectionCount = 1;
        readonly Session[] m_serverSession;
        readonly ClientService m_clientService;

        readonly Action<SocketError> _connectFailedCallback;
        readonly bool _reuseAddress;

        /// <summary>
        /// The referenced ClientService
        /// </summary>
        public ClientService ClientService => m_clientService;
        /// <summary>
        /// The count of the connections which is configured at first.
        /// </summary>
        public int ConnectionCount => m_connectionCount;

        /// <summary>
        /// The constructor of the connector.
        /// </summary>
        /// <param name="clientService">The configured client service</param>
        /// <param name="session">The empty sessions are created in advance</param>
        /// <param name="endPoint">The endpoint to connect</param>
        /// <param name="config">The ClientServiceConfig that contains information of service</param>
        /// <param name="connectFailedCallback">The callback which is invoked when the connection is failed.</param>
        public Connector(ClientService clientService, Session[] session, IPEndPoint endPoint, ClientServiceConfig config, Action<SocketError> connectFailedCallback = null)
        {
            _reuseAddress = config.ReuseAddress;

            _connectFailedCallback = connectFailedCallback;

            m_service = clientService;
            m_clientService = clientService;
            m_connectionCount = config.ClientServiceSessionCount;
            _endPoint = endPoint;
            m_serverSession = session;
        }

        internal sealed override void Dispatch(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is ConnectEventToken _)) throw new InvalidCastException();

            OnConnectCompleted(eventArgs);
        }

        internal void Connect()
        {
            CoreLogger.LogInfo("Connector.Connect", "Connector is trying to connect the server: {0}, count={1}", _endPoint, m_connectionCount);
            try
            {
                for (int i = 0; i < m_connectionCount; ++i)
                {
                    Socket socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    SocketAsyncEventArgs connectEventArgs = m_service.m_saeaPool.Pop();
                    connectEventArgs.RemoteEndPoint = _endPoint;
                    connectEventArgs.UserToken = new ConnectEventToken(this, socket, m_serverSession[i]);

                    socket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    socket.SetReuseAddress(_reuseAddress);

                    RegisterConnect(socket, connectEventArgs);
                }
            }
            catch (Exception ex)
            {
                CoreLogger.LogError("Connector.Connect", ex, "Exception");
                throw ex;
            }
        }

        void RegisterConnect(Socket socket, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is ConnectEventToken _)) throw new InvalidCastException();

            try
            {
                bool pending = socket.ConnectAsync(eventArgs);
                if (pending == false)
                {
                    OnConnectCompleted(eventArgs);
                }
            }
            catch (Exception ex)
            {
                CoreLogger.LogError("Connector.RegisterConnect", ex, "Exception");
            }
        }

        void OnConnectCompleted(SocketAsyncEventArgs eventArgs)
        {
            try
            {
                if (eventArgs.SocketError == SocketError.Success)
                {
                    Session session = (eventArgs.UserToken as ConnectEventToken).m_session;
                    session.Init(eventArgs.ConnectSocket);
                    session.OnConnected(eventArgs.RemoteEndPoint);
                }
                else
                {
                    CoreLogger.LogError("Connector.OnConnectCompleted", "SocketError was {0}. EndPoint: {1}", eventArgs.SocketError, eventArgs.RemoteEndPoint);
                    _connectFailedCallback?.Invoke(eventArgs.SocketError);
                }
            }
            catch (Exception ex)
            {
                CoreLogger.LogError("Connector.OnConnectedCompleted", ex, "An exception occurred.");
            }
        }
    }
}
