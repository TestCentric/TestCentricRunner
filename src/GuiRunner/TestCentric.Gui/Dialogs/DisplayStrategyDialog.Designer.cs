namespace TestCentric.Gui.Dialogs
{
    partial class DisplayStrategyDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayStrategyDialog));
            this.displayStrategyPanel = new System.Windows.Forms.Panel();
            this.exitButton = new System.Windows.Forms.Button();
            this.separatorPanel = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.testListRadioButton = new System.Windows.Forms.RadioButton();
            this.nunitTreeRadioButton = new System.Windows.Forms.RadioButton();
            this.testListOptionsPanel = new System.Windows.Forms.Panel();
            this.testListGroupByComboBox = new System.Windows.Forms.ComboBox();
            this.testListShowFixturesCheckBox = new System.Windows.Forms.CheckBox();
            this.testListShowAssembliesCheckBox = new System.Windows.Forms.CheckBox();
            this.nunitTreeOptionsPanel = new System.Windows.Forms.Panel();
            this.nunitTreeShowFixturesCheckBox = new System.Windows.Forms.CheckBox();
            this.nunitTreeShowNamespacesCheckBox = new System.Windows.Forms.CheckBox();
            this.nunitTreeShowAssembliesCheckBox = new System.Windows.Forms.CheckBox();
            this.displayStrategyPanel.SuspendLayout();
            this.testListOptionsPanel.SuspendLayout();
            this.nunitTreeOptionsPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // displayStrategyPanel
            // 
            this.displayStrategyPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.displayStrategyPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.displayStrategyPanel.Controls.Add(this.exitButton);
            this.displayStrategyPanel.Controls.Add(this.separatorPanel);
            this.displayStrategyPanel.Controls.Add(this.label2);
            this.displayStrategyPanel.Controls.Add(this.label1);
            this.displayStrategyPanel.Controls.Add(this.testListRadioButton);
            this.displayStrategyPanel.Controls.Add(this.nunitTreeRadioButton);
            this.displayStrategyPanel.Controls.Add(this.testListOptionsPanel);
            this.displayStrategyPanel.Controls.Add(this.nunitTreeOptionsPanel);
            this.displayStrategyPanel.Location = new System.Drawing.Point(0, 0);
            this.displayStrategyPanel.Name = "displayStrategyPanel";
            this.displayStrategyPanel.Size = new System.Drawing.Size(298, 113);
            this.displayStrategyPanel.TabIndex = 0;
            // 
            // exitButton
            // 
            this.exitButton.BackColor = System.Drawing.SystemColors.Control;
            this.exitButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.exitButton.FlatAppearance.BorderSize = 0;
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.ForeColor = System.Drawing.SystemColors.Window;
            this.exitButton.Image = ((System.Drawing.Image)(resources.GetObject("exitButton.Image")));
            this.exitButton.Location = new System.Drawing.Point(269, 8);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(20, 23);
            this.exitButton.TabIndex = 6;
            this.exitButton.UseVisualStyleBackColor = false;
            // 
            // separatorPanel
            // 
            this.separatorPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.separatorPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.separatorPanel.Location = new System.Drawing.Point(129, 14);
            this.separatorPanel.Name = "separatorPanel";
            this.separatorPanel.Size = new System.Drawing.Size(2, 85);
            this.separatorPanel.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(145, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Options";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Display Strategy";
            // 
            // testListRadioButton
            // 
            this.testListRadioButton.AutoSize = true;
            this.testListRadioButton.Location = new System.Drawing.Point(22, 56);
            this.testListRadioButton.Name = "testListRadioButton";
            this.testListRadioButton.Size = new System.Drawing.Size(65, 17);
            this.testListRadioButton.TabIndex = 1;
            this.testListRadioButton.TabStop = true;
            this.testListRadioButton.Text = "Test List";
            this.testListRadioButton.UseVisualStyleBackColor = true;
            // 
            // nunitTreeRadioButton
            // 
            this.nunitTreeRadioButton.AutoSize = true;
            this.nunitTreeRadioButton.Location = new System.Drawing.Point(22, 32);
            this.nunitTreeRadioButton.Name = "nunitTreeRadioButton";
            this.nunitTreeRadioButton.Size = new System.Drawing.Size(77, 17);
            this.nunitTreeRadioButton.TabIndex = 0;
            this.nunitTreeRadioButton.TabStop = true;
            this.nunitTreeRadioButton.Text = "NUnit Tree";
            this.nunitTreeRadioButton.UseVisualStyleBackColor = true;
            // 
            // testListOptionsPanel
            // 
            this.testListOptionsPanel.Controls.Add(this.testListGroupByComboBox);
            this.testListOptionsPanel.Controls.Add(this.testListShowFixturesCheckBox);
            this.testListOptionsPanel.Controls.Add(this.testListShowAssembliesCheckBox);
            this.testListOptionsPanel.Location = new System.Drawing.Point(137, 30);
            this.testListOptionsPanel.Name = "testListOptionsPanel";
            this.testListOptionsPanel.Size = new System.Drawing.Size(153, 73);
            this.testListOptionsPanel.TabIndex = 1;
            // 
            // testListGroupByComboBox
            // 
            this.testListGroupByComboBox.AllowDrop = true;
            this.testListGroupByComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.testListGroupByComboBox.FormattingEnabled = true;
            this.testListGroupByComboBox.Items.AddRange(new object[] {
            "Simple List",
            "By Category",
            "By Outcome",
            "By Duration"});
            this.testListGroupByComboBox.Location = new System.Drawing.Point(15, 1);
            this.testListGroupByComboBox.Name = "testListGroupByComboBox";
            this.testListGroupByComboBox.Size = new System.Drawing.Size(121, 21);
            this.testListGroupByComboBox.TabIndex = 9;
            // 
            // testListShowFixturesCheckBox
            // 
            this.testListShowFixturesCheckBox.AutoSize = true;
            this.testListShowFixturesCheckBox.Location = new System.Drawing.Point(17, 52);
            this.testListShowFixturesCheckBox.Name = "testListShowFixturesCheckBox";
            this.testListShowFixturesCheckBox.Size = new System.Drawing.Size(92, 17);
            this.testListShowFixturesCheckBox.TabIndex = 8;
            this.testListShowFixturesCheckBox.Text = "Show Fixtures";
            this.testListShowFixturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // testListShowAssembliesCheckBox
            // 
            this.testListShowAssembliesCheckBox.AutoSize = true;
            this.testListShowAssembliesCheckBox.Location = new System.Drawing.Point(17, 28);
            this.testListShowAssembliesCheckBox.Name = "testListShowAssembliesCheckBox";
            this.testListShowAssembliesCheckBox.Size = new System.Drawing.Size(108, 17);
            this.testListShowAssembliesCheckBox.TabIndex = 6;
            this.testListShowAssembliesCheckBox.Text = "Show Assemblies";
            this.testListShowAssembliesCheckBox.UseVisualStyleBackColor = true;
            // 
            // nunitTreeOptionsPanel
            // 
            this.nunitTreeOptionsPanel.Controls.Add(this.nunitTreeShowFixturesCheckBox);
            this.nunitTreeOptionsPanel.Controls.Add(this.nunitTreeShowNamespacesCheckBox);
            this.nunitTreeOptionsPanel.Controls.Add(this.nunitTreeShowAssembliesCheckBox);
            this.nunitTreeOptionsPanel.Location = new System.Drawing.Point(137, 30);
            this.nunitTreeOptionsPanel.Name = "nunitTreeOptionsPanel";
            this.nunitTreeOptionsPanel.Size = new System.Drawing.Size(153, 73);
            this.nunitTreeOptionsPanel.TabIndex = 1;
            // 
            // nunitTreeShowFixturesCheckBox
            // 
            this.nunitTreeShowFixturesCheckBox.AutoSize = true;
            this.nunitTreeShowFixturesCheckBox.Location = new System.Drawing.Point(18, 49);
            this.nunitTreeShowFixturesCheckBox.Name = "nunitTreeShowFixturesCheckBox";
            this.nunitTreeShowFixturesCheckBox.Size = new System.Drawing.Size(92, 17);
            this.nunitTreeShowFixturesCheckBox.TabIndex = 5;
            this.nunitTreeShowFixturesCheckBox.Text = "Show Fixtures";
            this.nunitTreeShowFixturesCheckBox.UseVisualStyleBackColor = true;
            // 
            // nunitTreeShowNamespacesCheckBox
            // 
            this.nunitTreeShowNamespacesCheckBox.AutoSize = true;
            this.nunitTreeShowNamespacesCheckBox.Location = new System.Drawing.Point(18, 26);
            this.nunitTreeShowNamespacesCheckBox.Name = "nunitTreeShowNamespacesCheckBox";
            this.nunitTreeShowNamespacesCheckBox.Size = new System.Drawing.Size(118, 17);
            this.nunitTreeShowNamespacesCheckBox.TabIndex = 4;
            this.nunitTreeShowNamespacesCheckBox.Text = "Show Namespaces";
            this.nunitTreeShowNamespacesCheckBox.UseVisualStyleBackColor = true;
            // 
            // nunitTreeShowAssembliesCheckBox
            // 
            this.nunitTreeShowAssembliesCheckBox.AutoSize = true;
            this.nunitTreeShowAssembliesCheckBox.Location = new System.Drawing.Point(18, 3);
            this.nunitTreeShowAssembliesCheckBox.Name = "nunitTreeShowAssembliesCheckBox";
            this.nunitTreeShowAssembliesCheckBox.Size = new System.Drawing.Size(108, 17);
            this.nunitTreeShowAssembliesCheckBox.TabIndex = 3;
            this.nunitTreeShowAssembliesCheckBox.Text = "Show Assemblies";
            this.nunitTreeShowAssembliesCheckBox.UseVisualStyleBackColor = true;
            // 
            // DisplayStrategyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.exitButton;
            this.ClientSize = new System.Drawing.Size(298, 113);
            this.ControlBox = false;
            this.Controls.Add(this.displayStrategyPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DisplayStrategyDialog";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "DisplayStrategyDialog";
            this.displayStrategyPanel.ResumeLayout(false);
            this.displayStrategyPanel.PerformLayout();
            this.testListOptionsPanel.ResumeLayout(false);
            this.testListOptionsPanel.PerformLayout();
            this.nunitTreeOptionsPanel.ResumeLayout(false);
            this.nunitTreeOptionsPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel displayStrategyPanel;
        private System.Windows.Forms.Panel nunitTreeOptionsPanel;
        private System.Windows.Forms.Panel testListOptionsPanel;
        private System.Windows.Forms.Panel separatorPanel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton testListRadioButton;
        private System.Windows.Forms.RadioButton nunitTreeRadioButton;
        private System.Windows.Forms.CheckBox nunitTreeShowAssembliesCheckBox;
        private System.Windows.Forms.CheckBox nunitTreeShowNamespacesCheckBox;
        private System.Windows.Forms.CheckBox nunitTreeShowFixturesCheckBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.CheckBox testListShowFixturesCheckBox;
        private System.Windows.Forms.CheckBox testListShowAssembliesCheckBox;
        private System.Windows.Forms.ComboBox testListGroupByComboBox;
    }
}
