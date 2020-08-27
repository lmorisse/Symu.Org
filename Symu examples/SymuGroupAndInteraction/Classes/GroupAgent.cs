﻿#region Licence

// Description: SymuBiz - SymuGroupAndInteraction
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using Symu.Classes.Agents;
using Symu.Common.Interfaces.Agent;
using Symu.Common.Interfaces.Entity;
using Symu.Environment;

#endregion

namespace SymuGroupAndInteraction.Classes
{
    public sealed class GroupAgent : ReactiveAgent
    {
        public const byte Class = 1;
        /// <summary>
        /// Factory method to create an agent
        /// Call the Initialize method
        /// </summary>
        /// <returns></returns>
        public static GroupAgent CreateInstance(UId id, SymuEnvironment environment)
        {
            var agent = new GroupAgent(id, environment);
            agent.Initialize();
            return agent;
        }

        /// <summary>
        /// Constructor of the agent
        /// </summary>
        /// <remarks>Call the Initialize method after the constructor, or call the factory method</remarks>
        private GroupAgent(UId id, SymuEnvironment environment) : base(
            new AgentId(id, Class), environment)
        {
        }
    }
}