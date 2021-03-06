﻿#region Licence

// Description: SymuBiz - SymuGroupAndInteraction
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using Symu.Classes.Scenario;
using Symu.Forms;
using Symu.OrgMod.GraphNetworks.TwoModesNetworks.Sphere;
using Symu.Repository.Entities;

#endregion

namespace SymuExamples.GroupAndInteraction
{
    public partial class Home : SymuForm
    {
        private readonly ExampleEnvironment _environment = new ExampleEnvironment();
        private readonly ExampleMainOrganization _mainOrganization = new ExampleMainOrganization();

        public Home()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            DisplayButtons();

            #region Environment

            tbWorkers.Text = _mainOrganization.WorkersCount.ToString(CultureInfo.InvariantCulture);
            GroupsCount.Text = _mainOrganization.GroupsCount.ToString(CultureInfo.InvariantCulture);

            #endregion

            #region sphere of interaction

            RadomlyGenerated.Checked = true;
            AllowNewInteractions.Checked = true;
            MaxSphereDensity.Text =
                _mainOrganization.Models.InteractionSphere.MaxSphereDensity.ToString(CultureInfo.InvariantCulture);
            MinSphereDensity.Text =
                _mainOrganization.Models.InteractionSphere.MinSphereDensity.ToString(CultureInfo.InvariantCulture);
            WeightKnowledge.Text =
                _mainOrganization.Models.InteractionSphere.RelativeKnowledgeWeight.ToString(CultureInfo
                    .InvariantCulture);
            WeightActivities.Text =
                _mainOrganization.Models.InteractionSphere.RelativeActivityWeight.ToString(
                    CultureInfo.InvariantCulture);
            WeightBeliefs.Text =
                _mainOrganization.Models.InteractionSphere.RelativeBeliefWeight.ToString(CultureInfo.InvariantCulture);
            WeightGroups.Text =
                _mainOrganization.Models.InteractionSphere.SocialDemographicWeight.ToString(CultureInfo
                    .InvariantCulture);

            #endregion

            #region Interaction

            _mainOrganization.Models.InteractionSphere.SetInteractionPatterns(InteractionStrategy.Homophily);
            _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.LimitNumberOfNewInteractions = true;
            _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.ThresholdForNewInteraction = 0.1F;
            ThresholdFornewInteraction.Text = _mainOrganization.Templates.Human.Cognitive.InteractionPatterns
                .ThresholdForNewInteraction.ToString(CultureInfo.InvariantCulture);
            MaxDailyInteractions.Text = "2";

            Homophily.Checked = true;

            #endregion

            #region Knowledge

            KnowledgeRandom.Checked = true;
            cbInitialKnowledgeLevel.Items.AddRange(KnowledgeLevelService.GetNames());
            cbInitialKnowledgeLevel.SelectedItem = KnowledgeLevelService.GetName(_mainOrganization.KnowledgeLevel);

            #endregion

            #region Activities

            ActivitiesRandom.Checked = true;

            #endregion
        }

        protected override void SetUpOrganization()
        {
            base.SetUpOrganization();

            #region Knowledge

            if (KnowledgeSame.Checked)
            {
                _mainOrganization.Knowledge = 0;
            }

            if (KnowledgeByGroup.Checked)
            {
                _mainOrganization.Knowledge = 1;
            }

            if (KnowledgeRandom.Checked)
            {
                _mainOrganization.Knowledge = 2;
            }

            _mainOrganization.KnowledgeLevel =
                KnowledgeLevelService.GetValue(cbInitialKnowledgeLevel.SelectedItem.ToString());

            #endregion

            #region sphere of interaction

            _mainOrganization.Models.InteractionSphere.RandomlyGeneratedSphere = RadomlyGenerated.Checked;

            #endregion

            #region Activities

            if (ActivitiesSame.Checked)
            {
                _mainOrganization.Activities = 0;
            }

            if (ActivitiesByGroup.Checked)
            {
                _mainOrganization.Activities = 1;
            }

            if (ActivitiesRandom.Checked)
            {
                _mainOrganization.Activities = 2;
            }

            #endregion

            #region Interactions

            _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.AllowNewInteractions =
                AllowNewInteractions.Checked;

            #endregion

            _mainOrganization.AddKnowledge();
            _mainOrganization.AddTasks();
        }

