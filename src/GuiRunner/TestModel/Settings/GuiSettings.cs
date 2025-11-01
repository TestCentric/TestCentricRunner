// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;

namespace TestCentric.Gui.Model.Settings
{
    using System.Configuration;
    using TestCentric.Gui.Model.Services;

    /// <summary>
    /// Settings specific to TestCentric. Because we store settings in the
    /// NUnit 3 settings file, we use our own unique prefix to avoid conflicts.
    /// </summary>
    public class GuiSettings : SettingsGroup
    {
        public GuiSettings(ISettings settings, string prefix) : base(settings, prefix + "Gui")
        {
            TestTree = new TestTreeSettings(_settingsService, GroupPrefix);
            RecentProjects = new RecentProjectsSettings(_settingsService, GroupPrefix);
            RecentFiles = new RecentFiles(settings);
            MainForm = new MainFormSettings(_settingsService, GroupPrefix);
            MiniForm = new MiniFormSettings(_settingsService, GroupPrefix);
            ErrorDisplay = new ErrorDisplaySettings(_settingsService, GroupPrefix);
            TextOutput = new TextOutputSettings(_settingsService, GroupPrefix);

            settings.Add(TestTree);
            settings.Add(RecentProjects);
            settings.Add(RecentFiles);
            settings.Add(MainForm);
            settings.Add(MiniForm);
            settings.Add(ErrorDisplay);
            settings.Add(TextOutput);
        }

        public TestTreeSettings TestTree { get; }

        public RecentProjectsSettings RecentProjects { get; }

        public RecentFiles RecentFiles { get; }

        public MiniFormSettings MiniForm { get; }

        public MainFormSettings MainForm { get; }

        public ErrorDisplaySettings ErrorDisplay { get; }

        public TextOutputSettings TextOutput { get; }

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
