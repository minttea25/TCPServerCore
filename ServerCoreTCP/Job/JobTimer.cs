using System.Diagnostics;
using ServerCoreTCP.Core;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.Job
{
    public interface IUseJobTimer
    {
        public RevocableJob AddAfter(IJob job, int millisecondsAfter);
    }

    public class JobTimer
    {
        readonly PriorityQueue<RevocableJob> _pq = new PriorityQueue<RevocableJob>();
        readonly object _queueLock = new object();

        public RevocableJob AddAfter(IJob job, long millisecondsAfter = 0) 
        { 
            long millisecondsExec = Global.G_Stopwatch.ElapsedMilliseconds + millisecondsAfter;
            RevocableJob revocableJob = new RevocableJob(job, millisecondsExec);

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

                RevocableJob revocableJob;
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
