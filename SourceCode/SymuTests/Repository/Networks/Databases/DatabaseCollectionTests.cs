﻿#region Licence

// Description: SymuBiz - SymuTests
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Symu.Classes.Agents;
using Symu.Classes.Agents.Models.CognitiveModels;
using Symu.Classes.Agents.Models.CognitiveTemplates;
using Symu.Classes.Organization;
using Symu.Repository.Networks;
using Symu.Repository.Networks.Databases;

#endregion

namespace SymuTests.Repository.Networks.Databases
{
    [TestClass]
    public class DatabaseCollectionTests
    {
        private readonly DatabaseCollection _databases =
            new DatabaseCollection();

        private Database _database;

        [TestInitialize]
        public void Initialize()
        {
            var agentId = new AgentId(1, 1);
            var models = new OrganizationModels();
            var network = new MetaNetwork(models.InteractionSphere, models.ImpactOfBeliefOnTask);
            var cognitive = new CognitiveArchitecture();
            var databaseEntity = new DataBaseEntity(agentId, cognitive);
            _database = new Database(databaseEntity, models, network.Knowledge);
        }

        [TestMethod]
        public void AddTest()
        {
            Assert.IsFalse(_databases.Exists(_database.Entity.AgentId.Id));
            _databases.Add(_database);
            Assert.IsTrue(_databases.Exists(_database.Entity.AgentId.Id));
            // Duplicate
            _databases.Add(_database);
            Assert.AreEqual(1, _databases.List.Count);
        }

        [TestMethod]
        public void ContainsTest()
        {
            Assert.IsFalse(_databases.Contains(_database));
            _databases.Add(_database);
            Assert.IsTrue(_databases.Contains(_database));
        }

        [TestMethod]
        public void GetDatabaseTest()
        {
            Assert.IsNull(_databases.GetDatabase(_database.Entity.AgentId.Id));
            _databases.Add(_database);
            Assert.IsNotNull(_databases.GetDatabase(_database.Entity.AgentId.Id));
            Assert.AreEqual(_database, _databases.GetDatabase(_database.Entity.AgentId.Id));
        }

        [TestMethod]
        public void ClearTest()
        {
            _databases.Add(_database);
            _databases.Clear();
            Assert.IsFalse(_databases.Contains(_database));
        }
    }
}