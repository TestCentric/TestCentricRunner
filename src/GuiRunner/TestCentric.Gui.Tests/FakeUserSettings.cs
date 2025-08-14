// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Fakes
{
    public class UserSettings : TestCentric.Gui.Model.Settings.UserSettings
    {
        public UserSettings() : base(new TestCentric.Gui.Model.Settings.SettingsStore()) { }
    }
}
