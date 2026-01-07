// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public interface ITestTreeSettings
    {
        string AlternateImageSet { get; set; }

        bool ShowCheckBoxes { get; set; }

        bool ShowTestDuration { get; set; }

        string DisplayFormat { get; set; }

        bool ShowNamespace { get; set; }

        string NUnitGroupBy { get; set; }

        bool ShowFilter { get; set; }

        ITestListSettings TestList { get; }
    }

    public class TestTreeSettings : ApplicationSettingsBase, ITestTreeSettings
    {
        public ITestListSettings TestList { get; } = new TestListSettings();

        [UserScopedSetting]
        [DefaultSettingValue("Classic")]
        public string AlternateImageSet
        {
            get 
            {
                // Image set 'Default' is removed:
                // If Image set 'Default' is still present in settings, use the new default image set 'Classic' instead
                string imageSet = (string)this[nameof(AlternateImageSet)];
                if (imageSet == "Default")
                {
                    imageSet = "Classic";
                    AlternateImageSet = imageSet;
                }

                return imageSet;
            }
            set { this[nameof(AlternateImageSet)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool ShowCheckBoxes
        {
            get { return (bool)this[nameof(ShowCheckBoxes)]; }
            set { this[nameof(ShowCheckBoxes)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool ShowTestDuration
        {
            get { return (bool)this[nameof(ShowTestDuration)]; }
            set { this[nameof(ShowTestDuration)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("NUNIT_TREE")]
        public string DisplayFormat
        {
            get { return (string)this[nameof(DisplayFormat)]; }
            set { this[nameof(DisplayFormat)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool ShowNamespace
        {
            get { return (bool)this[nameof(ShowNamespace)]; }
            set { this[nameof(ShowNamespace)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("UNGROUPED")]
        public string NUnitGroupBy
        {
            get { return (string)this[nameof(NUnitGroupBy)]; }
            set { this[nameof(NUnitGroupBy)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool ShowFilter
        {
            get { return (bool)this[nameof(ShowFilter)]; }
            set { this[nameof(ShowFilter)] = value; }
        }
    }
}

