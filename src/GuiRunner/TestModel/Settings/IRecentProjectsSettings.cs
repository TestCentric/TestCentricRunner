// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    public interface IRecentProjectsSettings
    {
        int MaxFiles { get; set; }

        bool CheckFilesExist { get; set; }
    }
}
