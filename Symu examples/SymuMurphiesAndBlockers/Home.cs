﻿#region Licence

// Description: Symu - SymuMurphiesAndBlockers
// Website: https://symu.org
// Copyright: (c) 2020 laurent morisseau
// License : the program is distributed under the terms of the GNU General Public License

#endregion

#region using directives

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Symu.Classes.Scenario;
using Symu.Common;
using Symu.Environment;
using Symu.Forms;
using Symu.Messaging.Messages;
using Symu.Repository.Networks.Beliefs;
using Symu.Repository.Networks.Knowledges;
using Symu.Tools;
using SymuMurphiesAndBlockers.Classes;

#endregion

namespace SymuMurphiesAndBlockers
{
    public partial class Home : BaseForm
    {
        private readonly ExampleEnvironment _environment = new ExampleEnvironment();

        public Home()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            DisplayButtons();

            tbWorkers.Text = _environment.WorkersCount.ToString(CultureInfo.InvariantCulture);
            tbKnowledge.Text = _environment.KnowledgeCount.ToString(CultureInfo.InvariantCulture);
            cbMultipleBlockers.Checked = OrganizationEntity.Murphies.MultipleBlockers;
            KnowledgeLevel.Items.AddRange(KnowledgeLevelService.GetNames());
            KnowledgeLevel.SelectedItem = KnowledgeLevelService.GetName(_environment.KnowledgeLevel);
            BeliefsLevel.Items.AddRange(BeliefLevelService.GetNames());
            BeliefsLevel.SelectedItem = BeliefLevelService.GetName(OrganizationEntity.Templates.Human.Cognitive
                .KnowledgeAndBeliefs.DefaultBeliefLevel);
            EmailComm.Checked = true;

            #region unavaibility

            tbUnavailabilityThreshold.Text =
                OrganizationEntity.Murphies.UnAvailability.RateOfUnavailability.ToString();
            UnavailabilityRate.Text = OrganizationEntity.Murphies.UnAvailability.RateOfAgentsOn.ToString();

            #endregion

            #region incomplete knowledge murphy

            tbKnowledgeThreshHoldForDoing.Text =
                OrganizationEntity.Murphies.IncompleteKnowledge.ThresholdForReacting.ToString();
            tbLackRateOfIncorrectGuess.Text =
                OrganizationEntity.Murphies.IncompleteKnowledge.RateOfIncorrectGuess.ToString();
            tbLackRateOfAnswers.Text = OrganizationEntity.Murphies.IncompleteKnowledge.RateOfAnswers.ToString();
            tbLackResponseTime.Text = OrganizationEntity.Murphies.IncompleteKnowledge.ResponseTime.ToString();
            cbLimitNumberOfTriesKnowledge.Checked =
                OrganizationEntity.Murphies.IncompleteKnowledge.LimitNumberOfTries != -1;
            tbMaxNumberOfTriesKnowledge.Text =
                OrganizationEntity.Murphies.IncompleteKnowledge.LimitNumberOfTries.ToString();
            tbLackDelayBeforeSearchingExternally.Text = OrganizationEntity.Murphies.IncompleteKnowledge
                .DelayBeforeSearchingExternally.ToString();
            tbRequiredMandatoryRatio.Text = OrganizationEntity.Murphies.IncompleteKnowledge.MandatoryRatio.ToString();

            KnowledgeRate.Text = OrganizationEntity.Murphies.IncompleteKnowledge.RateOfAgentsOn.ToString();
            EmailSearching.Checked = OrganizationEntity.Models.Learning.On;

            #endregion

            #region incomplete belief murphy

            BeliefsRate.Text = OrganizationEntity.Murphies.IncompleteBelief.RateOfAgentsOn.ToString();
            tbBeliefRateIncorrectGuess.Text =
                OrganizationEntity.Murphies.IncompleteBelief.RateOfIncorrectGuess.ToString();
            tbBeliefRateAnswers.Text = OrganizationEntity.Murphies.IncompleteBelief.RateOfAnswers.ToString();
            tbBeliefResponseTime.Text = OrganizationEntity.Murphies.IncompleteBelief.ResponseTime.ToString();
            cbLimitNumberOfTriesBelief.Checked = OrganizationEntity.Murphies.IncompleteBelief.LimitNumberOfTries != -1;
            tbMaxNumberOfTriesBelief.Text = OrganizationEntity.Murphies.IncompleteBelief.LimitNumberOfTries.ToString();
            BeliefsRiskAversion.Text = OrganizationEntity.Murphies.IncompleteBelief.ThresholdForReacting.ToString();

