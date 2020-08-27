﻿#region Licence

// Description: SymuBiz - Symu
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using Symu.Common.Interfaces.Agent;
using Symu.Common.Interfaces.Entity;
using Symu.Common.Math.ProbabilityDistributions;
using Symu.Repository.Entity;
using Symu.Repository.Networks;
using Symu.Repository.Networks.Beliefs;
using Symu.Repository.Networks.Influences;

#endregion

namespace Symu.Classes.Agents.Models.CognitiveModels
{
    /// <summary>
    ///     CognitiveArchitecture define how an actor will influence or be influenced
    ///     Entity enable or not this mechanism for all the agents during the simulation
    ///     The InfluenceModel initialize the real value of the agent's influence parameters
    /// </summary>
    /// <remarks>From Construct Software</remarks>
    public class InfluenceModel
    {
        private readonly AgentId _agentId;
        private readonly BeliefNetwork _networkBeliefs;
        private readonly InfluenceNetwork _networkInfluences;
        private readonly BeliefsModel _beliefsModel;

        /// <summary>
        ///     Initialize influence model :
        ///     update networkInfluences
        /// </summary>
        /// <param name="agentId"></param>
        /// <param name="entity"></param>
        /// <param name="cognitiveArchitecture"></param>
        /// <param name="network"></param>
        /// <param name="beliefsModel"></param>
        public InfluenceModel(AgentId agentId, ModelEntity entity, CognitiveArchitecture cognitiveArchitecture,
            MetaNetwork network, BeliefsModel beliefsModel)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (cognitiveArchitecture == null)
            {
                throw new ArgumentNullException(nameof(cognitiveArchitecture));
            }

            if (network == null)
            {
                throw new ArgumentNullException(nameof(network));
            }

            On = cognitiveArchitecture.KnowledgeAndBeliefs.HasBelief && entity.IsAgentOn();
            if (!On)
            {
                return;
            }

            _agentId = agentId;
            _networkInfluences = network.Influences;
            _networkBeliefs = network.Beliefs;
            _beliefsModel = beliefsModel;

            if (cognitiveArchitecture.InternalCharacteristics.CanInfluenceOrBeInfluence && On)
            {
                _networkInfluences.Add(agentId,
                    NextInfluenceability(cognitiveArchitecture.InternalCharacteristics),
                    NextInfluentialness(cognitiveArchitecture.InternalCharacteristics));
            }
            else
            {
                _networkInfluences.Add(agentId, 0, 0);
            }
        }

        public bool On { get; set; }

        /// <summary>
        ///     Clone the Influentialness for a specific agent with a random value between [InfluentialnessRateMin,
        ///     InfluentialnessRateMax]
        /// </summary>
        /// <returns></returns>
        public static float NextInfluentialness(InternalCharacteristics internalCharacteristics)
        {
            if (internalCharacteristics == null)
            {
                throw new ArgumentNullException(nameof(internalCharacteristics));
            }

            return ContinuousUniform.Sample(internalCharacteristics.InfluentialnessRateMin,
                internalCharacteristics.InfluentialnessRateMax);
        }

        /// <summary>
        ///     Clone the Influentialness for a specific agent with a random value between [InfluentialnessRateMin,
        ///     InfluentialnessRateMax]
        /// </summary>
        /// <returns></returns>
        public static float NextInfluenceability(InternalCharacteristics internalCharacteristics)
        {
            if (internalCharacteristics == null)
            {
                throw new ArgumentNullException(nameof(internalCharacteristics));
            }

            return ContinuousUniform.Sample(internalCharacteristics.InfluenceabilityRateMin,
                internalCharacteristics.InfluenceabilityRateMax);
        }

        /// <summary>
        ///     Be influenced beliefId from agentAgentId
        /// </summary>
        /// <param name="beliefId"></param>
        /// <param name="beliefBits">from agentAgentId beliefBits</param>
        /// <param name="agentId"></param>
        /// <param name="beliefLevel"></param>
        public void BeInfluenced(IId beliefId, Bits beliefBits, AgentId agentId, BeliefLevel beliefLevel)
        {
            if (!On || beliefBits == null)
            {
                return;
            }

            // Learning From agent
            var influentialness = _networkInfluences.GetInfluentialness(agentId);
            // to Learner
            var influenceability = _networkInfluences.GetInfluenceability(_agentId);
            // Learner learn beliefId from agentAgentId with a weight of influenceability * influentialness
            _beliefsModel.Learn(beliefId, beliefBits, influenceability * influentialness, beliefLevel);
        }

        public void ReinforcementByDoing(IId beliefId, byte beliefBit, BeliefLevel beliefLevel)
        {
            if (!On || _beliefsModel == null)
            {
                return;
            }
            _beliefsModel.LearnNewBelief(beliefId, beliefLevel);
            var agentBelief = _beliefsModel.GetAgentBelief(beliefId);
            agentBelief.Learn(_networkBeliefs.Model, beliefBit);
        }
    }
}