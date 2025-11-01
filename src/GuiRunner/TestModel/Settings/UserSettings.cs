// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    public class UserSettings : SettingsGroup
    {
        public UserSettings(ISettings settings) : base(settings, "TestCentric")
        {
            Gui = new GuiSettings(settings, GroupPrefix);
            Engine = new EngineSettings(settings, GroupPrefix);

            settings.Add(Gui);
            settings.Add(Engine);
        }

        public GuiSettings Gui { get; }

        public EngineSettings Engine { get; }
    }
}