            #endregion

            #region incomplete information murphy

            InformationRateAgentsOn.Text = OrganizationEntity.Murphies.IncompleteInformation.RateOfAgentsOn.ToString();
            InformationRateOfIncorrectGuess.Text =
                OrganizationEntity.Murphies.IncompleteInformation.RateOfIncorrectGuess.ToString();
            InformationRateOfAnswer.Text = OrganizationEntity.Murphies.IncompleteInformation.RateOfAnswers.ToString();
            InformationResponseTime.Text = OrganizationEntity.Murphies.IncompleteInformation.ResponseTime.ToString();
            InformationLimitOfTries.Checked =
                OrganizationEntity.Murphies.IncompleteInformation.LimitNumberOfTries != -1;
            InformationMaxOfTries.Text =
                OrganizationEntity.Murphies.IncompleteInformation.LimitNumberOfTries.ToString();
            InformationThreshold.Text =
                OrganizationEntity.Murphies.IncompleteInformation.ThresholdForReacting.ToString();

            #endregion

            foreach (ListViewItem item in lvMurphies.Items)
            {
                switch (item.Text)
                {
                    case "Incomplete information":
                        gbInformation.Visible = OrganizationEntity.Murphies.IncompleteInformation.On;
                        item.Checked = OrganizationEntity.Murphies.IncompleteInformation.On;
                        break;
                    case "Changing Information":
                        //item.Checked = murphies.ChangingInformation.On;
                        break;
                    case "Incorrect information":
                        //item.Checked = murphies.IncorrectInformation.On;
                        break;
                    case "Communication breakdowns":
                        //item.Checked = murphies.CommunicationBreakDown.On;
                        break;
                    case "Agent unavailability":
                        gbBelief.Visible = OrganizationEntity.Murphies.UnAvailability.On;
                        item.Checked = OrganizationEntity.Murphies.UnAvailability.On;
                        break;
                    case "Incomplete knowledge":
                        gbUncompleteKnowledge.Visible = OrganizationEntity.Murphies.IncompleteKnowledge.On;
                        item.Checked = OrganizationEntity.Murphies.IncompleteKnowledge.On;
                        break;
                    case "Incomplete belief":
                        gbBelief.Visible = OrganizationEntity.Murphies.IncompleteBelief.On;
                        item.Checked = OrganizationEntity.Murphies.IncompleteBelief.On;
                        break;
                }
            }
        }

