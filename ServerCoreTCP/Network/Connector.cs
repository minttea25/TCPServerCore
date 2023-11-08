using ServerCoreTCP.CLogger;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    public class Connector : SocketObject
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

        const int TimeoutMilliSeconds = 5000;

        IPEndPoint m_endPoint;
        readonly int m_connectionCount = 1;
        Session[] m_serverSession;
        ClientService m_clientService;

        public EventHandler<ConnectError> ConnectHandler;


        public Connector(ClientService clientService, Session[] session, IPEndPoint endPoint, int count = 1)
        {
            m_service = clientService;
            m_clientService = clientService;
            m_connectionCount = count;
            m_endPoint = endPoint;
            m_serverSession = session;
        }

        internal sealed override void Dispatch(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is ConnectEventToken _)) throw new InvalidCastException();

            OnConnectCompleted(eventArgs);
        }

        internal void Connect()
        {
            CoreLogger.LogInfo("Connector.Connect", "Connector is trying to connect the server: {0}, count={1}", m_endPoint, m_connectionCount);

            for (int i = 0; i < m_connectionCount; ++i)
            {
                Socket socket = new Socket(m_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                SocketAsyncEventArgs connectEventArgs = m_service.m_saeaPool.Pop();
                connectEventArgs.RemoteEndPoint = m_endPoint;
                connectEventArgs.UserToken = new ConnectEventToken(this, socket, m_serverSession[i]);

                RegisterConnect(socket, connectEventArgs);
            }
        }

        void RegisterConnect(Socket socket, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is ConnectEventToken token)) throw new InvalidCastException();

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
            }
        }


        ///// <summary>
        ///// Create a socket and connect to end point
        ///// </summary>
        ///// <param name="endPoint">The endpoint to connect to</param>
        ///// <param name="timeoutMilliSeconds">Timeout</param>
        //public async void Connect(int timeoutMilliSeconds = TimeoutMilliSeconds)
        //{
        //    if (CoreLogger.Logger != null)
        //        CoreLogger.Logger.Information("Connector is trying to connect the server: {m_endPoint}", m_endPoint);

        //    Socket socket = new Socket(
        //            m_endPoint.AddressFamily,
        //            SocketType.Stream,
        //            ProtocolType.Tcp);

        //    ConnectError error = await ConnectTimeout(socket, m_endPoint, timeoutMilliSeconds);
        //    ConnectHandler?.Invoke(this, error);
        //}

        //async Task<ConnectError> ConnectTimeout(Socket socket, EndPoint endPoint, int timeoutMilliseconds)
        //{
        //    Task t1 = socket.ConnectAsync(endPoint);
        //    Task t2 = Task.Delay(timeoutMilliseconds);
        //    Task completedTask = await Task.WhenAny(t1, t2);

        //    if (completedTask == t1)
        //    {
        //        try
        //        {
        //            await t1; // Await t1 to propagate exceptions

        //            if (CoreLogger.Logger != null)
        //                CoreLogger.Logger.Information("[Connecter] Connected: {RemoteEndPoint}", socket.RemoteEndPoint);

        //            OnConnectCompletedSession(socket);
        //            return ConnectError.Success;
        //        }
        //        catch (Exception ex)
        //        {
        //            if (ex is SocketException socketEx)
        //            {
        //                if (CoreLogger.Logger != null)
        //                    CoreLogger.Logger.Error(socketEx, "[Connecter] An error occurred during connecting: {Message}", socketEx.Message);
        //                return ConnectError.SocketError;
        //            }
        //            else if (ex is InvalidOperationException opEx)
        //            {
        //                if (CoreLogger.Logger != null)
        //                    CoreLogger.Logger.Error(opEx, "[Connecter] An error occurred during connecting: {Message}", opEx.Message);
        //                return ConnectError.InvalidOperation;
        //            }
        //            else if (ex is ArgumentOutOfRangeException argEx)
        //            {
        //                if (CoreLogger.Logger != null)
        //                    CoreLogger.Logger.Error(argEx, "[Connecter] An error occurred during connecting: {Message}", argEx.Message);
        //                return ConnectError.OutOfRangePort;
        //            }
        //            else
        //            {
        //                if (CoreLogger.Logger != null)
        //                    CoreLogger.Logger.Error(ex, "[Connecter] An error occurred during connecting: {Message}", ex.Message);
        //                return ConnectError.EtcError;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        if (CoreLogger.Logger != null)
        //            CoreLogger.Logger.Error("[Connector] Connect Timeout: during connecting to {endPoint}", endPoint); 
        //        return ConnectError.Timeout;
        //    }
        //}

        //void OnConnectCompletedSession(Socket connectedSocket)
        //{
        //    if (CoreLogger.Logger != null)
        //        CoreLogger.Logger.Information("Connected: {RemoteEndPoint}", connectedSocket?.RemoteEndPoint);

        //    // TODO with Session
        //    Session session = _sessionFactory.Invoke();
        //    session.Init(connectedSocket);
        //    session.OnConnected(connectedSocket.RemoteEndPoint);
        //}


        //public void ConnectSync(IPEndPoint endPoint, Func<Session> sessionFactory, Action<Socket> callback = null)
        //{
        //    Socket socket = new Socket(
        //            endPoint.AddressFamily,
        //            SocketType.Stream,
        //            ProtocolType.Tcp);
        //    _sessionFactory = sessionFactory;

        //    socket.Connect(endPoint);
        //    callback?.Invoke(socket);
        //}
    }
}
