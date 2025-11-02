// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings 
{
    using System.Collections.Specialized;
    using System.Linq;

    public sealed partial class RecentFiles 
    {
        private const int MAX_FILES = 24;

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
                    if (col.Count > MAX_FILES)
                        col.RemoveAt(MAX_FILES);

                    StringCollection strCol = new StringCollection();
                    strCol.AddRange(col.ToArray());
                    Entries = strCol;
                }
            }
        }
    }
}
