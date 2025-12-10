// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;
    using NUnit.Engine;

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

    public class GuiSettings : ApplicationSettingsBase, IGuiSettings
    {
        public ITestTreeSettings TestTree { get; } = new TestTreeSettings();

        public IRecentProjectsSettings RecentProjects { get; } = new RecentProjectsSettings();

        public IRecentFiles RecentFiles { get; } = new RecentFiles();

        public IMiniFormSettings MiniForm { get; } = new MiniFormSettings();

        public IMainFormSettings MainForm { get; } = new MainFormSettings();

        public IErrorDisplaySettings ErrorDisplay { get; } = new ErrorDisplaySettings();

        public ITextOutputSettings TextOutput { get; } = new TextOutputSettings();

        [UserScopedSetting]
        [DefaultSettingValue("Full")]
        public string GuiLayout
        {
            get { return (string)this[nameof(GuiLayout)]; }
            set { this[nameof(GuiLayout)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("true")]
        public bool LoadLastProject
        {
            get { return (bool)this[nameof(LoadLastProject)]; }
            set { this[nameof(LoadLastProject)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string InitialSettingsPage
        {
            get { return (string)this[nameof(InitialSettingsPage)]; }
            set { this[nameof(InitialSettingsPage)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool ClearResultsOnReload
        {
            get { return (bool)this[nameof(ClearResultsOnReload)]; }
            set { this[nameof(ClearResultsOnReload)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("Microsoft Sans Serif, 8.25pt")]
        public Font Font
        {
            get { return (Font)this[nameof(Font)]; }
            set { this[nameof(Font)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("Courier New, 8.0pt")]
        public Font FixedFont
        {
            get { return (Font)this[nameof(FixedFont)]; }
            set { this[nameof(FixedFont)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("Off")]
        public InternalTraceLevel InternalTraceLevel
        {
            get { return (InternalTraceLevel)this[nameof(InternalTraceLevel)]; }
            set { this[nameof(InternalTraceLevel)] = value; }
        }

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string ProjectEditorPath
        {
            get { return (string)this[nameof(ProjectEditorPath)]; }
            set { this[nameof(ProjectEditorPath)] = value; }
        }
    }
}
