// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using TestCentric.Common;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Model.Services;
    using Model.Settings;
    using Views;
    using Dialogs;
    using TestCentric.Gui.Elements;

    /// <summary>
    /// TestCentricPresenter does all file opening and closing that
    /// involves interacting with the user.
    /// 
    /// NOTE: This class originated as the static class
    /// TestLoaderUI and is slowly being converted to a
    /// true presenter. Current limitations include:
    /// 
    /// 1. Many functions, which should properly be in
    /// the presenter, remain in the form.
    /// 
    /// 2. The presenter creates dialogs itself, which
    /// limits testability.
    /// </summary>
    public class TestCentricPresenter
    {
        private static readonly Logger log = InternalTrace.GetLogger(nameof(TestCentricPresenter));

        #region Instance Variables

        private readonly IMainView _view;

        private readonly ITestModel _model;

        private readonly CommandLineOptions _options;

        private readonly UserSettings _settings;

        private string _guiLayout;


        private AgentSelectionController _agentSelectionController;

        private List<string> _resultFormats = new List<string>();

        private string[] _lastFilesLoaded = null;

        private bool _stopRequested;
        private bool _forcedStopRequested;

        #endregion

        #region Constructor

        // TODO: Use an interface for view
        public TestCentricPresenter(IMainView view, ITestModel model, CommandLineOptions options)
        {
            _view = view;
            _model = model;
            _options = options;

            _settings = _model.Settings;

            _agentSelectionController = new AgentSelectionController(_model, _view);

            _view.Font = _settings.Gui.Font;
            _view.ResultTabs.SelectedIndex = _settings.Gui.SelectedTab;

            UpdateViewCommands();
            UpdateTreeDisplayMenuItem();
            UpdateRunSelectedTestsTooltip();

            foreach (string format in _model.ResultFormats)
                if (format != "cases" && format != "user")
                    _resultFormats.Add(format);

            WireUpEvents();
            _view.ShowHideFilterButton.Checked = _settings.Gui.TestTree.ShowFilter;
        }

        #endregion

        #region Event Handling

        private void WireUpEvents()
        {
            #region Model Events

            _model.Events.TestCentricProjectLoaded += (TestEventArgs e) =>
            {
                _view.Title = $"TestCentric - {_model.TestCentricProject?.FileName ?? "UNNAMED.tcproj"}";
            };

            _model.Events.TestCentricProjectUnloaded += (TestEventArgs e) =>
            {
                _view.Title = "TestCentric Runner for NUnit";
            };

            _model.Events.TestsLoading += (TestFilesLoadingEventArgs e) =>
            {
                UpdateViewCommands(testLoading: true);

                var message = e.TestFilesLoading.Count == 1 ?
                    $"Loading Assembly: {e.TestFilesLoading[0]}" :
                    $"Loading {e.TestFilesLoading.Count} Assemblies...";

                BeginLongRunningOperation(message);
            };

            _model.Events.TestLoaded += (TestNodeEventArgs e) =>
            {
                OnLongRunningOperationComplete();

                UpdateViewCommands();

                _lastFilesLoaded = _model.TestCentricProject.TestFiles.ToArray();
                _view.ResultTabs.InvokeIfRequired(() => _view.ResultTabs.SelectedIndex = 0);
            };

            _model.Events.TestsUnloading += (TestEventArgse) =>
            {
                UpdateViewCommands();

                _view.RunSummaryDisplay.Hide();
                BeginLongRunningOperation("Unloading...");
            };

            _model.Events.TestUnloaded += (TestEventArgs e) =>
            {
                OnLongRunningOperationComplete();

                UpdateViewCommands();
            };

            _model.Events.TestsReloading += (TestEventArgs e) =>
            {
                UpdateViewCommands();

                _view.RunSummaryDisplay.Hide();
                BeginLongRunningOperation("Reloading...");
            };

            _model.Events.TestReloaded += (TestNodeEventArgs e) =>
            {
                OnLongRunningOperationComplete();

                UpdateViewCommands();

            };

            _model.Events.TestLoadFailure += (TestLoadFailureEventArgs e) =>
            {
                OnLongRunningOperationComplete();

                // HACK: Engine should recognize .NET Standard and give the
                // appropriate error message. For now, we compensate for its
                // failure by issuing the message ourselves and reloading the
                // previously loaded  test.
                var msg = e.Exception.Message;
                bool isNetStandardError =
                    e.Exception.Message == "Unrecognized Target Framework Identifier: .NETStandard";
                
                if (!isNetStandardError)
                {
                    _view.MessageDisplay.Error(e.Exception.Message);
                    return;
                }

                _view.MessageDisplay.Error("Test assemblies must target a specific platform, rather than .NETStandard.");
                if (_lastFilesLoaded == null)
                    _view.Close();
                else
                {
                    _model.UnloadTests();
                    _model.CreateNewProject(_lastFilesLoaded);
                }
            };

            _model.Events.RunStarting += (RunStartingEventArgs e) =>
            {
                _stopRequested = _forcedStopRequested = false;
                UpdateViewCommands();

                // Hide the run summary
                _view.RunSummaryButton.Checked = false;
            };

            _model.Events.RunFinished += (TestResultEventArgs e) => OnRunFinished(e.Result);

            // Separate internal method for testing
            void OnRunFinished(ResultNode result)
            {
                log.Debug("Test run complete");
                OnLongRunningOperationComplete();

                UpdateViewCommands();

                string resultPath = Path.Combine(_model.WorkDirectory, "TestResult.xml");
                _model.SaveResults(resultPath);
                _view.ResultTabs.InvokeIfRequired(() => _view.ResultTabs.SelectedIndex = 1);

                //try
                //{
                //    _model.SaveResults(resultPath);
                //    //log.Debug("Saved result to {0}", resultPath);
                //}
                //catch (Exception ex)
                //{
                //    //log.Warning("Unable to save result to {0}\n{1}", resultPath, ex.ToString());
                //}

                //if (e.Result.Outcome.Status == TestStatus.Failed)
                //    _treeView.Activate();

                // Display the run summary
                _view.RunSummaryButton.Checked = true;

                // If we were running unattended, it's time to close
                if (_options.Unattended)
                {
                    _view.Close();
                    Environment.Exit(0);
                }
            };

            _model.Events.SelectedTestsChanged += (e) => UpdateViewCommands();

            _settings.Changed += (s, e) =>
            {
                switch (e.SettingName)
                {
                    case "TestCentric.Gui.GuiLayout":
                        // Settings have changed (from settings dialog)
                        // so we want to update the GUI to match.
                        var newLayout = _settings.Gui.GuiLayout;
                        var oldLayout = _view.GuiLayout.SelectedItem;
                        // Make sure it hasn't already been changed
                        if (oldLayout != newLayout)
                        {
                            // Save position of form for old layout
                            SaveFormLocationAndSize(oldLayout);
                            // Update the GUI itself
                            SetGuiLayout(newLayout);
                            _view.GuiLayout.SelectedItem = newLayout;
                        }
                        break;
                    case "TestCentric.Gui.MainForm.ShowStatusBar":
                        _view.StatusBarView.Visible = _settings.Gui.MainForm.ShowStatusBar;
                        break;
                    case "TestCentric.Gui.TestTree.DisplayFormat":
                        _view.DisplayFormat.SelectedItem = _settings.Gui.TestTree.DisplayFormat;
                        UpdateTreeDisplayMenuItem();
                        UpdateViewCommands();
                        break;
                    case "TestCentric.Gui.TestTree.TestList.GroupBy":
                        _view.GroupBy.SelectedItem = _settings.Gui.TestTree.TestList.GroupBy;
                        break;
                    case "TestCentric.Gui.TestTree.FixtureList.GroupBy":
                        _view.GroupBy.SelectedItem = _settings.Gui.TestTree.FixtureList.GroupBy;
                        break;
                    case "TestCentric.Gui.TestTree.ShowNamespace":
                        _view.ShowNamespace.SelectedIndex = _settings.Gui.TestTree.ShowNamespace ? 0 : 1;
                        break;
                }
            };

            _model.Events.UnhandledException += (UnhandledExceptionEventArgs e) =>
            {
                MessageBoxDisplay.Error($"{e.Message}\n\n{e.StackTrace}", "TestCentric - Internal Error");
            };

            #endregion

            #region View Events

            _view.Load += (s, e) =>
            {
                _guiLayout = _options.GuiLayout ?? _settings.Gui.GuiLayout;
                _view.GuiLayout.SelectedItem = _guiLayout;
                SetGuiLayout(_guiLayout);

                _view.RunAsX86.Checked = _options.RunAsX86;
            };

            _view.Shown += (s, e) =>
            {
                Application.DoEvents();

                // Create an unnamed TestCentricProject and load test specified on command line
                if (_options.InputFiles.Count == 1)
                    _model.OpenExistingFile(_options.InputFiles[0]);
                else if (_options.InputFiles.Count >1)
                    _model.CreateNewProject(_options.InputFiles);
                else if (_settings.Gui.LoadLastProject && !_options.NoLoad)
                    _model.OpenMostRecentFile();

                //if ( guiOptions.include != null || guiOptions.exclude != null)
                //{
                //    testTree.ClearSelectedCategories();
                //    bool exclude = guiOptions.include == null;
                //    string[] categories = exclude
                //        ? guiOptions.exclude.Split(',')
                //        : guiOptions.include.Split(',');
                //    if ( categories.Length > 0 )
                //        testTree.SelectCategories( categories, exclude );
                //}

                // Run loaded test automatically if called for
                if (_model.HasTests && _options.RunAllTests)
                {
                    log.Debug("Running all tests");
                    _model.RunTests(_model.LoadedTests);
                }
                // Currently, --unattended without --run does nothing except exit.
                else if (_options.Unattended)
                    _view.Close();
            };

            _view.SplitterPositionChanged += (s, e) =>
            {
                _settings.Gui.MainForm.SplitPosition = _view.SplitterPosition;
            };

            _view.FormClosing += (s, e) =>
            {
                if (_model.IsProjectLoaded)
                {
                    if (_model.IsTestRunning)
                    {
                        if (!_view.MessageDisplay.YesNo("A test is running, do you want to forcibly stop the test and exit?"))
                        {
                            e.Cancel = true;
                            return;
                        }

                        _stopRequested = _forcedStopRequested = true;
                        _model.StopTestRun(true);
                    }

                    if (CloseProject() == MessageBoxResult.Cancel)
                        e.Cancel = true;
                }

                if (!e.Cancel)
                    SaveFormLocationAndSize(_guiLayout);
            };

            _view.FileMenu.Popup += () =>
            {
                bool isPackageLoaded = _model.IsProjectLoaded;
                bool isTestRunning = _model.IsTestRunning;

                _view.NewProjectCommand.Enabled = !isTestRunning;
                _view.OpenProjectCommand.Enabled = !isTestRunning;
                _view.CloseProjectCommand.Enabled = isPackageLoaded && !isTestRunning;

                _view.ReloadTestsCommand.Enabled = isPackageLoaded && !isTestRunning;

                _view.SelectAgentMenu.Enabled = _agentSelectionController.AllowAgentSelection();

                _view.RunAsX86.Enabled = isPackageLoaded && !isTestRunning;

                _view.RecentFilesMenu.Enabled = !isTestRunning;

                //if (!isTestRunning)
                //{
                //    _recentProjectsMenuHandler.Load();
                //}
            };

            _view.NewProjectCommand.Execute += () => NewProject();
            _view.OpenProjectCommand.Execute += () => OpenProject();
            _view.SaveProjectCommand.Execute += () => SaveProject();

            _view.CloseProjectCommand.Execute += () => CloseProject();
            _view.AddTestFilesCommand.Execute += () => AddTestFiles();
            _view.ReloadTestsCommand.Execute += () => ReloadTests();

            _view.SelectAgentMenu.Popup += () =>
            {
                _agentSelectionController.PopulateMenu();
            };

            _view.RunAsX86.CheckedChanged += () =>
            {
                var key = EnginePackageSettings.RunAsX86;
                if (_view.RunAsX86.Checked)
                    ChangePackageSettingAndReload(key, true);
                else
                    ChangePackageSettingAndReload(key, null);
            };

            _view.RecentFilesMenu.Popup += () =>
            {
                var menuItems = _view.RecentFilesMenu.MenuItems;
                // Test for null, in case we are running tests with a mock
                if (menuItems == null)
                    return;

                menuItems.Clear();
                int num = 0;
                foreach (string entry in _model.RecentFiles.Entries)
                {
                    var menuText = string.Format("{0} {1}", ++num, entry);
                    var menuItem = new ToolStripMenuItem(menuText);
                    menuItem.Click += (sender, ea) =>
                    {
                        string path = ((ToolStripMenuItem)sender).Text.Substring(2);
                        _model.OpenExistingFile(path);
                    };
                    menuItems.Add(menuItem);
                    if (num >= _settings.Gui.RecentProjects.MaxFiles) break;
                }
            };

            _view.ExitCommand.Execute += () => _view.Close();

            _view.GuiLayout.SelectionChanged += () =>
            {
                // Selection menu item has changed, so we want
                // to update both the display and the settings
                var oldSetting = _settings.Gui.GuiLayout;
                var newSetting = _view.GuiLayout.SelectedItem;
                if (oldSetting != newSetting)
                {
                    SaveFormLocationAndSize(oldSetting);
                    SetGuiLayout(newSetting);
                }

                _guiLayout = newSetting;
                _settings.Gui.GuiLayout = _view.GuiLayout.SelectedItem;
            };

            _view.IncreaseFontCommand.Execute += () =>
            {
                applyFont(IncreaseFont(_settings.Gui.Font));
            };

            _view.DecreaseFontCommand.Execute += () =>
            {
                applyFont(DecreaseFont(_settings.Gui.Font));
            };

            _view.ChangeFontCommand.Execute += () =>
            {
                Font currentFont = _settings.Gui.Font;
                Font newFont = _view.DialogManager.SelectFont(currentFont);
                if (newFont != _settings.Gui.Font)
                    applyFont(newFont);
            };

            _view.DialogManager.ApplyFont += (font) => applyFont(font);

            _view.RestoreFontCommand.Execute += () =>
            {
                applyFont(Form.DefaultFont);
            };

            _view.IncreaseFixedFontCommand.Execute += () =>
            {
                _settings.Gui.FixedFont = IncreaseFont(_settings.Gui.FixedFont);
            };

            _view.DecreaseFixedFontCommand.Execute += () =>
            {
                _settings.Gui.FixedFont = DecreaseFont(_settings.Gui.FixedFont);
            };

            _view.RestoreFixedFontCommand.Execute += () =>
            {
                _settings.Gui.FixedFont = new Font(FontFamily.GenericMonospace, 8.0f);
            };

            _view.RunAllButton.Execute += RunAllTests;

            _view.RunSelectedButton.Execute += RunSelectedTests;

            _view.RerunButton.Execute += () => _model.RepeatLastRun();

            _view.RunFailedButton.Execute += RunFailedTests;

            _view.DisplayFormat.SelectionChanged += () =>
            {
                _settings.Gui.TestTree.DisplayFormat = _view.DisplayFormat.SelectedItem;
            };

            _view.ShowNamespace.SelectionChanged += () =>
            {
                _settings.Gui.TestTree.ShowNamespace = _view.ShowNamespace.SelectedIndex == 0;
            };

            _view.ShowHideFilterButton.CheckedChanged += () =>
            {
                _settings.Gui.TestTree.ShowFilter = _view.ShowHideFilterButton.Checked;
            };

            _view.GroupBy.SelectionChanged += () =>
            {
                switch(_view.DisplayFormat.SelectedItem)
                {
                    case "TEST_LIST":
                        _settings.Gui.TestTree.TestList.GroupBy = _view.GroupBy.SelectedItem;
                        break;
                    case "FIXTURE_LIST":
                        _settings.Gui.TestTree.FixtureList.GroupBy = _view.GroupBy.SelectedItem;
                        break;
                }
            };

            _view.StopRunButton.Execute += ExecuteNormalStop;
            _view.ForceStopButton.Execute += ExecuteForcedStop;

            _view.RunParametersButton.Execute += DisplayTestParametersDialog;

            _view.RunSummaryButton.CheckedChanged += () =>
            {
                if (_view.RunSummaryButton.Checked)
                {
                    var report = ResultSummaryReporter.WriteSummaryReport(_model.ResultSummary);
                    _view.RunSummaryDisplay.Display(report);
                }
                else
                {
                    _view.RunSummaryDisplay.Hide();
                }
            };

            _view.ToolsMenu.Popup += () =>
            {
                _view.SaveResultsAsMenu.MenuItems.Clear();

                foreach (string format in _resultFormats)
                {
                    var formatItem = new ToolStripMenuItem(format);
                    formatItem.Click += (s, e) => SaveResults(format);
                    _view.SaveResultsAsMenu.MenuItems.Add(formatItem);
                }
            };

            _view.SaveResultsCommand.Execute += () => SaveResults();

            _view.OpenWorkDirectoryCommand.Execute += () => System.Diagnostics.Process.Start(_model.WorkDirectory);

            _view.TreeView.ShowCheckBoxes.CheckedChanged += () => UpdateRunSelectedTestsTooltip();

            _view.ExtensionsCommand.Execute += () =>
            {
                using (var extensionsDialog = new ExtensionDialog(_model.Services.ExtensionService))
                {
                    extensionsDialog.Font = _settings.Gui.Font;
                    extensionsDialog.ShowDialog();
                }
            };

            _view.SettingsCommand.Execute += () =>
            {
                SettingsDialog.Display(this, _model);
            };

            _view.TestCentricHelpCommand.Execute += () =>
            {
                System.Diagnostics.Process.Start("https://test-centric.org/testcentric-gui");
            };

            _view.NUnitHelpCommand.Execute += () =>
            {
                System.Diagnostics.Process.Start("https://docs.nunit.org/articles/nunit/intro.html");
            };

            _view.AboutCommand.Execute += () =>
            {
                using (AboutBox aboutBox = new AboutBox())
                {
                    aboutBox.ShowDialog();
                }
            };

            _view.ResultTabs.SelectionChanged += () =>
            {
                _settings.Gui.SelectedTab = _view.ResultTabs.SelectedIndex;
            };

            #endregion
        }

        private void SaveFormLocationAndSize(string guiLayout)
        {
            if (guiLayout == "Mini")
            {
                _settings.Gui.MiniForm.Location = _view.Location;
                _settings.Gui.MiniForm.Size = _view.Size;
            }
            else
            {
                _settings.Gui.MainForm.Location = _view.Location;
                _settings.Gui.MainForm.Size = _view.Size;
            }
        }

        private void ExecuteNormalStop()
        {
            BeginLongRunningOperation("Waiting for currently running tests to complete. Use the Kill button to terminate the process without waiting.");
            _stopRequested = true;
            _forcedStopRequested = false;
            _model.StopTestRun(false);
            UpdateViewCommands();
        }

        private void ExecuteForcedStop()
        {
            UpdateLongRunningOperation("Process is being terminated.");
            _stopRequested = _forcedStopRequested = true;
            UpdateViewCommands(false);

            _model.StopTestRun(true);
        }

        private void DisplayTestParametersDialog()
        {
            using (var dlg = new TestParametersDialog())
            {
                dlg.Font = _settings.Gui.Font;
                dlg.StartPosition = FormStartPosition.CenterParent;

                if (_model.TestCentricProject.Settings.ContainsKey("TestParametersDictionary"))
                {
                    var testParms = _model.TestCentricProject  .Settings["TestParametersDictionary"] as IDictionary<string, string>;
                    foreach (string key in testParms.Keys)
                        dlg.Parameters.Add(key, testParms[key]);
                }

                if (dlg.ShowDialog(_view as IWin32Window) == DialogResult.OK)
                {
                    ChangePackageSettingAndReload("TestParametersDictionary", dlg.Parameters);
                }
            }
        }

        #endregion

        #region Public Methods

        #region Project Management

        private void NewProject()
        {
            var files = _view.DialogManager.SelectMultipleFiles("New Project", CreateOpenFileFilter());
            if (files.Count > 0)
            {
                _model.CreateNewProject();
                _model.AddTests(files);
            }
        }

        private void OpenProject()
        {
            var projectPath = _view.DialogManager.GetFileOpenPath("Open TestCentric Project", "TestCentric Project (*.tcproj) | *.tcproj");
            if (projectPath != null)
                _model.OpenExistingProject(projectPath);
        }

        private void SaveProject()
        {
            var projectPath = _view.DialogManager.GetFileSavePath("Save TestCentric Project", "TestCentric Project(*.tcproj) | *.tcproj", _model.WorkDirectory, null);
            if (projectPath != null)
            {
                try
                {
                    _model.SaveProject(projectPath);
                }
                catch (Exception exception)
                {
                    _view.MessageDisplay.Error("Unable to Save Results\n\n" + MessageBuilder.FromException(exception));
                }
            }
        }

        #endregion

        #region Close Methods

        public MessageBoxResult CloseProject()
        {
            MessageBoxResult messageBoxResult = MessageBoxResult.OK;
            if (!_options.Unattended && _model.TestCentricProject.IsDirty)
            {
                messageBoxResult = _view.MessageDisplay.YesNoCancel($"Do you want to save {_model.TestCentricProject.Name}?");
                if (messageBoxResult == MessageBoxResult.Yes)
                    SaveProject();
            }

            if (messageBoxResult != MessageBoxResult.Cancel)
                _model.CloseProject();

            return messageBoxResult;
        }

        #endregion

        #region Add Methods

        public void AddTestFiles()
        {
            var filesToAdd = _view.DialogManager.SelectMultipleFiles("Add Test Files", CreateOpenFileFilter());

            if (filesToAdd.Count > 0)
                _model.AddTests(filesToAdd);
        }

        #endregion

        #region Save Methods

        public void SaveResults(string format = "nunit3")
        {
            string savePath = _view.DialogManager.GetFileSavePath($"Save Results in {format} format", "XML Files (*.xml)|*.xml|All Files (*.*)|*.*", _model.WorkDirectory, "TestResult.xml");

            if (savePath != null)
            {
                try
                {
                    _model.SaveResults(savePath, format);

                    _view.MessageDisplay.Info(String.Format($"Results saved in {format} format as {savePath}"));
                }
                catch (Exception exception)
                {
                    _view.MessageDisplay.Error("Unable to Save Results\n\n" + MessageBuilder.FromException(exception));
                }
            }
        }

        #endregion

        #region Reload Methods

        public void ReloadTests()
        {
            _model.ReloadTests();
            //NUnitProject project = loader.TestProject;

            //bool wrapper = project.IsAssemblyWrapper;
            //string projectPath = project.ProjectPath;
            //string activeConfigName = project.ActiveConfigName;

            //// Unload first to avoid message asking about saving
            //loader.UnloadProject();

            //if (wrapper)
            //    OpenProject(projectPath);
            //else
            //    OpenProject(projectPath, activeConfigName, null);
        }

        #endregion

        #endregion

        #region Helper Methods

        private void UpdateViewCommands(bool testLoading = false)
        {
            bool testLoaded = _model.HasTests;
            bool testRunning = _model.IsTestRunning;
            bool hasResults = _model.HasResults;
            bool hasFailures = _model.HasResults && _model.ResultSummary.FailedCount > 0;

            _view.RunAllButton.Enabled =
            _view.DisplayFormatButton.Enabled =
            _view.RunParametersButton.Enabled = testLoaded && !testRunning;
            _view.ShowHideFilterButton.Enabled = testLoaded && _view.DisplayFormat.SelectedItem == "NUNIT_TREE";
            _view.ShowHideFilterButton.Visible = testLoaded && _view.DisplayFormat.SelectedItem == "NUNIT_TREE";

            _view.RunSelectedButton.Enabled = testLoaded && !testRunning && _model.SelectedTests != null && _model.SelectedTests.Any();

            _view.RerunButton.Enabled = testLoaded && !testRunning && hasResults;

            _view.RunFailedButton.Enabled = testLoaded && !testRunning && hasFailures;

            bool displayForcedStop = testRunning && _stopRequested;
            _view.ForceStopButton.Visible = displayForcedStop;
            _view.ForceStopButton.Enabled = displayForcedStop && !_forcedStopRequested;
            _view.StopRunButton.Visible = !displayForcedStop;
            _view.StopRunButton.Enabled = testRunning && !_stopRequested;

            _view.RunSummaryButton.Enabled = testLoaded && !testRunning && hasResults;

            _view.NewProjectCommand.Enabled = !testLoading && !testRunning;
            _view.OpenProjectCommand.Enabled = !testLoading && !testRunning;
            _view.SaveProjectCommand.Enabled = testLoaded && !testRunning;

            _view.CloseProjectCommand.Enabled = testLoaded & !testRunning;
            _view.AddTestFilesCommand.Enabled = testLoaded && !testRunning;
            _view.ReloadTestsCommand.Enabled = testLoaded && !testRunning;
            _view.RecentFilesMenu.Enabled = !testRunning && !testLoading;
            _view.ExitCommand.Enabled = !testLoading;
            _view.SaveResultsCommand.Enabled = _view.SaveResultsAsMenu.Enabled = !testRunning && !testLoading && hasResults;
        }

        private void UpdateRunSelectedTestsTooltip()
        {
            bool showCheckBoxes = _view.TreeView.ShowCheckBoxes.Checked;
            IToolTip tooltip = _view.RunSelectedButton as IToolTip;
            if (tooltip != null)
                tooltip.ToolTipText = showCheckBoxes ? "Run Checked Tests" : "Run Selected Tests";
        }

        private string CreateOpenFileFilter()
        {
            StringBuilder sb = new StringBuilder();
            bool nunit = _model.NUnitProjectSupport;
            bool vs = _model.VisualStudioSupport;

            if (nunit && vs)
                sb.Append("Projects & Assemblies (*.nunit,*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln,*.dll,*.exe)|*.nunit;*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln;*.dll;*.exe|");
            else if (nunit)
                sb.Append("Projects & Assemblies (*.nunit,*.dll,*.exe)|*.nunit;*.dll;*.exe|");
            else if (vs)
                sb.Append("Projects & Assemblies (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln,*.dll,*.exe)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln;*.dll;*.exe|");

            if (nunit)
                sb.Append("NUnit Projects (*.nunit)|*.nunit|");

            if (vs)
                sb.Append("Visual Studio Projects (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln|");

            sb.Append("Assemblies (*.dll,*.exe)|*.dll;*.exe|");

            sb.Append("All Files (*.*)|*.*");

            return sb.ToString();
        }

        private static bool CanWriteProjectFile(string path)
        {
            return !File.Exists(path) ||
                (File.GetAttributes(path) & FileAttributes.ReadOnly) == 0;
        }

        private static string Quoted(string s)
        {
            return "\"" + s + "\"";
        }

        private void ChangePackageSettingAndReload(string key, object setting)
        {
            var settings = _model.TestCentricProject.Settings;
            if (setting == null || setting as string == "DEFAULT")
                settings.Remove(key);
            else
                settings[key] = setting;

            // Even though the _model has a Reload method, we cannot use it because Reload
            // does not re-create the Engine.  Since we just changed a setting, we must
            // re-create the Engine by unloading/reloading the tests. We make a copy of
            // __model.TestFiles because the method does an unload before it loads.
            _model.TestCentricProject.LoadTests();
        }

        private void applyFont(Font font)
        {
            _settings.Gui.Font = _view.Font = font;
        }

        private void SetGuiLayout(string guiLayout)
        {
            Point location;
            Size size;
            bool isMaximized = false;
            bool useFullGui = guiLayout != "Mini";

            // Configure the GUI
            _view.Configure(useFullGui);

            if (useFullGui)
            {
                location = _settings.Gui.MainForm.Location;
                size = _settings.Gui.MainForm.Size;
                isMaximized = _settings.Gui.MainForm.Maximized;
            }
            else
            {
                location = _settings.Gui.MiniForm.Location;
                size = _settings.Gui.MiniForm.Size;
                isMaximized = _settings.Gui.MiniForm.Maximized;
            }

            if (!IsValidLocation(location, size))
                location = new Point(10, 10);

            _view.Location = location;
            _view.Size = size;
            _view.Maximized = isMaximized;

            if (useFullGui)
                _view.SplitterPosition = _settings.Gui.MainForm.SplitPosition;
            
            _view.StatusBarView.Visible = useFullGui && _settings.Gui.MainForm.ShowStatusBar;
        }

        private static bool IsValidLocation(Point location, Size size)
        {
            Rectangle myArea = new Rectangle(location, size);
            bool intersect = false;
            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens)
            {
                intersect |= myArea.IntersectsWith(screen.WorkingArea);
            }
            return intersect;
        }

        private static Font IncreaseFont(Font font)
        {
            return new Font(font.FontFamily, font.SizeInPoints * 1.2f, font.Style);
        }

        private static Font DecreaseFont(Font font)
        {
            return new Font(font.FontFamily, font.SizeInPoints / 1.2f, font.Style);
        }

        private LongRunningOperationDisplay _longRunningOperation;

        private void BeginLongRunningOperation(string text)
        {
            _longRunningOperation = new LongRunningOperationDisplay();
            _longRunningOperation.Display(text);
        }

        private void UpdateLongRunningOperation(string text)
        {
            _longRunningOperation?.Update(text);
        }

        private void OnLongRunningOperationComplete()
        {
            if (_longRunningOperation != null)
            {
                _longRunningOperation.InvokeIfRequired(() => { _longRunningOperation.Close(); });
                _longRunningOperation = null;
            }
        }

        private void UpdateTreeDisplayMenuItem()
        { 
            // Get current display format from settings
            string displayFormat = _settings.Gui.TestTree.DisplayFormat;

            _view.DisplayFormat.SelectedItem = displayFormat;

            switch (displayFormat)
            {
                case "NUNIT_TREE":
                    _view.GroupBy.Enabled = false;
                    break;
                case "TEST_LIST":
                    _view.GroupBy.Enabled = true;
                    _view.GroupBy.SelectedItem = _settings.Gui.TestTree.TestList.GroupBy;
                    break;
                case "FIXTURE_LIST":
                    _view.GroupBy.Enabled = true;
                    // HACK: Should be handled by the element itself
                    if (_view.GroupBy is Elements.CheckedToolStripMenuGroup menuGroup)
                        menuGroup.MenuItems[1].Enabled = false;
                    _view.GroupBy.SelectedItem = _settings.Gui.TestTree.FixtureList.GroupBy;
                    break;
            }

            _view.ShowNamespace.SelectedIndex = _settings.Gui.TestTree.ShowNamespace ? 0 : 1;
            _view.ShowNamespace.Enabled = displayFormat == "NUNIT_TREE";
        }

        private void RunAllTests()
        {
            _model.ClearResults();
            var allTests = _model.LoadedTests;
            _model.RunTests(allTests);
        }

        private void RunSelectedTests()
        {
            var testSelection = _model.SelectedTests;
            _model.RunTests(testSelection);
        }

        private void RunFailedTests()
        {
            var failedTests = new TestSelection();

            foreach (var entry in _model.Results)
            {
                var test = entry.Value;
                if (!test.IsSuite && test.Outcome.Status == TestStatus.Failed)
                {
                    TestNode testNode = _model.GetTestById(test.Id);
                    failedTests.Add(testNode);
                }
            }

            _model.RunTests(failedTests);
        }

        #endregion
    }
}
