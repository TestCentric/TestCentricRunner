// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Linq;

    public interface IRecentFiles
    {
        string Latest { get; set; }

        StringCollection Entries { get; set; }
    }

    public class RecentFiles : ApplicationSettingsBase, IRecentFiles
    {
        private const int MAX_FILES = 24;

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
