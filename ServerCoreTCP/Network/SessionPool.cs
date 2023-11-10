using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    internal class SessionPool
    {
        public int TotalPoolCount => m_id - 1;
        public int CurrentPooledCount => m_pool.Count;

        readonly ConcurrentStack<Session> m_pool;
        readonly Func<Session> m_emptySessionFactory;
        readonly Service m_service;

        int m_id = 1; // starts at 1

        internal SessionPool(Service service, int capacity, Func<Session> emptySessionFactory)
        {
            m_service = service;
            m_pool = new ConcurrentStack<Session>();

            m_emptySessionFactory = emptySessionFactory;

            for (m_id = 1; m_id <= capacity; ++m_id)
            {
                Session session = m_emptySessionFactory.Invoke();
                session.SetService(m_service);
                session.SetSessionId((uint)m_id);
                m_pool.Push(session);
            }
        }

        internal Session Pop()
        {
            if (m_pool.TryPop(out var session) == true) return session;
            else return CreateNew();
        }

        internal void Push(Session session)
        {
            if (session == null) throw new NullReferenceException();

            // reset the session
            session.Clear();

            m_pool.Push(session);
        }

        internal void Clear()
        {
            m_pool.Clear();
        }

        Session CreateNew()
        {
            int id = Interlocked.Increment(ref m_id);

            Session session = m_emptySessionFactory.Invoke();
            session.SetService(m_service);
            session.SetSessionId((uint)m_id);

            return session;
        }
    }
}
