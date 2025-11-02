// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    public class SettingsGroup : ISettings
    {
        protected ISettings _settingsService;

        public readonly string GroupPrefix;

        public SettingsGroup(ISettings settingsService)
        {
            _settingsService = settingsService;
        }

        #region ISettings Implementation

        public event SettingsEventHandler Changed;

        protected void InvokeChanged(string settingName)
        {
            var b = new SettingsEventArgs(settingName);
            Changed?.Invoke(this, b);
        }

        #endregion
    }
}
