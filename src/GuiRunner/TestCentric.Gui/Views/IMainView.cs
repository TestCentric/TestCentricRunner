// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Drawing;
using System.Windows.Forms;

namespace TestCentric.Gui.Views
{
    using Controls;
    using Elements;

    public interface IMainView
    {
        // Title Bar
        string Title { get; set; }

        // View Parameters
        Point Location { get; set; }
        Size Size { get; set; }
        bool Maximized { get; set; }
        Font Font { get; set; }
        int SplitterPosition { get; set; }

        // UI Elements
        ISelection ResultTabs { get; }

        // File Menu Items
        IPopup FileMenu { get; }
        ICommand OpenProjectCommand { get; }

        ICommand OpenTestCentricProjectCommand { get; }

        ICommand OpenTestAssemblyCommand { get; }

        ICommand SaveProjectCommand { get; }

        ICommand CloseProjectCommand { get; }
        ICommand AddTestFilesCommand { get; }
        ICommand ReloadTestsCommand { get; }
        IPopup SelectAgentMenu { get; }
        IChecked RunAsX86 { get; }
        IPopup RecentFilesMenu { get; }
        ICommand ExitCommand { get; }

        // View Menu Items
        ISelection GuiLayout { get; }
        ICommand IncreaseFontCommand { get; }
        ICommand DecreaseFontCommand { get; }
        ICommand ChangeFontCommand { get; }
        ICommand RestoreFontCommand { get; }
        ICommand IncreaseFixedFontCommand { get; }
        ICommand DecreaseFixedFontCommand { get; }
        ICommand RestoreFixedFontCommand { get; }

        // Tools Menu Items
        IPopup ToolsMenu { get; }
        ICommand SaveResultsCommand { get; }
        IPopup SaveResultsAsMenu { get; }
        ICommand OpenWorkDirectoryCommand { get; }
        ICommand ExtensionsCommand { get; }
        ICommand SettingsCommand { get; }

        // Help Menu Items
        ICommand TestCentricHelpCommand { get; }
        ICommand NUnitHelpCommand { get; }
        ICommand CommandLineHelpCommand { get; }
        ICommand AboutCommand { get; }

        // Toolbar Items
        ICommand RunAllButton { get; }
        ICommand RunSelectedButton { get; }
        ICommand RerunButton { get; }
        ICommand RunFailedButton { get; }

        ICommand StopRunButton { get; }
        ICommand ForceStopButton { get; }

        IViewElement DisplayFormatButton { get; }
        ISelection DisplayFormat { get; }
        ISelection TestListGroupBy { get; }
        ISelection NUnitGroupBy { get; }
        ISelection FixtureListGroupBy { get; }

        IChecked ShowNamespace { get; }
        IChecked ShowHideFilterButton { get; }
        ICommand RunParametersButton { get; }

        // SubViews accessible from the main view or presenter
        ITestTreeView TreeView { get; }
        IStatusBarView StatusBarView { get; }
        IProgressBarView ProgressBarView { get; }
        ITestPropertiesView TestPropertiesView { get; }
        ITestResultSubView TestResultSubView { get; }
        IMessageDisplay MessageDisplay { get; }

        // Dialog Manager
        IDialogManager DialogManager { get; }

        // Methods used by Presenter
        void Configure(bool useFullGui);

        // Form methods that we have to use
        void Close();

        // Form Events that we use
        event EventHandler Load;
        event EventHandler Shown;
        event EventHandler Move;
        event EventHandler Resize;
        event FormClosingEventHandler FormClosing;

        // Our own events
        event EventHandler SplitterPositionChanged;
    }
}