        /// <summary>
        ///     Update scenarii settings via the form
        ///     Add scenarios after calling base.UpdateSettings
        /// </summary>
        protected override void SetUpScenarii()
        {
            base.SetUpScenarii();
            var scenario = TimeBasedScenario.CreateInstance(_environment);
            scenario.NumberOfSteps = ushort.Parse(tbSteps.Text, CultureInfo.InvariantCulture);
            AddScenario(scenario);
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            DisplayButtons();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Start(_environment, _mainOrganization);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        public override void DisplayStep()
        {
            DisplayButtons();
            WriteTextSafe(TimeStep, _environment.Schedule.Step.ToString(CultureInfo.InvariantCulture));
            UpDateMessages();
            UpdateAgents();
        }

        private void UpDateMessages()
        {
            WriteTextSafe(lblMessagesSent,
                _environment.Messages.Result.SentMessagesCount.ToString(CultureInfo.InvariantCulture));
            WriteTextSafe(NotAcceptedMessages,
                _environment.Messages.Result.NotAcceptedMessagesCount.ToString(CultureInfo.InvariantCulture));
        }

        private void UpdateAgents()
        {
            WriteTextSafe(Triads,
                _environment.IterationResult.OrganizationFlexibility.Triads.Last().Density
                    .ToString("F1", CultureInfo.InvariantCulture));
            WriteTextSafe(InteractionSphereCount,
                _environment.IterationResult.OrganizationFlexibility.Links.Last().Density
                    .ToString("F1", CultureInfo.InvariantCulture));
            WriteTextSafe(SphereDensity,
                _environment.IterationResult.OrganizationFlexibility.Sphere.Last().Density
                    .ToString("F1", CultureInfo.InvariantCulture));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Pause();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Resume();
        }

        private void DisplayButtons()
        {
            DisplayButtons(btnStart, btnStop, btnPause, btnResume);
        }

        private void tbWorkers_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.WorkersCount = byte.Parse(tbWorkers.Text, CultureInfo.InvariantCulture);
                tbWorkers.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbWorkers.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbWorkers.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void GroupsCount_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.GroupsCount = byte.Parse(GroupsCount.Text, CultureInfo.InvariantCulture);
                GroupsCount.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                GroupsCount.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                GroupsCount.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void Coworkers_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.InteractionsBasedOnActivities =
                    float.Parse(InteractionActivities.Text, CultureInfo.InvariantCulture);
                InteractionActivities.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InteractionActivities.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InteractionActivities.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void DeliberateSearch_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.InteractionsBasedOnKnowledge =
                    float.Parse(InteractionKnowledge.Text, CultureInfo.InvariantCulture);
                InteractionKnowledge.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InteractionKnowledge.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InteractionKnowledge.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void ThresholdForNewInteraction_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.ThresholdForNewInteraction =
                    float.Parse(ThresholdFornewInteraction.Text, CultureInfo.InvariantCulture);
                ThresholdFornewInteraction.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                ThresholdFornewInteraction.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                ThresholdFornewInteraction.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void DailyInteractions_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.MaxNumberOfNewInteractions =
                    byte.Parse(MaxDailyInteractions.Text, CultureInfo.InvariantCulture);
                MaxDailyInteractions.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                MaxDailyInteractions.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                MaxDailyInteractions.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void InteractionSocialDemographics_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.InteractionsBasedOnSocialDemographics =
                    float.Parse(InteractionSocialDemographics.Text, CultureInfo.InvariantCulture);
                InteractionSocialDemographics.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InteractionSocialDemographics.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InteractionSocialDemographics.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void InteractionBeliefs_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.InteractionsBasedOnBeliefs =
                    float.Parse(InteractionBeliefs.Text, CultureInfo.InvariantCulture);
                InteractionBeliefs.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InteractionBeliefs.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InteractionBeliefs.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void MinSphereDensity_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Models.InteractionSphere.MinSphereDensity =
                    float.Parse(MinSphereDensity.Text, CultureInfo.InvariantCulture);
                MinSphereDensity.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                MinSphereDensity.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                MinSphereDensity.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void MaxSphereDensity_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Models.InteractionSphere.MaxSphereDensity =
                    float.Parse(MaxSphereDensity.Text, CultureInfo.InvariantCulture);
                MaxSphereDensity.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                MaxSphereDensity.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                MaxSphereDensity.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Models.InteractionSphere.RelativeKnowledgeWeight =
                    float.Parse(WeightKnowledge.Text, CultureInfo.InvariantCulture);
                WeightKnowledge.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                WeightKnowledge.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                WeightKnowledge.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void WeightActivities_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Models.InteractionSphere.RelativeActivityWeight =
                    float.Parse(WeightActivities.Text, CultureInfo.InvariantCulture);
                WeightActivities.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                WeightActivities.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                WeightActivities.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void WeightGroups_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Models.InteractionSphere.SocialDemographicWeight =
                    float.Parse(WeightGroups.Text, CultureInfo.InvariantCulture);
                WeightGroups.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                WeightGroups.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                WeightGroups.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void WeightBeliefs_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _mainOrganization.Models.InteractionSphere.RelativeBeliefWeight =
                    float.Parse(WeightBeliefs.Text, CultureInfo.InvariantCulture);
                WeightBeliefs.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                WeightBeliefs.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                WeightBeliefs.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void Homophily_CheckedChanged(object sender, EventArgs e)
        {
            if (!Homophily.Checked)
            {
                return;
            }

            _mainOrganization.Templates.Human.Cognitive.InteractionPatterns.SetInteractionPatterns(
                InteractionStrategy.Homophily);
            InteractionActivities.Text = "0";
            InteractionBeliefs.Text = "0";
            InteractionKnowledge.Text = "0";
            InteractionSocialDemographics.Text = "0";
        }

        #region Menu

        private void symuorgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://symu.org");
        }

        private void documentationToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("http://docs.symu.org/");
        }

        private void sourceCodeToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start("http://github.symu.org/");
        }

        private void issuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://github.symu.org/issues");
        }

        #endregion
    }
}