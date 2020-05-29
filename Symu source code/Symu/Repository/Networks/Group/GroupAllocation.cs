﻿#region Licence

// Description: Symu - Symu
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using Symu.Classes.Agents;

#endregion

namespace Symu.Repository.Networks.Group
{
    public class GroupAllocation
    {
        public GroupAllocation(AgentId agentId, float allocation)
        {
            AgentId = agentId;
            Allocation = allocation;
        }

        public AgentId AgentId { get; }

        /// <summary>
        ///     Range 0 - 100
        /// </summary>
        public float Allocation { get; set; }
    }
}