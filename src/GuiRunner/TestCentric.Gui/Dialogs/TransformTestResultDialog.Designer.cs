namespace TestCentric.Gui.Dialogs
{
    partial class TransformTestResultDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TransformTestResultDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTransformationFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxTargetFile = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.buttonSelectTransformationFile = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Transformation file (*.xslt):";
            // 
            // textBoxTransformationFile
            // 
            this.textBoxTransformationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTransformationFile.Location = new System.Drawing.Point(13, 37);
            this.textBoxTransformationFile.Name = "textBoxTransformationFile";
            this.textBoxTransformationFile.Size = new System.Drawing.Size(675, 26);
            this.textBoxTransformationFile.TabIndex = 1;
            this.textBoxTransformationFile.TextChanged += new System.EventHandler(this.OnTransformationFile_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Target file:";
            // 
            // textBoxTargetFile
            // 
            this.textBoxTargetFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxTargetFile.Location = new System.Drawing.Point(13, 98);
            this.textBoxTargetFile.Name = "textBoxTargetFile";
            this.textBoxTargetFile.Size = new System.Drawing.Size(675, 26);
            this.textBoxTargetFile.TabIndex = 3;
            this.textBoxTargetFile.TextChanged += new System.EventHandler(this.OnTargetFile_TextChanged);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(510, 162);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 28);
            this.button1.TabIndex = 5;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(602, 162);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(86, 28);
            this.button2.TabIndex = 6;
            this.button2.Text = "Cancel";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Location = new System.Drawing.Point(694, 96);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(37, 32);
            this.button3.TabIndex = 4;
            this.button3.Text = "...";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.buttonSelectTargetFile_Click);
            // 
            // buttonSelectTransformationFile
            // 
            this.buttonSelectTransformationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSelectTransformationFile.Location = new System.Drawing.Point(694, 35);
            this.buttonSelectTransformationFile.Name = "buttonSelectTransformationFile";
            this.buttonSelectTransformationFile.Size = new System.Drawing.Size(37, 32);
            this.buttonSelectTransformationFile.TabIndex = 2;
            this.buttonSelectTransformationFile.Text = "...";
            this.buttonSelectTransformationFile.UseVisualStyleBackColor = true;
            this.buttonSelectTransformationFile.Click += new System.EventHandler(this.buttonSelectTransformationFile_Click);
            // 
            // TransformTestResultDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(748, 209);
            this.Controls.Add(this.buttonSelectTransformationFile);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBoxTargetFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxTransformationFile);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(5000, 265);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(500, 265);
            this.Name = "TransformTestResultDialog";
            this.ShowInTaskbar = false;
            this.Text = "Transform test result";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxTransformationFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxTargetFile;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button buttonSelectTransformationFile;
    }
}
