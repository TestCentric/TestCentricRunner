// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;

    /// <summary>
    /// SettingsStore implements the loading and saving of settings
    /// from an XML file and allows access to them via a key
    /// through the ISettings interface.
    /// </summary>
    public class SettingsStore : ISettings
    {
        static Logger log = InternalTrace.GetLogger("SettingsStore");

        protected Dictionary<string, object> _settings = new Dictionary<string, object>();

        private bool _writeable;
        private object _myLock;

        /// <summary>
        /// Construct a SettingsStore without a backing file - used for testing.
        /// </summary>
        public SettingsStore() { }

        /// <summary>
        /// Construct a SettingsStore with a file name and indicate whether it is writeable
        /// </summary>
        /// <param name="settingsFile"></param>
        /// <param name="writeable"></param>
        public SettingsStore(string settingsFile, bool writeable)
        {
            _writeable = writeable;
            _myLock = new object();
        }

        public void LoadSettings()
        {
            var userConfigFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);

            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = userConfigFile.FilePath };
                Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                var settings = configFile.AppSettings.Settings;
                foreach (KeyValueConfigurationElement setting in settings)
                {
                    SaveSetting(setting.Key, setting.Value);
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format("Error loading settings {0}. {1}", userConfigFile.FilePath, ex.Message);
                throw new Exception(msg, ex);
            }
        }

        public void SaveSettings()
        {
            if (!_writeable || _settings.Keys.Count <= 0)
                return;

            lock (_myLock)
            {
                try
                {
                    var userConfigFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                    ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = userConfigFile.FilePath };
                    Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

                    var settings = configFile.AppSettings.Settings;
                    List<string> keys = new List<string>(_settings.Keys);
                    keys.Sort();

                    foreach (string name in keys)
                    {
                        object value = GetSetting(name);
                        string valueToWrite = TypeDescriptor.GetConverter(value).ConvertToInvariantString(value);
                        if (settings[name] == null)
                        {
                            settings.Add(name, valueToWrite);
                        }
                        else
                        {
                            settings[name].Value = valueToWrite;
                        }
                    }

                    configFile.Save(ConfigurationSaveMode.Modified);
                }
                catch (Exception)
                {
                    // So we won't try this again
                    _writeable = false;
                    throw;
                }
            }
        }

        #region ISettings Implementation

        public event SettingsEventHandler Changed;

        /// <summary>
        /// Load the value of one of the group's settings
        /// </summary>
        /// <param name="settingName">The name of setting to load</param>
        /// <returns>The value of the setting</returns>
        public object GetSetting(string settingName)
        {
            return _settings.ContainsKey(settingName)
                ? _settings[settingName]
                : null;
        }

        /// <summary>
        /// Load the value of one of the group's settings or return a default value
        /// </summary>
        /// <param name="settingName">The name of setting to load</param>
        /// <param name="defaultValue">The value to return if the setting is not present</param>
        /// <returns>The value of the setting or the default</returns>
        public T GetSetting<T>(string settingName, T defaultValue)
        {
            object result = GetSetting(settingName);

            if (result == null)
                return defaultValue;

            if (result is T)
                return (T)result;

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                    return defaultValue;

                return (T)converter.ConvertFrom(null, CultureInfo.InvariantCulture, result);
            }
            catch (Exception ex)
            {
                log.Error("Unable to convert setting {0} to {1}", settingName, typeof(T).Name);
                log.Error(ex.Message);
                return defaultValue;
            }
        }

        /// <summary>
        /// Remove a setting from the group
        /// </summary>
        /// <param name="settingName">The name of the setting to remove</param>
        public void RemoveSetting(string settingName)
        {
            _settings.Remove(settingName);

            if (Changed != null)
                Changed(this, new SettingsEventArgs(settingName));
        }

        /// <summary>
        /// Remove a group of settings
        /// </summary>
        /// <param name="groupName">The name of the group to remove</param>
        public void RemoveGroup(string groupName)
        {
            List<string> keysToRemove = new List<string>();

            string prefix = groupName;
            if (!prefix.EndsWith("."))
                prefix = prefix + ".";

            foreach (string key in _settings.Keys)
                if (key.StartsWith(prefix))
                    keysToRemove.Add(key);

            foreach (string key in keysToRemove)
                _settings.Remove(key);
        }

        /// <summary>
        /// Save the value of one of the group's settings
        /// </summary>
        /// <param name="settingName">The name of the setting to save</param>
        /// <param name="settingValue">The value to be saved</param>
        public void SaveSetting(string settingName, object settingValue)
        {
            object oldValue = GetSetting(settingName);

            // Avoid signaling "changes" when there is not really a change
            if (oldValue != null)
            {
                if (oldValue is string && settingValue is string && (string)oldValue == (string)settingValue ||
                    oldValue is int && settingValue is int && (int)oldValue == (int)settingValue ||
                    oldValue is bool && settingValue is bool && (bool)oldValue == (bool)settingValue ||
                    oldValue is Enum && settingValue is Enum && oldValue.Equals(settingValue))
                    return;
            }

            _settings[settingName] = settingValue;

            if (Changed != null)
                Changed(this, new SettingsEventArgs(settingName));
        }

        #endregion
    }
}
