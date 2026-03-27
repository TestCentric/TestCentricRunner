// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace TestCentric.Gui.Dialogs
{
    public class AboutBox : Form
    {
        private Button OkButton;
        private Label versionLabel;
        private LinkLabel linkLabel1;
        private Label copyright;
        private Label label2;
        private PictureBox pictureBox1;
        private Label label1;
        private Label label3;
        private LinkLabel linkLabel2;
        private Label label4;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public AboutBox()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string versionText = $"Version {executingAssembly.GetName().Version}";

            object[] objectAttrs = executingAssembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
            if (objectAttrs.Length > 0)
                versionText = $"Version {((AssemblyInformationalVersionAttribute)objectAttrs[0]).InformationalVersion}";
            // Truncate the informational version to get the version with optional pre-release label.
            int plus = versionText.IndexOf('+');
            if (plus > 0)
                versionText = versionText.Substring(0, plus);

            objectAttrs = executingAssembly.GetCustomAttributes(typeof(AssemblyConfigurationAttribute), false);
            if (objectAttrs.Length > 0)
            {
                string configText = ((AssemblyConfigurationAttribute)objectAttrs[0]).Configuration;
                if (configText != "")
                    versionText += string.Format(" ({0})", configText);
            }

            string copyrightText = "Copyright (C) 2018-2021 Charlie Poole and TestCentric contributors";
            objectAttrs = executingAssembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (objectAttrs.Length > 0)
                copyrightText = ((AssemblyCopyrightAttribute)objectAttrs[0]).Copyright;

            versionLabel.Text = versionText;
            copyright.Text = copyrightText;
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.OkButton = new System.Windows.Forms.Button();
            this.versionLabel = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.copyright = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.linkLabel2 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // OkButton
            // 
            this.OkButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.OkButton.Location = new System.Drawing.Point(159, 349);
            this.OkButton.Name = "OkButton";
            this.OkButton.Size = new System.Drawing.Size(96, 29);
            this.OkButton.TabIndex = 0;
            this.OkButton.Text = "OK";
            this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // versionLabel
            // 
            this.versionLabel.AutoSize = true;
            this.versionLabel.Location = new System.Drawing.Point(164, 37);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(42, 13);
            this.versionLabel.TabIndex = 2;
            this.versionLabel.Text = "Version";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(0, 21);
            this.linkLabel1.Location = new System.Drawing.Point(230, 102);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(62, 17);
            this.linkLabel1.TabIndex = 5;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "TestCentric";
            this.linkLabel1.UseCompatibleTextRendering = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // copyright
            // 
            this.copyright.Location = new System.Drawing.Point(164, 63);
            this.copyright.Name = "copyright";
            this.copyright.Size = new System.Drawing.Size(215, 28);
            this.copyright.TabIndex = 10;
            this.copyright.Text = "Copyright";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(164, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(144, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "TestCentric Runner for NUnit";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(14, 11);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(135, 132);
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(164, 126);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Licensed under the";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(164, 100);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Published by";
            // 
            // linkLabel2
            // 
            this.linkLabel2.AutoSize = true;
            this.linkLabel2.Location = new System.Drawing.Point(261, 127);
            this.linkLabel2.Name = "linkLabel2";
            this.linkLabel2.Size = new System.Drawing.Size(66, 13);
            this.linkLabel2.TabIndex = 15;
            this.linkLabel2.TabStop = true;
            this.linkLabel2.Text = "MIT License";
            this.linkLabel2.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(14, 162);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(387, 185);
            this.label4.TabIndex = 16;
            this.label4.Text = resources.GetString("label4.Text");
            // 
            // AboutBox
            // 
            this.AcceptButton = this.OkButton;
            this.CancelButton = this.OkButton;
            this.ClientSize = new System.Drawing.Size(413, 385);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.linkLabel2);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.copyright);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.versionLabel);
            this.Controls.Add(this.OkButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About TestCentric Runner for NUnit";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void OkButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void linkLabel1_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://test-centric.org");
            linkLabel1.LinkVisited = true;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://test-centric.org/testcentric-gui/license");
            linkLabel2.LinkVisited = true;
        }
    }
}
