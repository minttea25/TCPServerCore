using ServerCoreTCP.CLogger;
using ServerCoreTCP.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    internal class Connector : SocketObject
    {
        public enum ConnectError
        {
            Success = 0,
            Timeout = 1,
            OutOfRangePort = 2,
            InvalidOperation = 3,
            SocketError = 4,
            EtcError = 5,
        }

        readonly IPEndPoint _endPoint;
        readonly int m_connectionCount = 1;
        readonly Session[] m_serverSession;
        readonly ClientService m_clientService;

        readonly Action<SocketError> _connectFailedCallback;
        readonly bool _reuseAddress;

        public ClientService ClientService => m_clientService;
        public int ConnectionCount => m_connectionCount;

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
            if (eventArgs.SocketError == SocketError.Success)
            {
                Session session = (eventArgs.UserToken as ConnectEventToken).m_session;
                session.Init(eventArgs.ConnectSocket);
                session.OnConnected(eventArgs.RemoteEndPoint);
            }
            else
            {
                CoreLogger.LogError("Connector.OnConnectCompleted", "SocketError was {0}. EndPoint: {1}",eventArgs.SocketError , eventArgs.RemoteEndPoint);
                _connectFailedCallback?.Invoke(eventArgs.SocketError);
            }
        }
    }
}
