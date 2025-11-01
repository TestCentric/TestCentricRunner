// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel;

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
            _myLock = new object();
            SettingGroups = new List<ApplicationSettingsBase>();
        }

        private IList<ApplicationSettingsBase> SettingGroups { get; }

        public void Add(ApplicationSettingsBase group)
        {
            SettingGroups.Add(group);
            group.PropertyChanged += OnSettingChanged;
        }

        private void OnSettingChanged(object sender, PropertyChangedEventArgs args)
        {
            string prefix = String.Empty;
            if (sender is SettingsGroup group)
                prefix = group.GroupPrefix;

            var a = new SettingsEventArgs(prefix + args.PropertyName);
            Changed?.Invoke(sender, a);
        }

        public void SaveSettings()
        {
            lock (_myLock)
            {
                try
                {
                    foreach (ApplicationSettingsBase group in SettingGroups)
                    {
                        group.Save();
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"Error saving settings: {ex.Message}";
                    throw new Exception(msg, ex);
                }
            }
        }

        #region ISettings Implementation

        public event SettingsEventHandler Changed;

        #endregion
    }
}
