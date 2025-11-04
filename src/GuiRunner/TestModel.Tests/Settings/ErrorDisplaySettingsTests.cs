// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TestCentric.Gui.Model.Settings
{
    [TestFixture]
    internal class ErrorDisplaySettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui.ErrorDisplay;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(ErrorDisplaySettings.WordWrapEnabled), "true"),
            new TestCaseData(nameof(ErrorDisplaySettings.ToolTipsEnabled), "true"),
            new TestCaseData(nameof(ErrorDisplaySettings.SplitterPosition), "0"),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeDisplay), "true"),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeSplitterOrientation), "Vertical"),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeVerticalSplitterPosition), "0.3"),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeHorizontalSplitterPosition), "0.3"),

        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(ErrorDisplaySettings.WordWrapEnabled), true),
            new TestCaseData(nameof(ErrorDisplaySettings.ToolTipsEnabled), true),
            new TestCaseData(nameof(ErrorDisplaySettings.SplitterPosition), 0),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeDisplay), true),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeSplitterOrientation), Orientation.Horizontal),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeVerticalSplitterPosition), 0.4f),
            new TestCaseData(nameof(ErrorDisplaySettings.SourceCodeHorizontalSplitterPosition), 0.4f),
        };
    }
}
