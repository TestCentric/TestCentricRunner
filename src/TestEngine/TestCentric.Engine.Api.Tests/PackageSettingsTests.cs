// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using NUnit.Engine;

namespace TestCentric.Engine
{
    [TestFixture]
    internal class PackageSettingsTests
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
            // Arrange
            PackageSettings settings = new PackageSettings();

            // Act
            settings.Add(settingName, stringValue);

            // Assert
            Assert.That(settings.GetSetting(settingName), Is.EqualTo(expectedValue));
        }

        [TestCase("UnknownSetting", "Value", "Value")]
        [TestCase("UnknownSetting", "100", "100")]
        [TestCase("UnknownSetting", "true", "true")]
        public void Create_UnknownSetting_StringIsStored(string settingName, string stringValue, object expectedValue)
        {
            // Arrange
            PackageSettings settings = new PackageSettings();

            // Act
            settings.Add(settingName, stringValue);

            // Assert
            Assert.That(settings.GetSetting(settingName), Is.EqualTo(expectedValue));
        }
    }
}
