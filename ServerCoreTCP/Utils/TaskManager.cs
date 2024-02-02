using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerCoreTCP.Utils
{
    public class TaskManager
    {
        Action _main = null;
        readonly List<Task> _taskList = null;

        public TaskManager()
        {
            _taskList = new List<Task>();
        }

        /// <summary>
        /// The main task does not contain infite-loop for the action.
        /// </summary>
        /// <param name="mainTask"></param>
        public void SetMain(Action mainTask)
        {
            _main = mainTask;
        }


        /// <summary>
        /// Add a new task that invokes the action infinitely with options of LongRunning.
        /// </summary>
        /// <param name="action"></param>
        public void AddTask(Action action)
        {
            _taskList.Add(new Task(() =>
            {
                while (true)
                {
                    action.Invoke();
                }
            }, TaskCreationOptions.LongRunning));
        }

        
        public void StartTasks()
        {
            foreach(Task task in _taskList)
            {
                task.Start();
            }

            _main?.Invoke();
        }
    }
}
