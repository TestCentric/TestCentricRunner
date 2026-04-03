// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using NUnit;
using NUnit.Engine;
using TestCentric.Gui.Dialogs;
using TestCentric.Gui.Elements;
using TestCentric.Gui.Model;
using TestCentric.Gui.Model.Settings;
using TestCentric.Gui.Views;

using SettingDefinitions = NUnit.Common.SettingDefinitions;

namespace TestCentric.Gui.Presenters
{
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

        private readonly IUserSettings _settings;

        private string _guiLayout;


        private AgentSelectionController _agentSelectionController;
        private RecentFileMenuController _recentProjectController;
        private RecentFileMenuController _recentFileController;
        private string[] _lastFilesLoaded = null;

        private bool _stopRequested;
        private bool _forcedStopRequested;

        #endregion

        #region Constructor

        public TestCentricPresenter(IMainView view, ITestModel model)
        {
            _view = view;
            _model = model;

            _options = _model.Options;
            TreeConfiguration = _model.TreeConfiguration;
            _settings = _model.Settings;

            _agentSelectionController = new AgentSelectionController(_model, _view);
            _recentProjectController = new RecentFileMenuController(_model, _view.RecentProjectsMenu, true);
            _recentFileController = new RecentFileMenuController(_model, _view.RecentFilesMenu, false);

            ImageSetManager = new ImageSetManager(_model, _view);

            _view.Font = _settings.Gui.Font;

            UpdateViewCommands();
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

            _model.Events.TestCentricProjectLoaded += (TestEventArgs e) =>
            {
                // Update checked state according to loaded project settings
                // Unregister CheckedChanged event temporarily to avoid reloading (while loading a project)
                _view.RunAsX86.CheckedChanged -= OnRunAsX86Changed;
                _view.RunAsX86.Checked = _model.TopLevelPackage.Settings.GetValueOrDefault(SettingDefinitions.RunAsX86);
                _view.RunAsX86.CheckedChanged += OnRunAsX86Changed;

                UpdateTitleBar();
            };

            _model.Events.TestCentricProjectUnloaded += (TestEventArgs e) => UpdateTitleBar();

            void UpdateTitleBar()
            {
                var projectPath = _model.TestCentricProject?.ProjectPath;
                _view.Title = projectPath is not null
                    ? projectPath is not null && _model.IsWrapperProjectPath(projectPath)
                        ? $"TestCentric - {Path.GetFileNameWithoutExtension(projectPath)}"
                        : $"TestCentric - {Path.GetFileName(projectPath)}"
                    : "TestCentric Runner for NUnit";
            }

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

            _model.Events.TestsUnloading += (TestEventArgs e) =>
            {
                UpdateViewCommands();

                _model.SaveProject();

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
                }
            };

            _model.Events.UnhandledException += (TestCentric.Gui.Model.UnhandledExceptionEventArgs e) =>
            {
                _view.MessageDisplay.Error($"{e.Message}\n\n{e.StackTrace}", "TestCentric - Internal Error");
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

                // Load Project and Tests
                var testFiles = _options.InputFiles.ToArray();
                if (testFiles.Length > 0)
                {
                    if (_options.Unattended)
                        _model.CreateNewProject("TestProject", testFiles);
                    else if (testFiles.Length == 1 && TestCentricProject.IsProjectFile(testFiles[0]))
                        _model.OpenExistingProject(testFiles[0]);
                    else 
                        CreateNewProject(testFiles);
                }
                else if (_settings.Gui.LoadLastProject && !_options.NoLoad)
                    _model.OpenMostRecentFile();

                // Run loaded test automatically if called for.
                if (_model.HasTests && _options.RunAllTests)
                    _model.RunTests(_model.LoadedTests);
                // If --unattended was specified and we are not running
                // tests automatically, close now. (If we did run tests,
                // the app will close at the end of the test run.)
                else if (_options.Unattended)
                    _view.Close();
            };

            _view.SplitterPositionChanged += (s, e) =>
            {
                _settings.Gui.MainForm.SplitPosition = _view.SplitterPosition;
            };

            _view.FormClosing += (s, e) =>
            {
                if (_model.IsProjectLoaded && _model.IsTestRunning)
                {
                    if (!_view.MessageDisplay.YesNo("A test is running, do you want to forcibly stop the test and exit?"))
                    {
                        e.Cancel = true;
                        return;
                    }

                    _stopRequested = _forcedStopRequested = true;
                    _model.StopTestRun(true);
                }

                _model.CloseProject();

                SaveFormLocationAndSize(_guiLayout);
            };

