using System.Diagnostics;
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
        readonly Stopwatch _stopwatch = new Stopwatch();
        readonly object _queueLock = new object();



        public RevocableJob AddAfter(IJob job, long millisecondsAfter = 0)
        {
            if (_stopwatch.IsRunning == false) _stopwatch.Start();

            long millisecondsExec = _stopwatch.ElapsedMilliseconds + millisecondsAfter;
            RevocableJob revocableJob = new RevocableJob(job, millisecondsExec);

            lock (_queueLock)
            {
                _pq.Enqueue(revocableJob);
            }

            return revocableJob;
        }

        public void Flush()
        {
            if (_stopwatch.IsRunning == false) return;

            while (true)
            {
                long now = _stopwatch.ElapsedMilliseconds;

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
