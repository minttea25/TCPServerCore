using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
        const int MaxSessionCount = 200;

        public int PooledSessionCount
        {
            get
            {
                if (m_sessionPool == null) return -1;
                else return m_sessionPool.PooledSessionCount;
            }
        }

        readonly Listener m_listener;
        internal SessionPool m_sessionPool;
        // Semaphore m_maxConnections;

        public ServerService(IPEndPoint endPoint, Func<Session> emptySessionFactory) : base(ServiceTypes.Server)
        {
            //m_maxConnections = new Semaphore(0, MaxSessionCount);
            m_listener = new Listener(this, endPoint, emptySessionFactory, endPoint.AddressFamily);
            m_sessionPool = new SessionPool(this, MaxSessionCount, emptySessionFactory);
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
        const int SessionCount = 1;

        readonly Connector m_connector;
        readonly Session[] m_session;

        public ClientService(IPEndPoint endPoint, Func<Session> emptySessionFactory, int count = SessionCount) : base(ServiceTypes.Client)
        {
            m_session = new Session[count];
            for (int i = 0; i < count; ++i)
            {
                m_session[i] = emptySessionFactory.Invoke();
                m_session[i].SetService(this);
            }
            m_connector = new Connector(this, m_session, endPoint, count);
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
