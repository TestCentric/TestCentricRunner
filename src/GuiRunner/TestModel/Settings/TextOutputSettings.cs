// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public class TextOutputSettings : SettingsGroup
    {
        public TextOutputSettings(ISettings settings, string prefix)
             : base(settings, prefix + "TextOutput") { }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool WordWrapEnabled
        {
            get { return (bool)this[nameof(WordWrapEnabled)]; }
            set { this[nameof(WordWrapEnabled)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("ON")]
        public string Labels
        {
            get { return (string)this[nameof(Labels)]; }
            set { this[nameof(Labels)] = value; }
        }
    }
}
