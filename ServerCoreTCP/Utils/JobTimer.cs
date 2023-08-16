using System;

namespace ServerCoreTCP.Utils
{
    struct JobTimerElement : IComparable<JobTimerElement>
    {
        public int targetTick;
        public Action job;

        public int CompareTo(JobTimerElement other)
        {
            return targetTick - other.targetTick;
        }
    }

    public class JobTimer
    {
        readonly PriorityQueue<JobTimerElement> _pq = new PriorityQueue<JobTimerElement>();
        readonly object _lock = new object();

        #region Singleton
        static JobTimer _instance = null;
        public static JobTimer Instance
        {
            get
            {
                if (_instance == null) _instance = new JobTimer();
                return _instance;
            }
        }
        #endregion

        public void Push(Action action, int tickAfter = 0)
        {
            JobTimerElement job = new JobTimerElement()
            {
                targetTick = System.Environment.TickCount + tickAfter,
                job = action,
            };

            lock (_lock)
            {
                _pq.Enqueue(job);
            }
        }

        public void Flush()
        {
            while (true)
            {
                int now = System.Environment.TickCount;

                JobTimerElement e;
                lock (_lock)
                {
                    if (_pq.Empty == true) break;

                    e = _pq.Peek();
                    if (e.targetTick > now) break;

                    _ = _pq.Dequeue();
                }

                e.job?.Invoke();
            }
        }

    }
}
