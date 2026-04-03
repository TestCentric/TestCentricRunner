// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TestCentric.Gui.Model;
using TestCentric.Gui.Presenters;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Dialogs
{
    public partial class DisplayStrategyDialog : Form
    {
        private ITreeConfiguration _treeConfig;
        private List<string> _groupBy = new(["UNGROUPED", "CATEGORY", "OUTCOME", "DURATION"]);

        public DisplayStrategyDialog(ITreeConfiguration treeConfig, TreeView treeView)
        {
            InitializeComponent();

            _treeConfig = treeConfig;

            var location = treeView.Location;
            location.Offset(treeView.Width / 2, 5);
            Location = treeView.PointToScreen(location);

            WireUpEvents();
        }

        public string DisplayFormat
        {
            get => _treeConfig.DisplayFormat;
            set => _treeConfig.DisplayFormat = value;
        }

        public bool NUnitTreeShowAssemblies
        {
            get => _treeConfig.NUnitTreeShowAssemblies;
            set => _treeConfig.NUnitTreeShowAssemblies = value;
        }

        public bool NUnitTreeShowNamespaces
        {
            get => _treeConfig.NUnitTreeShowNamespaces;
            set => _treeConfig.NUnitTreeShowNamespaces = value;
        }

        public bool NUnitTreeShowFixtures
        {
            get => _treeConfig.NUnitTreeShowFixtures;
            set => _treeConfig.NUnitTreeShowFixtures = value;
        }

        public string TestListGroupBy
        {
            get => _treeConfig.TestListGroupBy;
            set => _treeConfig.TestListGroupBy = value;
        }

        public bool TestListShowAssemblies
        {
            get => _treeConfig.TestListShowAssemblies;
            set => _treeConfig.TestListShowAssemblies = value;
        }

        public bool TestListShowFixtures
        {
            get => _treeConfig.TestListShowFixtures;
            set => _treeConfig.TestListShowFixtures = value;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            nunitTreeRadioButton.Checked = DisplayFormat == "NUNIT_TREE";
            nunitTreeShowAssembliesCheckBox.Checked = NUnitTreeShowAssemblies;
            nunitTreeShowNamespacesCheckBox.Checked = NUnitTreeShowNamespaces;
            nunitTreeShowFixturesCheckBox.Checked = NUnitTreeShowFixtures;

            testListRadioButton.Checked = DisplayFormat == "TEST_LIST";
            testListGroupByComboBox.SelectedIndex = Math.Max(_groupBy.IndexOf(_treeConfig.TestListGroupBy), 0);
            testListShowAssembliesCheckBox.Checked = TestListShowAssemblies;
            testListShowFixturesCheckBox.Checked = TestListShowFixtures;
        }

        private void WireUpEvents()
        {
            nunitTreeRadioButton.CheckedChanged += (s, e) =>
            {
                if (nunitTreeRadioButton.Checked)
                {
                    DisplayFormat = "NUNIT_TREE";
                    nunitTreeOptionsPanel.Visible = true;
                    testListOptionsPanel.Visible = false;
                }
                else
                {
                    DisplayFormat = "TEST_LIST";
                    nunitTreeOptionsPanel.Visible = false;
                    testListOptionsPanel.Visible = true;
                }
            };

            nunitTreeShowAssembliesCheckBox.CheckedChanged += (s, e) =>
                NUnitTreeShowAssemblies = nunitTreeShowAssembliesCheckBox.Checked;

            nunitTreeShowNamespacesCheckBox.CheckedChanged += (s, e) =>
                NUnitTreeShowNamespaces = nunitTreeShowNamespacesCheckBox.Checked;

            nunitTreeShowFixturesCheckBox.CheckedChanged += (s, e) =>
                NUnitTreeShowFixtures = nunitTreeShowFixturesCheckBox.Checked;

            testListGroupByComboBox.SelectedIndexChanged += (s, e) =>
                TestListGroupBy = _groupBy[testListGroupByComboBox.SelectedIndex];

            testListShowAssembliesCheckBox.CheckedChanged += (s, e) =>
                TestListShowAssemblies = testListShowAssembliesCheckBox.Checked;

            testListShowFixturesCheckBox.CheckedChanged += (s, e) =>
                TestListShowFixtures = testListShowFixturesCheckBox.Checked;

            exitButton.Click += (s, e) => Close();
        }
    }
}
