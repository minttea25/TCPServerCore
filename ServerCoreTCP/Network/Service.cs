using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ServerCoreTCP
{
    public abstract class Service
    {
        internal readonly SocketAsyncEventArgsPool m_saeaPool;

        public abstract void Start();
        public abstract void Stop();

        public Service()
        {
            m_saeaPool = new SocketAsyncEventArgsPool(500, Dispatch);
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
        const int MaxSessionCount = 100;

        readonly Listener m_listener;
        internal SessionPool m_sessionPool;
        Semaphore m_maxConnections;

        public ServerService(IPEndPoint endPoint, Func<Session> emptySessionFactory) : base()
        {
            m_maxConnections = new Semaphore(0, MaxSessionCount);
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

        // temp parameter
        public void BroadCast(byte[] buffers)
        {
            // TODO
        }
    }

    public class ClientService : Service
    {
        const int SessionCount = 1;

        readonly Connector m_connector;
        Session[] m_session;

        public ClientService(IPEndPoint endPoint, Func<Session> emptySessionFactory, int count = SessionCount) : base()
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
