using System;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace ServerCoreTCP
{
    internal class SocketAsyncEventArgsPool
    {
        readonly ConcurrentStack<SocketAsyncEventArgs> m_pool;

        internal SocketAsyncEventArgsPool(int capacity, Action<object, SocketAsyncEventArgs> completedCallback)
        {
            m_pool = new ConcurrentStack<SocketAsyncEventArgs>();

            for (int i = 0; i < capacity; ++i)
            {
                var e = new SocketAsyncEventArgs();
                e.Completed += new EventHandler<SocketAsyncEventArgs>(completedCallback);
                m_pool.Push(e);
            }
        }

        internal SocketAsyncEventArgs Pop()
        {
            if (m_pool.TryPop(out var args) == true) return args;
            //else return CreateNewArgs();
            else return null;
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
    }
}
