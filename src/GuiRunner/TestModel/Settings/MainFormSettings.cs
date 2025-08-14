// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;

namespace TestCentric.Gui.Model.Settings
{
    public class MainFormSettings : SettingsGroup
    {
        public MainFormSettings(ISettings settings, string prefix)
            : base(settings, prefix + "MainForm") { }

        public Point Location
        {
            get { return GetSetting(nameof(Location), new Point(10, 10)); }
            set { SaveSetting(nameof(Location), value); }
        }

        public Size Size
        {
            get { return GetSetting(nameof(Size), new Size(700, 400)); }
            set { SaveSetting(nameof(Size), value); }
        }

        public bool Maximized
        {
            get { return GetSetting(nameof(Maximized), false); }
            set { SaveSetting(nameof(Maximized), value); }
        }

        public int SplitPosition
        {
            get { return GetSetting(nameof(SplitPosition), 290); }
            set { SaveSetting(nameof(SplitPosition), value); }
        }

        public bool ShowStatusBar
        {
            get { return GetSetting(nameof(ShowStatusBar), true); }
            set { SaveSetting(nameof(ShowStatusBar), value); }
        }
    }
}
