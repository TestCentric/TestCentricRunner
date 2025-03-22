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

    public class TestCentricMainView : TestCentricFormBase, IMainView
    {
        #region Instance variables

        private System.ComponentModel.IContainer components;

        private System.Windows.Forms.Panel leftPanel;
        private System.Windows.Forms.Splitter treeSplitter;
        private System.Windows.Forms.Panel rightPanel;
        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.Panel progressPanel;

        private System.Windows.Forms.Panel testPanel;
        private TestCentric.Gui.Views.TestTreeView treeView;
        private TabControl resultTabs;
        private StatusBarView statusBar;
        private TestPropertiesView propertiesView;

        private System.Windows.Forms.ToolTip toolTip;

        private System.Windows.Forms.MenuStrip mainMenu;

        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem recentFilesMenu;
        private System.Windows.Forms.ToolStripMenuItem closeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitMenuItem;

        private System.Windows.Forms.ToolStripMenuItem toolsMenu;
        private System.Windows.Forms.ToolStripMenuItem settingsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveResultsMenuItem;

        private System.Windows.Forms.ToolStripMenuItem nunitHelpMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpItem;
        private System.Windows.Forms.ToolStripSeparator helpMenuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem aboutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMenu;
        private System.Windows.Forms.ToolStripMenuItem miniGuiMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fullGuiMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fontChangeMenuItem;
        private System.Windows.Forms.ToolStripMenuItem defaultFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decreaseFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem increaseFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem guiFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fixedFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem increaseFixedFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem decreaseFixedFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreFixedFontMenuItem;
        private System.Windows.Forms.ToolStripMenuItem reloadTestsMenuItem;
        private ToolStripMenuItem addTestFileMenuItem;
        private ToolStripMenuItem extensionsMenuItem;
        private ToolStripMenuItem testCentricHelpMenuItem;
        private TabPage errorTab;
        private TabPage outputTab;
        private TabPage propertiesTab;
        private ErrorsAndFailuresView errorsAndFailuresView1;
        private ToolStripMenuItem recentFilesDummyMenuItem;
        private ToolStripMenuItem openWorkDirectoryMenuItem;
        private ToolStripMenuItem saveResultsAsMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripSeparator toolStripSeparator8;
        private ToolStripSeparator toolStripSeparator10;
        private ToolStripSeparator toolStripSeparator11;
        private ToolStripSeparator toolStripSeparator7;
        private ToolStripMenuItem runAsX86MenuItem;
        private ToolStripMenuItem selectAgentMenu;
        private ToolStripMenuItem selectAgentDummyMenuItem;
        private ToolStripButton runSelectedButton;
        private ToolStripDropDownButton displayFormatButton;
        private ToolStripMenuItem nunitTreeMenuItem;
        private ToolStripMenuItem fixtureListMenuItem;
        private ToolStripMenuItem testListMenuItem;
        private ToolStripMenuItem nunitTreeShowNamespaceMenuItem;
        private ToolStripMenuItem byAssemblyMenuItem;
        private ToolStripMenuItem byFixtureMenuItem;
        private ToolStripMenuItem byCategoryMenuItem;
        private ToolStripMenuItem byOutcomeMenuItem;
        private ToolStripMenuItem byDurationMenuItem;
        private ToolStripButton stopRunButton;
        private ToolStripButton forceStopButton;
        private ProgressBarView progressBar;
        private ToolStripButton runAllButton;
        private ToolStripButton showFilterButton;
        private ToolStripButton runParametersButton;
        private ToolStripButton runFailedButton;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripButton rerunButton;
        private CheckBox runSummaryButton;
        private RunSummaryDisplay runSummaryDisplay;
        private ToolStripMenuItem newProjectMenuItem;
        private ToolStripMenuItem openProjectMenuItem;
        private ToolStripMenuItem saveProjectMenuItem;
        private ToolStripSeparator toolStripSeparator6;
        private TextOutputView textOutputView1;

        #endregion

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
            SaveProjectCommand = new CommandMenuElement(saveProjectMenuItem);
            CloseProjectCommand = new CommandMenuElement(closeMenuItem);
            AddTestFilesCommand = new CommandMenuElement(addTestFileMenuItem);
            ReloadTestsCommand = new CommandMenuElement(reloadTestsMenuItem);
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
            SaveResultsCommand = new CommandMenuElement(saveResultsMenuItem);
            SaveResultsAsMenu = new PopupMenuElement(saveResultsAsMenuItem);
            OpenWorkDirectoryCommand = new CommandMenuElement(openWorkDirectoryMenuItem);
            ExtensionsCommand = new CommandMenuElement(extensionsMenuItem);
            SettingsCommand = new CommandMenuElement(settingsMenuItem);

            // Initialize Help Menu
            TestCentricHelpCommand = new CommandMenuElement(testCentricHelpMenuItem);
            NUnitHelpCommand = new CommandMenuElement(nunitHelpMenuItem);
            AboutCommand = new CommandMenuElement(aboutMenuItem);

            // Initialize Toolbar
            RunAllButton = new ToolStripButtonElement(runAllButton);
            RunSelectedButton = new ToolStripButtonElement(runSelectedButton);
            RerunButton = new ToolStripButtonElement(rerunButton);
            RunFailedButton = new ToolStripButtonElement(runFailedButton);
            StopRunButton = new ToolStripButtonElement(stopRunButton);
            ForceStopButton = new ToolStripButtonElement(forceStopButton);
            DisplayFormatButton = new ToolStripElement(displayFormatButton);
            DisplayFormat = new CheckedToolStripMenuGroup(
                "displayFormat",
                nunitTreeMenuItem, fixtureListMenuItem, testListMenuItem);
            GroupBy = new CheckedToolStripMenuGroup(
                "testGrouping",
                byAssemblyMenuItem, byFixtureMenuItem, byCategoryMenuItem, byOutcomeMenuItem, byDurationMenuItem);
            ShowNamespace = new CheckedMenuElement(nunitTreeShowNamespaceMenuItem);
            ShowHideFilterButton = new ToolStripButtonElement(showFilterButton);
            RunParametersButton = new ToolStripButtonElement(runParametersButton);
            RunSummaryButton = new CheckBoxElement(runSummaryButton);

            DialogManager = new DialogManager();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestCentricMainView));
            this.statusBar = new TestCentric.Gui.Views.StatusBarView();
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.runAllButton = new System.Windows.Forms.ToolStripButton();
            this.runSelectedButton = new System.Windows.Forms.ToolStripButton();
            this.rerunButton = new System.Windows.Forms.ToolStripButton();
            this.runFailedButton = new System.Windows.Forms.ToolStripButton();
            this.stopRunButton = new System.Windows.Forms.ToolStripButton();
            this.forceStopButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.displayFormatButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.nunitTreeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixtureListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testListMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nunitTreeShowNamespaceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byAssemblyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byFixtureMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byCategoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byOutcomeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.byDurationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showFilterButton = new System.Windows.Forms.ToolStripButton();
            this.runParametersButton = new System.Windows.Forms.ToolStripButton();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.newProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.addTestFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.reloadTestsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAgentMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.selectAgentDummyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.runAsX86MenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.recentFilesDummyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.exitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.fullGuiMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miniGuiMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.guiFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.increaseFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decreaseFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.fontChangeMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.defaultFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fixedFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.increaseFixedFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.decreaseFixedFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.restoreFixedFontMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.saveResultsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveResultsAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openWorkDirectoryMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.extensionsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testCentricHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nunitHelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.aboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeSplitter = new System.Windows.Forms.Splitter();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.progressPanel = new System.Windows.Forms.Panel();
            this.runSummaryDisplay = new TestCentric.Gui.Controls.RunSummaryDisplay();
            this.runSummaryButton = new System.Windows.Forms.CheckBox();
            this.progressBar = new TestCentric.Gui.Views.ProgressBarView();
            this.resultTabs = new System.Windows.Forms.TabControl();
            this.propertiesTab = new System.Windows.Forms.TabPage();
            this.propertiesView = new TestCentric.Gui.Views.TestPropertiesView();
            this.errorTab = new System.Windows.Forms.TabPage();
            this.errorsAndFailuresView1 = new TestCentric.Gui.Views.ErrorsAndFailuresView();
            this.outputTab = new System.Windows.Forms.TabPage();
            this.textOutputView1 = new TestCentric.Gui.Views.TextOutputView();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.leftPanel = new System.Windows.Forms.Panel();
            this.testPanel = new System.Windows.Forms.Panel();
            this.treeView = new TestCentric.Gui.Views.TestTreeView();
            this.toolStrip.SuspendLayout();
            this.mainMenu.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.progressPanel.SuspendLayout();
            this.resultTabs.SuspendLayout();
            this.propertiesTab.SuspendLayout();
            this.errorTab.SuspendLayout();
            this.outputTab.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.testPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusBar
            // 
            this.statusBar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.statusBar.Location = new System.Drawing.Point(0, 407);
            this.statusBar.MinimumSize = new System.Drawing.Size(0, 24);
            this.statusBar.Name = "statusBar";
            this.statusBar.Size = new System.Drawing.Size(744, 24);
            this.statusBar.TabIndex = 0;
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runAllButton,
            this.runSelectedButton,
            this.rerunButton,
            this.runFailedButton,
            this.stopRunButton,
            this.forceStopButton,
            this.toolStripSeparator5,
            this.displayFormatButton,
            this.showFilterButton,
            this.runParametersButton});
            this.toolStrip.Location = new System.Drawing.Point(0, 24);
            this.toolStrip.MinimumSize = new System.Drawing.Size(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(744, 24);
            this.toolStrip.TabIndex = 0;
            // 
            // runAllButton
            // 
            this.runAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runAllButton.Image = ((System.Drawing.Image)(resources.GetObject("runAllButton.Image")));
            this.runAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runAllButton.Name = "runAllButton";
            this.runAllButton.Size = new System.Drawing.Size(23, 21);
            this.runAllButton.ToolTipText = "Run All Tests";
            // 
            // runSelectedButton
            // 
            this.runSelectedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runSelectedButton.Image = ((System.Drawing.Image)(resources.GetObject("runSelectedButton.Image")));
            this.runSelectedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runSelectedButton.Name = "runSelectedButton";
            this.runSelectedButton.Size = new System.Drawing.Size(23, 21);
            this.runSelectedButton.ToolTipText = "Run Selected Tests";
            // 
            // rerunButton
            // 
            this.rerunButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.rerunButton.Image = ((System.Drawing.Image)(resources.GetObject("rerunButton.Image")));
            this.rerunButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.rerunButton.Name = "rerunButton";
            this.rerunButton.Size = new System.Drawing.Size(23, 21);
            this.rerunButton.ToolTipText = "Repeat last test run";
            // 
            // runFailedButton
            // 
            this.runFailedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runFailedButton.Image = ((System.Drawing.Image)(resources.GetObject("runFailedButton.Image")));
            this.runFailedButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runFailedButton.Name = "runFailedButton";
            this.runFailedButton.Size = new System.Drawing.Size(23, 21);
            this.runFailedButton.ToolTipText = "Run Failed Tests";
            // 
            // stopRunButton
            // 
            this.stopRunButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.stopRunButton.Image = ((System.Drawing.Image)(resources.GetObject("stopRunButton.Image")));
            this.stopRunButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.stopRunButton.Name = "stopRunButton";
            this.stopRunButton.Size = new System.Drawing.Size(23, 21);
            this.stopRunButton.ToolTipText = "Stop the current test run.";
            // 
            // forceStopButton
            // 
            this.forceStopButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.forceStopButton.Image = ((System.Drawing.Image)(resources.GetObject("forceStopButton.Image")));
            this.forceStopButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.forceStopButton.Name = "forceStopButton";
            this.forceStopButton.Size = new System.Drawing.Size(23, 21);
            this.forceStopButton.Text = "Force Stop";
            this.forceStopButton.ToolTipText = "Force the test run to stop";
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 24);
            // 
            // displayFormatButton
            // 
            this.displayFormatButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.displayFormatButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.nunitTreeMenuItem,
            this.fixtureListMenuItem,
            this.testListMenuItem });
            this.displayFormatButton.Image = ((System.Drawing.Image)(resources.GetObject("displayFormatButton.Image")));
            this.displayFormatButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.displayFormatButton.Name = "displayFormatButton";
            this.displayFormatButton.Size = new System.Drawing.Size(29, 21);
            this.displayFormatButton.Text = "Display";
            this.displayFormatButton.ToolTipText = "Tree Display Format";
            this.displayFormatButton.DropDown.Closing += DisplayFormatDropDown_Closing;
            // 
            // nunitTreeShowNamespaceMenuItem
            // 
            this.nunitTreeShowNamespaceMenuItem.Name = "nunitTreeShowNamespaceMenuItem";
            this.nunitTreeShowNamespaceMenuItem.Size = new System.Drawing.Size(198, 22);
            this.nunitTreeShowNamespaceMenuItem.Tag = "NUNIT_TREE_SHOW_NAMESPACE";
            this.nunitTreeShowNamespaceMenuItem.Text = "Show Namespace";
            // 
            // nunitTreeMenuItem
            // 
            this.nunitTreeMenuItem.Name = "nunitTreeMenuItem";
            this.nunitTreeMenuItem.Size = new System.Drawing.Size(198, 22);
            this.nunitTreeMenuItem.Tag = "NUNIT_TREE";
            this.nunitTreeMenuItem.Text = "NUnit Tree";
            this.nunitTreeMenuItem.CheckedChanged += DisplayFormatNUnitTreeChanged;

            // 
            // fixtureListMenuItem
            // 
            this.fixtureListMenuItem.Name = "fixtureListMenuItem";
            this.fixtureListMenuItem.Size = new System.Drawing.Size(198, 22);
            this.fixtureListMenuItem.Tag = "FIXTURE_LIST";
            this.fixtureListMenuItem.Text = "Fixture List";
            this.fixtureListMenuItem.CheckedChanged += DisplayFormatFixtureListChanged;
            // 
            // testListMenuItem
            // 
            this.testListMenuItem.Name = "testListMenuItem";
            this.testListMenuItem.Size = new System.Drawing.Size(198, 22);
            this.testListMenuItem.Tag = "TEST_LIST";
            this.testListMenuItem.Text = "Test List";
            this.testListMenuItem.CheckedChanged += DisplayFormatTestListChanged;
            // 
            // byAssemblyMenuItem
            // 
            this.byAssemblyMenuItem.Name = "byAssemblyMenuItem";
            this.byAssemblyMenuItem.Size = new System.Drawing.Size(198, 22);
            this.byAssemblyMenuItem.Tag = "ASSEMBLY";
            this.byAssemblyMenuItem.Text = "By Assembly";
            // 
            // byFixtureMenuItem
            // 
            this.byFixtureMenuItem.Name = "byFixtureMenuItem";
            this.byFixtureMenuItem.Size = new System.Drawing.Size(198, 22);
            this.byFixtureMenuItem.Tag = "FIXTURE";
            this.byFixtureMenuItem.Text = "By Fixture";
            // 
            // byCategoryMenuItem
            // 
            this.byCategoryMenuItem.Name = "byCategoryMenuItem";
            this.byCategoryMenuItem.Size = new System.Drawing.Size(198, 22);
            this.byCategoryMenuItem.Tag = "CATEGORY";
            this.byCategoryMenuItem.Text = "By Category";
            // 
            // byOutcomeMenuItem
            // 
            this.byOutcomeMenuItem.Name = "byOutcomeMenuItem";
            this.byOutcomeMenuItem.Size = new System.Drawing.Size(198, 22);
            this.byOutcomeMenuItem.Tag = "OUTCOME";
            this.byOutcomeMenuItem.Text = "By Outcome";
            // 
            // byDurationMenuItem
            // 
            this.byDurationMenuItem.Name = "byDurationMenuItem";
            this.byDurationMenuItem.Size = new System.Drawing.Size(198, 22);
            this.byDurationMenuItem.Tag = "DURATION";
            this.byDurationMenuItem.Text = "By Duration";
            // 
            // showFilterButton
            // 
            this.showFilterButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showFilterButton.Image = ((System.Drawing.Image)(resources.GetObject("TestFilterButton.Image")));
            this.showFilterButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showFilterButton.Name = "showFilterButton";
            this.showFilterButton.CheckOnClick = true;
            this.showFilterButton.Size = new System.Drawing.Size(23, 21);
            this.showFilterButton.ToolTipText = "Show/Hide filter";
            // 
            // runParametersButton
            // 
            this.runParametersButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runParametersButton.Image = ((System.Drawing.Image)(resources.GetObject("runParametersButton.Image")));
            this.runParametersButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runParametersButton.Name = "runParametersButton";
            this.runParametersButton.Size = new System.Drawing.Size(23, 21);
            this.runParametersButton.ToolTipText = "Test Run Parameters";
            // 
            // mainMenu
            // 
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.viewMenu,
            this.toolsMenu,
            this.helpItem});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(744, 24);
            this.mainMenu.TabIndex = 5;
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newProjectMenuItem,
            this.openProjectMenuItem,
            this.saveProjectMenuItem,
            this.closeMenuItem,
            this.toolStripSeparator6,
            this.addTestFileMenuItem,
            this.toolStripSeparator1,
            this.reloadTestsMenuItem,
            this.selectAgentMenu,
            this.toolStripSeparator2,
            this.runAsX86MenuItem,
            this.toolStripSeparator3,
            this.recentFilesMenu,
            this.toolStripSeparator4,
            this.exitMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Text = "&File";
            // 
            // newProjectMenuItem
            // 
            this.newProjectMenuItem.Name = "newProjectMenuItem";
            this.newProjectMenuItem.Size = new System.Drawing.Size(187, 22);
            this.newProjectMenuItem.Text = "Create &New Project";
            // 
            // openProjectMenuItem
            // 
            this.openProjectMenuItem.Name = "openProjectMenuItem";
            this.openProjectMenuItem.Size = new System.Drawing.Size(187, 22);
            this.openProjectMenuItem.Text = "&Open Existing Project";
            // 
            // saveProjectMenuItem
            // 
            this.saveProjectMenuItem.Name = "saveProjectMenuItem";
            this.saveProjectMenuItem.Size = new System.Drawing.Size(187, 22);
            this.saveProjectMenuItem.Text = "&Save Project";
            // 
            // closeMenuItem
            // 
            this.closeMenuItem.Name = "closeMenuItem";
            this.closeMenuItem.Size = new System.Drawing.Size(187, 22);
            this.closeMenuItem.Text = "&Close Project";
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(184, 6);
            // 
            // addTestFileMenuItem
            // 
            this.addTestFileMenuItem.Name = "addTestFileMenuItem";
            this.addTestFileMenuItem.Size = new System.Drawing.Size(187, 22);
            this.addTestFileMenuItem.Text = "&Add Test File...";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(184, 6);
            // 
            // reloadTestsMenuItem
            // 
            this.reloadTestsMenuItem.Name = "reloadTestsMenuItem";
            this.reloadTestsMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.R)));
            this.reloadTestsMenuItem.Size = new System.Drawing.Size(187, 22);
            this.reloadTestsMenuItem.Text = "&Reload Tests";
            // 
            // selectAgentMenu
            // 
            this.selectAgentMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectAgentDummyMenuItem});
            this.selectAgentMenu.Name = "selectAgentMenu";
            this.selectAgentMenu.Size = new System.Drawing.Size(187, 22);
            this.selectAgentMenu.Text = "Select Agent";
            // 
            // selectAgentDummyMenuItem
            // 
            this.selectAgentDummyMenuItem.Name = "selectAgentDummyMenuItem";
            this.selectAgentDummyMenuItem.Size = new System.Drawing.Size(229, 22);
            this.selectAgentDummyMenuItem.Text = "Dummy entry to force Popup";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(184, 6);
            // 
            // runAsX86MenuItem
            // 
            this.runAsX86MenuItem.Name = "runAsX86MenuItem";
            this.runAsX86MenuItem.Size = new System.Drawing.Size(187, 22);
            this.runAsX86MenuItem.Text = "Run as X86";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(184, 6);
            // 
            // recentFilesMenu
            // 
            this.recentFilesMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.recentFilesDummyMenuItem});
            this.recentFilesMenu.Name = "recentFilesMenu";
            this.recentFilesMenu.Size = new System.Drawing.Size(187, 22);
            this.recentFilesMenu.Text = "Recent &Files";
            // 
            // recentFilesDummyMenuItem
            // 
            this.recentFilesDummyMenuItem.Name = "recentFilesDummyMenuItem";
            this.recentFilesDummyMenuItem.Size = new System.Drawing.Size(271, 22);
            this.recentFilesDummyMenuItem.Text = "Dummy Entry to force PopUp initially";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(184, 6);
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Name = "exitMenuItem";
            this.exitMenuItem.Size = new System.Drawing.Size(187, 22);
            this.exitMenuItem.Text = "E&xit";
            // 
            // viewMenu
            // 
            this.viewMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fullGuiMenuItem,
            this.miniGuiMenuItem,
            this.toolStripSeparator8,
            this.guiFontMenuItem,
            this.fixedFontMenuItem});
            this.viewMenu.Name = "viewMenu";
            this.viewMenu.Size = new System.Drawing.Size(44, 20);
            this.viewMenu.Text = "&View";
            // 
            // fullGuiMenuItem
            // 
            this.fullGuiMenuItem.Checked = true;
            this.fullGuiMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.fullGuiMenuItem.Name = "fullGuiMenuItem";
            this.fullGuiMenuItem.Size = new System.Drawing.Size(129, 22);
            this.fullGuiMenuItem.Tag = "Full";
            this.fullGuiMenuItem.Text = "&Full GUI";
            // 
            // miniGuiMenuItem
            // 
            this.miniGuiMenuItem.Name = "miniGuiMenuItem";
            this.miniGuiMenuItem.Size = new System.Drawing.Size(129, 22);
            this.miniGuiMenuItem.Tag = "Mini";
            this.miniGuiMenuItem.Text = "&Mini GUI";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(126, 6);
            // 
            // guiFontMenuItem
            // 
            this.guiFontMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.increaseFontMenuItem,
            this.decreaseFontMenuItem,
            this.toolStripSeparator10,
            this.fontChangeMenuItem,
            this.defaultFontMenuItem});
            this.guiFontMenuItem.Name = "guiFontMenuItem";
            this.guiFontMenuItem.Size = new System.Drawing.Size(129, 22);
            this.guiFontMenuItem.Text = "GUI Fo&nt";
            // 
            // increaseFontMenuItem
            // 
            this.increaseFontMenuItem.Name = "increaseFontMenuItem";
            this.increaseFontMenuItem.Size = new System.Drawing.Size(124, 22);
            this.increaseFontMenuItem.Text = "&Increase";
            // 
            // decreaseFontMenuItem
            // 
            this.decreaseFontMenuItem.Name = "decreaseFontMenuItem";
            this.decreaseFontMenuItem.Size = new System.Drawing.Size(124, 22);
            this.decreaseFontMenuItem.Text = "&Decrease";
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(121, 6);
            // 
            // fontChangeMenuItem
            // 
            this.fontChangeMenuItem.Name = "fontChangeMenuItem";
            this.fontChangeMenuItem.Size = new System.Drawing.Size(124, 22);
            this.fontChangeMenuItem.Text = "&Change...";
            // 
            // defaultFontMenuItem
            // 
            this.defaultFontMenuItem.Name = "defaultFontMenuItem";
            this.defaultFontMenuItem.Size = new System.Drawing.Size(124, 22);
            this.defaultFontMenuItem.Text = "&Restore";
            // 
            // fixedFontMenuItem
            // 
            this.fixedFontMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.increaseFixedFontMenuItem,
            this.decreaseFixedFontMenuItem,
            this.toolStripSeparator11,
            this.restoreFixedFontMenuItem});
            this.fixedFontMenuItem.Name = "fixedFontMenuItem";
            this.fixedFontMenuItem.Size = new System.Drawing.Size(129, 22);
            this.fixedFontMenuItem.Text = "Fi&xed Font";
            // 
            // increaseFixedFontMenuItem
            // 
            this.increaseFixedFontMenuItem.Name = "increaseFixedFontMenuItem";
            this.increaseFixedFontMenuItem.Size = new System.Drawing.Size(121, 22);
            this.increaseFixedFontMenuItem.Text = "&Increase";
            // 
            // decreaseFixedFontMenuItem
            // 
            this.decreaseFixedFontMenuItem.Name = "decreaseFixedFontMenuItem";
            this.decreaseFixedFontMenuItem.Size = new System.Drawing.Size(121, 22);
            this.decreaseFixedFontMenuItem.Text = "&Decrease";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(118, 6);
            // 
            // restoreFixedFontMenuItem
            // 
            this.restoreFixedFontMenuItem.Name = "restoreFixedFontMenuItem";
            this.restoreFixedFontMenuItem.Size = new System.Drawing.Size(121, 22);
            this.restoreFixedFontMenuItem.Text = "&Restore";
            // 
            // toolsMenu
            // 
            this.toolsMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveResultsMenuItem,
            this.saveResultsAsMenuItem,
            this.openWorkDirectoryMenuItem,
            this.toolStripSeparator7,
            this.extensionsMenuItem,
            this.settingsMenuItem});
            this.toolsMenu.Name = "toolsMenu";
            this.toolsMenu.Size = new System.Drawing.Size(46, 20);
            this.toolsMenu.Text = "T&ools";
            // 
            // saveResultsMenuItem
            // 
            this.saveResultsMenuItem.Name = "saveResultsMenuItem";
            this.saveResultsMenuItem.Size = new System.Drawing.Size(194, 22);
            this.saveResultsMenuItem.Text = "&Save Test Results...";
            // 
            // saveResultsAsMenuItem
            // 
            this.saveResultsAsMenuItem.Name = "saveResultsAsMenuItem";
            this.saveResultsAsMenuItem.Size = new System.Drawing.Size(194, 22);
            this.saveResultsAsMenuItem.Text = "Save Test Results As";
            // 
            // openWorkDirectoryMenuItem
            // 
            this.openWorkDirectoryMenuItem.Name = "openWorkDirectoryMenuItem";
            this.openWorkDirectoryMenuItem.Size = new System.Drawing.Size(194, 22);
            this.openWorkDirectoryMenuItem.Text = "Open Work Directory...";
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(191, 6);
            // 
            // extensionsMenuItem
            // 
            this.extensionsMenuItem.Name = "extensionsMenuItem";
            this.extensionsMenuItem.Size = new System.Drawing.Size(194, 22);
            this.extensionsMenuItem.Text = "Extensions...";
            // 
            // settingsMenuItem
            // 
            this.settingsMenuItem.Name = "settingsMenuItem";
            this.settingsMenuItem.Size = new System.Drawing.Size(194, 22);
            this.settingsMenuItem.Text = "&Settings...";
            // 
            // helpItem
            // 
            this.helpItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testCentricHelpMenuItem,
            this.nunitHelpMenuItem,
            this.helpMenuSeparator1,
            this.aboutMenuItem});
            this.helpItem.Name = "helpItem";
            this.helpItem.Size = new System.Drawing.Size(44, 20);
            this.helpItem.Text = "&Help";
            // 
            // testCentricHelpMenuItem
            // 
            this.testCentricHelpMenuItem.Name = "testCentricHelpMenuItem";
            this.testCentricHelpMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F1;
            this.testCentricHelpMenuItem.Size = new System.Drawing.Size(180, 22);
            this.testCentricHelpMenuItem.Text = "TestCentric ...";
            // 
            // nunitHelpMenuItem
            // 
            this.nunitHelpMenuItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.nunitHelpMenuItem.Name = "nunitHelpMenuItem";
            this.nunitHelpMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F1)));
            this.nunitHelpMenuItem.Size = new System.Drawing.Size(180, 22);
            this.nunitHelpMenuItem.Text = "NUnit ...";
            // 
            // helpMenuSeparator1
            // 
            this.helpMenuSeparator1.Name = "helpMenuSeparator1";
            this.helpMenuSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // aboutMenuItem
            // 
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Size = new System.Drawing.Size(180, 22);
            this.aboutMenuItem.Text = "&About TestCentric...";
            // 
            // treeSplitter
            // 
            this.treeSplitter.Location = new System.Drawing.Point(240, 48);
            this.treeSplitter.MinSize = 240;
            this.treeSplitter.Name = "treeSplitter";
            this.treeSplitter.Size = new System.Drawing.Size(6, 359);
            this.treeSplitter.TabIndex = 2;
            this.treeSplitter.TabStop = false;
            // 
            // rightPanel
            // 
            this.rightPanel.BackColor = System.Drawing.SystemColors.Control;
            this.rightPanel.Controls.Add(this.progressPanel);
            this.rightPanel.Controls.Add(this.resultTabs);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightPanel.Location = new System.Drawing.Point(246, 48);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(498, 359);
            this.rightPanel.TabIndex = 3;
            // 
            // progressPanel
            // 
            this.progressPanel.AutoSize = true;
            this.progressPanel.BackColor = System.Drawing.SystemColors.Control;
            this.progressPanel.Controls.Add(this.runSummaryDisplay);
            this.progressPanel.Controls.Add(this.runSummaryButton);
            this.progressPanel.Controls.Add(this.progressBar);
            this.progressPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.progressPanel.Location = new System.Drawing.Point(0, 0);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(498, 107);
            this.progressPanel.TabIndex = 3;
            // 
            // runSummaryDisplay
            // 
            this.runSummaryDisplay.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.runSummaryDisplay.BackColor = System.Drawing.Color.LightYellow;
            this.runSummaryDisplay.Location = new System.Drawing.Point(3, 28);
            this.runSummaryDisplay.Name = "runSummaryDisplay";
            this.runSummaryDisplay.Size = new System.Drawing.Size(488, 76);
            this.runSummaryDisplay.TabIndex = 5;
            this.runSummaryDisplay.Visible = false;
            // 
            // runSummaryButton
            // 
            this.runSummaryButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.runSummaryButton.Appearance = System.Windows.Forms.Appearance.Button;
            this.runSummaryButton.FlatAppearance.BorderSize = 0;
            this.runSummaryButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.runSummaryButton.Image = ((System.Drawing.Image)(resources.GetObject("runSummaryButton.Image")));
            this.runSummaryButton.Location = new System.Drawing.Point(472, 4);
            this.runSummaryButton.Name = "runSummaryButton";
            this.runSummaryButton.Size = new System.Drawing.Size(19, 19);
            this.runSummaryButton.TabIndex = 4;
            this.toolTip.SetToolTip(this.runSummaryButton, "Display Run Summary");
            this.runSummaryButton.UseVisualStyleBackColor = true;
            this.runSummaryButton.CheckedChanged += new System.EventHandler(this.runSummaryButton_CheckedChanged);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.BackColor = System.Drawing.Color.Transparent;
            this.progressBar.CausesValidation = false;
            this.progressBar.Enabled = false;
            this.progressBar.ForeColor = System.Drawing.SystemColors.Highlight;
            this.progressBar.Location = new System.Drawing.Point(0, 2);
            this.progressBar.Name = "progressBar";
            this.progressBar.Progress = 0;
            this.progressBar.Size = new System.Drawing.Size(471, 24);
            this.progressBar.Status = TestCentric.Gui.Views.ProgressBarStatus.Success;
            this.progressBar.TabIndex = 3;
            // 
            // resultTabs
            // 
            this.resultTabs.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.resultTabs.Controls.Add(this.propertiesTab);
            this.resultTabs.Controls.Add(this.errorTab);
            this.resultTabs.Controls.Add(this.outputTab);
            this.resultTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resultTabs.Location = new System.Drawing.Point(0, 0);
            this.resultTabs.Name = "resultTabs";
            this.resultTabs.SelectedIndex = 0;
            this.resultTabs.Size = new System.Drawing.Size(498, 359);
            this.resultTabs.TabIndex = 2;
            // 
            // propertiesTab
            // 
            this.propertiesTab.Controls.Add(this.propertiesView);
            this.propertiesTab.Location = new System.Drawing.Point(4, 4);
            this.propertiesTab.Name = "propertiesTab";
            this.propertiesTab.Size = new System.Drawing.Size(490, 333);
            this.propertiesTab.TabIndex = 1;
            this.propertiesTab.Text = "Test Properties";
            this.propertiesTab.UseVisualStyleBackColor = true;
            // 
            // propertiesView
            // 
            this.propertiesView.AutoScroll = true;
            this.propertiesView.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.propertiesView.Categories = "";
            this.propertiesView.Description = "";
            this.propertiesView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesView.FullName = "";
            this.propertiesView.Header = "";
            this.propertiesView.Location = new System.Drawing.Point(0, 0);
            this.propertiesView.Name = "propertiesView";
            this.propertiesView.PackageSettings = "";
            this.propertiesView.Properties = "";
            this.propertiesView.RunState = "";
            this.propertiesView.Size = new System.Drawing.Size(490, 333);
            this.propertiesView.SkipReason = "";
            this.propertiesView.TabIndex = 0;
            this.propertiesView.TestCount = "";
            this.propertiesView.TestType = "";
            // 
            // errorTab
            // 
            this.errorTab.Controls.Add(this.errorsAndFailuresView1);
            this.errorTab.Location = new System.Drawing.Point(4, 4);
            this.errorTab.Name = "errorTab";
            this.errorTab.Size = new System.Drawing.Size(490, 333);
            this.errorTab.TabIndex = 0;
            this.errorTab.Text = "Test Results";
            this.errorTab.UseVisualStyleBackColor = true;
            // 
            // errorsAndFailuresView1
            // 
            this.errorsAndFailuresView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorsAndFailuresView1.EnableToolTips = false;
            this.errorsAndFailuresView1.Location = new System.Drawing.Point(0, 0);
            this.errorsAndFailuresView1.Name = "errorsAndFailuresView1";
            this.errorsAndFailuresView1.Size = new System.Drawing.Size(490, 333);
            this.errorsAndFailuresView1.SourceCodeDisplay = true;
            this.errorsAndFailuresView1.SourceCodeSplitOrientation = System.Windows.Forms.Orientation.Vertical;
            this.errorsAndFailuresView1.SourceCodeSplitterDistance = 0.3F;
            this.errorsAndFailuresView1.SplitterPosition = 128;
            this.errorsAndFailuresView1.TabIndex = 0;
            // 
            // outputTab
            // 
            this.outputTab.Controls.Add(this.textOutputView1);
            this.outputTab.Location = new System.Drawing.Point(4, 4);
            this.outputTab.Name = "outputTab";
            this.outputTab.Size = new System.Drawing.Size(490, 333);
            this.outputTab.TabIndex = 3;
            this.outputTab.Text = "Text Output";
            this.outputTab.UseVisualStyleBackColor = true;
            // 
            // textOutputView1
            // 
            this.textOutputView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textOutputView1.Location = new System.Drawing.Point(0, 0);
            this.textOutputView1.Name = "textOutputView1";
            this.textOutputView1.Size = new System.Drawing.Size(490, 333);
            this.textOutputView1.TabIndex = 0;
            this.textOutputView1.WordWrap = true;
            // 
            // leftPanel
            // 
            this.leftPanel.Controls.Add(this.testPanel);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 48);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(240, 359);
            this.leftPanel.TabIndex = 4;
            // 
            // testPanel
            // 
            this.testPanel.Controls.Add(this.treeView);
            this.testPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testPanel.Location = new System.Drawing.Point(0, 0);
            this.testPanel.Name = "testPanel";
            this.testPanel.Size = new System.Drawing.Size(240, 359);
            this.testPanel.TabIndex = 0;
            // 
            // treeView
            // 
            this.treeView.AllowDrop = true;
            this.treeView.AlternateImageSet = null;
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.treeView.CheckBoxes = false;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.SelectedNode = null;
            this.treeView.Size = new System.Drawing.Size(240, 359);
            this.treeView.TabIndex = 0;
            this.treeView.TopNode = null;
            // 
            // TestCentricMainView
            // 
            this.ClientSize = new System.Drawing.Size(744, 431);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.treeSplitter);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.statusBar);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.mainMenu);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu;
            this.MinimumSize = new System.Drawing.Size(160, 32);
            this.Name = "TestCentricMainView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "TestCentric Runner for NUnit";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.rightPanel.ResumeLayout(false);
            this.rightPanel.PerformLayout();
            this.progressPanel.ResumeLayout(false);
            this.resultTabs.ResumeLayout(false);
            this.propertiesTab.ResumeLayout(false);
            this.errorTab.ResumeLayout(false);
            this.outputTab.ResumeLayout(false);
            this.leftPanel.ResumeLayout(false);
            this.testPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void DisplayFormatDropDown_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            e.Cancel = e.CloseReason == ToolStripDropDownCloseReason.ItemClicked;
        }

        private void DisplayFormatNUnitTreeChanged(object sender, EventArgs e)
        {
            this.nunitTreeMenuItem.DropDownItems.Clear();
            if (nunitTreeMenuItem.Checked)
            {
                nunitTreeMenuItem.DropDownItems.AddRange(new ToolStripItem[] { nunitTreeShowNamespaceMenuItem });
                if (nunitTreeMenuItem.Visible)
                    nunitTreeMenuItem.DropDown.Show();
            }
        }

        private void DisplayFormatFixtureListChanged(object sender, EventArgs e)
        {
            this.fixtureListMenuItem.DropDownItems.Clear();
            if (fixtureListMenuItem.Checked)
            {
                fixtureListMenuItem.DropDownItems.AddRange(new ToolStripItem[] { byCategoryMenuItem, byOutcomeMenuItem, byDurationMenuItem });
                if (fixtureListMenuItem.Visible)
                    fixtureListMenuItem.DropDown.Show();
            }
        }

        private void DisplayFormatTestListChanged(object sender, EventArgs e)
        {
            this.testListMenuItem.DropDownItems.Clear();
            if (testListMenuItem.Checked)
            {
                this.testListMenuItem.DropDownItems.AddRange(new ToolStripItem[] { byAssemblyMenuItem, byFixtureMenuItem, byCategoryMenuItem, byOutcomeMenuItem, byDurationMenuItem });
                if (testListMenuItem.Visible)
                    this.testListMenuItem.DropDown.Show();
            }
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
        public ICommand SaveResultsCommand { get; }
        public IPopup SaveResultsAsMenu { get; }
        public ICommand OpenWorkDirectoryCommand { get; }
        public ICommand ExtensionsCommand { get; }
        public ICommand SettingsCommand { get; }

        // Help Menu Items
        public ICommand TestCentricHelpCommand { get; }
        public ICommand NUnitHelpCommand { get; }
        public ICommand AboutCommand { get; }

        // ToolBar Elements
        public ICommand RunAllButton { get; private set; }
        public ICommand RunSelectedButton { get; private set; }
        public ICommand RerunButton { get; private set; }
        public ICommand RunFailedButton { get; private set; }

        public ICommand StopRunButton { get; private set; }
        public ICommand ForceStopButton { get; private set; }

        public IViewElement DisplayFormatButton { get; private set; }
        public ISelection DisplayFormat { get; private set; }
        public ISelection GroupBy { get; private set; }
        public IChecked ShowNamespace { get; private set; }

        public IChecked ShowHideFilterButton { get; private set; }
        public ICommand RunParametersButton { get; private set; }

        public IChecked RunSummaryButton { get; private set; }

        public IDialogManager DialogManager { get; }

        public IRunSummaryDisplay RunSummaryDisplay => runSummaryDisplay;

        #region Subordinate Views contained in main form

        public ITestTreeView TreeView => treeView;

        public ProgressBarView ProgressBarView => progressBar;

        public StatusBarView StatusBarView => statusBar;

        public TestPropertiesView TestPropertiesView => propertiesView;

        public ErrorsAndFailuresView ErrorsAndFailuresView { get { return errorsAndFailuresView1; } }

        public ITextOutputView TextOutputView { get { return textOutputView1; } }

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

        private void runSummaryButton_CheckedChanged(object sender, EventArgs e)
        {
            toolTip.SetToolTip(runSummaryButton, runSummaryButton.Checked ? "Hide Run Summary" : "Display Run Summary");
        }
    }
}

