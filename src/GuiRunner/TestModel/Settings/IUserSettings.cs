// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;

namespace TestCentric.Gui.Model.Settings
{
    /// <summary>
    /// Event handler for settings changes
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="args">The <see cref="SettingsEventArgs"/> instance containing the event data.</param>
    public delegate void SettingsEventHandler(object sender, SettingsEventArgs args);

    /// <summary>
    /// Event argument for settings changes
    /// </summary>
    public class SettingsEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsEventArgs"/> class.
        /// </summary>
        /// <param name="settingName">Name of the setting that has changed.</param>
        public SettingsEventArgs(string settingName)
        {
            SettingName = settingName;
        }

        /// <summary>
        /// Gets the name of the setting that has changed
        /// </summary>
        public string SettingName { get; private set; }
    }

    public interface IUserSettings
    {
        event SettingsEventHandler Changed;

        IGuiSettings Gui { get; }

        IEngineSettings Engine { get; }

        void SaveSettings();
    }
}
