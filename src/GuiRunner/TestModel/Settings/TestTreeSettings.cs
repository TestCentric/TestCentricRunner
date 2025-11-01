// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    public class TestTreeSettings : SettingsGroup
    {
        public TestTreeSettings(
            ISettings settings,
            string prefix) : base(settings, prefix + "TestTree")
        {
            FixtureList = new FixtureListSettings(_settingsService, GroupPrefix);
            TestList = new TestListSettings(_settingsService, GroupPrefix);

            settings.Add(FixtureList);
            settings.Add(TestList);
        }

        public FixtureListSettings FixtureList { get; }

        public TestListSettings TestList { get; }

        [UserScopedSetting]
        [DefaultSettingValue("0")]
        public int InitialTreeDisplay
        {
            get { return (int)this[nameof(InitialTreeDisplay)]; }
            set { this[nameof(InitialTreeDisplay)] = value; }
        }

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

        public class FixtureListSettings : SettingsGroup
        {
            public FixtureListSettings(ISettings settings, string prefix) : base(settings, prefix + "FixtureList") { }

            [UserScopedSetting]
            [DefaultSettingValue("OUTCOME")]
            public string GroupBy
            {
                get { return (string) this[nameof(GroupBy)]; }
                set { this[nameof(GroupBy)] = value; }
            }
}

        public class TestListSettings : SettingsGroup
        {
            public TestListSettings(ISettings settings, string prefix) : base(settings, prefix + "TestList") { }

            [UserScopedSetting]
            [DefaultSettingValue("OUTCOME")]
            public string GroupBy
            {
                get { return (string)this[nameof(GroupBy)]; }
                set { this[nameof(GroupBy)] = value; }
            }
        }
    }
}

