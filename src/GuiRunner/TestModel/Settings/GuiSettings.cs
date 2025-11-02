// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************


namespace TestCentric.Gui.Model.Settings {
    
    public sealed partial class GuiSettings 
    {
        public TestTreeSettings TestTree => TestTreeSettings.Default;

        public RecentProjectsSettings RecentProjects => RecentProjectsSettings.Default;

        public RecentFiles RecentFiles => RecentFiles.Default;

        public MiniFormSettings MiniForm => MiniFormSettings.Default;

        public MainFormSettings MainForm => MainFormSettings.Default;

        public ErrorDisplaySettings ErrorDisplay => ErrorDisplaySettings.Default;

        public TextOutputSettings TextOutput => TextOutputSettings.Default;
    }
}
