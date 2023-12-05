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
        readonly protected IJob _job;
        readonly protected long _millisecondsAfterExec;
        protected bool _canceled;

        public RevocableJob(IJob job, long millisecondsAfterExec)
        {
            _job = job;
            _millisecondsAfterExec = millisecondsAfterExec;
            _canceled = false;
        }

        public void Execute()
        {
            if (_canceled == false) _job?.Execute();
        }

        public bool Canceled
        {
            get => _canceled;
            set => _canceled = value;
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
