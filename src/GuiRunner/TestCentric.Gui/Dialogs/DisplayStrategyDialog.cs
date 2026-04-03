// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TestCentric.Gui.Model;
using TestCentric.Gui.Presenters;

namespace TestCentric.Gui.Dialogs
{
    public partial class DisplayStrategyDialog : Form
    {
        private ITestModel _model;
        private ITreeConfiguration _treeConfig;
        private List<string> _groupBy = new(["UNGROUPED", "CATEGORY", "OUTCOME", "DURATION"]);

        public DisplayStrategyDialog(ITestModel model)
        {
            InitializeComponent();

            _model = model;
            _treeConfig = _model.TreeConfiguration;

            nunitTreeRadioButton.Checked = _treeConfig.DisplayFormat == "NUNIT_TREE";
            nunitTreeShowAssembliesCheckBox.Checked = _treeConfig.NUnitTreeShowAssemblies;
            nunitTreeShowNamespacesCheckBox.Checked = _treeConfig.NUnitTreeShowNamespaces;
            nunitTreeShowFixturesCheckBox.Checked = _treeConfig.NUnitTreeShowFixtures;

            testListRadioButton.Checked = _treeConfig.DisplayFormat == "TEST_LIST";
            testListGroupByComboBox.SelectedIndex = Math.Max(_groupBy.IndexOf(_treeConfig.TestListGroupBy), 0);
            testListShowAssembliesCheckBox.Checked = _treeConfig.TestListShowAssemblies;
            testListShowFixturesCheckBox.Checked = _treeConfig.TestListShowFixtures;

            nunitTreeRadioButton.CheckedChanged += (s, e) =>
                {
                    if (nunitTreeRadioButton.Checked)
                    {
                        _model.TreeConfiguration.DisplayFormat = "NUNIT_TREE";
                        nunitTreeOptionsPanel.Visible = true;
                        testListOptionsPanel.Visible = false;
                    }
                    else
                    {
                        _model.TreeConfiguration.DisplayFormat = "TEST_LIST";
                        nunitTreeOptionsPanel.Visible = false;
                        testListOptionsPanel.Visible = true;
                    }
                };

            nunitTreeShowAssembliesCheckBox.CheckedChanged += (s, e) =>
                _treeConfig.NUnitTreeShowAssemblies = nunitTreeShowAssembliesCheckBox.Checked;

            nunitTreeShowNamespacesCheckBox.CheckedChanged += (s, e) =>
                _treeConfig.NUnitTreeShowNamespaces = nunitTreeShowNamespacesCheckBox.Checked;

            nunitTreeShowFixturesCheckBox.CheckedChanged += (s, e) =>
                _treeConfig.NUnitTreeShowFixtures = nunitTreeShowFixturesCheckBox.Checked;

            testListGroupByComboBox.SelectedIndexChanged += (s, e) =>
                _treeConfig.TestListGroupBy = _groupBy[testListGroupByComboBox.SelectedIndex];

            testListShowAssembliesCheckBox.CheckedChanged += (s, e) =>
                _treeConfig.TestListShowAssemblies = testListShowAssembliesCheckBox.Checked;

            testListShowFixturesCheckBox.CheckedChanged += (s, e) =>
                _treeConfig.TestListShowFixtures = testListShowFixturesCheckBox.Checked;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
