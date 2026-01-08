// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TestCentric.Gui.Presenters;

namespace TestCentric.Gui.SettingsPages
{
    public class TreeSettingsPage : SettingsPage
    {
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox showCheckBoxesCheckBox;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private Label label6;
        private PictureBox successImage;
        private PictureBox failureImage;
        private PictureBox warningImage;
        private PictureBox ignoredImage;
        private PictureBox inconclusiveImage;
        private PictureBox skippedImage;
        private System.ComponentModel.IContainer components = null;
        private Label label4;
        private ListBox imageSetListBox;
        private static readonly string[] imageExtensions = { ".png", ".jpg" };
        private ImageSetManager _imageSetManager;

        public TreeSettingsPage(string key, ImageSetManager imageSetManager) : base(key)
        {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            _imageSetManager = imageSetManager;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TreeSettingsPage));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.showCheckBoxesCheckBox = new System.Windows.Forms.CheckBox();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.label6 = new System.Windows.Forms.Label();
            this.successImage = new System.Windows.Forms.PictureBox();
            this.failureImage = new System.Windows.Forms.PictureBox();
            this.warningImage = new System.Windows.Forms.PictureBox();
            this.ignoredImage = new System.Windows.Forms.PictureBox();
            this.inconclusiveImage = new System.Windows.Forms.PictureBox();
            this.skippedImage = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.imageSetListBox = new System.Windows.Forms.ListBox();
            ((System.ComponentModel.ISupportInitialize)(this.successImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.failureImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ignoredImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inconclusiveImage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.skippedImage)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Location = new System.Drawing.Point(144, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(304, 8);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Tree View";
            // 
            // showCheckBoxesCheckBox
            // 
            this.showCheckBoxesCheckBox.AutoSize = true;
            this.helpProvider1.SetHelpString(this.showCheckBoxesCheckBox, "If checked, a checkbox is displayed next to each item in the tree.");
            this.showCheckBoxesCheckBox.Location = new System.Drawing.Point(32, 120);
            this.showCheckBoxesCheckBox.Name = "showCheckBoxesCheckBox";
            this.helpProvider1.SetShowHelp(this.showCheckBoxesCheckBox, true);
            this.showCheckBoxesCheckBox.Size = new System.Drawing.Size(227, 17);
            this.showCheckBoxesCheckBox.TabIndex = 36;
            this.showCheckBoxesCheckBox.Text = "Display a checkbox next to each tree item.";
            // 
            // label6
            // 
            this.label6.BackColor = System.Drawing.SystemColors.Window;
            this.label6.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label6.Location = new System.Drawing.Point(66, 45);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(164, 36);
            this.label6.TabIndex = 47;
            // 
            // successImage
            // 
            this.successImage.Image = ((System.Drawing.Image)(resources.GetObject("successImage.Image")));
            this.successImage.Location = new System.Drawing.Point(78, 56);
            this.successImage.Name = "successImage";
            this.successImage.Size = new System.Drawing.Size(16, 16);
            this.successImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.successImage.TabIndex = 48;
            this.successImage.TabStop = false;
            // 
            // failureImage
            // 
            this.failureImage.Image = ((System.Drawing.Image)(resources.GetObject("failureImage.Image")));
            this.failureImage.Location = new System.Drawing.Point(103, 56);
            this.failureImage.Name = "failureImage";
            this.failureImage.Size = new System.Drawing.Size(16, 16);
            this.failureImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.failureImage.TabIndex = 49;
            this.failureImage.TabStop = false;
            // 
            // warningImage
            // 
            this.warningImage.Image = ((System.Drawing.Image)(resources.GetObject("warningImage.Image")));
            this.warningImage.Location = new System.Drawing.Point(153, 56);
            this.warningImage.Name = "warningImage";
            this.warningImage.Size = new System.Drawing.Size(16, 16);
            this.warningImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.warningImage.TabIndex = 50;
            this.warningImage.TabStop = false;
            // 
            // ignoredImage
            // 
            this.ignoredImage.Image = ((System.Drawing.Image)(resources.GetObject("ignoredImage.Image")));
            this.ignoredImage.Location = new System.Drawing.Point(128, 56);
            this.ignoredImage.Name = "ignoredImage";
            this.ignoredImage.Size = new System.Drawing.Size(16, 16);
            this.ignoredImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.ignoredImage.TabIndex = 50;
            this.ignoredImage.TabStop = false;
            // 
            // inconclusiveImage
            // 
            this.inconclusiveImage.Enabled = false;
            this.inconclusiveImage.Image = ((System.Drawing.Image)(resources.GetObject("inconclusiveImage.Image")));
            this.inconclusiveImage.Location = new System.Drawing.Point(178, 56);
            this.inconclusiveImage.Name = "inconclusiveImage";
            this.inconclusiveImage.Size = new System.Drawing.Size(16, 16);
            this.inconclusiveImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.inconclusiveImage.TabIndex = 51;
            this.inconclusiveImage.TabStop = false;
            // 
            // skippedImage
            // 
            this.skippedImage.Image = ((System.Drawing.Image)(resources.GetObject("skippedImage.Image")));
            this.skippedImage.Location = new System.Drawing.Point(203, 56);
            this.skippedImage.Name = "skippedImage";
            this.skippedImage.Size = new System.Drawing.Size(16, 16);
            this.skippedImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.skippedImage.TabIndex = 52;
            this.skippedImage.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(32, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 53;
            this.label4.Text = "Tree Images";
            // 
            // imageSetListBox
            // 
            this.imageSetListBox.FormattingEnabled = true;
            this.imageSetListBox.Location = new System.Drawing.Point(270, 24);
            this.imageSetListBox.Name = "imageSetListBox";
            this.imageSetListBox.Size = new System.Drawing.Size(168, 56);
            this.imageSetListBox.TabIndex = 54;
            this.imageSetListBox.SelectedIndexChanged += new System.EventHandler(this.imageSetListBox_SelectedIndexChanged);
            // 
            // TreeSettingsPage
            // 
            this.Controls.Add(this.imageSetListBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.skippedImage);
            this.Controls.Add(this.inconclusiveImage);
            this.Controls.Add(this.ignoredImage);
            this.Controls.Add(this.warningImage);
            this.Controls.Add(this.failureImage);
            this.Controls.Add(this.successImage);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.showCheckBoxesCheckBox);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Name = "TreeSettingsPage";
            ((System.ComponentModel.ISupportInitialize)(this.successImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.failureImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.warningImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ignoredImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inconclusiveImage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.skippedImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        public override void LoadSettings()
        {
            showCheckBoxesCheckBox.Checked = Settings.Gui.TestTree.ShowCheckBoxes;

            foreach (string imageSetName in _imageSetManager.ImageSets.Keys)
                imageSetListBox.Items.Add(imageSetName);

            imageSetListBox.SelectedItem = _imageSetManager.CurrentImageSet.Name;
        }

        public override void ApplySettings()
        {
            Settings.Gui.TestTree.ShowCheckBoxes = showCheckBoxesCheckBox.Checked;

            if (imageSetListBox.SelectedIndex >= 0)
                Settings.Gui.TestTree.AlternateImageSet = (string)imageSetListBox.SelectedItem;
        }

        private void imageSetListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string imageSetName = imageSetListBox.SelectedItem as string;
            OutcomeImageSet imageSet = _imageSetManager.LoadImageSet(imageSetName);

            successImage.Image = imageSet.LoadImage("Success");
            failureImage.Image = imageSet.LoadImage("Failure");
            ignoredImage.Image = imageSet.LoadImage("Ignored");
            inconclusiveImage.Image = imageSet.LoadImage("Inconclusive");
            skippedImage.Image = imageSet.LoadImage("Skipped");
            warningImage.Image = imageSet.LoadImage("Warning");
        }
    }
}

