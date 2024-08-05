using NetCore.Utils;
using System;
using System.Diagnostics;

namespace NetCore.Job
{
    internal readonly struct ActionTimerElement : IComparable<ActionTimerElement>
    {
        readonly internal long MillisecondsExec;
        readonly internal Action Action;

        public ActionTimerElement(Action action, long millisecondsExec)
        {
            Action = action;
            MillisecondsExec = millisecondsExec;
        }

        
        public readonly int CompareTo(ActionTimerElement other)
        {
            // take faster tick
            return (int)(other.MillisecondsExec - MillisecondsExec);
        }
    }

    public class ActionTimer
    {
        readonly PriorityQueue<ActionTimerElement> _pq = new PriorityQueue<ActionTimerElement>();
        readonly Stopwatch _stopwatch = new Stopwatch();
        readonly object _queueLock = new object();

        public void Push(Action action, long millisecondsAfter = 0)
        {
            if (_stopwatch.IsRunning == false) _stopwatch.Start();

            long millisecondsExec = _stopwatch.ElapsedMilliseconds + millisecondsAfter;
            ActionTimerElement actionElement = new ActionTimerElement(action, millisecondsExec);

            lock (_queueLock)
            {
                _pq.Enqueue(actionElement);
            }
        }

        public void Flush()
        {
            while (true)
            {
                long now = _stopwatch.ElapsedMilliseconds;

                ActionTimerElement actionElement;
                lock (_queueLock)
                {
                    if (_pq.TryPeek(out actionElement) == false) break;
                    if (actionElement.MillisecondsExec >= now) break;

                    _ = _pq.Dequeue();
                }

                actionElement.Action.Invoke();
            }
        }

    }
}
