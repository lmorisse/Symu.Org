﻿#region Licence

// Description: Symu - SymuEngine
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using SymuEngine.Classes.Agents.Models.CognitiveModel;

#endregion

namespace SymuEngine.Classes.Agents.Models.Templates
{
    /// <summary>
    ///     Set all the CognitiveArchitecture parameters for the AgentTemplate
    /// </summary>
    public class CognitiveArchitectureTemplate
    {
        public CognitiveArchitecture Cognitive { get; set; } =
            new CognitiveArchitecture();

        public void Set(CognitiveArchitecture cognitive)
        {
            if (cognitive is null)
            {
                throw new ArgumentNullException(nameof(cognitive));
            }

            Cognitive.CopyTo(cognitive);
        }
    }
}