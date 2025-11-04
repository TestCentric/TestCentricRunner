// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Configuration;
using System.Runtime;
using Castle.Core.Internal;
using NUnit.Framework;

namespace TestCentric.Gui.Model.Settings
{
    internal abstract class SettingTestsBase
    {
        protected UserSettings UserSettings { get; private set; }
        protected object SettingGroup { get; set; }

        [SetUp]
        public void SetUp()
        {
            UserSettings = new UserSettings();
        }

        [TestCaseSource("DefaultValueTestCases")]
        public void CheckDefaultValue(string propertyName, object defaultValue)
        {
            // Arrange
            Type settingGroupType = SettingGroup.GetType();
            var propInfo = settingGroupType.GetProperty(propertyName);

            // Act
            var attribute = propInfo.GetAttribute<DefaultSettingValueAttribute>();

            // Assert
            Assert.That(attribute.Value, Is.EqualTo(defaultValue));
        }

        [TestCaseSource("SetValueTestCases")]
        public void SetValue(string propertyName, object value)
        {
            // Arrange
            string changedSetting = string.Empty;
            UserSettings.Changed += (sender, args) => { changedSetting = args.SettingName; };

            Type settingGroupType = SettingGroup.GetType();
            var propInfo = settingGroupType.GetProperty(propertyName);

            // Act
            propInfo.SetValue(SettingGroup, value);

            // Assert
            Assert.That(changedSetting, Contains.Substring(propertyName));
        }
    }
}
