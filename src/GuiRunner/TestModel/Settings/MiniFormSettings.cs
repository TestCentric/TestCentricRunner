// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public class MiniFormSettings : SettingsGroup
    {
        public MiniFormSettings(ISettings settings, string prefix)
            : base(settings, prefix + "MiniForm") { }

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
    }
}
