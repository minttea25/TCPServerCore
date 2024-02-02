using ServerCoreTCP.Core;
using ServerCoreTCP.Utils;

namespace ServerCoreTCP.Job
{
    /// <summary>
    /// The interface of the JobTimer using Cancelable jobs.
    /// </summary>
    public interface IUseJobTimer
    {
        public CancelableJob AddAfter(IJob job, int millisecondsAfter);
    }

    /// <summary>
    /// It is stored in the priority queue at the scheduled time, and when that time comes, it is automatically flushed and the job is executed. 
    /// <br/>If the Canceled value of the job is false, the job will not be executed.
    /// <br/>It uses the StopWatch in Global.
    /// </summary>
    public class JobTimer
    {
        readonly PriorityQueue<CancelableJob> _pq = new PriorityQueue<CancelableJob>();
        readonly object _queueLock = new object();

        /// <summary>
        /// Add Cancelable Job in the queue.
        /// </summary>
        /// <param name="job">The Job object</param>
        /// <param name="millisecondsAfter">The reserved time to be executed</param>
        /// <returns>The reserved Cancelable job.</returns>
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
