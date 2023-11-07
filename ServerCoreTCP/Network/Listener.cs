using ServerCoreTCP.LoggerDebug;
using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCoreTCP
{
    internal class Listener : SocketObject
    {
        public const int BackLog = 100;
        public int MaxAcceptClients = 100;

        internal ServerService m_serverService;

        readonly Socket m_listenSocket;
        readonly Func<Session> m_sessionFactory;

        /// <summary>
        /// Create a socket and bind the endpoint.
        /// </summary>
        /// <param name="service">The ServerService Instance</param>
        /// <param name="endPoint">The endpoint to bind to socket</param>
        /// <param name="sessionFactory"></param>
        /// <param name="addressFamily"></param>
        internal Listener(ServerService service, IPEndPoint endPoint, Func<Session> sessionFactory, AddressFamily addressFamily)
        {
            m_service = service;
            m_serverService = service;
            m_sessionFactory = sessionFactory;
            m_listenSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);

            m_listenSocket.Bind(endPoint);
        }

        /// <summary>
        /// Place the socket in a listening state.
        /// </summary>
        internal void StartListen()
        {
            m_listenSocket.Listen(BackLog);

            if (CoreLogger.Logger != null)
                CoreLogger.Logger.Information("Socket is listening now...");

            for (int i = 0; i < MaxAcceptClients; ++i)
            {
                AcceptEventToken token = new AcceptEventToken(this);
                SocketAsyncEventArgs args = m_service.m_saeaPool.Pop();
                args.UserToken = token;
                RegisterAccept(args);
            }
        }

        internal override void Dispatch(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is AcceptEventToken _)) throw new InvalidCastException();

            OnAcceptCompleted(eventArgs);
        }

        /// <summary>
        /// Register as waiting for Accept
        /// </summary>
        /// <param name="eventArgs">An object that contains the socket-async-event data</param>
        void RegisterAccept(SocketAsyncEventArgs eventArgs)
        {
            // Reset for re-using
            eventArgs.AcceptSocket = null;

            try
            {
                bool pending = m_listenSocket.AcceptAsync(eventArgs);
                // If pending is false => call callback function right away
                if (pending == false) OnAcceptCompleted(eventArgs);
            }
            catch(Exception ex)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error(ex, "RegisterAccept - Exception");
            }
            // If pending is true
            // => when the accept is completed, the registered delegate (OnAcceptCompelted) would be invoked.
        }

        /// <summary>
        /// Callback that is called when Accept.
        /// </summary>
        /// <param name="eventArgs">An object that contains the socket-async-event data</param>
        void OnAcceptCompleted(SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.SocketError == SocketError.Success)
            {
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Information("Accepted Socket: {RemoteEndPoint}", eventArgs.AcceptSocket.RemoteEndPoint);

                // initialize session
                Session session = m_serverService.m_sessionPool.Pop();
                session.Init(eventArgs.AcceptSocket);
                session.OnConnected(eventArgs.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                // error
                if (CoreLogger.Logger != null)
                    CoreLogger.Logger.Error("Listener: {SocketError}", eventArgs.SocketError);
            }

            // After Accept, wait again for other Accepts.
            RegisterAccept(eventArgs);
        }
    }
}
