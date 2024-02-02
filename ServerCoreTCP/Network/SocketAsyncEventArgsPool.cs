using System;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace ServerCoreTCP
{
    /// <summary>
    /// The pool of the SocketAsyncEventArgs of the socket events.
    /// </summary>
    internal class SocketAsyncEventArgsPool
    {
        /// <summary>
        /// The total count of the SocketAsyncEventArgs of the pool.
        /// </summary>
        public int TotalPoolCount => m_totalCount;
        /// <summary>
        /// The count of the currently pooled SocketAsyncEventArgs.
        /// </summary>
        public int CurrentPooledCount => _pool.Count;


        readonly ConcurrentStack<SocketAsyncEventArgs> _pool;
        readonly Action<object, SocketAsyncEventArgs> _completedCallback;

        int m_totalCount = 0;

        internal SocketAsyncEventArgsPool(int capacity, Action<object, SocketAsyncEventArgs> completedCallback)
        {
            _completedCallback = completedCallback;
            _pool = new ConcurrentStack<SocketAsyncEventArgs>();

            for (int i = 0; i < capacity; ++i)
            {
                var args = CreateNew();
                _pool.Push(args);

            }
        }

        internal SocketAsyncEventArgs Pop()
        {
            if (_pool.TryPop(out var args) == true) return args;
            else return CreateNew();
        }

        internal void Push(SocketAsyncEventArgs args)
        {
#if RELEASE
            if (args == null) return;
#else
            if (args == null) throw new NullReferenceException();
#endif

            // reset for reusing
            args.AcceptSocket = null;
            args.UserToken = null;
            args.BufferList = null;
            args.SetBuffer(null, 0, 0);

            _pool.Push(args);
        }

        internal void Clear()
        {
            _pool.Clear();
        }

        /// <summary>
        /// If there is no more pooled SocketAsyncEventArgs, makes new one.
        /// </summary>
        /// <returns></returns>
        SocketAsyncEventArgs CreateNew()
        {
            _ = Interlocked.Increment(ref m_totalCount);

            var args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(_completedCallback);

            return args;
        }
    }
}
