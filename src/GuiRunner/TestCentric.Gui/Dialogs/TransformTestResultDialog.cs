// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Dialogs
{
    public partial class TransformTestResultDialog : Form
    {
        private IDialogManager _dialogManager;

        public TransformTestResultDialog()
        {
            InitializeComponent();

            _dialogManager = new DialogManager();
        }

        public string TargetFile => textBoxTargetFile.Text;
        public string TransformationFile => textBoxTransformationFile.Text;

        private void buttonSelectTargetFile_Click(object sender, EventArgs e)
        {
            var fileName = _dialogManager.GetFileOpenPath("Target file", "All files (*.*)|*.*");

            this.textBoxTargetFile.Clear();
            this.textBoxTargetFile.AppendText(fileName);
        }

        private void buttonSelectTransformationFile_Click(object sender, EventArgs e)
        {
            var fileName = _dialogManager.GetFileOpenPath("Transformation file", "XSLT files (*.xslt)|*.xslt|All files (*.*)|*.*");

            this.textBoxTransformationFile.Clear();
            this.textBoxTransformationFile.AppendText(fileName);
        }

        private void OnTransformationFile_TextChanged(object sender, EventArgs e)
        {
            UpdateEnableState();
        }

        private void OnTargetFile_TextChanged(object sender, EventArgs e)
        {
            UpdateEnableState();
        }

        private void UpdateEnableState()
        {
            this.button1.Enabled = !string.IsNullOrEmpty(textBoxTargetFile.Text) && !string.IsNullOrEmpty(textBoxTransformationFile.Text);
        }
    }
}
