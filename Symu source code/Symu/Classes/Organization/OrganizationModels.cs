﻿#region Licence

// Description: Symu - Symu
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using Symu.Classes.Agents.Models;
using Symu.Common;
using Symu.Engine;
using Symu.Repository.Networks.Beliefs;

#endregion

namespace Symu.Classes.Organization
{
    /// <summary>
    ///     List of the models used by the organizationEntity
    /// </summary>
    public class OrganizationModels
    {
        /// <summary>
        ///     If set, the organizationEntity flexibility performance will be followed and stored during the symu
        /// </summary>
        //TODO should be with IterationResult as Results Settings with cadence of feeds
        public bool FollowGroupFlexibility { get; set; }

        /// <summary>
        ///     If set, the organizationEntity knowledge and belief performance will be followed and stored during the symu
        /// </summary>
        //TODO should be with IterationResult as Results Settings with cadence of feeds
        public bool FollowGroupKnowledge { get; set; }

        /// <summary>
        ///     Agent knowledge learning model
        /// </summary>
        public ModelEntity Learning { get; set; } = new ModelEntity();

        /// <summary>
        ///     Agent knowledge forgetting model
        /// </summary>
        public ModelEntity Forgetting { get; set; } = new ModelEntity();

        /// <summary>
        ///     Agent influence model
        /// </summary>
        public ModelEntity Influence { get; set; } = new ModelEntity();

        /// <summary>
        ///     Agent influence model
        /// </summary>
        public ModelEntity Beliefs { get; set; } = new ModelEntity();

        /// <summary>
        ///     Agent knowledge model
        /// </summary>
        public ModelEntity Knowledge { get; set; } = new ModelEntity();

        /// <summary>
        ///     Impact level of agent's belief on how agent will accept to do the task
        /// </summary>
        public BeliefWeightLevel ImpactOfBeliefOnTask { get; set; } = BeliefWeightLevel.RandomWeight;

        public InteractionSphereModel InteractionSphere { get; set; } = new InteractionSphereModel();

        /// <summary>
        ///     Random generator modes in order to create random network
        /// </summary>
        public RandomGenerator Generator { get; set; } = RandomGenerator.RandomUniform;

        /// <summary>
        ///     Define level of random for the symu.
        /// </summary>
        public RandomLevel RandomLevel { get; set; } = RandomLevel.NoRandom;

        public byte RandomLevelValue => (byte) RandomLevel;

        /// <summary>
        ///     Define the ratio of task splitting in intraday mode
        /// </summary>
        public float Intraday { get; set; } = 0.01F;

        public void SetRandomLevel(int level)
        {
            switch (level)
            {
                case 1:
                    RandomLevel = RandomLevel.Simple;
                    break;
                case 2:
                    RandomLevel = RandomLevel.Double;
                    break;
                case 3:
                    RandomLevel = RandomLevel.Triple;
                    break;
                default:
                    RandomLevel = RandomLevel.NoRandom;
                    break;
            }
        }

        public void CopyTo(OrganizationModels entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            Learning.CopyTo(entity.Learning);
            Forgetting.CopyTo(entity.Forgetting);
            Influence.CopyTo(entity.Influence);
            Beliefs.CopyTo(entity.Beliefs);
            InteractionSphere.CopyTo(entity.InteractionSphere);
            entity.FollowGroupFlexibility = FollowGroupFlexibility;
            entity.FollowGroupKnowledge = FollowGroupKnowledge;
            entity.Generator = Generator;
            entity.ImpactOfBeliefOnTask = ImpactOfBeliefOnTask;
        }
        /// <summary>
        ///     Set all models on
        /// </summary>
        /// <param name="rate"></param>
        public void On(float rate)
        {
            Learning.On = true;
            Learning.RateOfAgentsOn = rate;
            Forgetting.On = true;
            Forgetting.RateOfAgentsOn = rate;
            Influence.On = true;
            Influence.RateOfAgentsOn = rate;
            Beliefs.On = true;
            Beliefs.RateOfAgentsOn = rate;
            InteractionSphere.On = true;
            InteractionSphere.RateOfAgentsOn = rate;
        }

        /// <summary>
        ///     Set all models off
        /// </summary>
        public void Off()
        {
            Learning.On = false;
            Forgetting.On = false;
            Influence.On = false;
            Beliefs.On = false;
            InteractionSphere.On = false;
        }
    }
}