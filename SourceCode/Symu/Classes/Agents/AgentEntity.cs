﻿#region Licence

// Description: SymuBiz - Symu
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;

#endregion

namespace Symu.Classes.Agents
{
    /// <summary>
    ///     class for Entity class of the Agent
    /// </summary>
    public class AgentEntity
    {
        public AgentEntity()
        {
        }

        public AgentEntity(ushort key, byte classKey)
        {
            AgentId = new AgentId(key, classKey);
        }

        public AgentEntity(ushort key, byte classKey, string name) : this(key, classKey)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Name = name;
        }

        public AgentEntity(ushort key, byte classKey, string name, AgentId parent) : this(key, classKey, name)
        {
            Parent = parent;
        }

        /// <summary>
        ///     The Id of the agent. Each entity must have a unique Id
        ///     FIPA Norm : AID
        /// </summary>
        public AgentId AgentId { get; set; }

        public string Name { get; set; }
        public AgentId Parent { get; set; }

        public void CopyTo(AgentEntity entity)
        {
            if (entity is null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            entity.AgentId = AgentId;
            entity.Name = Name;
            entity.Parent = Parent;
        }

        public override bool Equals(object obj)
        {
            return obj is AgentEntity entity &&
                   AgentId.Equals(entity.AgentId);
        }

        protected bool Equals(AgentEntity other)
        {
            return other != null && AgentId.Equals(other.AgentId);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}