        protected override void UpdateSettings()
        {
            if (!cbLimitNumberOfTriesBelief.Checked)
            {
                OrganizationEntity.Murphies.IncompleteBelief.LimitNumberOfTries = -1;
            }
            else
            {
                OrganizationEntity.Murphies.IncompleteBelief.LimitNumberOfTries = Convert.ToSByte(BeliefsRate.Text);
            }

            if (!cbLimitNumberOfTriesKnowledge.Checked)
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.LimitNumberOfTries = -1;
            }
            else
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.LimitNumberOfTries =
                    Convert.ToSByte(tbMaxNumberOfTriesKnowledge.Text);
            }

            if (!InformationLimitOfTries.Checked)
            {
                OrganizationEntity.Murphies.IncompleteInformation.LimitNumberOfTries = -1;
            }
            else
            {
                OrganizationEntity.Murphies.IncompleteInformation.LimitNumberOfTries =
                    Convert.ToSByte(InformationMaxOfTries.Text);
            }

            OrganizationEntity.Murphies.MultipleBlockers = cbMultipleBlockers.Checked;
            _environment.KnowledgeLevel =
                KnowledgeLevelService.GetValue(KnowledgeLevel.SelectedItem.ToString());
            OrganizationEntity.Templates.Human.Cognitive.KnowledgeAndBeliefs.DefaultBeliefLevel =
                BeliefLevelService.GetValue(BeliefsLevel.SelectedItem.ToString());

            OrganizationEntity.Models.Learning.On = EmailSearching.Checked;

            OrganizationEntity.Templates.Human.Cognitive.InteractionCharacteristics.PreferredCommunicationMediums = EmailComm.Checked ? CommunicationMediums.Email : CommunicationMediums.FaceToFace;

            SetTimeStepType(TimeStepType.Daily);
        }

        protected override void SetScenarii()
        {
            _ = new TimeBasedScenario(_environment)
            {
                NumberOfSteps = ushort.Parse(tbSteps.Text)
            };
        }

        protected override void OnStopped()
        {
            base.OnStopped();
            DisplayButtons();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            Start(_environment);
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            Cancel();
        }

        public override void Display()
        {
            DisplayButtons();
            WriteTextSafe(TimeStep, _environment.Schedule.Step.ToString());
            UpdateAgents();
        }

        private void UpdateAgents()
        {
            var capacityRatio = _environment.Schedule.Step * _environment.WorkersCount < Constants.Tolerance
                ? 0
                : _environment.IterationResult.Capacity * 100 /
                  (_environment.Schedule.Step * _environment.WorkersCount);
            WriteTextSafe(Capacity, capacityRatio
                .ToString("F1", CultureInfo.InvariantCulture));

            var tasksDoneRatio = _environment.Schedule.Step * _environment.WorkersCount < Constants.Tolerance
                ? 0
                : _environment.IterationResult.Tasks.Done * 100 /
                  (_environment.Schedule.Step * _environment.WorkersCount);

            WriteTextSafe(TasksDone, tasksDoneRatio
                .ToString("F1", CultureInfo.InvariantCulture));
            WriteTextSafe(Incorrectness,
                _environment.IterationResult.Tasks.Incorrectness
                    .ToString("F0", CultureInfo.InvariantCulture));

            WriteTextSafe(BlockersInDone,
                _environment.IterationResult.Blockers.TotalBlockersDone
                    .ToString("F0", CultureInfo.InvariantCulture));

            WriteTextSafe(BlockersInProgress,
                _environment.IterationResult.Blockers.BlockersStillInProgress
                    .ToString("F0", CultureInfo.InvariantCulture));

            var totalExternalHelp = _environment.IterationResult.Blockers.TotalBlockersDone < Constants.Tolerance
                ? 0
                : _environment.IterationResult.Blockers.TotalExternalHelp * 100 /
                  _environment.IterationResult.Blockers.TotalBlockersDone;

            WriteTextSafe(BlockersExternal, totalExternalHelp
                .ToString("F1", CultureInfo.InvariantCulture));

            var totalInternalHelp = _environment.IterationResult.Blockers.TotalBlockersDone < Constants.Tolerance
                ? 0
                : _environment.IterationResult.Blockers.TotalInternalHelp * 100 /
                  _environment.IterationResult.Blockers.TotalBlockersDone;
            WriteTextSafe(BlockersInternal, totalInternalHelp
                .ToString("F1", CultureInfo.InvariantCulture));
            var totalGuesses = _environment.IterationResult.Blockers.TotalBlockersDone < Constants.Tolerance
                ? 0
                : _environment.IterationResult.Blockers.TotalGuesses * 100 /
                  _environment.IterationResult.Blockers.TotalBlockersDone;
            WriteTextSafe(BlockersGuessing, totalGuesses
                .ToString("F1", CultureInfo.InvariantCulture));
            var totalSearches = _environment.IterationResult.Blockers.TotalBlockersDone < Constants.Tolerance
                ? 0
                : _environment.IterationResult.Blockers.TotalSearches * 100 /
                  _environment.IterationResult.Blockers.TotalBlockersDone;
            WriteTextSafe(BlockersSearching, totalSearches
                .ToString("F1", CultureInfo.InvariantCulture));
            WriteTextSafe(BlockersCancelled, _environment.IterationResult.Blockers.TotalCancelled
                .ToString("F0", CultureInfo.InvariantCulture));
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
            switch (State)
            {
                case AgentState.Stopped:
                case AgentState.NotStarted:
                    WriteButtonSafe(btnStart, true);
                    WriteButtonSafe(btnStop, false);
                    WriteButtonSafe(btnPause, false);
                    WriteButtonSafe(btnResume, false);
                    break;
                case AgentState.Stopping:
                case AgentState.Starting:
                    WriteButtonSafe(btnStart, false);
                    WriteButtonSafe(btnStop, false);
                    WriteButtonSafe(btnPause, false);
                    WriteButtonSafe(btnResume, false);
                    break;
                case AgentState.Started:
                    WriteButtonSafe(btnStart, false);
                    WriteButtonSafe(btnStop, true);
                    WriteButtonSafe(btnPause, true);
                    WriteButtonSafe(btnResume, false);
                    break;
                case AgentState.Paused:
                    WriteButtonSafe(btnStart, false);
                    WriteButtonSafe(btnStop, true);
                    WriteButtonSafe(btnPause, false);
                    WriteButtonSafe(btnResume, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected void WriteButtonSafe(Button button, bool enabled)
        {
            if (button is null)
            {
                throw new ArgumentNullException(nameof(button));
            }

            if (button.InvokeRequired)
            {
                var d = new SafeCallButtonDelegate(WriteButtonSafe);
                button.Invoke(d, button, enabled);
            }
            else
            {
                button.Enabled = enabled;
            }
        }

        private void tbWorkers_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _environment.WorkersCount = byte.Parse(tbWorkers.Text);
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

        private void tbKnowledge_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _environment.KnowledgeCount = byte.Parse(tbKnowledge.Text);
                tbKnowledge.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbKnowledge.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbKnowledge.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbKnowledgeThreshHoldForDoing_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.ThresholdForReacting =
                    float.Parse(tbKnowledgeThreshHoldForDoing.Text);
                tbKnowledgeThreshHoldForDoing.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbKnowledgeThreshHoldForDoing.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbKnowledgeThreshHoldForDoing.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbLackRateOfIncorrectGuess_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.RateOfIncorrectGuess =
                    float.Parse(tbLackRateOfIncorrectGuess.Text);
                tbLackRateOfIncorrectGuess.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbLackRateOfIncorrectGuess.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbLackRateOfIncorrectGuess.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbLackRateOfAnswers_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.RateOfAnswers = float.Parse(tbLackRateOfAnswers.Text);
                tbLackRateOfAnswers.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbLackRateOfAnswers.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbLackRateOfAnswers.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbLackResponseTime_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.ResponseTime = byte.Parse(tbLackResponseTime.Text);
                tbLackResponseTime.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbLackResponseTime.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbLackResponseTime.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbMaxNumberOfTriesKnowledge_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.LimitNumberOfTries =
                    sbyte.Parse(tbMaxNumberOfTriesKnowledge.Text);
                tbMaxNumberOfTriesKnowledge.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbMaxNumberOfTriesKnowledge.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbMaxNumberOfTriesKnowledge.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbLackDelayBeforeSearchingExternally_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.DelayBeforeSearchingExternally =
                    byte.Parse(tbLackDelayBeforeSearchingExternally.Text);
                tbLackDelayBeforeSearchingExternally.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbLackDelayBeforeSearchingExternally.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbLackDelayBeforeSearchingExternally.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbRequiredMandatoryRatio_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.MandatoryRatio =
                    float.Parse(tbRequiredMandatoryRatio.Text);
                tbRequiredMandatoryRatio.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbRequiredMandatoryRatio.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbRequiredMandatoryRatio.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbBeliefRateIncorrectGuess_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteBelief.RateOfIncorrectGuess =
                    float.Parse(tbBeliefRateIncorrectGuess.Text);
                tbBeliefRateIncorrectGuess.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbBeliefRateIncorrectGuess.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbBeliefRateIncorrectGuess.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbBeliefRateAnswers_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteBelief.RateOfAnswers = float.Parse(tbBeliefRateAnswers.Text);
                tbBeliefRateAnswers.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbBeliefRateAnswers.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbBeliefRateAnswers.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbBeliefResponseTime_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteBelief.ResponseTime = byte.Parse(tbBeliefResponseTime.Text);
                tbBeliefResponseTime.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbBeliefResponseTime.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbBeliefResponseTime.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbMaxNumberOfTriesBelief_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteBelief.LimitNumberOfTries =
                    sbyte.Parse(tbMaxNumberOfTriesBelief.Text);
                tbMaxNumberOfTriesBelief.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbMaxNumberOfTriesBelief.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbMaxNumberOfTriesBelief.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void tbUnavailabilityThreshold_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.UnAvailability.RateOfUnavailability =
                    float.Parse(tbUnavailabilityThreshold.Text);
                tbUnavailabilityThreshold.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                tbUnavailabilityThreshold.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                tbUnavailabilityThreshold.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void BeliefsRate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteBelief.RateOfAgentsOn = float.Parse(BeliefsRate.Text);
                BeliefsRate.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                BeliefsRate.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                BeliefsRate.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void UnavailabilityRate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.UnAvailability.RateOfAgentsOn = float.Parse(UnavailabilityRate.Text);
                UnavailabilityRate.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                UnavailabilityRate.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                UnavailabilityRate.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void KnowledgeRate_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteKnowledge.RateOfAgentsOn = float.Parse(KnowledgeRate.Text);
                KnowledgeRate.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                KnowledgeRate.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                KnowledgeRate.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void BeliefsRiskAversion_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteBelief.ThresholdForReacting =
                    float.Parse(BeliefsRiskAversion.Text);
                BeliefsRiskAversion.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                BeliefsRiskAversion.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                BeliefsRiskAversion.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void lvMurphies_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            switch (e.Item.Text)
            {
                case "Incomplete information":
                    OrganizationEntity.Murphies.IncompleteInformation.On = e.Item.Checked;
                    gbInformation.Visible = e.Item.Checked;
                    break;
                case "Changing Information":
                    break;
                case "Incorrect information":
                    break;
                case "Communication breakdowns":
                    break;
                case "Agent unavailability":
                    OrganizationEntity.Murphies.UnAvailability.On = e.Item.Checked;
                    gbUnavailabilities.Visible = e.Item.Checked;
                    break;
                case "Incomplete knowledge":
                    gbUncompleteKnowledge.Visible = e.Item.Checked;
                    OrganizationEntity.Murphies.IncompleteKnowledge.On = e.Item.Checked;
                    break;
                case "Incomplete belief":
                    gbBelief.Visible = e.Item.Checked;
                    OrganizationEntity.Murphies.IncompleteBelief.On = e.Item.Checked;
                    break;
            }
        }

        private void InformationRateAgentsOn_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteInformation.RateOfAgentsOn =
                    float.Parse(InformationRateAgentsOn.Text);
                InformationRateAgentsOn.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InformationRateAgentsOn.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InformationRateAgentsOn.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void InformationThreshold_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteInformation.ThresholdForReacting =
                    float.Parse(InformationThreshold.Text);
                InformationThreshold.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InformationThreshold.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InformationThreshold.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void InformationRateOfAnswer_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteInformation.RateOfAnswers =
                    float.Parse(InformationRateOfAnswer.Text);
                InformationRateOfAnswer.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InformationRateOfAnswer.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InformationRateOfAnswer.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void InformationResponseTime_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteInformation.ResponseTime =
                    byte.Parse(InformationResponseTime.Text);
                InformationResponseTime.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InformationResponseTime.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InformationResponseTime.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void InformationMaxOfTries_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteInformation.LimitNumberOfTries =
                    sbyte.Parse(InformationMaxOfTries.Text);
                InformationMaxOfTries.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InformationMaxOfTries.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InformationMaxOfTries.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void InformationRateOfIncorrectGuess_TextChanged(object sender, EventArgs e)
        {
            try
            {
                OrganizationEntity.Murphies.IncompleteInformation.RateOfIncorrectGuess =
                    float.Parse(InformationRateOfIncorrectGuess.Text);
                InformationRateOfIncorrectGuess.BackColor = SystemColors.Window;
            }
            catch (FormatException)
            {
                InformationRateOfIncorrectGuess.BackColor = Color.Red;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                InformationRateOfIncorrectGuess.BackColor = Color.Red;
                MessageBox.Show(exception.Message);
            }
        }

        private void cbLimitNumberOfTriesKnowledge_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbLimitNumberOfTriesKnowledge.Checked)
            {
                tbMaxNumberOfTriesKnowledge.Text = "-1";
                tbMaxNumberOfTriesKnowledge.Enabled = false;
            }
            else
            {
                tbMaxNumberOfTriesKnowledge.Text = "1";
                tbMaxNumberOfTriesKnowledge.Enabled = true;
            }
        }

        private void cbLimitNumberOfTriesBelief_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbLimitNumberOfTriesBelief.Checked)
            {
                tbMaxNumberOfTriesBelief.Text = "-1";
                tbMaxNumberOfTriesBelief.Enabled = false;
            }
            else
            {
                tbMaxNumberOfTriesBelief.Text = "1";
                tbMaxNumberOfTriesBelief.Enabled = true;
            }
        }

        private void InformationLimitOfTries_CheckedChanged(object sender, EventArgs e)
        {
            if (!InformationLimitOfTries.Checked)
            {
                InformationMaxOfTries.Text = "-1";
                InformationMaxOfTries.Enabled = false;
            }
            else
            {
                InformationMaxOfTries.Text = "1";
                InformationMaxOfTries.Enabled = true;
            }
        }

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

        #region Nested type: SafeCallButtonDelegate

        protected delegate void SafeCallButtonDelegate(Button button, bool enabled);

        #endregion
    }
}