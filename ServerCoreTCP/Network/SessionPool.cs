using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerCoreTCP
{
    internal class SessionPool
    {
        readonly ConcurrentStack<Session> m_pool;
        readonly Func<Session> m_emptySessionFactory;

        internal SessionPool(Service service, int capacity, Func<Session> emptySessionFactory)
        {
            m_pool = new ConcurrentStack<Session>();

            m_emptySessionFactory = emptySessionFactory;

            for (uint i = 0; i < capacity; ++i)
            {
                Session session = m_emptySessionFactory.Invoke();
                session.SetService(service);
                session.SetSessionId(i + 1);
                m_pool.Push(session);
            }
        }

        internal Session Pop()
        {
            if (m_pool.TryPop(out var session) == true) return session;
            else return null;
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
    }
}
