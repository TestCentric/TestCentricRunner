// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public class ErrorDisplaySettings : SettingsGroup
    {
        public ErrorDisplaySettings(ISettings settings, string prefix)
             : base(settings, prefix + "ErrorDisplay") { }

        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int SplitterPosition
        {
            get { return (int)this[nameof(SplitterPosition)]; }
            set { this[nameof(SplitterPosition)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool WordWrapEnabled
        {
            get { return (bool)this[nameof(WordWrapEnabled)]; }
            set { this[nameof(WordWrapEnabled)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool ToolTipsEnabled
        {
            get { return (bool)this[nameof(ToolTipsEnabled)]; }
            set { this[nameof(ToolTipsEnabled)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool SourceCodeDisplay
        {
            get { return (bool)this[nameof(SourceCodeDisplay)]; }
            set { this[nameof(SourceCodeDisplay)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("Vertical")]
        public Orientation SourceCodeSplitterOrientation
        {
            get { return (Orientation)this[nameof(SourceCodeSplitterOrientation)]; }
            set { this[nameof(SourceCodeSplitterOrientation)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("0.3")]
        public float SourceCodeVerticalSplitterPosition
        {
            get { return (float)this[nameof(SourceCodeVerticalSplitterPosition)]; }
            set { this[nameof(SourceCodeVerticalSplitterPosition)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("0.3")]
        public float SourceCodeHorizontalSplitterPosition
        {
            get { return (float)this[nameof(SourceCodeHorizontalSplitterPosition)]; }
            set { this[nameof(SourceCodeHorizontalSplitterPosition)] = value; }
        }
    }
}
