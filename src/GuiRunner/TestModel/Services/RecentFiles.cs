// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using TestCentric.Gui.Model.Settings;

namespace TestCentric.Gui.Model.Services
{
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Linq;

    public class RecentFiles : SettingsGroup
    {
        private const string PREFIX = "TestCentric.Gui.RecentProjects";

        private const int MAX_FILES = 24;

        public RecentFiles(ISettings userSettings) : base(userSettings, PREFIX)
        {
        }

        public int MaxFiles { get; } = MAX_FILES;

        public string Latest
        {
            get { return Entries.Count == 0 ? null : Entries[0]; }
            set
            {
                if (Latest != value)
                {
                    var col = Entries.Cast<string>().ToList();
                    col.Remove(value);

                    col.Insert(0, value);
                    if (col.Count > MaxFiles)
                        col.RemoveAt(MaxFiles);

                    StringCollection strCol = new StringCollection();
                    strCol.AddRange(col.ToArray());
                    Entries = strCol;
                }
            }
        }

        [UserScopedSetting]
        [DefaultSettingValue("<?xml version = \"1.0\" encoding=\"utf-16\"?><ArrayOfString xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"></ArrayOfString>")]
        public StringCollection Entries
        {
            get { return (StringCollection)this[nameof(Entries)]; }
            set { this[nameof(Entries)] = value; }
        }

        public void Remove(string fileName)
        {
            Entries.Remove(fileName);
        }
    }
}
