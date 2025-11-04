// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public interface ITextOutputSettings
    {
        string Labels { get; set; }

        bool WordWrapEnabled { get; set; }
    }

    public class TextOutputSettings : ApplicationSettingsBase, ITextOutputSettings
    {
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
