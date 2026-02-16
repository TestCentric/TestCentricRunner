namespace TestCentric.Gui.Dialogs
{
    partial class ProjectNameDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ProjectNameDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.projectNameTextBox = new System.Windows.Forms.TextBox();
            this.projectDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.browseButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.createProjectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Project Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Location";
            // 
            // projectNameTextBox
            // 
            this.projectNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.projectNameTextBox.Location = new System.Drawing.Point(15, 25);
            this.projectNameTextBox.Name = "projectNameTextBox";
            this.projectNameTextBox.Size = new System.Drawing.Size(343, 20);
            this.projectNameTextBox.TabIndex = 3;
            this.projectNameTextBox.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // projectDirectoryTextBox
            // 
            this.projectDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.projectDirectoryTextBox.Location = new System.Drawing.Point(15, 78);
            this.projectDirectoryTextBox.Name = "projectDirectoryTextBox";
            this.projectDirectoryTextBox.Size = new System.Drawing.Size(343, 20);
            this.projectDirectoryTextBox.TabIndex = 4;
            this.projectDirectoryTextBox.TextChanged += new System.EventHandler(this.OnTextChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(361, 27);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(36, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = ".tcproj";
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(378, 78);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 6;
            this.browseButton.Text = "Browse";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(266, 122);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(89, 23);
            this.cancelButton.TabIndex = 12;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // createProjectButton
            // 
            this.createProjectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.createProjectButton.Enabled = false;
            this.createProjectButton.Location = new System.Drawing.Point(364, 122);
            this.createProjectButton.Name = "createProjectButton";
            this.createProjectButton.Size = new System.Drawing.Size(89, 23);
            this.createProjectButton.TabIndex = 13;
            this.createProjectButton.Text = "Create Project";
            this.createProjectButton.UseVisualStyleBackColor = true;
            this.createProjectButton.Click += new System.EventHandler(this.createProjectButton_Click);
            // 
            // ProjectNameDialog
            // 
            this.AcceptButton = this.createProjectButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(464, 157);
            this.Controls.Add(this.createProjectButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.projectDirectoryTextBox);
            this.Controls.Add(this.projectNameTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(350, 196);
            this.Name = "ProjectNameDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create New Project";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox projectNameTextBox;
        private System.Windows.Forms.TextBox projectDirectoryTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button createProjectButton;
    }
}
