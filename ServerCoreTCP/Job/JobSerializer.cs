using System;
using System.Collections.Generic;

namespace ServerCoreTCP.Job
{
    /// <summary>
    /// This is JobSerilizer class for executing jobs in one thread. 
    /// It need to be called `Flush()` manually.
    /// When `Flush()` called, the thread will invoke all actions in the queue.
    /// </summary>
    public class JobSerializer
    {
        readonly Queue<IJob> _jobQueue = new Queue<IJob>();
        readonly object _queueLock = new object();

        public void Add(Action action) { Add(new Job(action)); }
        public void Add<T1>(Action<T1> action, T1 t1) { Add(new Job<T1>(action, t1)); }
        public void Add<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Add(new Job<T1, T2>(action, t1, t2)); }
        public void Add<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Add(new Job<T1, T2, T3>(action, t1, t2, t3)); }
        public void Add<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { Add(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }
        public void Add<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { Add(new Job<T1, T2, T3, T4, T5>(action, t1, t2, t3, t4, t5)); }


        public void Flush()
        {
            while (true)
            {
                if (TryPop(out IJob job) == true)
                {
                    job.Execute();
                }
                else return;
            }
        }

        public void Add(IJob job)
        {
            lock (_queueLock)
            {
                _jobQueue.Enqueue(job);
            }
        }

        bool TryPop(out IJob action)
        {
            lock (_queueLock)
            {
                if (_jobQueue.Count == 0)
                {
                    action = null;
                    return false;
                }
                action = _jobQueue.Dequeue();
                return true;
            }
        }
    }

    /// <summary>
    /// This is ActionSerializerAuto class for executing actions in one thread.
    /// If you want to user parameters, use IJobSerializer.
    /// When `Add()` called, the thread will begin to invoke actions in the queue if no thread is flushing.
    /// At this time, other `Add()` can be called and it just adds a action to a queue.
    /// </summary>
    public class JobSerializerAuto
    {
        readonly Queue<IJob> _queue = new Queue<IJob>();
        readonly object _queueLock = new object();

        bool _flush = false;

        public void Add(Action action) { Add(new Job(action)); }
        public void Add<T1>(Action<T1> action, T1 t1) { Add(new Job<T1>(action, t1)); }
        public void Add<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Add(new Job<T1, T2>(action, t1, t2)); }
        public void Add<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Add(new Job<T1, T2, T3>(action, t1, t2, t3)); }
        public void Add<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { Add(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }
        public void Add<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { Add(new Job<T1, T2, T3, T4, T5>(action, t1, t2, t3, t4, t5)); }


        public void Add(IJob job)
        {
            bool flush = false;
            lock (_queueLock)
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
                if (TryPop(out IJob e) == true)
                {
                    e.Execute();
                }
                else return;
            }
        }

        bool TryPop(out IJob job)
        {
            lock (_queueLock)
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
}
