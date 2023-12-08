using System;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;

namespace ServerCoreTCP
{
    internal class SocketAsyncEventArgsPool
    {
        public int TotalPoolCount => m_totalCount;
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
            if (args == null) throw new NullReferenceException();

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

        SocketAsyncEventArgs CreateNew()
        {
            _ = Interlocked.Increment(ref m_totalCount);

            var args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(_completedCallback);

            return args;
        }
    }
}
