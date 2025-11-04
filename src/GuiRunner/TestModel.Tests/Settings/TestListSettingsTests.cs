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
    internal class TestListSettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Gui.TestTree.TestList;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(TestListSettings.GroupBy), "OUTCOME"),
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(TestListSettings.GroupBy), "CATEGORY"),
        };
    }
}