            _view.FileMenu.Popup += () =>
            {
                bool isPackageLoaded = _model.IsProjectLoaded;
                bool isTestRunning = _model.IsTestRunning;

                _view.OpenProjectCommand.Enabled = _view.OpenTestFileCommand.Enabled = !isTestRunning;
                _view.CloseProjectCommand.Enabled = isPackageLoaded && !isTestRunning;

                var projectPath = _model.TestCentricProject?.ProjectPath;
                _view.EditProjectCommand.Enabled = projectPath is not null;
                _view.EditProjectCommand.Text = projectPath is not null && _model.IsWrapperProjectPath(projectPath)
                    ? "Add Test Files..."
                    : "Edit Project...";

                _view.ReloadTestsCommand.Enabled = isPackageLoaded && !isTestRunning;

                _agentSelectionController.UpdateMenuItems();
                _recentFileController.PopulateMenu();
                _recentProjectController.PopulateMenu();

                _view.RunAsX86.Enabled = isPackageLoaded && !isTestRunning;

                _view.RecentProjectsMenu.Enabled = _view.RecentFilesMenu.Enabled = !isTestRunning;
            };

            _view.NewProjectCommand.Execute += () => CreateNewProject();

            _view.OpenProjectCommand.Execute += () => OpenExistingProject();

            _view.OpenTestFileCommand.Execute += () =>
            {
                string file = _view.DialogManager.GetFileOpenPath("Open Test File", _view.DialogManager.CreateOpenTestFileFilter());
                if (file != null)
                    _model.OpenOrCreateWrapperProject(file);
            };

            _view.SaveProjectCommand.Execute += () =>
            {
                var projectPath = _model.TestCentricProject.ProjectPath;

                if (string.IsNullOrEmpty(projectPath))
                    projectPath = _model.TestCentricProject.TestFiles.Count == 1
                        ? _model.TestCentricProject.TestFiles[0] + ".tcproj"
                        : _view.DialogManager.GetFileSavePath(
                            "Save TestCentric Project",
                            "TestCentric Project(*.tcproj) | *.tcproj",
                            _model.WorkDirectory, null);

                if (projectPath is not null)
                    _model.SaveProject(projectPath);
            };

            _view.SaveAsCommand.Execute += () =>
                _view.MessageDisplay.Error("Not Yet Implemented!");

            _view.CloseProjectCommand.Execute += () => _model.CloseProject();

            _view.EditProjectCommand.Execute += () => EditProject();

            _view.ReloadTestsCommand.Execute += ReloadTests;

            _view.RunAsX86.CheckedChanged += OnRunAsX86Changed;

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

            _view.DisplayFormatButton.Execute += () =>
                new DisplayStrategyDialog(_model).ShowDialog();

            _view.ShowHideFilterButton.CheckedChanged += () =>
                _settings.Gui.TestTree.ShowFilter = _view.ShowHideFilterButton.Checked;

            _view.StopRunButton.Execute += ExecuteNormalStop;
            _view.ForceStopButton.Execute += ExecuteForcedStop;

            _view.RunParametersButton.Execute += DisplayTestParametersDialog;

            _view.TransformResultsCommand.Execute += TransformResults;

            _view.OpenWorkDirectoryCommand.Execute += () => System.Diagnostics.Process.Start(_model.WorkDirectory);

            _view.TreeView.ShowCheckBoxes.CheckedChanged += UpdateRunSelectedTestsTooltip;

            _view.ExtensionsCommand.Execute += () =>
            {
                using (var extensionsDialog = new ExtensionDialog(_model.Services.GetService<IExtensionService>()))
                {
                    extensionsDialog.Font = _settings.Gui.Font;
                    extensionsDialog.ShowDialog();
                }
            };

            _view.SettingsCommand.Execute += () =>
            {
                SettingsDialog.Display(this, _model);
            };

            _view.TestRunSettingsCommand.Execute += () =>
            {
                SettingsDialog.DisplayTestRunSettings(this, _model);
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

        /// <summary>
        /// Create a new project using information from user
        /// </summary>
        private void CreateNewProject()
        {
            var dlg = new ProjectEditor(_view, _model);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var projectPath = dlg.ProjectPath;
                var testFiles = dlg.TestFiles;
                _model.CreateNewProject(projectPath, dlg.TestFiles);
            }
        }

