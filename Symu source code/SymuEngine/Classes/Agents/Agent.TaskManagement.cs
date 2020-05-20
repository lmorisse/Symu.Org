﻿#region Licence

// Description: Symu - SymuEngine
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using System.Collections.Generic;
using SymuEngine.Classes.Agents.Models;
using SymuEngine.Classes.Blockers;
using SymuEngine.Classes.Murphies;
using SymuEngine.Classes.Task;
using SymuEngine.Environment;
using SymuEngine.Messaging.Messages;
using SymuEngine.Results.Blocker;
using static SymuTools.Constants;

#endregion

namespace SymuEngine.Classes.Agents
{
    /// <summary>
    ///     An abstract base class for agents.
    ///     You must define your own agent derived classes derived
    ///     This partial class focus on tasks management methods
    /// </summary>
    public abstract partial class Agent
    {
        /// <summary>
        ///     Override this method to specify how an agent will get new tasks to complete
        ///     By default, if worker can't perform task or has reached the maximum number of tasks,
        ///     he can't ask for more tasks, just finished the tasks in the taskManager
        /// </summary>
        public virtual void GetNewTasks()
        {
        }

        /// <summary>
        ///     Work on the next task
        /// </summary>
        public void WorkInProgress(SymuTask task)
        {
            if (task == null)
            {
                Status = AgentStatus.Available;
                return;
            }

            // The task may be blocked, try to unlock it
            TryRecoverBlockedTask(task);
            // Agent may discover new blockers
            CheckBlockers(task);
            // Task may have been blocked
            // Capacity may have been used for blockers
            if (!task.IsBlocked && Capacity.HasCapacity)
            {
                WorkOnTask(task);
            }

            if (Capacity.HasCapacity)
                // We start a new loop on the current tasks of the agent
            {
                SwitchingContextModel();
            }
        }

        /// <summary>
        ///     Simulate the work on a specific task
        /// </summary>
        /// <param name="task"></param>
        public virtual float WorkOnTask(SymuTask task)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            float timeSpent;
            if (TimeStep.Type == TimeStepType.Intraday)
            {
                timeSpent = Math.Min(Environment.Organization.Models.Intraday, Capacity.Actual);
            }
            else
            {
                timeSpent = Cognitive.TasksAndPerformance.TasksLimit.LimitSimultaneousTasks
                    // Mono tasking
                    ? Math.Min(task.Weight, Capacity.Actual)
                    // Multi tasking
                    : Math.Min(task.Weight / 2, Capacity.Actual);
            }

            timeSpent = Math.Min(task.WorkToDo, timeSpent);
            task.WorkToDo -= timeSpent;
            if (task.WorkToDo < Tolerance)
            {
                SetTaskDone(task);
            }
            else
            {
                UpdateTask(task);
            }

            // As the agent work on task that requires knowledge, the agent can't forget the associate knowledge today
            foreach (var knowledgeId in task.KnowledgesBits.KnowledgeIds)
            {
                ForgettingModel.UpdateForgettingProcess(knowledgeId, task.KnowledgesBits.GetBits(knowledgeId));
            }

            Capacity.Decrement(timeSpent);
            return timeSpent;
        }

        /// <summary>
        ///     Set the task done in task manager
        /// </summary>
        /// <param name="task"></param>
        public void SetTaskDone(SymuTask task)
        {
            TaskProcessor.PushDone(task);
        }

