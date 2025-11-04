// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

namespace TestCentric.Gui.Model.Settings
{
    [TestFixture]
    internal class TextOutputSettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui.TextOutput;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(TextOutputSettings.WordWrapEnabled), "true"),
            new TestCaseData(nameof(TextOutputSettings.Labels), "ON"),
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(TextOutputSettings.WordWrapEnabled), false),
            new TestCaseData(nameof(TextOutputSettings.Labels), "Off"),
        };
    }
}
