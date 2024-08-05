using System;
using System.Collections.Generic;
using System.Threading;

namespace NetCore.Utils
{
    /// <summary>
    /// It uses Thread directly and is recommended in Debug mode for identifying the thread names.
    /// </summary>
    public class ThreadManager
    {
        readonly public int ThreadCount;

        Action _mainTask = null;
        readonly List<Action> _taskList = null;
        readonly List<string> _taskNames = null;

        /// <summary>
        /// The taskCount does not contains the main task.
        /// </summary>
        /// <param name="taskCount">The task count except main task.</param>
        public ThreadManager(int taskCount)
        {
            ThreadCount = taskCount;
            _taskList = new List<Action>();
            _taskNames = new List<string>();
        }

        /// <summary>
        /// Do not change the thread name before or after calling this function.
        /// </summary>
        /// <param name="mainTaskAction"></param>
        /// <param name="mainThreadName"></param>
        public void SetMainTask(Action mainTaskAction, string mainThreadName = "Main")
        {
            _mainTask = mainTaskAction;
            Thread.CurrentThread.Name = mainThreadName;
        }

        /// <summary>
        /// Do not change the thread name before or after calling this function.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="threadName"></param>
        public void AddTask(Action task, string threadName)
        {
            if (_taskList.Count >= ThreadCount) throw new Exception("Can not add any more task.");
            _taskList.Add(task);
            _taskNames.Add(threadName);
        }

        /// <summary>
        /// Start all registered tasks. 
        /// NOTE: Each task will invoke repeatedly with `Thread.Sleep(0)` in infinite loop at each thread, but the main task will be called only invoke once.
        /// </summary>
        public void StartTasks()
        {
            for (int i=0; i<_taskList.Count; ++i)
            {
                int idx = i;
                Thread th = new Thread(() =>
                {
                    Thread.CurrentThread.Name = _taskNames[idx];
                    while (true)
                    {
                        _taskList[idx].Invoke();
                        Thread.Sleep(0); // yield
                    }
                });
                th.IsBackground = true;
                th.Start();
            }

            _mainTask?.Invoke();
        }
    }
}
