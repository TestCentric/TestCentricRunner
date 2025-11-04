// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;

namespace TestCentric.Gui.Model.Settings
{
    public interface IGuiSettings
    {
        ITestTreeSettings TestTree { get; }

        IRecentProjectsSettings RecentProjects { get; }

        IRecentFiles RecentFiles { get; }

        IMiniFormSettings MiniForm { get; }

        IMainFormSettings MainForm { get; }

        IErrorDisplaySettings ErrorDisplay { get; }

        ITextOutputSettings TextOutput { get; }

        string GuiLayout { get; set; }

        bool LoadLastProject { get; set; }

        string InitialSettingsPage { get; set; }

        bool ClearResultsOnReload { get; set; }

        Font Font { get; set; }

        Font FixedFont { get; set; }

        InternalTraceLevel InternalTraceLevel { get; set; }

        string ProjectEditorPath { get; set; }
    }
}
