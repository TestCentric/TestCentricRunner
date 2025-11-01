// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public class MainFormSettings : SettingsGroup
    {
        public MainFormSettings(ISettings settings, string prefix)
            : base(settings, prefix + "MainForm") { }

        [UserScopedSetting]
        [DefaultSettingValue("10, 10")]
        public Point Location
        {
            get { return (Point)this[nameof(Location)]; }
            set { this[nameof(Location)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("700, 400")]
        public Size Size
        {
            get { return (Size)this[nameof(Size)]; }
            set { this[nameof(Size)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool Maximized
        {
            get { return (bool)this[nameof(Maximized)]; }
            set { this[nameof(Maximized)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("290")]
        public int SplitPosition
        {
            get { return (int)this[nameof(SplitPosition)]; }
            set { this[nameof(SplitPosition)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool ShowStatusBar
        {
            get { return (bool)this[nameof(ShowStatusBar)]; }
            set { this[nameof(ShowStatusBar)] = value; }
        }
    }
}
