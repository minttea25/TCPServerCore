using System;
using System.Diagnostics;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.Job
{
    internal struct JobTimerElement : IComparable<JobTimerElement>
    {
        readonly internal long MillisecondsExec;
        readonly internal IJob Job;

        public JobTimerElement(IJob job, long millisecondsExec)
        {
            Job = job;
            MillisecondsExec = millisecondsExec;
        }

        // take faster tick
        public int CompareTo(JobTimerElement other)
        {
            return (int)(other.MillisecondsExec - MillisecondsExec);
        }
    }

    public class JobTimer
    {
        readonly PriorityQueue<JobTimerElement> _pq = new PriorityQueue<JobTimerElement>();
        readonly Stopwatch _stopwatch = new Stopwatch();
        readonly object _queueLock = new object();

        public void Add(Action action, int ticktAfter) { Add(new Job(action), ticktAfter); }
        public void Add<T1>(Action<T1> action, int ticktAfter, T1 t1) { Add(new Job<T1>(action, t1), ticktAfter); }
        public void Add<T1, T2>(Action<T1, T2> action, int ticktAfter, T1 t1, T2 t2) { Add(new Job<T1, T2>(action, t1, t2), ticktAfter); }
        public void Add<T1, T2, T3>(Action<T1, T2, T3> action, int ticktAfter, T1 t1, T2 t2, T3 t3) { Add(new Job<T1, T2, T3>(action, t1, t2, t3), ticktAfter); }
        public void Add<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action, int ticktAfter, T1 t1, T2 t2, T3 t3, T4 t4) { Add(new Job<T1, T2, T3, T4>(action, t1, t2, t3, t4), ticktAfter); }
        public void Add<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action, int ticktAfter, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5) { Add(new Job<T1, T2, T3, T4, T5>(action, t1, t2, t3, t4, t5), ticktAfter); }


        public void Add(IJob job, long millisecondsAfter = 0)
        {
            if (_stopwatch.IsRunning == false) _stopwatch.Start();

            long millisecondsExec = _stopwatch.ElapsedMilliseconds + millisecondsAfter;
            JobTimerElement jobElement = new JobTimerElement(job, millisecondsExec);

            lock (_queueLock)
            {
                _pq.Enqueue(jobElement);
            }
        }

        public void Flush()
        {
            if (_stopwatch.IsRunning == false) return;

            while (true)
            {
                long now = _stopwatch.ElapsedMilliseconds;

                JobTimerElement jobElement;
                lock (_queueLock)
                {
                    if (_pq.TryPeek(out jobElement) == false) break;
                    if (jobElement.MillisecondsExec >= now) break;

                    _ = _pq.Dequeue();
                }

                jobElement.Job.Execute();
            }
        }
    }
}
