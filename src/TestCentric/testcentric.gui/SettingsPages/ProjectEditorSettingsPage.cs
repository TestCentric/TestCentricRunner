﻿// ***********************************************************************
// Copyright (c) 2018 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace TestCentric.Gui.SettingsPages
{
    public partial class ProjectEditorSettingsPage : SettingsPage
    {
        public ProjectEditorSettingsPage(string key) : base(key)
        {
            InitializeComponent();
        }

        public override void LoadSettings()
        {
            string editorPath = Settings.Gui.ProjectEditorPath;

            if (editorPath == null)
            {
                useNUnitEditorRadioButton.Checked = true;
                editorPathTextBox.Text = "";
            }
            else
            {
                useOtherEditorRadioButton.Checked = true;
                editorPathTextBox.Text = editorPath;
            }
        }

        public override void ApplySettings()
        {
            Settings.Gui.ProjectEditorPath = useNUnitEditorRadioButton.Checked
                ? null
                : editorPathTextBox.Text;
        }

        private void editorPathTextBox_TextChanged(object sender, EventArgs e)
        {
            if (editorPathTextBox.TextLength == 0)
                useNUnitEditorRadioButton.Checked = true;
            else
                useOtherEditorRadioButton.Checked = true;
        }

        private void editorPathBrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if ( Site != null ) dlg.Site = Site;
            dlg.Title = "Select Project Editor";

            dlg.Filter = "Executable Files (*.exe)|*.exe";
            dlg.FilterIndex = 1;
            dlg.FileName = "";

            if ( dlg.ShowDialog() == DialogResult.OK ) 
                editorPathTextBox.Text = dlg.FileName;
        }

        private void editorPathTextBox_Validating(object sender, CancelEventArgs e)
        {
            var path = editorPathTextBox.Text;
            if (!string.IsNullOrWhiteSpace(path) && !File.Exists(path))
                if (MessageDisplay.Ask("The specified path does not exist. Do you want to continue anyway?") == DialogResult.Yes)
                    e.Cancel = true;
        }
    }
}
