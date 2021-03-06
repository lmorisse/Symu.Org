#region Licence

// Description: SymuBiz - Symu
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using System.Threading.Tasks;

#endregion

namespace Symu.Classes.Task.Manager
{
    /// <summary>
    ///     TaskProcessor handle the tasksManager.
    ///     It propose events trigger by the tasksManager to customize the behaviour of the manager
    /// </summary>
    public class TaskProcessor : IDisposable
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="tasksLimit">Agent.Cognitive.TasksAndPerformance.TasksLimit</param>
        /// <param name="debug">Environment.Debug</param>
        public TaskProcessor(TasksLimit tasksLimit, bool debug)
        {
            TasksManager = new TasksManager(tasksLimit, debug);
            TasksManager.OnAfterSetTaskInProgress += AfterSetTaskInProgress;
            TasksManager.OnPrioritizeTasks += PrioritizeTasks;
        }

        public TasksManager TasksManager { get; }

        #region IDisposable Members

        public void Dispose()
        {
            TasksManager.Dispose();
        }

        #endregion

        /// <summary>
        ///     EventHandler triggered after the event SetDone
        /// </summary>
        public event EventHandler<TaskEventArgs> OnAfterSetTaskDone;

        /// <summary>
        ///     EventHandler triggered after the event SetTaskInProgress
        /// </summary>
        public event EventHandler<TaskEventArgs> OnAfterSetTaskInProgress;

        /// <summary>
        ///     EventHandler triggered after the event CancelTask
        /// </summary>
        public event EventHandler<TaskEventArgs> OnAfterCancelTask;

        /// <summary>
        ///     EventHandler triggered after the event OnPrioritizeTasks
        /// </summary>
        public event EventHandler<TasksEventArgs> OnPrioritizeTasks;

        public void Post(SymuTask task)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            TasksManager.Post(task);
        }

        public Task<SymuTask> Receive(ushort step)
        {
            return TasksManager.Receive(step);
        }

        /// <summary>
        ///     Clone a task done in TasksManager
        /// </summary>
        public void SetTaskDone(SymuTask task)
        {
            TasksManager.SetDone(task);
            OnAfterSetTaskDone?.Invoke(this, new TaskEventArgs(task));
        }

        /// <summary>
        ///     Clone a task in Progress in TasksManager
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AfterSetTaskInProgress(object sender, TaskEventArgs e)
        {
            OnAfterSetTaskInProgress?.Invoke(this, new TaskEventArgs(e.Task));
        }

        private void PrioritizeTasks(object sender, TasksEventArgs e)
        {
            OnPrioritizeTasks?.Invoke(this, new TasksEventArgs(e.Tasks));
        }

        /// <summary>
        ///     Cancel a task in the TasksManager
        /// </summary>
        /// <param name="task"></param>
        public void Cancel(SymuTask task)
        {
            TasksManager.Cancel(task);
            OnAfterCancelTask?.Invoke(this, new TaskEventArgs(task));
        }
    }
}