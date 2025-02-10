// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
    using System.Xml.Serialization;
using TestCentric.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Gui.Model
{
    using System.Runtime.InteropServices;
    using Services;
    using Settings;
    using TestCentric.Gui.Model.Filter;

    public class TestModel : ITestModel
    {
        static Logger log = InternalTrace.GetLogger(typeof(TestModel));

        private const string PROJECT_LOADER_EXTENSION_PATH = "/NUnit/Engine/TypeExtensions/IProjectLoader";
        private const string NUNIT_PROJECT_LOADER = "TestCentric.Engine.Services.ProjectLoaders.NUnitProjectLoader";
        private const string VISUAL_STUDIO_PROJECT_LOADER = "TestCentric.Engine.Services.ProjectLoaders.VisualStudioProjectLoader";

        // Our event dispatcher. Events are exposed through the Events
        // property. This is used when firing events from the model.
        private TestEventDispatcher _events;

        // Check if the loaded Assemblies has been changed
        private AssemblyWatcher _assemblyWatcher;

        private SettingsService _settingsService;

        private TestRunSpecification _lastTestRun = TestRunSpecification.Empty;

        private bool _lastRunWasDebugRun;

        #region Constructor and Creation

        public TestModel(ITestEngine testEngine, CommandLineOptions options = null)
        {
            TestEngine = testEngine;
            Options = options ?? new CommandLineOptions();

            _settingsService = new SettingsService(true);
            _events = new TestEventDispatcher(this);
            _assemblyWatcher = new AssemblyWatcher();

            _settingsService.LoadSettings();
            Settings = new UserSettings(_settingsService);
            RecentFiles = new RecentFiles(_settingsService);

            Services = new TestServices(testEngine);
            TestCentricTestFilter = new TestCentricTestFilter(this, () => _events.FireTestFilterChanged());

            AvailableAgents = new List<string>(
                Services.TestAgentService.GetAvailableAgents().Select((a) => a.AgentName));

            foreach (var node in Services.ExtensionService.GetExtensionNodes(PROJECT_LOADER_EXTENSION_PATH))
            {
                if (node.TypeName == NUNIT_PROJECT_LOADER)
                    NUnitProjectSupport = true;
                else if (node.TypeName == VISUAL_STUDIO_PROJECT_LOADER)
                    VisualStudioSupport = true;
            }
        }

        public static ITestModel CreateTestModel(CommandLineOptions options)
        {
            var engine = TestEngineActivator.CreateInstance();
            if (engine == null)
                throw new EngineNotFoundException();

            return CreateTestModel(engine, options);
        }

        // Public for testing
        public static ITestModel CreateTestModel(ITestEngine testEngine, CommandLineOptions options)
        {
            // Currently the InternalTraceLevel can only be set from the command-line.
            // We can't use user settings to provide a default because the settings
            // are an engine service and the engine have the internal trace level
            // set as part of its initialization.
            var traceLevel = options.InternalTraceLevel != null
                ? (InternalTraceLevel)Enum.Parse(typeof(InternalTraceLevel), options.InternalTraceLevel)
                : InternalTraceLevel.Off;

            var logFile = $"InternalTrace.{Process.GetCurrentProcess().Id}.log";
            if (options.WorkDirectory != null)
                logFile = Path.Combine(options.WorkDirectory, logFile);

            testEngine.InternalTraceLevel = traceLevel;
            if (options.WorkDirectory != null)
                testEngine.WorkDirectory = options.WorkDirectory;

            return new TestModel(testEngine, options);
        }

        #endregion

        #region ITestModel Implementation

        #region General Properties

        // Command-line options
        public CommandLineOptions Options { get; }

        // Work Directory
        public string WorkDirectory { get { return TestEngine.WorkDirectory; } }

        // Event Dispatcher
        public ITestEvents Events { get { return _events; } }

        // Services provided either by the model itself or by the engine
        public ITestServices Services { get; }

        public UserSettings Settings { get; }

        public IList<string> AvailableAgents { get; }

        public RecentFiles RecentFiles { get; }

        // Project Support
        public bool NUnitProjectSupport { get; }
        public bool VisualStudioSupport { get; }

        // Runtime Support
        private List<IRuntimeFramework> _runtimes;
        public IList<IRuntimeFramework> AvailableRuntimes
        {
            get
            {
                if (_runtimes == null)
                    _runtimes = GetAvailableRuntimes();

                return _runtimes;
            }
        }

        // Result Format Support
        private List<string> _resultFormats;
        public IEnumerable<string> ResultFormats
        {
            get
            {
                if (_resultFormats == null)
                {
                    _resultFormats = new List<string>();
                    foreach (string format in Services.ResultService.Formats)
                        _resultFormats.Add(format);
                }

                return _resultFormats;
            }
        }

        #endregion

        #region Current State of the Model

        /// <summary>
        /// The current TestProject
        /// </summary>
        public TestCentricProject TestCentricProject { get; set; }

        public bool IsProjectLoaded => TestCentricProject != null;

        public TestNode LoadedTests { get; private set; }
        public bool HasTests => LoadedTests != null;

        public IList<string> AvailableCategories { get; private set; }

        public bool IsTestRunning => Runner != null && Runner.IsTestRunning;

        public IDictionary<string, ResultNode> Results { get; } = new Dictionary<string, ResultNode>();
        public ResultSummary ResultSummary { get; internal set; }
        public bool HasResults => ResultSummary != null;

        /// <summary>
        /// Gets or sets the active test item. This is the item
        /// for which details are displayed in the various views.
        /// </summary>
        public ITestItem ActiveTestItem
        {
            get { return _activeItem; }
            set { _activeItem = value; _events.FireActiveItemChanged(_activeItem); }
        }
        private ITestItem _activeItem;

        /// <summary>
        ///  Gets or sets the list of selected tests.
        /// </summary>
        public TestSelection SelectedTests 
        { 
            get {  return _selectedTests; }
            set { _selectedTests = value; _events?.FireSelectedTestsChanged(_selectedTests); }
        }
        private TestSelection _selectedTests;

        public List<string> SelectedCategories { get; private set; }

        public bool ExcludeSelectedCategories { get; private set; }

        public TestFilter CategoryFilter { get; private set; } = TestFilter.Empty;

        public ITestCentricTestFilter TestCentricTestFilter { get; private set; }

        #endregion

        #region Specifications passed as arguments to methods

        private class TestRunSpecification
        {
            public static TestRunSpecification Empty = new TestRunSpecification(new TestSelection(), null, false);

            // The selected tests to run (ITestItem may be a TestSelection or a TestNode
            public TestSelection SelectedTests { get; }

            // A possibly empty filter to be applied to the selected tests.
            // NOTE: Currently, filter is always empty
            public TestFilter CategoryFilter { get; }

            public bool DebuggingRequested { get; }

            public bool IsEmpty => SelectedTests.Count() == 0;

            public TestRunSpecification(TestSelection selectedTests, TestFilter filter, bool debuggingRequested)
            {
                SelectedTests = selectedTests;
                CategoryFilter = filter;
                DebuggingRequested = debuggingRequested;
            }

            public TestRunSpecification(TestNode testNode, TestFilter filter, bool debuggingRequested)
            {
                SelectedTests = new TestSelection { testNode };
                CategoryFilter = filter;
                DebuggingRequested = debuggingRequested;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Create a new unnamed project, assign it to our TestProject property and
        /// load tests for the project. We return the project as well.
        /// </summary>
        /// <param name="filenames">The test files contained as subprojects of the new project.</param>
        /// <returns>The newly created test project</returns>
        public TestCentricProject CreateNewProject(IList<string> filenames)
        {
            if (IsProjectLoaded)
                CloseProject();

            TestCentricProject = new TestCentricProject(this, filenames);

            _events.FireTestCentricProjectLoaded();

            TestCentricProject.LoadTests();

            return TestCentricProject;
        }

        public void OpenExistingProject(string projectPath)
        {
            if (IsProjectLoaded)
                CloseProject();

            TestCentricProject = new TestCentricProject(this);

            TestCentricProject.Load(projectPath);

            _events.FireTestCentricProjectLoaded();
        }

        public void OpenMostRecentFile()
        {
            // Find the most recent file loaded, which still exists
            foreach (string entry in RecentFiles.Entries)
            {
                if (entry != null && File.Exists(entry))
                {
                    OpenExistingFile(entry);
                    break;
                }
            }
        }

        public void OpenExistingFile(string filename)
        {
            if (TestCentricProject.IsProjectFile(filename))
                OpenExistingProject(filename);
            else
                CreateNewProject(new[] { filename });
        }

        public void SaveProject(string filename)
        {
            TestCentricProject.SaveAs(filename);
        }

        public void CloseProject()
        {
            if (HasTests)
                UnloadTests();

            TestCentricProject = null;

            _events.FireTestCentricProjectUnloaded();
        }

        public void LoadTests(IList<string> files)
        {
            log.Info($"Loading test files '{string.Join("', '", files.ToArray())}'");
            if (IsProjectLoaded && LoadedTests != null)
                UnloadTests();

            _events.FireTestsLoading(files);

            _lastTestRun = TestRunSpecification.Empty;
            _lastRunWasDebugRun = false;

            Runner = TestEngine.GetRunner(TestCentricProject);
            log.Debug($"Got {Runner.GetType().Name} for package");

            try
            {
                log.Debug("Loading tests");
                LoadedTests = new TestNode(Runner.Explore(Engine.TestFilter.Empty));
                log.Debug($"Loaded {LoadedTests.Xml.GetAttribute("TestCaseCount")} tests");
            }
            catch(Exception ex)
            {
                _events.FireTestLoadFailure(ex);
                log.Error("Failed to load tests", ex);
                return;
            }

            BuildTestIndex();
            MapTestsToPackages();
            AvailableCategories = GetAvailableCategories();
            TestCentricTestFilter.Init();

            ClearResults();

            _assemblyWatcher.Setup(1000, files as IList);
            _assemblyWatcher.AssemblyChanged += (path) => _events.FireTestChanged();
            _assemblyWatcher.Start();

            _events.FireTestLoaded(LoadedTests);

            if (TestCentricProject.ProjectPath == null)
                foreach (var subPackage in TestCentricProject.SubPackages)
                    RecentFiles.Latest = subPackage.FullName;
            else 
                RecentFiles.Latest = TestCentricProject.ProjectPath;
        }

        private Dictionary<string, TestNode> _testsById = new Dictionary<string, TestNode>();

        private void BuildTestIndex()
        {
            _testsById.Clear();
            BuildTestIndex(LoadedTests);
        }

        private void BuildTestIndex(TestNode node)
        {
            _testsById[node.Id] = node;

            foreach (TestNode child in node.Children)
                BuildTestIndex(child);
        }

        private Dictionary<string, TestPackage> _packageMap = new Dictionary<string, TestPackage>();

        private void MapTestsToPackages()
        {
            _packageMap.Clear();
            MapTestToPackage(LoadedTests, TestCentricProject);
        }

        private void MapTestToPackage(TestNode test, TestPackage package)
        {
            _packageMap[test.Id] = package;
            
            for (int index = 0; index < package.SubPackages.Count && index < test.Children.Count; index++)
                MapTestToPackage(test.Children[index], package.SubPackages[index]);
        }

        public IList<string> GetAgentsForPackage(TestPackage package = null)
        {
            if (package == null) package = TestCentricProject;

            return new List<string>(
                Services.TestAgentService.GetAgentsForPackage(package).Select(a => a.AgentName));
        }

        public void UnloadTests()
        {
            _events.FireTestsUnloading();

            UnloadTestsIgnoringErrors();
            Runner.Dispose();
            
            TestCentricTestFilter.ResetAll(true);
            LoadedTests = null;
            AvailableCategories = null;
            ClearResults();
            _assemblyWatcher.Stop();

            _events.FireTestUnloaded();
        }

        private void UnloadTestsIgnoringErrors()
        {
            try
            {
                Runner.Unload();
            }
            catch (EngineUnloadException)
            {

            }
        }

        public void ReloadTests()
        {
            _events.FireTestsReloading();

#if false
            Runner.Reload();
#else
            // NOTE: The `ITestRunner.Reload` method supported by the engine
            // has some problems, so we simulate Unload+Load. See issue #328.

                // Replace Runner in case settings changed
            UnloadTestsIgnoringErrors();
            Runner.Dispose();
            Runner = TestEngine.GetRunner(TestCentricProject);

            // Discover tests
            LoadedTests = new TestNode(Runner.Explore(Engine.TestFilter.Empty));
            AvailableCategories = GetAvailableCategories();
            TestCentricTestFilter.Init();

            ClearResults();
#endif

            _events.FireTestReloaded(LoadedTests);
        }

        public void ReloadPackage(TestPackage package, string config)
        {
            //var originalSubPackages = new List<TestPackage>(package.SubPackages);
            //package.SubPackages.Clear();
            package.Settings[EnginePackageSettings.ActiveConfig] = config;

            //foreach (var subPackage in package.SubPackages)
            //    foreach (var original in originalSubPackages)
            //        if (subPackage.Name == original.Name)
            //            subPackage.SetID(original.ID);

            ReloadTests();
        }

        public void RunTests(TestNode testNode)
        {
            if (testNode == null)
                throw new ArgumentNullException(nameof(testNode));

            log.Info($"Running test: {testNode.GetAttribute("name")}");
            RunTests(new TestRunSpecification(testNode, CategoryFilter, false));
        }

        public void RunTests(TestSelection tests)
        {
            if (tests == null)
                throw new ArgumentNullException(nameof(tests));

            log.Info($"Running test: {string.Join(", ", tests.Select(node => node.GetAttribute("name").ToArray()))}");
            RunTests(new TestRunSpecification(tests, CategoryFilter, false));
        }

        public void RepeatLastRun()
        {
            if (_lastTestRun == null)
                throw new InvalidOperationException("RepeatLastRun called before any tests were run");

            log.Info($"Running test: {string.Join(", ", _lastTestRun.SelectedTests.Select(node => node.GetAttribute("name").ToArray()))}");
            RunTests(_lastTestRun);
        }

        public void DebugTests(TestNode testNode)
        {
            if (testNode == null)
                throw new ArgumentNullException(nameof(testNode));

            log.Info($"Debugging test: {testNode.GetAttribute("name")}");
            RunTests(new TestRunSpecification(testNode, CategoryFilter, true));
        }

        public void StopTestRun(bool force)
        {
            Runner.StopRun(force);
        }

        public void SaveResults(string filePath, string format = "nunit3")
        {
            log.Debug($"Saving test results to {filePath} in {format} format");

            try
            {
                var resultWriter = Services.ResultService.GetResultWriter(format, new object[0]);
                var results = GetResultForTest(LoadedTests.Id);
                log.Debug(results.Xml.OuterXml);
                resultWriter.WriteResultFile(results.Xml, filePath);
            }
            catch(Exception ex)
            {
                log.Error("Failed to save results", ex);
            }
        }

        public TestNode GetTestById(string id)
        {
            return _testsById.TryGetValue(id, out var node) ? node : null;
        }

        public ResultNode GetResultForTest(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                ResultNode result;
                if (Results.TryGetValue(id, out result))
                    return result;
            }

            return null;
        }

        public IDictionary<string, object> GetPackageSettingsForTest(string id)
        {
            return GetPackageForTest(id)?.Settings;
        }
        public TestPackage GetPackageForTest(string id)
        {
            return _packageMap.ContainsKey(id) 
                ? _packageMap[id] 
                : null;
        }

        public void ClearResults()
        {
            Results.Clear();
            ResultSummary = null;
        }

        public void SelectCategories(IList<string> categories, bool exclude)
        {
            SelectedCategories = new List<string>(categories);
            ExcludeSelectedCategories = exclude;

            UpdateCategoryFilter();

            _events.FireCategorySelectionChanged();
        }

        private void UpdateCategoryFilter()
        {
            var catFilter = TestFilter.Empty;

            if (SelectedCategories != null && SelectedCategories.Count > 0)
            {
                catFilter = TestFilter.MakeCategoryFilter(SelectedCategories);

                if (ExcludeSelectedCategories)
                    catFilter = TestFilter.MakeNotFilter(catFilter);
            }

            CategoryFilter = catFilter;
        }

#endregion

#endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            try
            {
                if (IsProjectLoaded)
                    UnloadTests();

                if (Runner != null)
                    Runner.Dispose();

                if (TestEngine != null)
                    TestEngine.Dispose();

                if (_assemblyWatcher != null)
                    _assemblyWatcher.Dispose();

                if (_settingsService != null)
                    _settingsService.SaveSettings();
            }
            catch (EngineUnloadException)
            {
                // TODO: Figure out what to do about this
            }
        }

        #endregion

        #region Private and Internal Properties

        private ITestEngine TestEngine { get; }

        private ITestRunner Runner { get; set; }

        #endregion

        #region Helper Methods

        // The engine returns more values than we really want.
        // For example, we don't currently distinguish between
        // client and full profiles when executing tests. We
        // drop unwanted entries here. Even if some of these items
        // are removed in a later version of the engine, we may
        // have to retain this code to work with older engines.
        private List<IRuntimeFramework> GetAvailableRuntimes()
        {
            var runtimes = new List<IRuntimeFramework>();

            foreach (var runtime in Services.GetService<IAvailableRuntimes>().AvailableRuntimes)
            {
                // We don't support anything below .NET Framework 2.0
                if (runtime.Id.StartsWith("net-") && runtime.FrameworkVersion.Major < 2)
                    continue;

                runtimes.Add(runtime);
            }

            runtimes.Sort((x, y) => x.DisplayName.CompareTo(y.DisplayName));

            // Now eliminate client entries where full entry follows
            for (int i = runtimes.Count - 2; i >= 0; i--)
            {
                var rt1 = runtimes[i];
                var rt2 = runtimes[i + 1];

                if (rt1.Id != rt2.Id)
                    continue;

                if (rt1.Profile == "Client" && rt2.Profile == "Full")
                    runtimes.RemoveAt(i);
            }

            return runtimes;
        }

        // All Test running eventually comes down to this method
        private void RunTests(TestRunSpecification runSpec)
        {
            if (runSpec == null)
                throw new ArgumentNullException(nameof(runSpec));
            if (_lastTestRun == null)
                throw new InvalidOperationException("Field '_lastTestRun' is null");

            // Create a test filter incorporating both the
            // selected tests and the category filter.
            var filter = runSpec.SelectedTests.GetTestFilter();
            if (!runSpec.CategoryFilter.IsEmpty)
                filter = TestFilter.MakeAndFilter(filter, runSpec.CategoryFilter);

            // If a filter is active in the UI, a TestFilter must be created accordingly that contains the ID all visible children of the selected nodes.
            if (Settings.Gui.TestTree.DisplayFormat == "NUNIT_TREE" && TestCentricTestFilter.IsActive)
                filter = TestFilter.MakeVisibleIdFilter(runSpec.SelectedTests);

            // We need to re-create the test runner because settings such
            // as debugging have already been passed to the test runner.
            // For performance reasons, we only do this if we did run
            // in a different mode than last time.
            if (_lastRunWasDebugRun != runSpec.DebuggingRequested)
            {
                foreach (var subPackage in TestCentricProject.SubPackages)
                {
                    subPackage.Settings["DebugTests"] = runSpec.DebuggingRequested;
                }

                Runner = TestEngine.GetRunner(TestCentricProject);

                // It is not strictly necessary to load the tests
                // because the runner will do that automatically, however,
                // the initial test count will be incorrect causing UI crashes.
                Runner.Load();

                _lastRunWasDebugRun = runSpec.DebuggingRequested;
            }

            _lastTestRun = runSpec;

            // TODO: Does this belong here? Maybe need to do before creating the run specification.
            if (Settings.Engine.ReloadOnRun)
            {
                // TODO: reinstate when engine Reload works. Currently
                // we simulate it with Unload + Load, so the results
                // are always cleared.
                //if (Settings.Gui.ClearResultsOnReload)
                //    ClearResults();
                ReloadTests();
            }

            Runner.RunAsync(_events, filter.AsNUnitFilter());
        }

        public IList<string> GetAvailableCategories()
        {
            var categories = new Dictionary<string, string>();
            CollectCategories(LoadedTests, categories);

            var list = new List<string>(categories.Values);
            list.Sort();
            return list;
        }

        private void CollectCategories(TestNode test, Dictionary<string, string> categories)

        {
            foreach (string name in test.GetPropertyList("Category").Split(new[] { ',' }))
                if (IsValidCategoryName(name))
                    categories[name] = name;

            if (test.IsSuite)
                foreach (TestNode child in test.Children)
                    CollectCategories(child, categories);
        }

        public static bool IsValidCategoryName(string name)
        {
            return name.Length > 0 && name.IndexOfAny(new char[] { ',', '!', '+', '-' }) < 0;
        }

        #endregion
    }
}
