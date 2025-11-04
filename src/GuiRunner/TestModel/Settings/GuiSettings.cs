// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************


namespace TestCentric.Gui.Model.Settings {
    
    public sealed partial class GuiSettings : IGuiSettings
    {
        public ITestTreeSettings TestTree => TestTreeSettings.Default;

        public IRecentProjectsSettings RecentProjects => RecentProjectsSettings.Default;

        public IRecentFiles RecentFiles => Settings.RecentFiles.Default;

        public IMiniFormSettings MiniForm => MiniFormSettings.Default;

        public IMainFormSettings MainForm => MainFormSettings.Default;

        public IErrorDisplaySettings ErrorDisplay => ErrorDisplaySettings.Default;

        public ITextOutputSettings TextOutput => TextOutputSettings.Default;
    }
}
