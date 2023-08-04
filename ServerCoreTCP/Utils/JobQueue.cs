using System;
using System.Collections.Generic;

namespace ServerCoreTCP.Utils
{
    public class JobQueue
    {
        readonly Queue<Action> _queue = new();
        bool _flush = false;
        readonly object _lock = new();

        public void Add(Action job)
        {
            bool flush = false;
            lock (_lock)
            {
                _queue.Enqueue(job);
                if (_flush == false) flush = _flush = true;
            }

            if (flush == true) Flush();
        }

        void Flush()
        {
            while (true)
            {
                if (TryPop(out Action job) == true)
                {
                    job.Invoke();
                }
                else return;
            }
        }

        bool TryPop(out Action job)
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flush = false;
                    job = null;
                    return false;
                }
                job = _queue.Dequeue();
                return true;
            }
        }
    }

    public class JobQueue<T> where T : IJob
    {
        readonly Queue<T> _queue = new();
        bool _flush = false;
        readonly object _lock = new();

        public void Add(T job)
        {
            bool flush = false;
            lock (_lock)
            {
                _queue.Enqueue(job);
                if (_flush == false) flush = _flush = true;
            }

            if (flush == true) Flush();
        }

        void Flush()
        {
            while (true)
            {
                if (TryPop(out T e) == true)
                {
                    e.Execute();
                }
                else return;
            }
        }

        bool TryPop(out T e)
        {
            lock (_lock)
            {
                if (_queue.Count == 0)
                {
                    _flush = false;
                    e = default(T);
                    return false;
                }
                e = _queue.Dequeue();
                return true;
            }
        }
    }
}
