// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using NUnit.Common;
using TestCentric.Gui.Elements;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters
{
    public class AgentSelectionController
    {
        private ITestModel _model;
        private IMainView _view;

        public AgentSelectionController(ITestModel model, IMainView view)
        {
            _model = model;
            _view = view;           
        }

        public bool AllowAgentSelection()
        {
            var package = _model.TestCentricProject;
            return package != null &&
                _model.GetAgentsForPackage(package).Count > 1;
        }

        public void PopulateMenu()
        {
            IPopup agentMenu = _view.SelectAgentMenu;
            agentMenu.MenuItems.Clear();

            var defaultItem = new ToolStripMenuItem("Default");
            defaultItem.Tag = "DEFAULT";
            defaultItem.Checked = true;
            defaultItem.Enabled = true; // Should always remain enabled
            agentMenu.MenuItems.Add(defaultItem);
            defaultItem.Click += OnAgentMenuItemClicked;

            foreach (var agentFullName in _model.AvailableAgents)
            {
                // TODO: The full name is not always provided in AvailableAgents
                int lastDot = agentFullName.LastIndexOf('.');
                string agentName = lastDot == -1
                    ? agentFullName
                    : agentFullName.Substring(lastDot + 1);

                var agentItem = new ToolStripMenuItem(agentName);
                agentItem.Tag = agentFullName;
                agentMenu.MenuItems.Add(agentItem);
                agentItem.Click += OnAgentMenuItemClicked;
            }
        }

        public void UpdateMenuItems()
        {
            IPopup agentMenu = _view.SelectAgentMenu;
            IList<string> agentsToEnable = _model.GetAgentsForPackage(_model.TestCentricProject);

            agentMenu.Enabled = agentsToEnable.Count > 1;
            if (agentMenu.Enabled)
            {
                var packageSettings = _model.TestCentricProject.Settings;
                foreach (ToolStripMenuItem item in agentMenu.MenuItems)
                {
                    string itemTag = item.Tag as string;
                    item.Enabled = itemTag == "DEFAULT" || agentsToEnable.Contains(itemTag);
                }
            }
        }

        private void OnAgentMenuItemClicked(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (!item.Checked)
            {
                EnsureSingleItemChecked(item);

                // TODO: We currently need to use both settings. Investigate.
                string itemTag = item.Tag as string;
                if (itemTag == null || itemTag == "DEFAULT")
                {
                    _model.TestCentricProject.RemoveSetting(SettingDefinitions.SelectedAgentName);
                    _model.TestCentricProject.RemoveSetting(SettingDefinitions.RequestedAgentName);
                }
                else
                {
                    _model.TestCentricProject.AddSetting(SettingDefinitions.SelectedAgentName.WithValue(itemTag));
                    _model.TestCentricProject.AddSetting(SettingDefinitions.RequestedAgentName.WithValue(itemTag));
                }

                // Even though the _model has a Reload method, we cannot use it because Reload
                // does not re-create the Engine.  Since we just changed a setting, we must
                // re-create the Engine by unloading/reloading the tests. We make a copy of
                // __model.TestFiles because the method does an unload before it loads.
                _model.TestCentricProject.LoadTests();
            }

            void EnsureSingleItemChecked(ToolStripMenuItem itemToCheck)
            {
                foreach (ToolStripMenuItem item in _view.SelectAgentMenu.MenuItems)
                    item.Checked = false;
                itemToCheck.Checked = true;
            }
        }
    }
}
