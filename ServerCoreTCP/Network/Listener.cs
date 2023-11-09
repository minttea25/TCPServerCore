using ServerCoreTCP.CLogger;
using ServerCoreTCP.Utils;
using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCoreTCP
{
    internal class Listener : SocketObject
    {
        public int Port => m_port;

        internal ServerService m_serverService;

        readonly Socket m_listenSocket;
        readonly Func<Session> m_sessionFactory;
        readonly int m_port;

        readonly int m_backlog;
        readonly int m_registerCount;

        /// <summary>
        /// Create a socket and bind the endpoint.
        /// </summary>
        /// <param name="service">The ServerService Instance</param>
        /// <param name="endPoint">The endpoint to bind to socket</param>
        /// <param name="sessionFactory"></param>
        /// <param name="addressFamily"></param>
        internal Listener(ServerService service, IPEndPoint endPoint, Func<Session> sessionFactory, AddressFamily addressFamily, ServerServiceConfig config)
        {
            m_backlog = config.ListenerBacklogCount;
            m_registerCount = config.RegisterListenCount;

            m_service = service;
            m_serverService = service;
            m_sessionFactory = sessionFactory;
            m_port = endPoint.Port;

            m_listenSocket = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            m_listenSocket.SetLinger(config.Linger);
            m_listenSocket.SetKeepAlive(config.KeepAlive);
            m_listenSocket.SetReuseAddress(config.ReuseAddress);
            m_listenSocket.SetNoDelay(config.NoDelay);

            m_listenSocket.Bind(endPoint);
        }

        /// <summary>
        /// Place the socket in a listening state.
        /// </summary>
        internal void StartListen()
        {
            m_listenSocket.Listen(m_backlog);

            CoreLogger.LogInfo("Listener.StartListen", "Socket is listening on port={0}... [MaxAcceptClients={1}]", m_port, m_registerCount);

            for (int i = 0; i < m_registerCount; ++i)
            {
                AcceptEventToken token = new AcceptEventToken(this);
                SocketAsyncEventArgs args = m_service.m_saeaPool.Pop();
                args.UserToken = token;
                RegisterAccept(args);
            }
        }

        internal sealed override void Dispatch(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is AcceptEventToken _)) throw new InvalidCastException("[Listener.Dispatch] The UserToken was not AcceptEventToken");

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
                CoreLogger.LogError("Listener.RegisterAccept", ex, "Exception");
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
                CoreLogger.LogInfo("Listener.OnAcceptCompleted", "Accepted Socket. EndPoint: {0}", eventArgs.AcceptSocket.RemoteEndPoint);

                // initialize session
                Session session = m_serverService.m_sessionPool.Pop();
                session.Init(eventArgs.AcceptSocket);
                session.OnConnected(eventArgs.AcceptSocket.RemoteEndPoint);
            }
            else
            {
                // error
                CoreLogger.LogError("Listener.OnAcceptCompleted", "SocketError was {0}.", eventArgs.SocketError);
            }

            // After Accept, wait again for other Accepts.
            RegisterAccept(eventArgs);
        }
    }
}
