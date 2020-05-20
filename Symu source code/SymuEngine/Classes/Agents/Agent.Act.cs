﻿#region Licence

// Description: Symu - SymuEngine
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using SymuEngine.Classes.Task;
using SymuEngine.Common;
using SymuEngine.Messaging.Messages;
using SymuEngine.Repository;

#endregion

namespace SymuEngine.Classes.Agents
{
    /// <summary>
    ///     An abstract base class for agents.
    ///     You must define your own agent derived classes derived
    ///     This partial class focus on Act methods
    /// </summary>
    public abstract partial class Agent
    {
        /// <summary>
        ///     This is the method that is called when the agent receives a message and is activated.
        ///     When TimeStep.Type is Intraday, messages are treated as tasks and stored in task.Parent attribute
        /// </summary>
        /// <param name="message">The message that the agent has received and should respond to</param>
        public void Act(Message message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            //if (TimeStep.Type == TimeStepType.Intraday && message.Medium != CommunicationMediums.System)
            if (Cognitive.TasksAndPerformance.CanPerformTask && message.Medium != CommunicationMediums.System)
            {
                // Switch message into a task in the task manager
                var communication =
                    Environment.WhitePages.Network.NetworkCommunications.TemplateFromChannel(message.Medium);
                var task = new SymuTask(TimeStep.Step)
                {
                    Type = message.Medium.ToString(),
                    TimeToLive = communication.Cognitive.InternalCharacteristics.TimeToLive,
                    Parent = message,
                    Weight = Environment.WhitePages.Network.NetworkCommunications.TimeSpent(message.Medium, false,
                        Environment.Organization.Models.RandomLevelValue)
                };
                TaskProcessor.Post(task);
            }
            else
            {
                ActMessage(message);
            }
        }

        /// <summary>
        ///     This is where the main logic of the agent should be placed.
        /// </summary>
        /// <param name="message"></param>
        public virtual void ActMessage(Message message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            switch (message.Subject)
            {
                case SymuYellowPages.Stop:
                    State = AgentState.Stopping;
                    break;
                case SymuYellowPages.Subscribe:
                    ActSubscribe(message);
                    break;
                default:
                    ActClassKey(message);
                    break;
            }
        }

        /// <summary>
        ///     Trigger every event before the new step
        ///     Do not send messages, use NextStep for that
        /// </summary>
        public virtual async void PreStep()
        {
            MessageProcessor?.ClearMessagesPerPeriod();
            ForgettingModel?.InitializeForgettingProcess();

            // Databases
            if (HasEmail)
            {
                Email.ForgettingProcess(TimeStep.Step);
            }

            _newInteractionCounter = 0;
            HandleStatus();
            // intentionally after Status
            HandleCapacity(true);
            // Task manager
            if (!Cognitive.TasksAndPerformance.CanPerformTask)
            {
                return;
            }

            async Task<bool> ProcessWorkInProgress()
            {
                while (Capacity.HasCapacity && Status != AgentStatus.Offline)
                {
                    try
                    {
                        var task = await TaskProcessor.Receive(TimeStep.Step).ConfigureAwait(false);
                        switch (task.Parent)
                        {
                            case Message message:
                                // When TimeStep.Type is Intraday, messages are treated as tasks and stored in task.Parent attribute
                                // Once a message (as a task) is receive it is treated as a message
                                if (task.IsToDo)
                                {
                                    ActMessage(message);
                                }

                                WorkOnTask(task);
                                break;
                            default:
                                WorkInProgress(task);
                                break;
                        }
                    }
                    catch (Exception exception)
                    {
                        var exceptionDispatchInfo = ExceptionDispatchInfo.Capture(exception);
                        exceptionDispatchInfo.Throw();
                    }
                }

                // If we didn't deschedule then run the continuation immediately
                return true;
            }

            await ProcessWorkInProgress().ConfigureAwait(false);

            ActEndOfDay();
        }

        /// <summary>
        ///     Trigger event after the taskManager is started.
        ///     Used by the agent to subscribe to AfterSetTaskDone event
        /// </summary>
        /// <example>TaskManager.AfterSetTaskDone += AfterSetTaskDone;</example>
        public virtual void OnAfterTaskProcessorStart()
        {
        }

        /// <summary>
        ///     Trigger every event after the actual step,
        ///     Do not send messages
        /// </summary>
        public virtual void PostStep()
        {
            ForgettingModel?.FinalizeForgettingProcess(TimeStep.Step);
        }

        /// <summary>
        ///     Trigger at the end of day,
        ///     agent can still send message
        /// </summary>
        public virtual void ActEndOfDay()
        {
            SendNewInteractions();
            TaskProcessor?.TasksManager.TasksCheck(TimeStep.Step);
        }

        /// <summary>
        ///     Send new interactions to augment its sphere of interaction if possible
        ///     Depends on Cognitive.InteractionPatterns && Cognitive.InteractionCharacteristics
        /// </summary>
        public void SendNewInteractions()
        {
            var agents = GetAgentIdsForNewInteractions().ToList();
            if (!agents.Any())
            {
                return;
            }

            // Send new interactions
            SendToMany(agents, MessageAction.Ask, SymuYellowPages.Actor, CommunicationMediums.FaceToFace);
        }

        /// <summary>
        ///     Start a weekend, by asking new tasks if agent perform tasks on weekends
        /// </summary>
        public virtual void ActWeekEnd()
        {
            if (!Cognitive.TasksAndPerformance.CanPerformTaskOnWeekEnds ||
                TaskProcessor.TasksManager.HasReachedTotalMaximumLimit)
            {
                return;
            }

            GetNewTasks();
        }

        public virtual void ActCadence()
        {
        }

        /// <summary>
        ///     Start the working day, by asking new tasks
        /// </summary>
        public virtual void ActWorkingDay()
        {
            ImpactOfBlockersOnCapacity();

            if (!Cognitive.TasksAndPerformance.CanPerformTask || TaskProcessor.TasksManager.HasReachedTotalMaximumLimit)
            {
                return;
            }

            GetNewTasks();
        }

        /// <summary>
        ///     Event that occur on friday to end the work week
        /// </summary>
        public virtual void ActEndOfWeek()
        {
        }

        public virtual void ActEndOfMonth()
        {
        }

        public virtual void ActEndOfYear()
        {
        }

        /// <summary>
        ///     Check if agent is performing task today depending on its settings or if agent is active
        /// </summary>
        /// <returns>true if agent is performing task, false if agent is not</returns>
        public bool IsPerformingTask()
        {
            // Agent can be temporary isolated
            var isPerformingTask = !Cognitive.InteractionPatterns.IsIsolated();
            return isPerformingTask && (Cognitive.TasksAndPerformance.CanPerformTask && TimeStep.IsWorkingDay ||
                                        Cognitive.TasksAndPerformance.CanPerformTaskOnWeekEnds &&
                                        !TimeStep.IsWorkingDay);
        }

        /// <summary>
        ///     Set the Status to available if agent as InitialCapacity, Offline otherwise
        /// </summary>
        public virtual void HandleStatus()
        {
            Status = !Cognitive.InteractionPatterns.IsIsolated() ? AgentStatus.Available : AgentStatus.Offline;
            if (Status != AgentStatus.Offline)
                // Open the agent mailbox with all the waiting messages
            {
                PostDelayedMessages();
            }
        }

        protected virtual void ActClassKey(Message message)
        {
        }
    }
}