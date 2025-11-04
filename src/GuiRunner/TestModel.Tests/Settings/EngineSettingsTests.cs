// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TestCentric.Gui.Model.Settings 
{
    [TestFixture]
    internal class EngineSettingsTests : SettingTestsBase
    {
        [SetUp]
        public void Setup()
        {
            SettingGroup = UserSettings.Engine;
        }

        public static TestCaseData[] DefaultValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(EngineSettings.RerunOnChange), "False"),
            new TestCaseData(nameof(EngineSettings.ShadowCopyFiles), "True"),
            new TestCaseData(nameof(EngineSettings.Agents), "0"),
            new TestCaseData(nameof(EngineSettings.SetPrincipalPolicy), "False"),
            new TestCaseData(nameof(EngineSettings.PrincipalPolicy), "UnauthenticatedPrincipal")
        };

        public static TestCaseData[] SetValueTestCases = new TestCaseData[]
        {
            new TestCaseData(nameof(EngineSettings.RerunOnChange), false),
            new TestCaseData(nameof(EngineSettings.ShadowCopyFiles), true),
            new TestCaseData(nameof(EngineSettings.Agents), 0),
            new TestCaseData(nameof(EngineSettings.SetPrincipalPolicy), false),
            new TestCaseData(nameof(EngineSettings.PrincipalPolicy), "UnauthenticatedPrincipal")
        };
    }
}
