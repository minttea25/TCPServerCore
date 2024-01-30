using ServerCoreTCP.Core;
using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCoreTCP
{
    public abstract class Service
    {
        /// <summary>
        /// The types of service.
        /// </summary>
        public enum ServiceTypes
        {
            Server = 1,
            Client = 2
        }

        /// <summary>
        /// The ServiceType
        /// </summary>
        public ServiceTypes ServiceType => m_serviceType;
        readonly ServiceTypes m_serviceType;

        /// <summary>
        /// The total count of SocketAsyncEventArgs in the pool.
        /// </summary>
        public int SAEATotalPoolCount => m_saeaPool.TotalPoolCount;
        /// <summary>
        /// The count of the currently pooled SocketAsyncEventArgs in the pool.
        /// </summary>
        public int SAEACurrentPooledCount => m_saeaPool.CurrentPooledCount;

        internal readonly SocketAsyncEventArgsPool m_saeaPool;

        public virtual void Start()
        {
            Global.Init();
        }

        public virtual void Stop()
        {
            Global.Clear();
        }

        /// <summary>
        /// The constructor of Service.
        /// </summary>
        /// <param name="serviceType">The service type</param>
        /// <param name="saeaPoolCount">The count of SAEA of the pool</param>
        public Service(ServiceTypes serviceType, int saeaPoolCount)
        {
            m_serviceType = serviceType;
            m_saeaPool = new SocketAsyncEventArgsPool(saeaPoolCount, Dispatch);
        }

        void Dispatch(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (!(eventArgs.UserToken is SocketEventToken token)) throw new InvalidCastException();
            if (token.m_socketObject == null) throw new NullReferenceException();

            token.m_socketObject.Dispatch(sender, eventArgs);
        }

        protected void Clear()
        {
            m_saeaPool.Clear();
        }
    }

    /// <summary>
    /// The service that can listen the connections of others.
    /// </summary>
    public class ServerService : Service
    {
        readonly int m_sessionPoolCount;

        /// <summary>
        /// The total count of the sessions in the pool.
        /// </summary>
        public int SessionTotalPoolCount => m_sessionPool.TotalPoolCount;
        /// <summary>
        /// The count of the currently pooled sessions in the pool.
        /// </summary>
        public int SessionCurrentPooledCount => m_sessionPool.CurrentPooledCount;
        /// <summary>
        /// The listening port number.
        /// </summary>
        public int Port => m_listener.Port;
        /// <summary>
        /// The count of the listening backlogs.
        /// </summary>
        public int ListenerBackLog => m_listener.Backlog;
        /// <summary>
        /// The count of the registered counts of the listener.
        /// </summary>
        public int ListenrRegisterCount => m_listener.RegisterCount;

        readonly Listener m_listener;
        internal SessionPool m_sessionPool;
        // Semaphore m_maxConnections;

        /// <summary>
        /// The constructor of serverservice. (Note: The sessions are created in the pool before formed connection.)
        /// </summary>
        /// <param name="endPoint">The endpoint to be opened.</param>
        /// <param name="emptySessionFactory">The factory of the empty session.</param>
        /// <param name="config">The configs of the server service.</param>
        public ServerService(IPEndPoint endPoint, Func<Session> emptySessionFactory, ServerServiceConfig config) : base(ServiceTypes.Server, config.SocketAsyncEventArgsPoolCount)
        {
            m_sessionPoolCount = config.SessionPoolCount;

            //m_maxConnections = new Semaphore(0, MaxSessionCount);
            m_listener = new Listener(this, endPoint, endPoint.AddressFamily, config);
            m_sessionPool = new SessionPool(this, m_sessionPoolCount, emptySessionFactory);
        }

        /// <summary>
        /// Starts the service. (Starts to listen)
        /// </summary>
        public sealed override void Start()
        {
            base.Start();
            m_listener.StartListen();
        }

        /// <summary>
        /// Stop the service.
        /// </summary>
        public sealed override void Stop()
        {
            base.Stop();
            m_sessionPool.Clear();
            Clear();
        }

        //public void BroadCast(byte[] buffers)
        //{
        //    // TODO
        //}
    }

    /// <summary>
    /// The service for connecting to other.
    /// </summary>
    public class ClientService : Service
    {
        readonly int m_sessionCount = 1;

        readonly Connector m_connector;
        readonly Session[] m_session;

        /// <summary>
        /// The count of connections configured in advance.
        /// </summary>
        public int ConnectionCount => m_connector.ConnectionCount;

        /// <summary>
        /// The constructor of clientservice. (Note: The sessions are created before formed connection.)
        /// </summary>
        /// <param name="endPoint">The endpoint to connect to.</param>
        /// <param name="emptySessionFactory">The factory of the empty session.</param>
        /// <param name="config">The configs of the client service.</param>
        /// <param name="connectFailedCallback">The callback which is invoked when the connection is failed.</param>
        public ClientService(IPEndPoint endPoint, Func<Session> emptySessionFactory, ClientServiceConfig config, Action<SocketError> connectFailedCallback = null) : base(ServiceTypes.Client, config.SocketAsyncEventArgsPoolCount)
        {
            m_sessionCount = config.ClientServiceSessionCount;

            m_session = new Session[m_sessionCount];
            for (int i = 0; i < m_sessionCount; ++i)
            {
                m_session[i] = emptySessionFactory.Invoke();
                m_session[i].m_service = this;
            }
            m_connector = new Connector(this, m_session, endPoint, config, connectFailedCallback);
        }

        /// <summary>
        /// Start the servie. (Starts to connect)
        /// </summary>
        public sealed override void Start()
        {
            base.Start();
            m_connector.Connect();
        }

        /// <summary>
        /// Stop the service.
        /// </summary>
        public sealed override void Stop()
        {
            base.Stop();
            Clear();
        }
    }
}