        /// <summary>
        ///     Update the task as the agent has worked on it, but not complete it
        /// </summary>
        /// <param name="task"></param>
        public void UpdateTask(SymuTask task)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            task.Update(TimeStep.Step);
        }

        /// <summary>
        ///     Switching context may have an impact on the agent capacity
        /// </summary>
        public virtual void SwitchingContextModel()
        {
        }

        #region Capacity

        /// <summary>
        ///     Describe the agent capacity
        /// </summary>
        public AgentCapacity Capacity { get; } = new AgentCapacity();

        /// <summary>
        ///     Set the initial capacity for the new step based on SetInitialCapacity, working day,
        ///     By default = Initial capacity if it's a working day, 0 otherwise
        ///     If resetRemainingCapacity set to true, Remaining capacity is reset to Initial Capacity value
        /// </summary>
        public void HandleCapacity(bool resetRemainingCapacity)
        {
            // Intentionally no test on Agent that must be able to perform tasks
            // && Cognitive.TasksAndPerformance.CanPerformTask
            // Example : internet access don't perform task, but is online
            if (IsPerformingTask())
            {
                SetInitialCapacity();
                if (Cognitive.TasksAndPerformance.CanPerformTask)
                {
                    Environment.IterationResult.Capacity += Capacity.Initial;
                }
            }
            else
            {
                Capacity.Initial = 0;
            }

            if (resetRemainingCapacity)
            {
                Capacity.Reset();
            }
        }

        /// <summary>
        ///     Use to set the baseline value of the initial capacity
        /// </summary>
        /// <returns></returns>
        public virtual void SetInitialCapacity()
        {
            Capacity.Initial = 1;
        }

        #endregion

        #region Post task

        /// <summary>
        ///     Post a task in the TasksProcessor
        /// </summary>
        /// <param name="task"></param>
        /// <remarks>Don't use TaskProcessor.Post directly to handle the OnBeforeTaskPost event</remarks>
        public void Post(SymuTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            OnBeforePostTask(task);
            if (!task.IsBlocked)
            {
                TaskProcessor.Post(task);
            }
        }

        /// <summary>
        ///     Post a task in the TasksProcessor
        /// </summary>
        /// <param name="tasks"></param>
        /// <remarks>Don't use TaskProcessor.Post directly to handle the OnBeforeTaskPost event</remarks>
        public void Post(IEnumerable<SymuTask> tasks)
        {
            if (tasks is null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            foreach (var task in tasks)
            {
                OnBeforePostTask(task);
                if (!task.IsBlocked)
                {
                    TaskProcessor.Post(task);
                }
            }
        }

        /// <summary>
        ///     EventHandler triggered before the event TaskProcessor.Post(task)
        ///     By default CheckBlockerBeliefs
        ///     If task must be posted, use task.Blockers
        /// </summary>
        /// <param name="task"></param>
        protected virtual void OnBeforePostTask(SymuTask task)
        {
            CheckBlockerBeliefs(task);
        }

        #endregion

        #region Common Blocker

        /// <summary>
        ///     Launch a recovery for all the blockers
        /// </summary>
        /// <param name="task"></param>
        private void TryRecoverBlockedTask(SymuTask task)
        {
            foreach (var blocker in task.Blockers.FilterBlockers(TimeStep.Step))
            {
                blocker.Update(TimeStep.Step);
                TryRecoverBlocker(task, blocker);
            }
        }

        /// <summary>
        ///     Launch a recovery for the blocker
        /// </summary>
        /// <param name="task"></param>
        /// <param name="blocker"></param>
        protected virtual void TryRecoverBlocker(SymuTask task, Blocker blocker)
        {
            if (blocker is null)
            {
                throw new ArgumentNullException(nameof(blocker));
            }

            switch (blocker.Type)
            {
                case Murphy.IncompleteKnowledge:
                    TryRecoverBlockerIncompleteKnowledge(task, blocker);
                    break;
                case Murphy.IncompleteBelief:
                    TryRecoverBlockerIncompleteBelief(task, blocker);
                    break;
            }
        }

        /// <summary>
        ///     Check if there are  blockers today on the task
        /// </summary>
        /// <param name="task"></param>
        private void CheckBlockers(SymuTask task)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (!Environment.Organization.Models.MultipleBlockers && task.IsBlocked)
                // One blocker at a time
            {
                return;
            }

            CheckNewBlockers(task);
        }

        /// <summary>
        ///     Launch a recovery for the blocker
        /// </summary>
        /// <param name="task"></param>
        public virtual void CheckNewBlockers(SymuTask task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (task.Parent is Message)
            {
                return;
            }

            CheckBlockerIncompleteKnowledge(task);
        }

        /// <summary>
        ///     Impact of blockers on the remaining capacity
        ///     Blockers may create idle time
        /// </summary>
        public virtual void ImpactOfBlockersOnCapacity()
        {
        }

        /// <summary>
        ///     Add a new blocker to the task
        ///     And follow it in the IterationResult if FollowBlocker is true
        /// </summary>
        /// <param name="task"></param>
        /// <param name="murphyType"></param>
        /// <param name="parameter"></param>
        /// <param name="parameter2"></param>
        /// <returns></returns>
        public Blocker AddBlocker(SymuTask task, int murphyType, object parameter, object parameter2)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var blocker = task.Blockers.Add(murphyType, TimeStep.Step, parameter, parameter2);
            Environment.IterationResult.Blockers.AddBlockerInProgress(TimeStep.Step);
            return blocker;
        }

        /// <summary>
        ///     Remove an existing blocker from a task
        ///     And update IterationResult if FollowBlocker is true
        /// </summary>
        /// <param name="task"></param>
        /// <param name="blocker"></param>
        /// <param name="resolution"></param>
        public void RecoverBlocker(SymuTask task, Blocker blocker, BlockerResolution resolution)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (blocker != null)
            {
                task.Blockers.Remove(blocker);
            }

            Environment.IterationResult.Blockers.BlockerDone(resolution, TimeStep.Step);
        }

        #endregion

        #region Incomplete Knowledge

        /// <summary>
        ///     Check Task.KnowledgesBits against WorkerAgent.expertise
        ///     If Has expertise Task will be complete
        ///     If has expertise for mandatoryKnowledgesBits but not for requiredKnowledgesBits, he will guess, and possibly
        ///     complete the task incorrectly
        ///     If hasn't expertise for mandatoryKnowledgesBits && for requiredKnowledgesBits, he will ask for help (co workers,
        ///     internet forum, ...) && learn
        /// </summary>
        /// <param name="task"></param>
        public void CheckBlockerIncompleteKnowledge(SymuTask task)
        {
            if (!Environment.Organization.Murphies.IncompleteKnowledge.On)
            {
                return;
            }

            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            foreach (var knowledgeId in task.KnowledgesBits.KnowledgeIds)
            {
                CheckBlockerIncompleteKnowledge(task, knowledgeId);
            }
        }

        /// <summary>
        ///     Check Task.KnowledgesBits for a specific KnowledgeBit against WorkerAgent.expertise
        ///     If Has expertise Task will be complete
        ///     If has expertise for mandatoryKnowledgesBits but not for requiredKnowledgesBits, he will guess, and possibly
        ///     complete the task incorrectly
        ///     If hasn't expertise for mandatoryKnowledgesBits && for requiredKnowledgesBits, he will ask for help (co workers,
        ///     internet forum, ...) && learn
        /// </summary>
        /// <param name="task"></param>
        /// <param name="knowledgeId"></param>
        protected virtual void CheckBlockerIncompleteKnowledge(SymuTask task, ushort knowledgeId)
        {
            var taskBits = task.KnowledgesBits.GetBits(knowledgeId);
            // If taskBits.Mandatory.Any => mandatoryCheck is false unless workerKnowledge has the good knowledge or there is no mandatory knowledge
            var mandatoryOk = taskBits.GetMandatory().Length == 0;
            // If taskBits.Required.Any => RequiredCheck is false unless workerKnowledge has the good knowledge or there is no required knowledge
            var requiredOk = taskBits.GetRequired().Length == 0;
            byte mandatoryIndex = 0;
            byte requiredIndex = 0;
            KnowledgeModel.CheckKnowledge(knowledgeId, taskBits, ref mandatoryOk, ref requiredOk,
                ref mandatoryIndex, ref requiredIndex, TimeStep.Step);
            if (!mandatoryOk)
            {
                // mandatoryCheck is false => Task is blocked
                var blocker = AddBlocker(task, Murphy.IncompleteKnowledge, knowledgeId, mandatoryIndex);
                TryRecoverBlocker(task, blocker);
            }
            else if (!requiredOk)
            {
                RecoverBlockerKnowledgeByDoing(task, null, knowledgeId, requiredIndex, BlockerResolution.Guessing);
            }
        }

        /// <summary>
        ///     Missing knowledge is guessed or searched in agent's databases
        ///     The worker possibly complete the task incorrectly
        ///     and learn by doing
        /// </summary>
        /// <param name="task"></param>
        /// <param name="knowledgeId"></param>
        /// <param name="knowledgeBit"></param>
        /// <param name="blocker"></param>
        /// <param name="resolution">guessing or searched</param>
        public void RecoverBlockerKnowledgeByDoing(SymuTask task, Blocker blocker, ushort knowledgeId,
            byte knowledgeBit, BlockerResolution resolution)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var impact = Environment.Organization.Murphies.IncompleteKnowledge.NextGuess();
            if (impact > task.Incorrect)
            {
                task.Incorrect = impact;
            }

            task.Weight *= Cognitive.TasksAndPerformance.CostFactorOfLearningByDoing;
            LearningModel.LearnByDoing(knowledgeId, knowledgeBit,
                TimeStep.Step);
            switch (blocker)
            {
                // No blocker, it's a required knowledgeBit
                case null:
                    task.KnowledgesBits.RemoveFirstRequired(knowledgeId);
                    // Blockers Management - no blocker has been created
                    // We create a fake one to follow the impact of the murphy
                    Environment.IterationResult.Blockers.AddBlockerInProgress(TimeStep.Step);
                    break;
                // blocker, it's a mandatory knowledgeBit
                default:
                    task.KnowledgesBits.RemoveFirstMandatory(knowledgeId);
                    break;
            }

            RecoverBlocker(task, blocker, resolution);
        }

        /// <summary>
        ///     Ask Help to teammates that have the adequate knowledge
        ///     when block from lack of knowledge
        /// </summary>
        /// <param name="task"></param>
        /// <param name="blocker"></param>
        public virtual void TryRecoverBlockerIncompleteKnowledge(SymuTask task, Blocker blocker)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (blocker is null)
            {
                throw new ArgumentNullException(nameof(blocker));
            }

            var knowledgeId = (ushort) blocker.Parameter;
            var knowledgeBit = (byte) blocker.Parameter2;
            // Check if he has the right to receive knowledge from others agents
            if (!Cognitive.MessageContent.CanReceiveKnowledge)
            {
                RecoverBlockerKnowledgeByDoing(task, blocker, knowledgeId, knowledgeBit, BlockerResolution.Guessing);
                return;
            }

            // He has the right
            // first search in the databases of the actor
            if (HasEmail && Email.SearchKnowledge(knowledgeId, knowledgeBit,
                Cognitive.TasksAndPerformance.LearningRate))
            {
                RecoverBlockerKnowledgeByDoing(task, blocker, knowledgeId, knowledgeBit, BlockerResolution.Searching);
            }
        }

        #endregion

        #region Incomplete beliefs

        /// <summary>
        ///     Check Task.BeliefBits against Agent.Beliefs
        ///     Prevent the agent from acting on a particular belief
        ///     Task may be blocked if it is the case
        /// </summary>
        public void CheckBlockerBeliefs(SymuTask task)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (task.Parent is Message)
            {
                return;
            }

            foreach (var knowledgeId in task.KnowledgesBits.KnowledgeIds)
            {
                CheckBlockerBelief(task, knowledgeId);
            }
        }

        /// <summary>
        ///     Check a particular beliefId from Task.BeliefBits against Agent.Beliefs
        ///     Prevent the agent from acting on a particular belief
        ///     Task may be blocked if it is the case
        /// </summary>
        public void CheckBlockerBelief(SymuTask task, ushort knowledgeId)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            var taskBits = task.KnowledgesBits.GetBits(knowledgeId);
            float mandatoryScore = 0;
            float requiredScore = 0;
            byte mandatoryIndex = 0;
            byte requiredIndex = 0;
            BeliefsModel.CheckBelief(knowledgeId, taskBits, ref mandatoryScore, ref requiredScore,
                ref mandatoryIndex, ref requiredIndex);
            CheckBlockerBelief(task, knowledgeId, mandatoryScore, requiredScore, mandatoryIndex, requiredIndex);
        }

        protected virtual void CheckBlockerBelief(SymuTask task, ushort knowledgeId, float mandatoryScore,
            float requiredScore, byte mandatoryIndex, byte requiredIndex)
        {
            if (!(mandatoryScore <= -Cognitive.InternalCharacteristics.RiskAversionThreshold))
            {
                return;
            }

            // Prevent the agent from acting on a particular belief
            // mandatoryScore is not enough => agent don't want to do the task, the task is blocked
            var blocker = AddBlocker(task, Murphy.IncompleteBelief, knowledgeId, mandatoryIndex);
            TryRecoverBlockerIncompleteBelief(task, blocker);
        }

        /// <summary>
        ///     Ask Help to teammates about it belief
        ///     when task is blocked because of a lack of belief
        /// </summary>
        /// <param name="task"></param>
        /// <param name="blocker"></param>
        public virtual void TryRecoverBlockerIncompleteBelief(SymuTask task, Blocker blocker)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (blocker is null)
            {
                throw new ArgumentNullException(nameof(blocker));
            }

            // Check if he has the right to receive knowledge from others agents
            if (!Cognitive.MessageContent.CanReceiveBeliefs)
            {
                // If agent has no other strategy 
                // Blocker must be unblocked in a way or another
                RecoverBlockerBeliefByGuessing(task, blocker);
            }
        }

        /// <summary>
        ///     Missing belief is guessed
        ///     The worker possibly complete the task incorrectly
        ///     and learn by doing
        /// </summary>
        /// <param name="task"></param>
        /// <param name="blocker"></param>
        public void RecoverBlockerBeliefByGuessing(SymuTask task, Blocker blocker)
        {
            if (task is null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            if (blocker == null)
            {
                throw new ArgumentNullException(nameof(blocker));
            }

            var beliefId = (ushort) blocker.Parameter;
            var beliefBit = (byte) blocker.Parameter2;

            var impact = Environment.Organization.Murphies.IncompleteBelief.NextGuess();
            if (impact > task.Incorrect)
            {
                task.Incorrect = impact;
            }

            task.Weight += Environment.Organization.Murphies.IncompleteBelief.NextImpactOnTimeSpent();
            InfluenceModel.ReinforcementByDoing(beliefId, beliefBit, Cognitive.KnowledgeAndBeliefs.DefaultBeliefLevel);
            // Blockers Management
            RecoverBlocker(task, blocker, BlockerResolution.Guessing);
        }

        #endregion
    }
}