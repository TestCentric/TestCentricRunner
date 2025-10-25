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
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Reflection;
    using Dialogs;
    using Model;
    using Model.Services;
    using Model.Settings;
    using TestCentric.Engine;
    using TestCentric.Gui.Controls;
    using TestCentric.Gui.Elements;
    using Views;

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

        private readonly GuiOptions _options;

        private readonly UserSettings _settings;

        private string _guiLayout;


        private AgentSelectionController _agentSelectionController;

        private string[] _lastFilesLoaded = null;

        private bool _stopRequested;
        private bool _forcedStopRequested;

        #endregion

        #region Constructor

        public TestCentricPresenter(IMainView view, ITestModel model, GuiOptions options)
        {
            _view = view;
            _model = model;
            _options = options;

            _settings = _model.Settings;

            _agentSelectionController = new AgentSelectionController(_model, _view);
            ImageSetManager = new ImageSetManager(_model, _view);

            _view.Font = _settings.Gui.Font;

            UpdateViewCommands();
            UpdateTreeDisplayMenuItem();
            UpdateRunSelectedTestsTooltip();
            UpdateSaveResultFormatsMenuItem();

            WireUpEvents();
            _view.ShowHideFilterButton.Checked = _settings.Gui.TestTree.ShowFilter;
        }

        #endregion

        #region Event Handling

        private void WireUpEvents()
        {
            #region Model Events

            _model.Events.TestCentricProjectLoaded += (TestEventArgs e) => UpdateTitlebar();

            _model.Events.TestCentricProjectUnloaded += (TestEventArgs e) => UpdateTitlebar();

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

            _model.Events.TestChanged += (e) =>
            {
                _model.ReloadTests();
            };

            _model.Events.RunStarting += (RunStartingEventArgs e) =>
            {
                _stopRequested = _forcedStopRequested = false;
                UpdateViewCommands();
            };

            _model.Events.RunFinished += (TestResultEventArgs e) => OnRunFinished(e.Result);

            // Separate internal method for testing
            void OnRunFinished(ResultNode result)
            {
                log.Debug("Test run complete");
                OnLongRunningOperationComplete();

                UpdateViewCommands();

                string resultPath = Path.Combine(_model.WorkDirectory, "TestResult.xml");
                log.Debug($"Saving result to {resultPath}");
                _model.SaveResults(resultPath);
                _view.ResultTabs.InvokeIfRequired(() => _view.ResultTabs.SelectedIndex = 1);

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
                    case "TestCentric.Gui.TestTree.NUnitGroupBy":
                        _view.NUnitGroupBy.SelectedItem = _settings.Gui.TestTree.NUnitGroupBy;
                        break;
                    case "TestCentric.Gui.TestTree.TestList.GroupBy":
                        _view.TestListGroupBy.SelectedItem = _settings.Gui.TestTree.TestList.GroupBy;
                        break;
                    case "TestCentric.Gui.TestTree.FixtureList.GroupBy":
                        _view.FixtureListGroupBy.SelectedItem = _settings.Gui.TestTree.FixtureList.GroupBy;
                        break;
                    case "TestCentric.Gui.TestTree.ShowNamespace":
                        _view.ShowNamespace.Checked = _settings.Gui.TestTree.ShowNamespace;
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

                _agentSelectionController.PopulateMenu();

                // Create an unnamed TestCentricProject and load test specified on command line
                if (_options.InputFiles.Count == 1)
                    _model.OpenExistingFile(_options.InputFiles[0]);
                else if (_options.InputFiles.Count > 1)
                    _model.CreateNewProject(_options.InputFiles);
                else if (_settings.Gui.LoadLastProject && !_options.NoLoad)
                    _model.OpenMostRecentFile();

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

                _view.OpenProjectCommand.Enabled = _view.OpenTestCentricProjectCommand.Enabled = _view.OpenTestAssemblyCommand.Enabled = !isTestRunning;
                _view.CloseProjectCommand.Enabled = isPackageLoaded && !isTestRunning;

                _view.ReloadTestsCommand.Enabled = isPackageLoaded && !isTestRunning;

                _agentSelectionController.UpdateMenuItems();

                _view.RunAsX86.Enabled = isPackageLoaded && !isTestRunning;

                _view.RecentFilesMenu.Enabled = !isTestRunning;
            };

            _view.OpenTestCentricProjectCommand.Execute += OpenTestCentricProject;
            _view.OpenTestAssemblyCommand.Execute += OpenTestAssembly;

            _view.SaveProjectCommand.Execute += SaveProject;

            _view.CloseProjectCommand.Execute += () => CloseProject();
            _view.AddTestFilesCommand.Execute += AddTestFiles;
            _view.ReloadTestsCommand.Execute += ReloadTests;

            //_view.SelectAgentMenu.Popup += () =>
            //{
            //    _agentSelectionController.PopulateMenu();
            //};

            _view.RunAsX86.CheckedChanged += () =>
            {
                var key = SettingDefinitions.RunAsX86.Name;
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

            _view.ShowNamespace.CheckedChanged += () =>
            {
                _settings.Gui.TestTree.ShowNamespace = _view.ShowNamespace.Checked;
            };

            _view.ShowHideFilterButton.CheckedChanged += () =>
            {
                _settings.Gui.TestTree.ShowFilter = _view.ShowHideFilterButton.Checked;
            };

            _view.NUnitGroupBy.SelectionChanged += () =>
            {
                _settings.Gui.TestTree.NUnitGroupBy = _view.NUnitGroupBy.SelectedItem;
            };

            _view.TestListGroupBy.SelectionChanged += () =>
            {
                _settings.Gui.TestTree.TestList.GroupBy = _view.TestListGroupBy.SelectedItem;
            };

            _view.FixtureListGroupBy.SelectionChanged += () =>
            {
                _settings.Gui.TestTree.FixtureList.GroupBy = _view.FixtureListGroupBy.SelectedItem;
            };

            _view.StopRunButton.Execute += ExecuteNormalStop;
            _view.ForceStopButton.Execute += ExecuteForcedStop;

            _view.RunParametersButton.Execute += DisplayTestParametersDialog;

            _view.TransformResultsCommand.Execute += TransformResults;

            _view.OpenWorkDirectoryCommand.Execute += () => System.Diagnostics.Process.Start(_model.WorkDirectory);

            _view.TreeView.ShowCheckBoxes.CheckedChanged += UpdateRunSelectedTestsTooltip;

            _view.ExtensionsCommand.Execute += () =>
            {
                using (var extensionsDialog = new ExtensionDialog(_model.Services.GetService<Engine.Services.IExtensionService>()))
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

            _view.CommandLineHelpCommand.Execute += () =>
            {
                _view.MessageDisplay.Info(_model.Options.GetHelpText());
            };

            _view.AboutCommand.Execute += () =>
            {
                using (AboutBox aboutBox = new AboutBox())
                {
                    aboutBox.ShowDialog();
                }
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

                if (_model.TestCentricProject.Settings.HasSetting("TestParametersDictionary"))
                {
                    var testParms = _model.TestCentricProject.Settings.GetSetting("TestParametersDictionary") as IDictionary<string, string>;
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

        #region Public Properties and Methods

        #region Properties

        public ImageSetManager ImageSetManager { get; }

        #endregion

        #region Project Management

        private void OpenTestCentricProject()
        {
            var filter = "TestCentric Projects (*.tcproj)|*.tcproj";

            string file = _view.DialogManager.GetFileOpenPath("Existing Project", filter);
            if (!string.IsNullOrEmpty(file))
                _model.OpenExistingProject(file);
        }

        private void OpenTestAssembly()
        {
            IList<string> files = _view.DialogManager.SelectMultipleFiles("New Project", CreateOpenFileFilter());
            if (files.Any())
                _model.CreateNewProject(files);
        }

        private void SaveProject()
        {
            var projectPath = _view.DialogManager.GetFileSavePath("Save TestCentric Project", "TestCentric Project(*.tcproj) | *.tcproj", _model.WorkDirectory, null);
            if (projectPath != null)
            {
                try
                {
                    _model.SaveProject(projectPath);
                    UpdateTitlebar();
                }
                catch (Exception exception)
                {
                    _view.MessageDisplay.Error("Unable to save project\n\n" + MessageBuilder.FromException(exception));
                }
            }
        }

        private void UpdateTitlebar()
        {
            string title = "TestCentric Runner for NUnit";
            if (_model.TestCentricProject != null)
            {
                title = $"TestCentric - {_model.TestCentricProject.FileName ?? "UNNAMED.tcproj"}";

            }
            _view.Title = title;
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
            string savePath = _view.DialogManager.GetFileSavePath($"Save results in {format} format", "XML Files (*.xml)|*.xml|All Files (*.*)|*.*", _model.WorkDirectory, "TestResult.xml");

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

        public void TransformResults()
        {
            TransformTestResultDialog dlg = new TransformTestResultDialog();
            dlg.StartPosition = FormStartPosition.CenterParent;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                string fileName = dlg.TransformationFile;
                string targetFileName = dlg.TargetFile;
                _model.TransformResults(targetFileName, fileName);

                _view.MessageDisplay.Info(String.Format($"Results transformed into {targetFileName} using {fileName}"));
            }
            catch (Exception exception)
            {
                _view.MessageDisplay.Error("Unable to Transform Results\n\n" + MessageBuilder.FromException(exception));
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

            _view.OpenProjectCommand.Enabled = _view.OpenTestCentricProjectCommand.Enabled = _view.OpenTestAssemblyCommand.Enabled = !testLoading && !testRunning;
            _view.SaveProjectCommand.Enabled = testLoaded && !testRunning;

            _view.CloseProjectCommand.Enabled = testLoaded & !testRunning;
            _view.AddTestFilesCommand.Enabled = testLoaded && !testRunning;
            _view.ReloadTestsCommand.Enabled = testLoaded && !testRunning;
            _view.RecentFilesMenu.Enabled = !testRunning && !testLoading;
            _view.ExitCommand.Enabled = !testLoading;
            _view.SaveResultsCommand.Enabled = _view.TransformResultsCommand.Enabled = !testRunning && !testLoading && hasResults;
        }

        private void UpdateRunSelectedTestsTooltip()
        {
            bool showCheckBoxes = _view.TreeView.ShowCheckBoxes.Checked;
            IToolTip tooltip = _view.RunSelectedButton as IToolTip;
            if (tooltip != null)
                tooltip.ToolTipText = showCheckBoxes ? "Run Checked Tests" : "Run Selected Tests";
        }

        private void UpdateSaveResultFormatsMenuItem()
        {
            int index = 0;
            foreach (string format in _model.ResultFormats.Except(new[] { "user", "cases" }))
            {
                var formatItem = new ToolStripMenuItem($"Format: {format}");
                formatItem.Click += (s, e) => SaveResults(format);
                _view.SaveResultsCommand.MenuItems?.Insert(index++, formatItem);
            }
        }

        private string CreateOpenFileFilter(bool testCentricProject = false)
        {
            StringBuilder sb = new StringBuilder();
            bool nunit = _model.NUnitProjectSupport;
            bool vs = _model.VisualStudioSupport;

            if (nunit || vs || testCentricProject)
            {
                List<string> supportedSuffix = new List<string>();
                if (nunit)
                    supportedSuffix.Add("*.nunit");
                if (vs)
                    supportedSuffix.AddRange(new[] { "*.csproj", "*.fsproj", "*.vbproj", "*.vjsproj", "*.vcproj", "*.sln" });
                if (testCentricProject)
                    supportedSuffix.Add("*.tcproj");

                supportedSuffix.AddRange(new[] { "*.dll", "*.exe" });

                var description = string.Join(",", supportedSuffix);
                var filter = string.Join(";", supportedSuffix);

                string str = $"Projects & Assemblies ({description})|{filter}|";
                sb.Append(str);
            }

            if (nunit)
                sb.Append("NUnit Projects (*.nunit)|*.nunit|");

            if (vs)
                sb.Append("Visual Studio Projects (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln|");

            if (testCentricProject)
                sb.Append("TestCentric Projects (*.tcproj)|*.tcproj|");

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
                settings.Set(key, setting);

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
            _view.ResultTabs.InvokeIfRequired(() => _longRunningOperation.Display(text));
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
                    _view.NUnitGroupBy.SelectedItem = _settings.Gui.TestTree.NUnitGroupBy;
                    break;
                case "TEST_LIST":
                    _view.TestListGroupBy.SelectedItem = _settings.Gui.TestTree.TestList.GroupBy;
                    break;
                case "FIXTURE_LIST":
                    _view.FixtureListGroupBy.SelectedItem = _settings.Gui.TestTree.FixtureList.GroupBy;
                    break;
            }

            _view.ShowNamespace.Checked = _settings.Gui.TestTree.ShowNamespace;
            _view.ShowNamespace.Enabled = displayFormat == "NUNIT_TREE";
        }

        private void RunAllTests()
        {
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
            var failedTests = new TestSelection(_model.TestResultManager.GetFailedTests());
            _model.RunTests(failedTests);
        }

        #endregion
    }
}
