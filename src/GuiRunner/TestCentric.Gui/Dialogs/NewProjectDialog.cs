// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Dialogs
{
    public partial class NewProjectDialog : Form
    {
        private IMainView _view;
        private ITestModel _model;

        public NewProjectDialog(IMainView view, ITestModel model) 
        {
            _view = view;
            _model = model;

            InitializeComponent();
        }

        public string ProjectName => projectNameTextBox.Text;
        public string ProjectDirectory => projectDirectoryTextBox.Text;
        public string ProjectPath => Path.Combine(ProjectDirectory, ProjectName + ".tcproj");
        public string[] TestFiles => testFilesListBox.Items.Cast<string>().ToArray();

        private void browseButton_Click(object sender, EventArgs e)
        {
            projectDirectoryTextBox.Text = _view.DialogManager.GetFolderPath(
                "Select directory where project will be saved",
                string.IsNullOrWhiteSpace(projectDirectoryTextBox.Text)
                    ? _model.WorkDirectory
                    : projectDirectoryTextBox.Text);
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            var filesToAdd = _view.DialogManager.SelectMultipleFiles(
                "Select Test Files for New Project",
                _view.DialogManager.CreateOpenTestFileFilter(_model.NUnitProjectSupport, _model.VisualStudioSupport));

            if (filesToAdd.Length > 0)
            {
                testFilesListBox.Items.AddRange(filesToAdd);

                if (testFilesListBox.SelectedIndex < 0)
                    testFilesListBox.SelectedIndex = 0;

                EnableDisableButtons();
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int index = testFilesListBox.SelectedIndex;
            testFilesListBox.Items.RemoveAt(index);
            testFilesListBox.SelectedIndex = Math.Min(index, testFilesListBox.Items.Count - 1);

            EnableDisableButtons();
        }

        private void moveUpButton_Click(object sender, EventArgs e)
        {
            MoveItem(testFilesListBox.SelectedIndex, testFilesListBox.SelectedIndex - 1);
        }

        private void moveDownButton_Click(object sender, EventArgs e)
        {
            MoveItem(testFilesListBox.SelectedIndex, testFilesListBox.SelectedIndex + 1);
        }

        private void MoveItem(int from, int to)
        {
            var item = testFilesListBox.Items[from];
            testFilesListBox.Items.RemoveAt(from);
            testFilesListBox.Items.Insert(to, item);
            testFilesListBox.SelectedIndex = to;
        }

        private void createProjectButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void projectNameTextBox_TextChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }

        private void projectDirectoryTextBox_TextChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }

        private void testFilesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableDisableButtons();
        }

        private void EnableDisableButtons()
        {
            createProjectButton.Enabled =
                !string.IsNullOrWhiteSpace(ProjectName) &&
                !string.IsNullOrWhiteSpace(ProjectDirectory) &&
                testFilesListBox.Items.Count > 0;

            int index = testFilesListBox.SelectedIndex;
            deleteButton.Enabled = index >= 0;
            moveUpButton.Enabled = index > 0;
            moveDownButton.Enabled = index >= 0 && index < testFilesListBox.Items.Count - 1;
        }
    }
}
