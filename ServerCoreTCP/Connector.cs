using ServerCoreTCP.LoggerDebug;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    public class Connector
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

        Func<Session> _sessionFactory;

#if RELEASE
        public EventHandler<ConnectError> OnConnectCompleted;
#else
#endif

#if RELEASE
        /// <summary>
        /// Create a socket and connect to end point
        /// </summary>
        /// <param name="endPoint">The endpoint to connect to</param>
        /// <param name="sessionFactory">The session of the socket</param>
        /// <param name="timeoutMilliSeconds">Timeout</param>
        public async void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int timeoutMilliSeconds = TimeoutMilliSeconds)
        {
            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("Connector is trying to connect the server: {endPoint}", endPoint);

            Socket socket = new Socket(
                    endPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

            _sessionFactory = sessionFactory;

            ConnectError error = await ConnectTimeout(socket, endPoint, timeoutMilliSeconds);
            OnConnectCompleted?.Invoke(this, error);
        }

        async Task<ConnectError> ConnectTimeout(Socket socket, EndPoint endPoint, int timeoutMilliseconds)
        {
            Task t1 = socket.ConnectAsync(endPoint);
            Task t2 = Task.Delay(timeoutMilliseconds);
            Task completedTask = await Task.WhenAny(t1, t2);

            if (completedTask == t1)
            {
                try
                {
                    await t1; // Await t1 to propagate exceptions

                    if (CoreLogger.Logger != null)
                        CoreLogger.Logger.Information("[Connecter] Connected: {RemoteEndPoint}", socket.RemoteEndPoint);

                    OnConnectCompletedSession(socket);
                    return ConnectError.Success;
                }
                catch (Exception ex)
                {
                    if (ex is SocketException socketEx)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error(socketEx, "[Connecter] An error occurred during connecting: {Message}", socketEx.Message);
                        return ConnectError.SocketError;
                    }
                    else if (ex is InvalidOperationException opEx)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error(opEx, "[Connecter] An error occurred during connecting: {Message}", opEx.Message);
                        return ConnectError.InvalidOperation;
                    }
                    else if (ex is ArgumentOutOfRangeException argEx)
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error(argEx, "[Connecter] An error occurred during connecting: {Message}", argEx.Message);
                        return ConnectError.OutOfRangePort;
                    }
                    else
                    {
                        if (CoreLogger.Logger != null)
                            CoreLogger.Logger.Error(ex, "[Connecter] An error occurred during connecting: {Message}", ex.Message);
                        return ConnectError.EtcError;
                    }
                }
            }
            else
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("[Connector] Connect Timeout: during connecting to {endPoint}", endPoint); 
                return ConnectError.Timeout;
            }
        }

        void OnConnectCompletedSession(Socket connectedSocket)
        {
            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("Connected: {RemoteEndPoint}", connectedSocket?.RemoteEndPoint);

            // TODO with Session
            Session session = _sessionFactory.Invoke();
            session.Init(connectedSocket);
            session.OnConnected(connectedSocket.RemoteEndPoint);
        }
#else
        /// <summary>
        /// Create a socket and connect to end point
        /// </summary>
        /// <param name="endPoint">The endpoint to connect to</param>
        /// <param name="session">The session of the socket</param>
        /// <param name="count">[Debug] The count of socket to connect</param>
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("Connector is trying to connect the server: {endPoint}, count={count}", endPoint, count);
            else
                Console.WriteLine("Connector is trying to connect the server: {0}, count={1}", endPoint, count);

            for (int i = 0; i < count; i++)
            {
                Socket socket = new Socket(
                    endPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);

                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.Completed += OnConnectCompleted;
                e.RemoteEndPoint = endPoint;
                // Set socket as UserToken object
                e.UserToken = socket;

                RegisterConnect(e);
            }
        }

        /// <summary>
        /// Register function waiting for completed connection
        /// </summary>
        /// <param name="e">An object that contains the socket-async-event data</param>
        void RegisterConnect(SocketAsyncEventArgs e)
        {
            // Check the user token of event is socket & Cast UserToken to Socket
            if (!(e.UserToken is Socket socket)) return;

            bool pending = socket.ConnectAsync(e);

            // If pending is false => call callback function right away.
            if (pending == false) OnConnectCompleted(null, e);

            // If pending is true
            // => when the socket is connected, the registered delegate (OnConnectedCompleted) would be invoked.
        }

        /// <summary>
        /// Callback that is called when Connected
        /// </summary>
        /// <param name="sender">[Ignored] The source of the event</param>
        /// <param name="e">An object that contains the socket-async-event data</param>
        void OnConnectCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Information("Connected: {RemoteEndPoint}", e.ConnectSocket?.RemoteEndPoint);

                // TODO with Session
                Session session = _sessionFactory.Invoke();
                session.Init(e.ConnectSocket);
                session.OnConnected(e.RemoteEndPoint);
            }
            else
            {
                // error
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("Connector: {SocketError}", e.SocketError);
                else
                    Console.WriteLine("Connector: {0}", e.SocketError);
            }
        }
#endif

        public void ConnectSync(IPEndPoint endPoint, Func<Session> sessionFactory, Action<Socket> callback = null)
        {
            Socket socket = new Socket(
                    endPoint.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp);
            _sessionFactory = sessionFactory;

            socket.Connect(endPoint);
            callback?.Invoke(socket);
        }
    }
}
