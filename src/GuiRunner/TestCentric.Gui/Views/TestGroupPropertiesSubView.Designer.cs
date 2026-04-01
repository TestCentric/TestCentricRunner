using TestCentric.Gui.Controls;

namespace TestCentric.Gui.Views
{
    partial class TestGroupPropertiesSubView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.testCaseCountLabel = new System.Windows.Forms.Label();
            this.testCaseCount = new System.Windows.Forms.Label();
            this.fullNameLabel = new System.Windows.Forms.Label();
            this.fullName = new TestCentric.Gui.Controls.ExpandingLabel();
            this.testType = new System.Windows.Forms.Label();
            this.testTypeLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // testCaseCountLabel
            // 
            this.testCaseCountLabel.Location = new System.Drawing.Point(5, 44);
            this.testCaseCountLabel.Name = "testCaseCountLabel";
            this.testCaseCountLabel.Size = new System.Drawing.Size(65, 13);
            this.testCaseCountLabel.TabIndex = 24;
            this.testCaseCountLabel.Text = "Test Count:";
            // 
            // testCaseCount
            // 
            this.testCaseCount.Location = new System.Drawing.Point(74, 45);
            this.testCaseCount.Name = "testCaseCount";
            this.testCaseCount.Size = new System.Drawing.Size(71, 13);
            this.testCaseCount.TabIndex = 25;
            // 
            // fullNameLabel
            // 
            this.fullNameLabel.Location = new System.Drawing.Point(5, 24);
            this.fullNameLabel.Name = "fullNameLabel";
            this.fullNameLabel.Size = new System.Drawing.Size(65, 13);
            this.fullNameLabel.TabIndex = 18;
            this.fullNameLabel.Text = "Full Name:";
            // 
            // fullName
            // 
            this.fullName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.fullName.AutoEllipsis = true;
            this.fullName.BackColor = System.Drawing.Color.LightYellow;
            this.fullName.Location = new System.Drawing.Point(73, 23);
            this.fullName.Name = "fullName";
            this.fullName.Size = new System.Drawing.Size(399, 18);
            this.fullName.TabIndex = 19;
            // 
            // testType
            // 
            this.testType.Location = new System.Drawing.Point(74, 4);
            this.testType.Name = "testType";
            this.testType.Text = "Test group";
            this.testType.Size = new System.Drawing.Size(117, 13);
            this.testType.TabIndex = 3;
            // 
            // testTypeLabel
            // 
            this.testTypeLabel.Location = new System.Drawing.Point(5, 4);
            this.testTypeLabel.Name = "testTypeLabel";
            this.testTypeLabel.Size = new System.Drawing.Size(58, 13);
            this.testTypeLabel.TabIndex = 2;
            this.testTypeLabel.Text = "Test Type:";
            // 
            // TestGroupPropertiesSubView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.testCaseCountLabel);
            this.Controls.Add(this.testCaseCount);
            this.Controls.Add(this.fullNameLabel);
            this.Controls.Add(this.fullName);
            this.Controls.Add(this.testType);
            this.Controls.Add(this.testTypeLabel);
            this.MinimumSize = new System.Drawing.Size(2, 80);
            this.Name = "TestGroupPropertiesSubView";
            this.Size = new System.Drawing.Size(476, 80);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label testTypeLabel;
        private System.Windows.Forms.Label testType;
        private System.Windows.Forms.Label fullNameLabel;
        private ExpandingLabel fullName;
        private System.Windows.Forms.Label testCaseCountLabel;
        private System.Windows.Forms.Label testCaseCount;
    }
}
