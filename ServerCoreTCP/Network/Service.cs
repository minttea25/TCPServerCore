using System;
using System.Net;
using System.Net.Sockets;

namespace ServerCoreTCP
{
    public abstract class Service
    {
        public enum ServiceTypes
        {
            Server = 1,
            Client = 2
        }

        public ServiceTypes ServiceType => m_serviceType;
        readonly internal ServiceTypes m_serviceType;

        public int SAEATotalPoolCount => m_saeaPool.TotalPoolCount;
        public int SAEACurrentPooledCount => m_saeaPool.CurrentPooledCount;

        internal readonly SocketAsyncEventArgsPool m_saeaPool;

        public abstract void Start();
        public abstract void Stop();

        public Service(ServiceTypes serviceType)
        {
            m_serviceType = serviceType;
            m_saeaPool = new SocketAsyncEventArgsPool(1000, Dispatch);
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

    public class ServerService : Service
    {
        readonly int m_sessionPoolCount;

        public int SessionTotalPoolCount => m_sessionPool.TotalPoolCount;
        public int SessionCurrentPooledCount => m_sessionPool.CurrentPooledCount;
        public int Port => m_listener.Port;
        public int ListenerBackLog => m_listener.Backlog;
        public int ListenrRegisterCount => m_listener.RegisterCount;

        readonly Listener m_listener;
        internal SessionPool m_sessionPool;
        // Semaphore m_maxConnections;

        public ServerService(IPEndPoint endPoint, Func<Session> emptySessionFactory, ServerServiceConfig config) : base(ServiceTypes.Server)
        {
            m_sessionPoolCount = config.SessionPoolCount;

            //m_maxConnections = new Semaphore(0, MaxSessionCount);
            m_listener = new Listener(this, endPoint, emptySessionFactory, endPoint.AddressFamily, config);
            m_sessionPool = new SessionPool(this, m_sessionPoolCount, emptySessionFactory);
        }

        public override void Start()
        {
            m_listener.StartListen();
        }

        public override void Stop()
        {
            m_sessionPool.Clear();
            Clear();
        }

        //public void BroadCast(byte[] buffers)
        //{
        //    // TODO
        //}
    }

    public class ClientService : Service
    {
        readonly int m_sessionCount = 1;

        readonly Connector m_connector;
        readonly Session[] m_session;

        public int ConnectionCount => m_connector.ConnectionCount;

        public ClientService(IPEndPoint endPoint, Func<Session> emptySessionFactory, ClientServiceConfig config) : base(ServiceTypes.Client)
        {
            m_sessionCount = config.ClientServiceSessionCount;

            m_session = new Session[m_sessionCount];
            for (int i = 0; i < m_sessionCount; ++i)
            {
                m_session[i] = emptySessionFactory.Invoke();
                m_session[i].SetService(this);
            }
            m_connector = new Connector(this, m_session, endPoint, m_sessionCount, config);
        }

        public override void Start()
        {
            m_connector.Connect();
        }

        public override void Stop()
        {
            Clear();
        }
    }
}
