// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

namespace TestCentric.Engine
{
    [TestFixture]
    internal class PackageSettingFactoryTests
    {
        [TestCase(nameof(SettingDefinitions.ShadowCopyFiles), "True", true)]
        [TestCase(nameof(SettingDefinitions.ShadowCopyFiles), "False", false)]
        [TestCase(nameof(SettingDefinitions.ShadowCopyFiles), "false", false)]
        [TestCase(nameof(SettingDefinitions.ShadowCopyFiles), "FALSE", false)]
        [TestCase(nameof(SettingDefinitions.TestRunTimeout), "10", 10)]
        [TestCase(nameof(SettingDefinitions.TestRunTimeout), "0", 0)]
        [TestCase(nameof(SettingDefinitions.TestRunTimeout), "-100", -100)]
        [TestCase(nameof(SettingDefinitions.SelectedAgentName), "Agent", "Agent")]
        [TestCase(nameof(SettingDefinitions.SelectedAgentName), "", "")]
        public void Create_KnownSetting_StringIsConvertedIntoTargetType(string settingName, string stringValue, object expectedValue)
        {
            // Act
            PackageSetting setting = PackageSettingFactory.Create(settingName, stringValue);

            // Assert
            Assert.That(setting.Value, Is.EqualTo(expectedValue));
        }

        [TestCase("UnknownSetting", "Value", "Value")]
        [TestCase("UnknownSetting", "100", "100")]
        [TestCase("UnknownSetting", "true", "true")]
        public void Create_UnknownSetting_StringIsStored(string settingName, string stringValue, object expectedValue)
        {
            // Act
            PackageSetting setting = PackageSettingFactory.Create(settingName, stringValue);

            // Assert
            Assert.That(setting.Value, Is.EqualTo(expectedValue));
        }
    }
}
