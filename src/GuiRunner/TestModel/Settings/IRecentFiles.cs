// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Specialized;

namespace TestCentric.Gui.Model.Settings
{
    public interface IRecentFiles
    {
        string Latest { get; set; }

        StringCollection Entries { get; set; }
    }
}
