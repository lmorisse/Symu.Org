﻿#region Licence

// Description: SymuBiz - SymuTests
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using Symu.Classes.Agents;
using Symu.Environment;
using Symu.Repository;

#endregion

namespace SymuTests.Helpers
{
    /// <summary>
    ///     Class for tests
    /// </summary>
    internal sealed class TestCognitiveAgent : CognitiveAgent
    {
        public static byte ClassId = SymuYellowPages.Actor;

        public TestCognitiveAgent(ushort key, SymuEnvironment environment) : base(new AgentId(key, ClassId), environment,
            environment.Organization.Templates.Human)
        {
        }

        public TestCognitiveAgent(ushort key, byte classKey, SymuEnvironment environment) : base(new AgentId(key, classKey),
            environment, environment.Organization.Templates.Human)
        {
        }
    }
}