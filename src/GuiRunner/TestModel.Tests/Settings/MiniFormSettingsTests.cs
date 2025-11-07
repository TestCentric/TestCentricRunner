// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;
using NUnit.Framework;

namespace TestCentric.Gui.Model.Settings
{
    [TestFixture]
    internal class MiniFormSettingsTests : SettingTestsBase
    {
        private static Point newLocationValue = new Point(100, 100);
        private static Size newSizeValue = new Size(100, 100);

        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui.MiniForm;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(MainFormSettings.Location), "10, 10"),
            new TestCaseData(nameof(MainFormSettings.Size), "700, 400"),
            new TestCaseData(nameof(MainFormSettings.Maximized), "false"),
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(MainFormSettings.Location), newLocationValue),
            new TestCaseData(nameof(MainFormSettings.Size), newSizeValue),
            new TestCaseData(nameof(MainFormSettings.Maximized), true),
        };
    }
}
