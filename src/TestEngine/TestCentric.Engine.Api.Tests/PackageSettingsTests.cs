// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

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
        public void Add_Setting_AsString(string settingName, string stringValue, object expectedValue)
        {
            // Arrange
            PackageSettings settings = new PackageSettings();

            // Act
            settings.Add(settingName, stringValue);

            // Assert
            var value = settings.GetSetting(settingName);
            Assert.That(value, Is.EqualTo(expectedValue));
        }

        [TestCase("UnknownSetting", "Value", "Value")]
        public void Add_UnknownSetting_AsString(string settingName, string stringValue, string expectedValue)
        {
            // Arrange
            PackageSettings settings = new PackageSettings();

            // Act
            settings.Add(settingName, stringValue);

            // Assert
            Assert.That(settings.HasSetting(settingName), Is.True);

            var value = settings.GetSetting(settingName);
            Assert.That(value, Is.EqualTo(expectedValue));
        }
    }
}
