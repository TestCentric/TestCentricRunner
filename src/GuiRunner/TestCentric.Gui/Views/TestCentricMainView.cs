// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TestCentric.Gui.Views
{
    using Controls;
    using Elements;
    using TestCentric.Gui.Elements.ToolStripElements;

    public partial class TestCentricMainView : TestCentricFormBase, IMainView
    {
        #region Construction and Disposal

        public TestCentricMainView() : base("TestCentric")
        {
            InitializeComponent();

            treeSplitter.SplitterMoved += (s, e) =>
            {
                SplitterPositionChanged?.Invoke(s, e);
            };

            // UI Elements on main form
            ResultTabs = new TabSelector(resultTabs);

            // Initialize File Menu Commands
            FileMenu = new PopupMenuElement(fileMenu);
            NewProjectCommand = new CommandMenuElement(newProjectMenuItem);
            OpenProjectCommand = new CommandMenuElement(openProjectMenuItem);
            OpenTestCentricProjectCommand = new CommandMenuElement(openTestCentricProjectMenuItem);
            OpenTestAssemblyCommand = new CommandMenuElement(openTestAssemblyMenuItem);

            SaveProjectCommand = new CommandMenuElement(saveProjectMenuItem);
            CloseProjectCommand = new CommandMenuElement(closeMenuItem);
            AddTestFilesCommand = new CommandMenuElement(addTestFileMenuItem);
            ReloadTestsCommand = new CommandMenuElement(reloadTestsMenuItem);
            TestRunSettingsCommand = new CommandMenuElement(testRunSettingsMenuItem);
            SelectAgentMenu = new PopupMenuElement(selectAgentMenu);
            RunAsX86 = new CheckedMenuElement(runAsX86MenuItem);
            RecentFilesMenu = new PopupMenuElement(recentFilesMenu);
            ExitCommand = new CommandMenuElement(exitMenuItem);

            // Initialize View Menu Commands
            GuiLayout = new CheckedToolStripMenuGroup("", fullGuiMenuItem, miniGuiMenuItem);
            IncreaseFontCommand = new CommandMenuElement(increaseFontMenuItem);
            DecreaseFontCommand = new CommandMenuElement(decreaseFontMenuItem);
            ChangeFontCommand = new CommandMenuElement(fontChangeMenuItem);
            RestoreFontCommand = new CommandMenuElement(defaultFontMenuItem);
            IncreaseFixedFontCommand = new CommandMenuElement(increaseFixedFontMenuItem);
            DecreaseFixedFontCommand = new CommandMenuElement(decreaseFixedFontMenuItem);
            RestoreFixedFontCommand = new CommandMenuElement(restoreFixedFontMenuItem);

            // Initialize Tools Menu Comands
            ToolsMenu = new PopupMenuElement(toolsMenu);
            SaveResultsCommand = new PopupMenuElement(saveResultsMenuItem);
            TransformResultsCommand = new CommandMenuElement(transformResultsMenuItem);
            OpenWorkDirectoryCommand = new CommandMenuElement(openWorkDirectoryMenuItem);
            ExtensionsCommand = new CommandMenuElement(extensionsMenuItem);
            SettingsCommand = new CommandMenuElement(settingsMenuItem);

            // Initialize Help Menu
            TestCentricHelpCommand = new CommandMenuElement(testCentricHelpMenuItem);
            NUnitHelpCommand = new CommandMenuElement(nunitHelpMenuItem);
            CommandLineHelpCommand = new CommandMenuElement(commandLineHelpMenuItem);
            AboutCommand = new CommandMenuElement(aboutMenuItem);

            // Initialize Toolbar
            RunAllButton = new ToolStripButtonElement(runAllButton);
            RunSelectedButton = new ToolStripButtonElement(runSelectedButton);
            RerunButton = new ToolStripButtonElement(rerunButton);
            RunFailedButton = new ToolStripButtonElement(runFailedButton);
            StopRunButton = new ToolStripButtonElement(stopRunButton);
            ForceStopButton = new ToolStripButtonElement(forceStopButton);
            DisplayFormatButton = new DisplayFormatButton(displayFormatButton);
            DisplayFormat = new CheckedToolStripMenuGroup(
                "displayFormat",
                nunitTreeMenuItem, testListMenuItem);
            TestListGroupBy = new CheckedToolStripMenuGroup(
                "TestListGroupBy",
                testListUngroupedMenuItem, testListByAssemblyMenuItem, testListByFixtureMenuItem, testListByCategoryMenuItem, testListByOutcomeMenuItem, testListByDurationMenuItem);
            ShowNamespaces = new CheckedMenuElement(nunitTreeShowNamespacesMenuItem);
            ShowHideFilterButton = new ToolStripButtonElement(showFilterButton);
            RunParametersButton = new ToolStripButtonElement(runParametersButton);

            DialogManager = new DialogManager();
        }

        #endregion

        #region Events and Properties

        public event EventHandler SplitterPositionChanged;

        public string Title
        {
            get { return Text; }
            set { Text = value ?? "UNNAMED.tcproj"; }
        }

        public bool Maximized
        {
            get { return WindowState == FormWindowState.Maximized; }
            set
            {
                if (value)
                    WindowState = FormWindowState.Maximized;
                else if (WindowState == FormWindowState.Maximized)
                    WindowState = FormWindowState.Normal;
                // No actionif minimized
            }
        }

        public int SplitterPosition
        {
            get { return treeSplitter.SplitPosition; }
            set { treeSplitter.SplitPosition = value; }
        }

        // UI Elements
        public ISelection ResultTabs { get; }

        // File Menu Items
        public IPopup FileMenu { get; }
        public ICommand NewProjectCommand { get; }
        public ICommand OpenProjectCommand { get; }
        public ICommand OpenTestCentricProjectCommand { get; }
        public ICommand OpenTestAssemblyCommand { get; }
        public ICommand SaveProjectCommand { get; }
        public ICommand CloseProjectCommand { get; }
        public ICommand AddTestFilesCommand { get; }
        public ICommand ReloadTestsCommand { get; }
        public IPopup SelectAgentMenu { get; }
        public IChecked RunAsX86 { get; private set; }
        public IPopup RecentFilesMenu { get; }
        public ICommand ExitCommand { get; }

        // View Menu Items
        public ISelection GuiLayout { get; }
        public ICommand IncreaseFontCommand { get; }
        public ICommand DecreaseFontCommand { get; }
        public ICommand ChangeFontCommand { get; }
        public ICommand RestoreFontCommand { get; }
        public ICommand IncreaseFixedFontCommand { get; }
        public ICommand DecreaseFixedFontCommand { get; }
        public ICommand RestoreFixedFontCommand { get; }

        // Tools Menu Items
        public IPopup ToolsMenu { get; }
        public IPopup SaveResultsCommand { get; }
        public ICommand TransformResultsCommand { get; }
        public ICommand OpenWorkDirectoryCommand { get; }
        public ICommand ExtensionsCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand TestRunSettingsCommand { get; }

        // Help Menu Items
        public ICommand TestCentricHelpCommand { get; }
        public ICommand NUnitHelpCommand { get; }
        public ICommand CommandLineHelpCommand { get; }
        public ICommand AboutCommand { get; }

        // ToolBar Elements
        public ICommand RunAllButton { get; private set; }
        public ICommand RunSelectedButton { get; private set; }
        public ICommand RerunButton { get; private set; }
        public ICommand RunFailedButton { get; private set; }

        public ICommand StopRunButton { get; private set; }
        public ICommand ForceStopButton { get; private set; }

        public IPopup DisplayFormatButton { get; private set; }
        public ISelection DisplayFormat { get; private set; }
        public ISelection TestListGroupBy { get; private set; }

        public IChecked ShowNamespaces { get; private set; }

        public IChecked ShowHideFilterButton { get; private set; }
        public ICommand RunParametersButton { get; private set; }

        public IDialogManager DialogManager { get; }

        #region Subordinate Views contained in main form

        public ITestTreeView TreeView => treeView;

        public IProgressBarView ProgressBarView => progressBar;

        public IStatusBarView StatusBarView => statusBar;

        public ITestPropertiesView TestPropertiesView => propertiesView;

        public ITestResultSubView TestResultSubView => errorsAndFailuresView1.TestResultSubView;

        public ErrorsAndFailuresView ErrorsAndFailuresView { get { return errorsAndFailuresView1; } }

        public ITextOutputView TextOutputView { get { return textOutputView; } }

        #endregion

        #endregion

        #region Menu Handlers

        #region View Menu

        public void Configure(bool useFullGui)
        {
            leftPanel.Visible = true;
            leftPanel.Dock = useFullGui
                ? DockStyle.Left
                : DockStyle.Fill;
            treeSplitter.Visible = useFullGui;
            rightPanel.Visible = useFullGui;

            if (useFullGui)
            {
                // Move progress bar from left to right
                leftPanel.Controls.Remove(progressPanel);
                rightPanel.Controls.Add(progressPanel);
            }
            else
            {
                // Move progress bar from right to left
                rightPanel.Controls.Remove(progressPanel);
                leftPanel.Controls.Add(progressPanel);
            }
        }

        #endregion

        #endregion

        #region Helper Methods

        private static Font MakeBold(Font font)
        {
            return font.FontFamily.IsStyleAvailable(FontStyle.Bold)
                       ? new Font(font, FontStyle.Bold) : font;
        }

        #endregion
    }
}
