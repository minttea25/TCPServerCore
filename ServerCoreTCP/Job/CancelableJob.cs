using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCoreTCP.Job
{
    /// <summary>
    /// The interface of cancelable job.
    /// </summary>
    public interface ICancelable : IJob
    {
        bool Canceled { get; set; }
        long MillisecondsExec { get; }
    }

    /// <summary>
    /// The cancelable job object which is used in JobTimer. Set `Canceled` to false when you want to cancel to invoke this job.
    /// </summary>
    public class CancelableJob : ICancelable, IComparable<CancelableJob>
    {
        readonly IJob _job;
        readonly long _millisecondsAfterExec;

        bool m_canceled;
        /// <summary>
        /// Set this value to false if you want to cancel to invoke this job.
        /// </summary>
        public bool Canceled
        {
            get => m_canceled;
            set => m_canceled = value;
        }

        public CancelableJob(IJob job, long millisecondsAfterExec)
        {
            _job = job;
            _millisecondsAfterExec = millisecondsAfterExec;
            m_canceled = false;
        }

        public void Execute()
        {
            if (m_canceled == false) _job?.Execute();
        }

        public long MillisecondsExec
        {
            get { return _millisecondsAfterExec; }
        }

        // take faster tick
        public int CompareTo(CancelableJob other)
        {
            return (int)(other.MillisecondsExec - MillisecondsExec);
        }
    }

}
