using System;

namespace ServerCoreTCP.Job
{
    /// <summary>
    /// The interface of Job.
    /// </summary>
    public interface IJob
    {
        void Execute();
    }

    /// <summary>
    /// The job object which is used in JobSerialier and JobTimer.<br/>Up to 5 arguments are provided.
    /// </summary>
    public class Job : IJob
    {
        readonly Action _action;

        public Job(Action action)
        {
            _action = action;
        }

        public void Execute()
        {
            _action.Invoke();
        }
    }

    public class Job<T1> : IJob
    {
        readonly Action<T1> _action;
        readonly T1 _t1;

        public Job(Action<T1> action, T1 t1)
        {
            _action = action;
            _t1 = t1;
        }

        public void Execute()
        {
            _action.Invoke(_t1);
        }
    }

    public class Job<T1, T2> : IJob
    {
        readonly Action<T1, T2> _action;
        readonly T1 _t1;
        readonly T2 _t2;

        public Job(Action<T1, T2> action, T1 t1, T2 t2)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
        }

        public void Execute()
        {
            _action.Invoke(_t1, _t2);
        }
    }

    public class Job<T1, T2, T3> : IJob
    {
        readonly Action<T1, T2, T3> _action;
        readonly T1 _t1;
        readonly T2 _t2;
        readonly T3 _t3;

        public Job(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
        }

        public void Execute()
        {
            _action.Invoke(_t1, _t2, _t3);
        }
    }

    public class Job<T1, T2, T3, T4> : IJob
    {
        readonly Action<T1, T2, T3, T4> _action;
        readonly T1 _t1;
        readonly T2 _t2;
        readonly T3 _t3;
        readonly T4 _t4;

        public Job(Action<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
            _t4 = t4;
        }

        public void Execute()
        {
            _action.Invoke(_t1, _t2, _t3, _t4);
        }
    }

    public class Job<T1, T2, T3, T4, T5> : IJob
    {
        readonly Action<T1, T2, T3, T4, T5> _action;
        readonly T1 _t1;
        readonly T2 _t2;
        readonly T3 _t3;
        readonly T4 _t4;
        readonly T5 _t5;

        public Job(Action<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
        {
            _action = action;
            _t1 = t1;
            _t2 = t2;
            _t3 = t3;
            _t4 = t4;
            _t5 = t5;
        }

        public void Execute()
        {
            _action.Invoke(_t1, _t2, _t3, _t4, _t5);
        }
    }
}
