﻿#region Licence

// Description: SymuBiz - Symu
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using Symu.Common.Interfaces.Agent;
using Symu.Common.Interfaces.Entity;
using Symu.DNA.Networks.OneModeNetworks;
using Symu.DNA.Networks.TwoModesNetworks;

#endregion

namespace Symu.Repository.Entity
{
    public class AgentTask : IAgentTask
    {
        public AgentTask(IAgentId id, ITask task)
        {
            Id = id;
            Task = task;
        }

        /// <summary>
        /// The value used to feed the matrix network
        /// For a binary matrix network, the value is 1
        /// </summary>
        public float Value => 1;

        public IAgentId Id { get; }
        public ITask Task { get; set; }
    }
}