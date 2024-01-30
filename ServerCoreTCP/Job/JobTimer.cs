using ServerCoreTCP.Core;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.Job
{
    public interface IUseJobTimer
    {
        public CancelableJob AddAfter(IJob job, int millisecondsAfter);
    }

    public class JobTimer
    {
        readonly PriorityQueue<CancelableJob> _pq = new PriorityQueue<CancelableJob>();
        readonly object _queueLock = new object();

        public CancelableJob AddAfter(IJob job, long millisecondsAfter = 0) 
        { 
            long millisecondsExec = Global.G_Stopwatch.ElapsedMilliseconds + millisecondsAfter;
            CancelableJob revocableJob = new CancelableJob(job, millisecondsExec);

            lock (_queueLock)
            {
                _pq.Enqueue(revocableJob);
            }

            return revocableJob;
        }

        public void Flush()
        {
            while (true)
            {
                long now = Global.G_Stopwatch.ElapsedMilliseconds;

                CancelableJob revocableJob;
                lock (_queueLock)
                {
                    if (_pq.TryPeek(out revocableJob) == false) break;
                    if (revocableJob.MillisecondsExec >= now) break;

                    _ = _pq.Dequeue();
                }

                revocableJob.Execute();
            }
        }
    }
}
