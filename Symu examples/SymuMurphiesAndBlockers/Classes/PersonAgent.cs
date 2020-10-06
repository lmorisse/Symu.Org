﻿#region Licence

// Description: SymuBiz - SymuMurphiesAndBlockers
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Symu.Classes.Agents;
using Symu.Classes.Agents.Models.CognitiveTemplates;
using Symu.Classes.Blockers;
using Symu.Classes.Task;
using Symu.Common;
using Symu.Common.Classes;
using Symu.Common.Interfaces;
using Symu.DNA.Entities;
using Symu.Environment;
using Symu.Messaging.Messages;
using Symu.Repository;

#endregion

namespace SymuMurphiesAndBlockers.Classes
{
    public sealed class PersonAgent : CognitiveAgent
    {
        public const byte Class = SymuYellowPages.Actor;
        public static IClassId ClassId => new ClassId(Class);
        private ExampleOrganization Organization => ((ExampleEnvironment)Environment).ExampleOrganization;
        /// <summary>
        /// Factory method to create an agent
        /// Call the Initialize method
        /// </summary>
        /// <returns></returns>
        public static PersonAgent CreateInstance(SymuEnvironment environment, CognitiveArchitectureTemplate template)
        {
            if (environment == null)
            {
                throw new ArgumentNullException(nameof(environment));
            }

            var agent = new PersonAgent(environment, template);
            agent.Initialize();
            return agent;
        }

        /// <summary>
        /// Constructor of the agent
        /// </summary>
        /// <remarks>Call the Initialize method after the constructor, or call the factory method</remarks>
        private PersonAgent(SymuEnvironment environment, CognitiveArchitectureTemplate template) : base(
            ClassId, environment, template)
        {
        }

        public IAgentId GroupId { get; set; }

        private MurphyTask Model => ((ExampleEnvironment) Environment).Model;
        public InternetAccessAgent Internet => ((ExampleEnvironment) Environment).Internet;

        /// <summary>
        ///     Customize the cognitive architecture of the agent
        ///     After setting the Agent template
        /// </summary>
        protected override void SetCognitive()
        {
            base.SetCognitive();
            Cognitive.KnowledgeAndBeliefs.HasKnowledge = true;
            Cognitive.KnowledgeAndBeliefs.HasInitialKnowledge = true;
            Cognitive.KnowledgeAndBeliefs.HasBelief = true;
            Cognitive.KnowledgeAndBeliefs.HasInitialBelief = true;
            Cognitive.TasksAndPerformance.CanPerformTask = true;
            Cognitive.TasksAndPerformance.CanPerformTaskOnWeekEnds = true;
            Cognitive.TasksAndPerformance.TasksLimit.LimitSimultaneousTasks = true;
            Cognitive.TasksAndPerformance.TasksLimit.MaximumSimultaneousTasks = 1;
            Cognitive.InteractionPatterns.IsolationCyclicity = Cyclicity.None;
            Cognitive.InteractionPatterns.AgentCanBeIsolated = Frequency.Never;
            Cognitive.InteractionPatterns.AllowNewInteractions = false;
        }

        /// <summary>
        ///     Customize the models of the agent
        ///     After setting the Agent basics models
        /// </summary>
        public override void SetModels()
        {
            base.SetModels();
            foreach (var knowledgeId in Environment.Organization.MetaNetwork.Knowledge.GetEntityIds())
            {
                KnowledgeModel.AddKnowledge(knowledgeId, Organization.KnowledgeLevel,
                    Cognitive.InternalCharacteristics);
                BeliefsModel.AddBeliefFromKnowledgeId(knowledgeId, Cognitive.KnowledgeAndBeliefs.DefaultBeliefLevel);
            }
        }

        public override void GetNewTasks()
        {
            var task = new SymuTask(Schedule.Step)
            {
                Weight = 1,
                // Creator is randomly  a person of the group - for the incomplete information murphy
                Creator = (AgentId)Environment.WhitePages.FilteredAgentIdsByClassId(ClassId).Shuffle().First()
            };
            task.SetKnowledgesBits(Model, Environment.Organization.MetaNetwork.Knowledge.GetEntities<IKnowledge>(), 1);
            Post(task);
        }

        public override void TryRecoverBlockerIncompleteKnowledgeExternally(SymuTask task, Blocker blocker,
            IAgentId knowledgeId,
            byte knowledgeBit)
        {
            if (blocker == null)
            {
                throw new ArgumentNullException(nameof(blocker));
            }

            var attachments = new MessageAttachments();
            attachments.Add(blocker);
            attachments.Add(task);
            attachments.KnowledgeId = knowledgeId;
            attachments.KnowledgeBit = knowledgeBit;
            Send(Internet.AgentId, MessageAction.Ask, SymuYellowPages.Help, attachments,
                CommunicationMediums.ViaAPlatform);
        }
    }
}