// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public interface IFixtureListSettings
    {
        string GroupBy { get; set; }
    }

    public class FixtureListSettings : ApplicationSettingsBase, IFixtureListSettings
    {
        [UserScopedSetting]
        [DefaultSettingValue("OUTCOME")]
        public string GroupBy
        {
            get { return (string)this[nameof(GroupBy)]; }
            set { this[nameof(GroupBy)] = value; }
        }
    }
}
