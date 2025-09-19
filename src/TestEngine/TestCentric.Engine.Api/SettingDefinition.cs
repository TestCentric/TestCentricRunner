// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;

namespace TestCentric.Engine
{
    /// <summary>
    ///
    /// </summary>
    public abstract class SettingDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingDefinition"/> class.
        /// </summary>
        public SettingDefinition(string name, Type valueType)
        {
            Name = name;
            ValueType = valueType;
        }

        /// <summary>
        /// Gets the name of this setting.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Overridden in each derived definition to return the Type
        /// required for the value of the setting.
        /// </summary>
        public Type ValueType { get; }

        /// <summary>
        /// Create a PackageSetting based on this definition.
        /// </summary>
        /// <param name="value">The value to assign the setting.</param>
        /// <returns>A PackageSetting.</returns>
        public PackageSetting WithValue<T>(T value)
            where T : notnull
        {
            return new PackageSetting<T>(Name, value);
        }

        /// <summary>
        /// Create a PackageSetting based on this definition.
        /// </summary>
        /// <param name="value">A string value which is parsed to the target type (ValueType).</param>
        /// <returns>A PackageSetting.</returns>
        public PackageSetting WithParsedValue(string value)
        {
            if (ValueType == typeof(bool) && bool.TryParse(value, out bool boolValue))
                return new PackageSetting<bool>(Name, boolValue);
            if (ValueType == typeof(int) && int.TryParse(value, out int intValue))
                return new PackageSetting<int>(Name, intValue);
            if (ValueType == typeof(string))
                return new PackageSetting<string>(Name, value);

            throw new NotSupportedException($"Unsupported type {ValueType}");
        }

    }

    /// <summary>
    /// SettingDefinition class is used as a sort of template for creating settings.
    /// It specifies the name of the setting and the Type required for its value but not
    /// the value itself. Its WithValue method is used to generate an actual PackageSetting.
    /// </summary>
    public sealed class SettingDefinition<T> : SettingDefinition
        where T : notnull
    {
        /// <summary>
        /// Construct a SettingDefinition
        /// </summary>
        /// <param name="name">Name of this setting</param>
        /// <param name="defaultValue">The default value used when the setting is not provided</param>
        public SettingDefinition(string name, T defaultValue)
            : base(name, typeof(T))
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// The Default value used if the setting is not present
        /// </summary>
        public T DefaultValue { get; }

        /// <summary>
        /// Create a PackageSetting based on this definition.
        /// </summary>
        /// <param name="value">The value to assign the setting.</param>
        /// <returns>A PackageSetting.</returns>
        public PackageSetting<T> WithValue(T value)
        {
            return new PackageSetting<T>(Name, value);
        }
    }
}
