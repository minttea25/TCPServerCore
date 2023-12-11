using System;
using System.Collections.Generic;

namespace ServerCoreTCP.Job
{
    public interface IJobSerializer
    {
        public void Flush();
        public void Add(IJob job);
    }

    /// <summary>
    /// This is JobSerilizer class for executing jobs in one thread. 
    /// It need to be called `Flush()` manually.
    /// When `Flush()` called, the thread will invoke all actions in the queue.
    /// </summary>
    public abstract class JobSerializer : IJobSerializer
    {
        readonly protected Queue<IJob> _jobQueue = new Queue<IJob>();
        readonly protected object _queueLock = new object();

        public void Add(Action action) { Add(new Job(action)); }
        public void Add<T1>(Action<T1> action, T1 t1) { Add(new Job<T1>(action, t1)); }
        public void Add<T1, T2>(Action<T1, T2> action, T1 t1, T2 t2) { Add(new Job<T1, T2>(action, t1, t2)); }
        public void Add<T1, T2, T3>(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3) { Add(new Job<T1, T2, T3>(action, t1, t2, t3)); }
        public void Add<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4) { Add(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4)); }
        public void Add<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { Add(new Job<T1, T2, T3, T4, T5>(action, t1, t2, t3, t4, t5)); }

        public virtual void Flush()
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

        bool TryPop(out IJob job)
        {
            lock (_queueLock)
            {
                if (_jobQueue.Count == 0)
                {
                    job = null;
                    return false;
                }
                job = _jobQueue.Dequeue();
                return true;
            }
        }
    }

    public class JobSerializerWithTimer : JobSerializer, IUseJobTimer
    {
        readonly JobTimer _jobTimer = new JobTimer();

        public RevocableJob AddAfter(Action action, int millisecondsAfter) { return AddAfter(new Job(action), millisecondsAfter); }
        public RevocableJob AddAfter<T1>(Action<T1> action, int millisecondsAfter, T1 t1) { return AddAfter(new Job<T1>(action, t1), millisecondsAfter); }
        public RevocableJob AddAfter<T1, T2>(Action<T1, T2> action, int millisecondsAfter, T1 t1, T2 t2) { return AddAfter(new Job<T1, T2>(action, t1, t2), millisecondsAfter); }
        public RevocableJob AddAfter<T1, T2, T3>(Action<T1, T2, T3> action, int millisecondsAfter, T1 t1, T2 t2, T3 t3) { return AddAfter(new Job<T1, T2, T3>(action, t1, t2, t3), millisecondsAfter); }
        public RevocableJob AddAfter<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, int millisecondsAfter, T1 t1, T2 t2, T3 t3, T4 t4) { return AddAfter(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4), millisecondsAfter); }
        public RevocableJob AddAfter<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, int millisecondsAfter, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { return AddAfter(new Job<T1, T2, T3, T4, T5>(action, t1, t2, t3, t4, t5), millisecondsAfter); }


        public RevocableJob AddAfter(IJob job, int millisecondsAfter)
        {
            return _jobTimer.AddAfter(job, millisecondsAfter);
        }

        public override void Flush()
        {
            _jobTimer.Flush();

            base.Flush();
        }

    }
}