        /// <summary>
        /// Create a new project for a set of files already provided
        /// </summary>
        /// <param name="testFiles">The test files to be used for the project</param>
        private void CreateNewProject(string[] testFiles)
        {
            using (var dlg = new ProjectNameDialog(_view, _model))
                if (dlg.ShowDialog() == DialogResult.OK)
                    _model.CreateNewProject(dlg.ProjectPath, testFiles);
        }

        private void OpenExistingProject()
        {
            string file = _view.DialogManager.GetFileOpenPath(
                "Open TestCentric Project", "TestCentric Projects (*.tcproj)|*.tcproj");
            if (!string.IsNullOrEmpty(file))
                _model.OpenExistingProject(file);
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

                if (_model.TopLevelPackage.Settings.HasSetting("TestParametersDictionary"))
                {
                    var testParms = _model.TopLevelPackage.Settings.GetSetting("TestParametersDictionary") as IDictionary<string, string>;
                    foreach (string key in testParms.Keys)
                        dlg.Parameters.Add(key, testParms[key]);
                }

                if (dlg.ShowDialog(_view as IWin32Window) == DialogResult.OK)
                    _model.TestCentricProject.SetTopLevelSetting(SettingDefinitions.TestParametersDictionary.WithValue(dlg.Parameters));
            }
        }

        #endregion

        #region Public Properties and Methods

        #region Properties

        public ImageSetManager ImageSetManager { get; }

        private ITreeConfiguration TreeConfiguration { get; }

        #endregion

        #region Add Methods

        public void AddTestFiles()
        {
            string[] filesToAdd = _view.DialogManager.SelectMultipleFiles("Add Test Files", _view.DialogManager.CreateOpenTestFileFilter());

            if (filesToAdd.Length > 0)
                _model.AddTests(filesToAdd);
        }

        public void EditProject()
        {
            var dlg = new ProjectEditor(_view, _model, _model.TestCentricProject);

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var projectPath = dlg.ProjectPath;
                var testFiles = dlg.TestFiles;
                _model.CreateNewProject(projectPath, dlg.TestFiles);
            }
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
            _view.ShowHideFilterButton.Enabled = testLoaded;
            _view.ShowHideFilterButton.Visible = testLoaded;

            _view.RunSelectedButton.Enabled = testLoaded && !testRunning && _model.SelectedTests != null && _model.SelectedTests.Any();

            _view.RerunButton.Enabled = testLoaded && !testRunning && hasResults;

            _view.RunFailedButton.Enabled = testLoaded && !testRunning && hasFailures;

            bool displayForcedStop = testRunning && _stopRequested;
            _view.ForceStopButton.Visible = displayForcedStop;
            _view.ForceStopButton.Enabled = displayForcedStop && !_forcedStopRequested;
            _view.StopRunButton.Visible = !displayForcedStop;
            _view.StopRunButton.Enabled = testRunning && !_stopRequested;

            _view.OpenProjectCommand.Enabled = _view.OpenProjectCommand.Enabled = _view.OpenTestFileCommand.Enabled = !testLoading && !testRunning;
            _view.SaveProjectCommand.Enabled = testLoaded && !testRunning;
            _view.SaveAsCommand.Enabled = testLoaded && !testRunning;

            _view.CloseProjectCommand.Enabled = testLoaded & !testRunning;
            _view.EditProjectCommand.Enabled = testLoaded && !testRunning;
            _view.ReloadTestsCommand.Enabled = testLoaded && !testRunning;
            _view.RecentFilesMenu.Enabled = !testRunning && !testLoading;
            _view.ExitCommand.Enabled = !testLoading;
            _view.SaveResultsCommand.Enabled = _view.TransformResultsCommand.Enabled = !testRunning && !testLoading && hasResults;
            _view.TestRunSettingsCommand.Enabled = testLoaded && !testRunning;
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

        private static bool CanWriteProjectFile(string path)
        {
            return !File.Exists(path) ||
                (File.GetAttributes(path) & FileAttributes.ReadOnly) == 0;
        }

        private static string Quoted(string s)
        {
            return "\"" + s + "\"";
        }

        private void OnRunAsX86Changed()
        {
            _model.TestCentricProject.SetTopLevelSetting(SettingDefinitions.RunAsX86.WithValue(_view.RunAsX86.Checked));
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

            _view.StatusBarView.Visible = useFullGui;
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
