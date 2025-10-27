// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using NUnit.Engine;

namespace TestCentric.Engine
{
    /// <summary>
    /// PackageSettingList contains all the PackageSettings for a TestPackage.
    /// It supports enumeration of the settings and various operations on
    /// individual settings in the list.
    /// </summary>
    /// <remarks>
    /// This implementation extends the NUnit's PackageSettings class to
    /// replace one method and add two new ones. This is hopefully only 
    /// a temporary workaround until the NUnit class is modified.
    /// </remarks>
    public class PackageSettings : NUnit.Engine.PackageSettings
    {
        private static readonly FieldInfo _settingsField = typeof(NUnit.Engine.PackageSettings)
            .GetField("_settings", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);
        private readonly Dictionary<string, PackageSetting> _settingsDictionary;

        public PackageSettings()
        {
            _settingsDictionary = (Dictionary<string, PackageSetting>)_settingsField.GetValue(this);
        }

        /// <summary>
        /// Creates and adds a setting to the list, specified by the name and a string value.
        /// The string value is converted to a typed PackageSetting if the name specifies a known SettingDefinition.
        /// </summary>
        /// <param name="name">The name of the setting.</param>
        /// <param name="value">The corresponding value to set.</param>
        public new void Add(string name, string value) // Different from NUnit
        {
            SettingDefinition definition = PackageSettingHelper.LookupSetting(name);
            if (definition is null)
                Add(new PackageSetting<string>(name, value));
            else if (definition.ValueType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                Add(new PackageSetting<bool>(name, boolValue));
            else if (definition.ValueType == typeof(int) && int.TryParse(value, out int intValue))
                Add(new PackageSetting<int>(name, intValue));
            else if (definition.ValueType == typeof(string))
                Add(new PackageSetting<string>(name, value));
            else
                throw new NotSupportedException($"Unsupported type {definition.ValueType}");
        }

        public void Remove (SettingDefinition setting) // Not in NUnit
        {
            _settingsDictionary.Remove(setting.Name);
        }

        public void Remove (string settingName) // Not in NUnit
        {
            _settingsDictionary.Remove(settingName);
        }
    }
}
