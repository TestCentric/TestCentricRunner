// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Windows.Forms;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Dialogs
{
    public partial class ProjectNameDialog : Form
    {
        private IMainView _view;
        private ITestModel _model;

        public ProjectNameDialog(IMainView view, ITestModel model) 
        {
            _view = view;
            _model = model;

            InitializeComponent();
        }

        public string ProjectName => projectNameTextBox.Text;
        public string ProjectDirectory => projectDirectoryTextBox.Text;
        public string ProjectPath => Path.Combine(ProjectDirectory, ProjectName + ".tcproj");

        private void browseButton_Click(object sender, EventArgs e)
        {
            projectDirectoryTextBox.Text = _view.DialogManager.GetFolderPath(
                "Select directory where project will be saved",
                string.IsNullOrWhiteSpace(projectDirectoryTextBox.Text)
                    ? _model.WorkDirectory
                    : projectDirectoryTextBox.Text);
        }

        private void createProjectButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            createProjectButton.Enabled =
                !string.IsNullOrWhiteSpace(ProjectName) &&
                !string.IsNullOrWhiteSpace(ProjectDirectory);
        }
    }
}
