﻿#region Licence

// Description: SymuBiz - SymuTests
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Symu.Repository.Networks.Knowledges;

#endregion

namespace SymuTests.Repository.Networks.Knowledges
{
    [TestClass]
    public class AgentKnowledgeTests
    {
        private readonly AgentKnowledge _agentKnowledge = new AgentKnowledge(4, KnowledgeLevel.BasicKnowledge, 0, -1);
        private readonly float[] _knowledge01Bits = {0, 1};
        private readonly float[] _knowledge0Bits = {0, 0};
        private readonly float[] _knowledge1Bits = {1, 1};
        private AgentKnowledge _agentKnowledge0;
        private AgentKnowledge _agentKnowledge01;
        private AgentKnowledge _agentKnowledge1;

        [TestInitialize]
        public void Initialize()
        {
            _agentKnowledge0 = new AgentKnowledge(0, _knowledge0Bits, 0, -1, 0);
            _agentKnowledge1 = new AgentKnowledge(1, _knowledge1Bits, 0, -1, 0);
            _agentKnowledge01 = new AgentKnowledge(2, _knowledge01Bits, 0, -1, 0);
        }

        [TestMethod]
        public void GetKnowledgeSumTest()
        {
            // Non passing test knowledgeBits == null
            Assert.AreEqual(0, _agentKnowledge.GetKnowledgeSum());
            // Passing tests
            Assert.AreEqual(0, _agentKnowledge0.GetKnowledgeSum());
            Assert.AreEqual(1, _agentKnowledge01.GetKnowledgeSum());
            Assert.AreEqual(2, _agentKnowledge1.GetKnowledgeSum());
        }

        [TestMethod]
        public void SizeTest()
        {
            // Non passing test knowledgeBits == null
            Assert.AreEqual(0, _agentKnowledge.Length);
            // Passing tests
            Assert.AreEqual(2, _agentKnowledge0.Length);
        }

        [TestMethod]
        public void GetKnowledgeBitsTest()
        {
            Assert.AreEqual(_knowledge0Bits[0], _agentKnowledge0.GetKnowledgeBit(0));
            Assert.AreEqual(_knowledge1Bits[0], _agentKnowledge1.GetKnowledgeBit(0));
            Assert.AreEqual(_knowledge01Bits[0], _agentKnowledge01.GetKnowledgeBit(0));
        }

        /// <summary>
        ///     Non passing test knowledgeBits == null
        /// </summary>
        [TestMethod]
        public void GetKnowledgeBitTest()
        {
            Assert.AreEqual(0, _agentKnowledge.GetKnowledgeBit(0));
        }

        /// <summary>
        ///     Passing test
        /// </summary>
        [TestMethod]
        public void GetKnowledgeBitTest1()
        {
            for (byte i = 0; i < 2; i++)
            {
                Assert.AreEqual(_knowledge0Bits[i], _agentKnowledge0.GetKnowledgeBit(i));
                Assert.AreEqual(_knowledge1Bits[i], _agentKnowledge1.GetKnowledgeBit(i));
                Assert.AreEqual(_knowledge01Bits[i], _agentKnowledge01.GetKnowledgeBit(i));
            }
        }

        [TestMethod]
        public void SetKnowledgeBitsTest()
        {
            _agentKnowledge.SetKnowledgeBits(_knowledge1Bits, 0);
            for (byte i = 0; i < 2; i++)
            {
                Assert.AreEqual(_knowledge1Bits[i], _agentKnowledge.GetKnowledgeBit(i));
            }
        }

        [TestMethod]
        public void SetKnowledgeBitTest()
        {
            for (byte i = 0; i < 2; i++)
            {
                _agentKnowledge0.SetKnowledgeBit(i, _knowledge1Bits[i], 0);
                Assert.AreEqual(_knowledge1Bits[i], _agentKnowledge0.GetKnowledgeBit(i));
            }
        }

        [TestMethod]
        public void GetKnowledgeBitsTest1()
        {
            var knowledgeBits = _agentKnowledge1.CloneWrittenKnowledgeBits(1.1F);
            Assert.AreEqual(0, knowledgeBits.GetBit(0));
            Assert.AreEqual(0, knowledgeBits.GetBit(1));
            knowledgeBits = _agentKnowledge1.CloneWrittenKnowledgeBits(0);
            Assert.AreEqual(1, knowledgeBits.GetBit(0));
            Assert.AreEqual(1, knowledgeBits.GetBit(1));
        }

        [TestMethod]
        public void CloneTest()
        {
            var clone = _agentKnowledge1.CloneBits();
            Assert.IsNotNull(clone);
            Assert.AreNotEqual(_agentKnowledge1.KnowledgeBits, clone);
            Assert.AreEqual(_agentKnowledge1.KnowledgeBits.GetBit(0), clone.GetBit(0));
            Assert.AreEqual(_agentKnowledge1.KnowledgeBits.GetBit(1), clone.GetBit(1));
        }
    }
}