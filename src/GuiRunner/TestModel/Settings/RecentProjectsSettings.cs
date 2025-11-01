// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public class RecentProjectsSettings : SettingsGroup
    {
        public RecentProjectsSettings(ISettings settings, string prefix)
            : base(settings, prefix + "RecentProjects") { }

        [UserScopedSetting]
        [DefaultSettingValue("24")]
        public int MaxFiles
        {
            get { return (int)this[nameof(MaxFiles)]; }
            set { this[nameof(MaxFiles)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool CheckFilesExist
        {
            get { return (bool)this[nameof(CheckFilesExist)]; }
            set { this[nameof(CheckFilesExist)] = value; }
        }
    }
}
