// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

namespace TestCentric.Gui.Model.Settings
{
    [TestFixture]
    internal class GuiSettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(GuiSettings.GuiLayout), "Full"),
            new TestCaseData(nameof(GuiSettings.LoadLastProject), "true"),
            new TestCaseData(nameof(GuiSettings.InitialSettingsPage), string.Empty),
            new TestCaseData(nameof(GuiSettings.ClearResultsOnReload), "false"),
            new TestCaseData(nameof(GuiSettings.GuiLayout), "Full"),
            new TestCaseData(nameof(GuiSettings.Font), "Microsoft Sans Serif, 8.25pt"),
            new TestCaseData(nameof(GuiSettings.FixedFont), "Courier New, 8.0pt"),
            new TestCaseData(nameof(GuiSettings.InternalTraceLevel), "Off"),
            new TestCaseData(nameof(GuiSettings.ProjectEditorPath), string.Empty),
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(GuiSettings.GuiLayout), "Full"),
            new TestCaseData(nameof(GuiSettings.LoadLastProject), true),
            new TestCaseData(nameof(GuiSettings.InitialSettingsPage), "Results"),
            new TestCaseData(nameof(GuiSettings.ClearResultsOnReload), true),
            new TestCaseData(nameof(GuiSettings.InternalTraceLevel), NUnit.Engine.InternalTraceLevel.Error),
            new TestCaseData(nameof(GuiSettings.ProjectEditorPath), "d:/temp"),
        };
    }
}
