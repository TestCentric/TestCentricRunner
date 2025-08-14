// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using TestCentric.Gui.Model.Settings;

namespace TestCentric.Gui.Model.Services
{
    /// <summary>
    /// Summary description for UserSettingsService.
    /// </summary>
    public class SettingsService : SettingsStore
    {
        private const string SETTINGS_FILE = "TestCentricSettings.xml";
        private static readonly string APPLICATION_DIRECTORY = 
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TestCentric");

        public SettingsService(bool writeable)
            : base(Path.Combine(APPLICATION_DIRECTORY, SETTINGS_FILE), writeable) { }
    }
}
