// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

namespace TestCentric.Gui.Model.Settings
{
    [TestFixture]
    internal class TestTreeSettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui.TestTree;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(TestTreeSettings.InitialTreeDisplay), "0"),
            new TestCaseData(nameof(TestTreeSettings.AlternateImageSet), "Classic"),
            new TestCaseData(nameof(TestTreeSettings.ShowCheckBoxes), "false"),
            new TestCaseData(nameof(TestTreeSettings.ShowTestDuration), "false"),
            new TestCaseData(nameof(TestTreeSettings.DisplayFormat), "NUNIT_TREE"),
            new TestCaseData(nameof(TestTreeSettings.ShowNamespace), "true"),
            new TestCaseData(nameof(TestTreeSettings.NUnitGroupBy), "UNGROUPED"),
            new TestCaseData(nameof(TestTreeSettings.ShowFilter), "true"),
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(TestTreeSettings.InitialTreeDisplay), 1),
            new TestCaseData(nameof(TestTreeSettings.AlternateImageSet), "Circle"),
            new TestCaseData(nameof(TestTreeSettings.ShowCheckBoxes), false),
            new TestCaseData(nameof(TestTreeSettings.ShowTestDuration), true),
            new TestCaseData(nameof(TestTreeSettings.DisplayFormat), "TEST_LIST"),
            new TestCaseData(nameof(TestTreeSettings.ShowNamespace), true),
            new TestCaseData(nameof(TestTreeSettings.NUnitGroupBy), "CATEGORY"),
            new TestCaseData(nameof(TestTreeSettings.ShowFilter), false),
        };
    }
}
