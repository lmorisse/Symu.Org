﻿#region Licence

// Description: SymuBiz - SymuScenariosAndEvents
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using Symu.Classes.Organization;
using Symu.Classes.Task;
using Symu.Common.Classes;
using Symu.Common.Interfaces;
using Symu.DNA.Edges;
using Symu.DNA.Entities;
using Symu.Environment;
using Symu.Repository.Entities;

#endregion

namespace SymuScenariosAndEvents.Classes
{
    public class ExampleEnvironment : SymuEnvironment
    {
        private IAgentId _groupId;

        public MurphyTask Model => Organization.Murphies.IncompleteKnowledge;
        public ExampleOrganization ExampleOrganization => (ExampleOrganization) Organization;

        public ExampleEnvironment()
        {

            IterationResult.Blockers.On = true;
            IterationResult.Tasks.On = true;

            SetDebug(false);
            SetTimeStepType(TimeStepType.Daily);
        }

        public override void SetAgents()
        {
            base.SetAgents();

            var group = GroupAgent.CreateInstance(this);
            _groupId = group.AgentId;
            for (var j = 0; j < ExampleOrganization.WorkersCount; j++)
            {
                AddPersonAgent();
            }
        }

        private PersonAgent AddPersonAgent()
        {
            var actor = PersonAgent.CreateInstance(this, ExampleOrganization.Templates.Human);
            actor.GroupId = _groupId;
            var email = EmailEntity.CreateInstance(ExampleOrganization.MetaNetwork, Organization.Models);
            var actorResource = new ActorResource(actor.AgentId,email.EntityId, new ResourceUsage(0));
            ExampleOrganization.MetaNetwork.ActorResource.Add(actorResource);
            var actorOrganization = new ActorOrganization(actor.AgentId, _groupId);
            ExampleOrganization.MetaNetwork.ActorOrganization.Add(actorOrganization);
            return actor;
        }

        #region events

        public void PersonEvent(object sender, EventArgs e)
        {
            var actor = AddPersonAgent();
            actor.Start();
        }

        public void KnowledgeEvent(object sender, EventArgs e)
        {
            // knowledge length of 10 is arbitrary in this example
            var knowledge = new Knowledge(ExampleOrganization.MetaNetwork, ExampleOrganization.Models, ExampleOrganization.KnowledgeCount.ToString(), 10);

            foreach (var person in WhitePages.FilteredCognitiveAgentsByClassId(PersonAgent.ClassId))
            {
                person.KnowledgeModel.AddKnowledge(knowledge.EntityId, KnowledgeLevel.BasicKnowledge, 0.15F, -1);
                person.KnowledgeModel.InitializeKnowledge(knowledge.EntityId, Schedule.Step);
            }
        }

        #endregion
    }
}