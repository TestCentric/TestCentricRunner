// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Security.Principal;
using System.Windows.Forms;
using NUnit.Common;

namespace TestCentric.Gui.SettingsPages
{
    using NUnit.Engine;

    public class AdvancedLoaderSettingsPage : SettingsPage
    {
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox disableShadowCopyCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private CheckBox principalPolicyCheckBox;
        private Label label7;
        private Label label6;
        private GroupBox groupBox1;
        private ListBox principalPolicyListBox;
        private Label label1;
        private Label label4;
        private GroupBox groupBox2;
        private CheckBox numberOfAgentsCheckBox;
        private NumericUpDown numberOfAgentsUpDown;
        private System.ComponentModel.IContainer components = null;

        public AdvancedLoaderSettingsPage(string key) : base(key)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdvancedLoaderSettingsPage));
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.disableShadowCopyCheckBox = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.principalPolicyCheckBox = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.principalPolicyListBox = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.numberOfAgentsCheckBox = new System.Windows.Forms.CheckBox();
            this.numberOfAgentsUpDown = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.numberOfAgentsUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 70);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Shadow Copy";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Location = new System.Drawing.Point(139, 70);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(309, 8);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            // 
            // disableShadowCopyCheckBox
            // 
            this.disableShadowCopyCheckBox.AutoSize = true;
            this.helpProvider1.SetHelpString(this.disableShadowCopyCheckBox, resources.GetString("disableShadowCopyCheckBox.HelpString"));
            this.disableShadowCopyCheckBox.Location = new System.Drawing.Point(24, 94);
            this.disableShadowCopyCheckBox.Name = "disableShadowCopyCheckBox";
            this.helpProvider1.SetShowHelp(this.disableShadowCopyCheckBox, true);
            this.disableShadowCopyCheckBox.Size = new System.Drawing.Size(130, 17);
            this.disableShadowCopyCheckBox.TabIndex = 2;
            this.disableShadowCopyCheckBox.Text = "Disable Shadow Copy";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(139, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(260, 59);
            this.label2.TabIndex = 6;
            this.label2.Text = "Shadow copy should normally be enabled. If it is disabled, the TestCentric Gui ma" +
    "y not function correctly.";
            // 
            // principalPolicyCheckBox
            // 
            this.principalPolicyCheckBox.AutoSize = true;
            this.principalPolicyCheckBox.Location = new System.Drawing.Point(24, 207);
            this.principalPolicyCheckBox.Name = "principalPolicyCheckBox";
            this.principalPolicyCheckBox.Size = new System.Drawing.Size(214, 17);
            this.principalPolicyCheckBox.TabIndex = 9;
            this.principalPolicyCheckBox.Text = "Set Principal Policy for test AppDomains";
            this.principalPolicyCheckBox.UseVisualStyleBackColor = true;
            this.principalPolicyCheckBox.CheckedChanged += new System.EventHandler(this.principalPolicyCheckBox_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(42, 233);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 13);
            this.label7.TabIndex = 10;
            this.label7.Text = "Policy:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 179);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Principal Policy";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(139, 184);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(309, 8);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // principalPolicyListBox
            // 
            this.principalPolicyListBox.FormattingEnabled = true;
            this.principalPolicyListBox.Items.AddRange(new object[] {
            "UnauthenticatedPrincipal",
            "NoPrincipal",
            "WindowsPrincipal"});
            this.principalPolicyListBox.Location = new System.Drawing.Point(139, 233);
            this.principalPolicyListBox.Name = "principalPolicyListBox";
            this.principalPolicyListBox.Size = new System.Drawing.Size(241, 69);
            this.principalPolicyListBox.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(42, 123);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 13);
            this.label1.TabIndex = 12;
            this.label1.Text = "Warning:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Agent Limit";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Location = new System.Drawing.Point(139, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(309, 8);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            // 
            // numberOfAgentsCheckBox
            // 
            this.numberOfAgentsCheckBox.AutoSize = true;
            this.numberOfAgentsCheckBox.Location = new System.Drawing.Point(24, 31);
            this.numberOfAgentsCheckBox.Name = "numberOfAgentsCheckBox";
            this.numberOfAgentsCheckBox.Size = new System.Drawing.Size(205, 17);
            this.numberOfAgentsCheckBox.TabIndex = 44;
            this.numberOfAgentsCheckBox.Text = "Limit simultaneous agent processes to";
            this.numberOfAgentsCheckBox.UseVisualStyleBackColor = true;
            this.numberOfAgentsCheckBox.CheckedChanged += new System.EventHandler(this.numberOfAgentsCheckBox_CheckedChanged);
            // 
            // numberOfAgentsUpDown
            // 
            this.numberOfAgentsUpDown.Enabled = false;
            this.numberOfAgentsUpDown.Location = new System.Drawing.Point(324, 28);
            this.numberOfAgentsUpDown.Name = "numberOfAgentsUpDown";
            this.numberOfAgentsUpDown.Size = new System.Drawing.Size(66, 20);
            this.numberOfAgentsUpDown.TabIndex = 43;
            // 
            // AdvancedLoaderSettingsPage
            // 
            this.Controls.Add(this.numberOfAgentsCheckBox);
            this.Controls.Add(this.numberOfAgentsUpDown);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.principalPolicyListBox);
            this.Controls.Add(this.principalPolicyCheckBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.disableShadowCopyCheckBox);
            this.Name = "AdvancedLoaderSettingsPage";
            ((System.ComponentModel.ISupportInitialize)(this.numberOfAgentsUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private PackageSettings PackageSettings => Model.TestCentricProject.Settings;

        public override void LoadSettings()
        {
            // Update UI elements based on current settings in TestCentricProject
            int agents = PackageSettings.GetValueOrDefault(SettingDefinitions.MaxAgents);
            numberOfAgentsCheckBox.Checked = agents > 0;
            numberOfAgentsUpDown.Value = agents;

            disableShadowCopyCheckBox.Checked = !PackageSettings.GetValueOrDefault(SettingDefinitions.ShadowCopyFiles); ;
            
            string principalPolicy = PackageSettings.GetValueOrDefault(SettingDefinitions.PrincipalPolicy);
            if (string.IsNullOrEmpty(principalPolicy))
                principalPolicy = nameof(PrincipalPolicy.UnauthenticatedPrincipal);

            principalPolicyCheckBox.Checked = principalPolicyListBox.Enabled =
                principalPolicy != nameof(PrincipalPolicy.UnauthenticatedPrincipal);
            principalPolicyListBox.SelectedItem = principalPolicy;
        }

        public override void ApplySettings()
        {
            // Check if current values in UI elements differ from those in TestCentricProject
            // If values differ, add them to SettingsChanges list, so they can be applied later
            int numAgents = numberOfAgentsCheckBox.Checked
                ? (int)numberOfAgentsUpDown.Value : 0;
            if (numAgents != PackageSettings.GetValueOrDefault(SettingDefinitions.MaxAgents))
                TopLevelPackageSettingChanges.Add(SettingDefinitions.MaxAgents.WithValue(numAgents));

            bool shadowCopyFiles = !disableShadowCopyCheckBox.Checked;
            if (shadowCopyFiles != PackageSettings.GetValueOrDefault(SettingDefinitions.ShadowCopyFiles))
                SubPackageSettingChanges.Add(SettingDefinitions.ShadowCopyFiles.WithValue(shadowCopyFiles));

            string principalPolicy = principalPolicyCheckBox.Checked
                ? (string)principalPolicyListBox.SelectedItem
                : nameof(PrincipalPolicy.UnauthenticatedPrincipal);
            if (principalPolicy != PackageSettings.GetValueOrDefault(SettingDefinitions.PrincipalPolicy))
                SubPackageSettingChanges.Add(SettingDefinitions.PrincipalPolicy.WithValue(principalPolicy));
        }

        private void numberOfAgentsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            numberOfAgentsUpDown.Enabled = numberOfAgentsCheckBox.Checked;
        }

        private void principalPolicyCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            principalPolicyListBox.Enabled = principalPolicyCheckBox.Checked;
            if (!principalPolicyCheckBox.Checked)
                principalPolicyListBox.SelectedItem = nameof(PrincipalPolicy.UnauthenticatedPrincipal);
        }
    }
}

