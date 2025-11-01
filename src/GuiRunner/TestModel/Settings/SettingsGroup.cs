// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public class SettingsGroup : ApplicationSettingsBase
    {
        protected ISettings _settingsService;

        public readonly string GroupPrefix;

        public SettingsGroup(ISettings settingsService, string groupPrefix)
        {
            _settingsService = settingsService;

            GroupPrefix = groupPrefix ?? string.Empty;

            if (GroupPrefix != string.Empty && !groupPrefix.EndsWith("."))
                GroupPrefix += ".";

            // Forward any changes from the engine
            _settingsService.Changed += (object s, SettingsEventArgs args) =>
            {
                Changed?.Invoke(s, args);
            };
        }

        #region ISettings Implementation

        public event SettingsEventHandler Changed;

        #endregion
    }
}
