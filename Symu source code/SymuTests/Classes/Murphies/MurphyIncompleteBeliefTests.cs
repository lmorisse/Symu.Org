﻿#region Licence

// Description: Symu - SymuTests
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Symu.Classes.Agents;
using Symu.Classes.Agents.Models;
using Symu.Classes.Agents.Models.CognitiveModel;
using Symu.Classes.Murphies;
using Symu.Classes.Organization;
using Symu.Classes.Task;
using Symu.Common;
using Symu.Repository.Networks;
using Symu.Repository.Networks.Beliefs;

#endregion

namespace SymuTests.Classes.Murphies
{
    [TestClass]
    public class MurphyIncompleteBeliefTests
    {
        private readonly AgentId _agentId = new AgentId(1, 1);
        private readonly MurphyIncompleteBelief _murphy = new MurphyIncompleteBelief();
        private readonly TaskKnowledgeBits _taskBits = new TaskKnowledgeBits();
        private AgentBeliefs _agentBeliefs;
        private Belief _belief;
        private BeliefsModel _beliefsModel;
        private CognitiveArchitecture _cognitiveArchitecture;
        private Network _network;

        [TestInitialize]
        public void Initialize()
        {
            _network = new Network(new AgentTemplates(), new OrganizationModels());
            _cognitiveArchitecture = new CognitiveArchitecture
            {
                KnowledgeAndBeliefs = {HasBelief = true, HasKnowledge = true},
                MessageContent = {CanReceiveBeliefs = true, CanReceiveKnowledge = true},
                InternalCharacteristics = {CanLearn = true, CanForget = true, CanInfluenceOrBeInfluence = true}
            };
            var modelEntity = new ModelEntity();
            _beliefsModel = new BeliefsModel(_agentId, modelEntity, _cognitiveArchitecture, _network) {On = true};
            _belief = new Belief(1, "1", 1, RandomGenerator.RandomUniform, BeliefWeightLevel.RandomWeight);

            _network.NetworkBeliefs.AddBelief(_belief);
            _network.NetworkBeliefs.Add(_agentId, _belief, BeliefLevel.NeitherAgreeNorDisagree);
            _agentBeliefs = _network.NetworkBeliefs.GetAgentBeliefs(_agentId);

            _taskBits.SetMandatory(new byte[] {0});
            _taskBits.SetRequired(new byte[] {0});
        }

        /// <summary>
        ///     Non passing test
        /// </summary>
        [TestMethod]
        public void NullCheckBeliefTest()
        {
            float mandatoryCheck = 0;
            float requiredCheck = 0;
            byte mandatoryIndex = 0;
            byte requiredIndex = 0;
            Assert.ThrowsException<ArgumentNullException>(() =>
                _murphy.CheckBelief(_belief, null, _agentBeliefs, ref mandatoryCheck, ref requiredCheck,
                    ref mandatoryIndex,
                    ref requiredIndex));
            // no belief
            Assert.ThrowsException<NullReferenceException>(() => _murphy.CheckBelief(null, _taskBits, _agentBeliefs,
                ref mandatoryCheck, ref requiredCheck, ref mandatoryIndex, ref requiredIndex));
        }

        /// <summary>
        ///     Model off
        /// </summary>
        [TestMethod]
        public void CheckBeliefTest()
        {
            float mandatoryCheck = 0;
            float requiredCheck = 0;
            byte mandatoryIndex = 0;
            byte requiredIndex = 0;
            _murphy.CheckBelief(_belief, _taskBits, _agentBeliefs, ref mandatoryCheck, ref requiredCheck,
                ref mandatoryIndex,
                ref requiredIndex);
            Assert.AreEqual(0, mandatoryCheck);
            Assert.AreEqual(0, requiredCheck);
        }

        /// <summary>
        ///     Model on
        /// </summary>
        [TestMethod]
        public void CheckBeliefTest1()
        {
            float mandatoryCheck = 0;
            float requiredCheck = 0;
            byte mandatoryIndex = 0;
            byte requiredIndex = 0;
            _beliefsModel.On = true;
            _beliefsModel.AddBelief(_belief.Id, BeliefLevel.NeitherAgreeNorDisagree);
            _beliefsModel.InitializeBeliefs();
            // Force beliefBits
            _beliefsModel.GetBelief(_belief.Id).BeliefBits.SetBit(0, 1);
            _belief.Weights.SetBit(0, 1);
            _murphy.CheckBelief(_belief, _taskBits, _agentBeliefs, ref mandatoryCheck, ref requiredCheck,
                ref mandatoryIndex,
                ref requiredIndex);
            Assert.AreEqual(1, mandatoryCheck);
            Assert.AreEqual(1, requiredCheck);
        }
    }
}