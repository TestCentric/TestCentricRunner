// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Windows.Forms;
using TestCentric.Engine;
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
            IList<string> agentsToEnable = _model.GetAgentsForPackage(_model.TestCentricProject);
            string selectedAgentName = _model.TestCentricProject.Settings.GetValueOrDefault(SettingDefinitions.SelectedAgentName);

            agentMenu.Enabled = agentsToEnable.Count > 1;
            if (agentMenu.Enabled)
            {
                //agentMenu.MenuItems.Clear();

                var defaultMenuItem = new ToolStripMenuItem("Default")
                {
                    Name = "defaultMenuItem",
                    Tag = "DEFAULT"
                };

                agentMenu.MenuItems.Add(defaultMenuItem);

                foreach (var agentFullName in _model.AvailableAgents)
                {
                    int lastDot = agentFullName.LastIndexOf('.');
                    string agentName = lastDot == -1
                        ? agentFullName
                        : agentFullName.Substring(lastDot + 1);

                    var menuItem = new ToolStripMenuItem(agentName)
                    {
                        Tag = agentFullName,
                        Enabled = agentsToEnable.Contains(agentFullName)
                    };

                    agentMenu.MenuItems.Add(menuItem);
                }

                // Go through all menu items and check one
                bool isItemChecked = false;
                var packageSettings = _model.TestCentricProject.Settings;
                foreach (ToolStripMenuItem item in agentMenu.MenuItems)
                {
                    if ((string)item.Text == selectedAgentName)
                        item.Checked = isItemChecked = true;
                    else
                        item.Click += (s, e) =>
                        {
                            item.Checked = isItemChecked = true;

                            if (item.Tag == null || item.Tag as string == "DEFAULT")
                                packageSettings.Remove(SettingDefinitions.SelectedAgentName);
                            else
                                packageSettings.Set(SettingDefinitions.SelectedAgentName.WithValue(item.Tag));

                            // Even though the _model has a Reload method, we cannot use it because Reload
                            // does not re-create the Engine.  Since we just changed a setting, we must
                            // re-create the Engine by unloading/reloading the tests. We make a copy of
                            // __model.TestFiles because the method does an unload before it loads.
                            _model.TestCentricProject.LoadTests();
                        };
                }

                if (!isItemChecked)
                {
                    defaultMenuItem.Checked = true;
                    //packageSettings.Remove(SettingDefinitions.SelectedAgentName);
                }
            }
        }
    }
}
