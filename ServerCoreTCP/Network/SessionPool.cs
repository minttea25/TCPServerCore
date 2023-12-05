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
        public int TotalPoolCount => _id - 1;
        public int CurrentPooledCount => _pool.Count;

        readonly ConcurrentStack<Session> _pool;
        readonly Func<Session> _emptySessionFactory;
        readonly Service _service;

        int _id = 1; // starts at 1

        internal SessionPool(Service service, int capacity, Func<Session> emptySessionFactory)
        {
            _service = service;
            _pool = new ConcurrentStack<Session>();

            _emptySessionFactory = emptySessionFactory;

            for (_id = 1; _id <= capacity; ++_id)
            {
                Session session = _emptySessionFactory.Invoke();
                session.SetService(_service);
                session.SetSessionId((uint)_id);
                _pool.Push(session);
            }
        }

        internal Session Pop()
        {
            if (_pool.TryPop(out var session) == true) return session;
            else return CreateNew();
        }

        internal void Push(Session session)
        {
            if (session == null) throw new NullReferenceException();

            // reset the session
            session.Clear();

            _pool.Push(session);
        }

        internal void Clear()
        {
            _pool.Clear();
        }

        Session CreateNew()
        {
            int id = Interlocked.Increment(ref _id);

            Session session = _emptySessionFactory.Invoke();
            session.SetService(_service);
            session.SetSessionId((uint)_id);

            return session;
        }
    }
}
