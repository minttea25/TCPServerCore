using System;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace ServerCoreTCP
{
    internal class SocketAsyncEventArgsPool
    {
        public int TotalPoolCount => m_totalCount;
        public int CurrentPooledCount => m_pool.Count;


        readonly ConcurrentStack<SocketAsyncEventArgs> m_pool;
        readonly Action<object, SocketAsyncEventArgs> m_completedCallback;

        int m_totalCount = 0;

        internal SocketAsyncEventArgsPool(int capacity, Action<object, SocketAsyncEventArgs> completedCallback)
        {
            m_completedCallback = completedCallback;
            m_pool = new ConcurrentStack<SocketAsyncEventArgs>();

            for (int i = 0; i < capacity; ++i)
            {
                var args = CreateNew();
                m_pool.Push(args);

                //var e = new SocketAsyncEventArgs();
                //e.Completed += new EventHandler<SocketAsyncEventArgs>(m_completedCallback);
                //m_pool.Push(e);
            }
        }

        internal SocketAsyncEventArgs Pop()
        {
            if (m_pool.TryPop(out var args) == true) return args;
            else return CreateNew();
        }

        internal void Push(SocketAsyncEventArgs args)
        {
            if (args == null) throw new NullReferenceException();

            // reset for reusing
            args.AcceptSocket = null;
            args.UserToken = null;
            args.BufferList = null;
            args.SetBuffer(null, 0, 0);

            m_pool.Push(args);
        }

        internal void Clear()
        {
            m_pool.Clear();
        }

        SocketAsyncEventArgs CreateNew()
        {
            _ = Interlocked.Increment(ref m_totalCount);

            var args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(m_completedCallback);

            return args;
        }
    }
}
