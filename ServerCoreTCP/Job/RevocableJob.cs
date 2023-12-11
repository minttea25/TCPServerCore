using System;
using System.Collections.Generic;
using System.Text;

namespace ServerCoreTCP.Job
{
    public interface IRevocableJob : IJob
    {
        bool Canceled { get; set; }
        long MillisecondsExec { get; }
    }

    public class RevocableJob : IRevocableJob, IComparable<RevocableJob>
    {
        readonly IJob _job;
        readonly long _millisecondsAfterExec;

        bool m_canceled;
        public bool Canceled
        {
            get => m_canceled;
            set => m_canceled = value;
        }

        public RevocableJob(IJob job, long millisecondsAfterExec)
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
        public int CompareTo(RevocableJob other)
        {
            return (int)(other.MillisecondsExec - MillisecondsExec);
        }
    }

}
