using TestCentric.Gui.Controls;

namespace TestCentric.Gui.Views
{
    partial class TestPropertiesSubView
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
            this.testTypeLabel = new System.Windows.Forms.Label();
            this.testType = new System.Windows.Forms.Label();
            this.testCaseCountLabel = new System.Windows.Forms.Label();
            this.testCaseCount = new System.Windows.Forms.Label();
            this.runStateLabel = new System.Windows.Forms.Label();
            this.runState = new System.Windows.Forms.Label();
            this.fullNameLabel = new System.Windows.Forms.Label();
            this.propertiesLabel = new System.Windows.Forms.Label();
            this.fullName = new TestCentric.Gui.Controls.ExpandingLabel();
            this.properties = new TestCentric.Gui.Controls.ExpandingLabel();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.reasonLabel = new System.Windows.Forms.Label();
            this.description = new TestCentric.Gui.Controls.ExpandingLabel();
            this.skipReason = new TestCentric.Gui.Controls.ExpandingLabel();
            this.categoriesLabel = new System.Windows.Forms.Label();
            this.displayHiddenProperties = new System.Windows.Forms.CheckBox();
            this.categories = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // testTypeLabel
            // 
            this.testTypeLabel.Location = new System.Drawing.Point(5, 4);
            this.testTypeLabel.Name = "testTypeLabel";
            this.testTypeLabel.Size = new System.Drawing.Size(58, 13);
            this.testTypeLabel.TabIndex = 2;
            this.testTypeLabel.Text = "Test Type:";
            // 
            // testType
            // 
            this.testType.Location = new System.Drawing.Point(74, 4);
            this.testType.Name = "testType";
            this.testType.Size = new System.Drawing.Size(117, 13);
            this.testType.TabIndex = 3;
            // 
            // testCaseCountLabel
            // 
            this.testCaseCountLabel.Location = new System.Drawing.Point(5, 89);
            this.testCaseCountLabel.Name = "testCaseCountLabel";
            this.testCaseCountLabel.Size = new System.Drawing.Size(65, 13);
            this.testCaseCountLabel.TabIndex = 24;
            this.testCaseCountLabel.Text = "Test Count:";
            // 
            // testCaseCount
            // 
            this.testCaseCount.Location = new System.Drawing.Point(74, 89);
            this.testCaseCount.Name = "testCaseCount";
            this.testCaseCount.Size = new System.Drawing.Size(71, 13);
            this.testCaseCount.TabIndex = 25;
            // 
            // runStateLabel
            // 
            this.runStateLabel.Location = new System.Drawing.Point(169, 89);
            this.runStateLabel.Name = "runStateLabel";
            this.runStateLabel.Size = new System.Drawing.Size(61, 13);
            this.runStateLabel.TabIndex = 26;
            this.runStateLabel.Text = "Run State: ";
            // 
            // runState
            // 
            this.runState.Location = new System.Drawing.Point(236, 89);
            this.runState.Name = "runState";
            this.runState.Size = new System.Drawing.Size(239, 17);
            this.runState.TabIndex = 12;
            // 
            // fullNameLabel
            // 
            this.fullNameLabel.Location = new System.Drawing.Point(5, 23);
            this.fullNameLabel.Name = "fullNameLabel";
            this.fullNameLabel.Size = new System.Drawing.Size(65, 13);
            this.fullNameLabel.TabIndex = 18;
            this.fullNameLabel.Text = "Full Name:";
            // 
            // propertiesLabel
            // 
            this.propertiesLabel.Location = new System.Drawing.Point(5, 133);
            this.propertiesLabel.Name = "propertiesLabel";
            this.propertiesLabel.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
            this.propertiesLabel.Size = new System.Drawing.Size(65, 14);
            this.propertiesLabel.TabIndex = 29;
            this.propertiesLabel.Text = "Properties:";
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
            // properties
            // 
            this.properties.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.properties.BackColor = System.Drawing.Color.LightYellow;
            this.properties.Expansion = TestCentric.Gui.Controls.TipWindow.ExpansionStyle.Both;
            this.properties.Font = new System.Drawing.Font("Lucida Console", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.properties.Location = new System.Drawing.Point(5, 159);
            this.properties.Name = "properties";
            this.properties.Size = new System.Drawing.Size(467, 40);
            this.properties.TabIndex = 31;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Location = new System.Drawing.Point(5, 45);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(65, 13);
            this.descriptionLabel.TabIndex = 20;
            this.descriptionLabel.Text = "Description:";
            // 
            // reasonLabel
            // 
            this.reasonLabel.Location = new System.Drawing.Point(5, 111);
            this.reasonLabel.Name = "reasonLabel";
            this.reasonLabel.Size = new System.Drawing.Size(47, 13);
            this.reasonLabel.TabIndex = 27;
            this.reasonLabel.Text = "Reason:";
            // 
            // description
            // 
            this.description.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.description.AutoEllipsis = true;
            this.description.BackColor = System.Drawing.Color.LightYellow;
            this.description.Location = new System.Drawing.Point(73, 45);
            this.description.Name = "description";
            this.description.Size = new System.Drawing.Size(399, 18);
            this.description.TabIndex = 21;
            // 
            // skipReason
            // 
            this.skipReason.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.skipReason.AutoEllipsis = true;
            this.skipReason.BackColor = System.Drawing.Color.LightYellow;
            this.skipReason.Expansion = TestCentric.Gui.Controls.TipWindow.ExpansionStyle.Both;
            this.skipReason.Location = new System.Drawing.Point(73, 111);
            this.skipReason.Name = "skipReason";
            this.skipReason.Size = new System.Drawing.Size(399, 18);
            this.skipReason.TabIndex = 28;
            // 
            // categoriesLabel
            // 
            this.categoriesLabel.Location = new System.Drawing.Point(5, 67);
            this.categoriesLabel.Name = "categoriesLabel";
            this.categoriesLabel.Size = new System.Drawing.Size(65, 13);
            this.categoriesLabel.TabIndex = 22;
            this.categoriesLabel.Text = "Categories:";
            // 
            // displayHiddenProperties
            // 
            this.displayHiddenProperties.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.displayHiddenProperties.Location = new System.Drawing.Point(74, 133);
            this.displayHiddenProperties.Name = "displayHiddenProperties";
            this.displayHiddenProperties.Size = new System.Drawing.Size(157, 19);
            this.displayHiddenProperties.TabIndex = 30;
            this.displayHiddenProperties.Text = "Display hidden properties";
            this.displayHiddenProperties.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.displayHiddenProperties.UseVisualStyleBackColor = true;
            // 
            // categories
            // 
            this.categories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.categories.AutoEllipsis = true;
            this.categories.BackColor = System.Drawing.Color.LightYellow;
            this.categories.Location = new System.Drawing.Point(73, 67);
            this.categories.Name = "categories";
            this.categories.Size = new System.Drawing.Size(399, 18);
            this.categories.TabIndex = 23;
            // 
            // TestPropertiesSubView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.testCaseCountLabel);
            this.Controls.Add(this.testCaseCount);
            this.Controls.Add(this.runStateLabel);
            this.Controls.Add(this.runState);
            this.Controls.Add(this.fullNameLabel);
            this.Controls.Add(this.propertiesLabel);
            this.Controls.Add(this.fullName);
            this.Controls.Add(this.properties);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.reasonLabel);
            this.Controls.Add(this.description);
            this.Controls.Add(this.skipReason);
            this.Controls.Add(this.categoriesLabel);
            this.Controls.Add(this.displayHiddenProperties);
            this.Controls.Add(this.categories);
            this.Controls.Add(this.testType);
            this.Controls.Add(this.testTypeLabel);
            this.Name = "TestPropertiesSubView";
            this.Size = new System.Drawing.Size(476, 204);
            this.MinimumSize = new System.Drawing.Size(0, 204);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label testTypeLabel;
        private System.Windows.Forms.Label testType;
        private System.Windows.Forms.Label fullNameLabel;
        private ExpandingLabel fullName;
        private System.Windows.Forms.Label descriptionLabel;
        private ExpandingLabel description;
        private System.Windows.Forms.Label categoriesLabel;
        private System.Windows.Forms.Label categories;
        private System.Windows.Forms.Label testCaseCountLabel;
        private System.Windows.Forms.Label testCaseCount;
        private System.Windows.Forms.Label runStateLabel;
        private System.Windows.Forms.Label runState;
        private System.Windows.Forms.Label reasonLabel;
        private ExpandingLabel skipReason;
        private System.Windows.Forms.Label propertiesLabel;
        private ExpandingLabel properties;
        private System.Windows.Forms.CheckBox displayHiddenProperties;
    }
}
