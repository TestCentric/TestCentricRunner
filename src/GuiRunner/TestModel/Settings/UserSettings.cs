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

    public interface IUserSettings
    {
        /// <summary>
        /// Occurs when the settings are changed.
        /// </summary>
        event SettingsEventHandler Changed;

        IGuiSettings Gui { get; }

        IEngineSettings Engine { get; }

        void SaveSettings();
    }


    public class UserSettings : ApplicationSettingsBase, IUserSettings
    {
        private IDictionary<ApplicationSettingsBase, string> _settingGroups;

        public event SettingsEventHandler Changed;

        public UserSettings()
        {
            // Add all sub settings objects with their prefix
            _settingGroups = new Dictionary<ApplicationSettingsBase, string>()
            {
                { (ApplicationSettingsBase)Engine, "TestCentric.Engine" },
                { (ApplicationSettingsBase)Gui, "TestCentric.Gui" },
                { (ApplicationSettingsBase)Gui.MainForm, "TestCentric.Gui.MainForm" },
                { (ApplicationSettingsBase)Gui.MiniForm, "TestCentric.Gui.MiniForm" },
                { (ApplicationSettingsBase)Gui.ErrorDisplay, "TestCentric.Gui.ErrorDisplay" },
                { (ApplicationSettingsBase)Gui.RecentProjects, "TestCentric.Gui.RecentProjects" },
                { (ApplicationSettingsBase)Gui.RecentFiles, "TestCentric.Gui.RecentFiles" },
                { (ApplicationSettingsBase)Gui.TextOutput, "TestCentric.Gui.TextOutput" },
                { (ApplicationSettingsBase)Gui.TestTree, "TestCentric.Gui.TestTree" },
                { (ApplicationSettingsBase)Gui.TestTree.TestList, "TestCentric.Gui.TestTree.TestList" },
            };

            foreach (ApplicationSettingsBase settingsBase in _settingGroups.Keys)
                settingsBase.PropertyChanged += OnSettingChanged;
        }


        public IGuiSettings Gui { get; } = new GuiSettings();

        public IEngineSettings Engine { get; } = new EngineSettings();

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

        private void OnSettingChanged(object sender, PropertyChangedEventArgs args)
        {
            string prefix = string.Empty;
            if (sender is ApplicationSettingsBase settingsBase)
                prefix = _settingGroups[settingsBase];

            Changed?.Invoke(this, new SettingsEventArgs(prefix + "." + args.PropertyName));
        }
    }
}
