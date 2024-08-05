using System;
using System.Collections.Concurrent;
using System.Threading;

namespace NetCore
{
    /// <summary>
    /// The pool of the sessions.
    /// </summary>
    internal class SessionPool
    {
        /// <summary>
        /// The total count of the session of the pool.
        /// </summary>
        public int TotalPoolCount => _id - 1;
        /// <summary>
        /// The count of the curently pooled sessions.
        /// </summary>
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
                session.m_service = _service;
                session.SessionId = (uint)_id;
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
#if RELEASE
            if (session == null) return;
#else
            if (session == null) throw new NullReferenceException();
#endif

            // reset the session
            session.Clear();

            _pool.Push(session);
        }

        internal void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// If there is no more pooled session, makes new one.
        /// </summary>
        /// <returns></returns>
        Session CreateNew()
        {
            int id = Interlocked.Increment(ref _id);

            Session session = _emptySessionFactory.Invoke();
            session.m_service = _service;
            session.SessionId = (uint)_id;

            return session;
        }
    }
}
