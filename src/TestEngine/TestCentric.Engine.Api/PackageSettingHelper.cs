// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;

using SettingDefinition = NUnit.Engine.SettingDefinition;

namespace TestCentric.Engine
{
    /// <summary>
    /// Factory class to create PackageSetting objects
    /// </summary>
    public static class PackageSettingHelper
    {
        private static readonly FieldInfo _settingsField = typeof(NUnit.Engine.PackageSettings)
            .GetField("_settings", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField);

        public static SettingDefinition LookupSetting(string name)
        {
            // Get a list of all public properties
            var properties = typeof(NUnit.Common.SettingDefinitions).GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo propertyInfo in properties)
                if (propertyInfo.Name == name)
                    return propertyInfo.GetValue(null, null) as SettingDefinition;

            return null;

        }

        /// <summary>
        /// Creates a PackageSetting<T> object
        /// If the name is a known SettingDefinition name, the string is converted to the TargeType of that SettingDefinition.
        /// Otherwise a PackageSetting<string> object is created 
        /// This method is required to deserialize PackageSettings from a string representation back to a typed PackageSetting<T>
        public static NUnit.Engine.PackageSetting CreateSettingWithParsedValue(this NUnit.Engine.PackageSettings settings, string name, string value)
        {
            SettingDefinition definition = LookupSetting(name);
            if (definition is null)
                return new NUnit.Engine.PackageSetting<string>(name, value);
            else if (definition.ValueType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                return new NUnit.Engine.PackageSetting<bool>(name, boolValue);
            else if (definition.ValueType == typeof(int) && int.TryParse(value, out int intValue))
                return new NUnit.Engine.PackageSetting<int>(name, intValue);
            else if (definition.ValueType == typeof(string))
                return new NUnit.Engine.PackageSetting<string>(name, value);
            else
                throw new NotSupportedException($"Unsupported type {definition.ValueType}");
        }

        public static void Remove(this NUnit.Engine.PackageSettings settings, SettingDefinition setting)
        {
            var settingsDictionary = (Dictionary<string, NUnit.Engine.PackageSetting>)_settingsField.GetValue(settings);
            settingsDictionary.Remove(setting.Name);
        }

        public static void Remove(this NUnit.Engine.PackageSettings settings, string settingName)
        {
            var settingsDictionary = (Dictionary<string, NUnit.Engine.PackageSetting>)_settingsField.GetValue(settings);
            settingsDictionary.Remove(settingName);
        }
    }
}
