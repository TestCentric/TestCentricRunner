// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Configuration;

    public class UserSettings
    {
        private IDictionary<ApplicationSettingsBase, string> _settingGroups;

        public event SettingsEventHandler Changed;

        public UserSettings()
        {
            // Add all sub settings objects with their prefix
            _settingGroups = new Dictionary<ApplicationSettingsBase, string>()
            {
                { Engine, "TestCentric.Engine" },
                { Gui, "TestCentric.Gui" },
                { Gui.MainForm, "TestCentric.Gui.MainForm" },
                { Gui.MiniForm, "TestCentric.Gui.MiniForm" },
                { Gui.ErrorDisplay, "TestCentric.Gui.ErrorDisplay" },
                { Gui.RecentProjects, "TestCentric.Gui.RecentProjects" },
                { Gui.RecentFiles, "TestCentric.Gui.RecentFiles" },
                { Gui.TextOutput, "TestCentric.Gui.TextOutput" },
                { Gui.TestTree, "TestCentric.Gui.TestTree" },
                { Gui.TestTree.FixtureList, "TestCentric.Gui.TestTree.FixtureList" },
                { Gui.TestTree.TestList, "TestCentric.Gui.TestTree.TestList" },
            };

            foreach (ApplicationSettingsBase settingsBase in _settingGroups.Keys)
            {
                settingsBase.PropertyChanged += OnSettingChanged;
            }
        }

        public GuiSettings Gui => GuiSettings.Default;

        public EngineSettings Engine => EngineSettings.Default;

        private void OnSettingChanged(object sender, PropertyChangedEventArgs args)
        {
            string prefix = string.Empty;
            if (sender is ApplicationSettingsBase settingsBase)
                prefix = _settingGroups[settingsBase];

            Changed?.Invoke(this, new SettingsEventArgs(prefix + "." + args.PropertyName));
        }

        public void SaveSettings()
        {
            try
            {
                foreach (ApplicationSettingsBase group in _settingGroups.Keys)
                {
                    group.Save();
                }
            }
            catch (Exception ex)
            {
                string msg = $"Error saving settings: {ex.Message}";
                throw new Exception(msg, ex);
            }
        }
    }
}
