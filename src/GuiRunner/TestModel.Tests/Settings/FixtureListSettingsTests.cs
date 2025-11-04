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
    internal class FixtureListSettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui.TestTree.FixtureList;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(FixtureListSettings.GroupBy), "OUTCOME"),
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(FixtureListSettings.GroupBy), "CATEGORY"),
        };
    }
}
