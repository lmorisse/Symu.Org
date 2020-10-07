﻿#region Licence

// Description: SymuBiz - Symu
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using System.Collections.Generic;
using Symu.Common.Interfaces;
using Symu.OrgMod.Edges;
using Symu.OrgMod.Entities;
using Symu.OrgMod.GraphNetworks;
using Symu.OrgMod.GraphNetworks.TwoModesNetworks;

#endregion

namespace Symu.Classes.Agents.Models.CognitiveModels
{
    /// <summary>
    ///     CognitiveArchitecture define how an actor will perform task
    ///     Entity enable or not this mechanism for all the agents during the simulation
    ///     The ActivityModel initialize the real value of the agent's activity parameters
    /// </summary>
    /// <remarks>From Construct Software</remarks>
    public class ResourceTaskModel
    {
        private readonly IAgentId _resourceId;
        private readonly ResourceTaskNetwork _resourceTaskNetwork;
        private readonly OneModeNetwork _taskNetwork;

        /// <summary>
        ///     Initialize influence model :
        ///     update networkInfluences
        /// </summary>
        /// <param name="resourceId"></param>
        /// <param name="network"></param>
        public ResourceTaskModel(IAgentId resourceId, GraphMetaNetwork network)
        {
            if (network == null)
            {
                throw new ArgumentNullException(nameof(network));
            }

            _resourceId = resourceId;
            _resourceTaskNetwork = network.ResourceTask;
            _taskNetwork = network.Task;
        }

        /// <summary>
        ///     Get all the tasks (activities) that an agent can do
        /// </summary>
        public IEnumerable<IAgentId> TaskIds => _resourceTaskNetwork.TargetsFilteredBySource(_resourceId);

        /// <summary>
        ///     Get the all the knowledges for all the tasks of an agent
        /// </summary>
        /// <returns></returns>
        public IDictionary<ITask, IEnumerable<IKnowledge>> Knowledge
        {
            get
            {
                var knowledge = new Dictionary<ITask, IEnumerable<IKnowledge>>();
                foreach (var taskId in TaskIds)
                {
                    var task = _taskNetwork.GetEntity<ITask>(taskId);
                    knowledge.Add(task, task.Knowledge);
                }

                return knowledge;
            }
        }

        /// <summary>
        ///     Add a list of activities an agent can perform
        /// </summary>
        /// <param name="tasks"></param>
        public void AddResourceTasks(IEnumerable<ITask> tasks)
        {
            if (tasks == null)
            {
                throw new ArgumentNullException(nameof(tasks));
            }

            foreach (var task in tasks)
            {
                _ = new ResourceTask(_resourceTaskNetwork, _resourceId, task.EntityId);
            }
        }
    }
}