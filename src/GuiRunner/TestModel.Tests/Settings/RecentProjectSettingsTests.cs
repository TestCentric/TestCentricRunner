// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TestCentric.Gui.Model.Settings 
{
    [TestFixture]
    internal class RecentProjectSettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui.RecentProjects;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(RecentProjectsSettings.MaxFiles), "24"),
            new TestCaseData(nameof(RecentProjectsSettings.CheckFilesExist), "True"),
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(RecentProjectsSettings.MaxFiles), 10),
            new TestCaseData(nameof(RecentProjectsSettings.CheckFilesExist), false),
        };
    }
}
