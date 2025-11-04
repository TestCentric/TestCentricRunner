// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************


namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public interface ITestListSettings
    {
        string GroupBy { get; set; }
    }

    public class TestListSettings : ApplicationSettingsBase, ITestListSettings
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
