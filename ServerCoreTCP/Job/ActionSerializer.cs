using System;
using System.Collections.Generic;

namespace ServerCoreTCP.Job
{
    /// <summary>
    /// This is ActionSerilizer class for executing actions in one thread. 
    /// <br/>If you want to user parameters, use IJobSerializer. 
    /// <br/>It need to be called `Flush()` manually.
    /// <br/>When `Flush()` called, the it will invoke all actions in the queue.
    /// </summary>
    public class ActionSerializer
    {
        readonly Queue<Action> _actionQueue = new Queue<Action>();
        readonly object _queueLock = new object();

        public void Flush()
        {
            while (true)
            {
                if (TryPop(out Action action) == true)
                {
                    action.Invoke();
                }
                else return;
            }
        }

        public void Add(Action action)
        {
            lock(_queueLock)
            {
                _actionQueue.Enqueue(action);
            }
        }

        bool TryPop(out Action action)
        {
            lock (_queueLock)
            {
                if (_actionQueue.Count == 0)
                {
                    action = null;
                    return false;
                }
                action = _actionQueue.Dequeue();
                return true;
            }
        }
    }

    /// <summary>
    /// This is ActionSerializerAuto class for executing actions in one thread.
    /// <br/>If you want to user parameters, use IJobSerializerAuto.
    /// <br/>When `Add()` called, it will begin to invoke actions in the queue if any thread is not flushing.
    /// <br/>At this time, other `Add()` can be called and it just adds a action to a queue.
    /// </summary>
    public class ActionSerializerAuto
    {
        readonly Queue<Action> _queue = new Queue<Action>();
        readonly object _queueLock = new object();

        bool _flush = false;

        public void Add(Action action)
        {
            bool flush = false;
            lock (_queueLock)
            {
                _queue.Enqueue(action);
                if (_flush == false) flush = _flush = true;
            }

            if (flush == true) Flush();
        }

        void Flush()
        {
            while (true)
            {
                if (TryPop(out Action job) == true)
                {
                    job.Invoke();
                }
                else return;
            }
        }

        bool TryPop(out Action action)
        {
            lock (_queueLock)
            {
                if (_queue.Count == 0)
                {
                    _flush = false;
                    action = null;
                    return false;
                }
                action = _queue.Dequeue();
                return true;
            }
        }
    }

    
}
