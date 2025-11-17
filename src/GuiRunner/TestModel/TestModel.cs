// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Common;

namespace TestCentric.Gui.Model
{
    using System.Threading.Tasks;
    using Services;
    using Settings;
    using TestCentric.Engine;
    using TestCentric.Engine.Internal;
    using TestCentric.Engine.Services;
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

        private TestRunSpecification _lastTestRun = TestRunSpecification.Empty;

        private bool _lastRunWasDebugRun;

        #region Constructor and Creation

        public TestModel(ITestEngine testEngine, GuiOptions options = null)
        {
            TestEngine = testEngine;
            Options = options ?? new GuiOptions();

            _events = new TestEventDispatcher(this);
            _assemblyWatcher = new AssemblyWatcher();

            Settings = new UserSettings();

            //Services = new TestServices(testEngine);
            TestCentricTestFilter = new TestCentricTestFilter(this, () => _events.FireTestFilterChanged());
            TestResultManager = new TestResultManager(this);

            Services.GetService<IExtensionService>().InstallExtensions();

            //foreach (var node in Services.ExtensionService.GetExtensionNodes(PROJECT_LOADER_EXTENSION_PATH))
            //{
            //    if (node.TypeName == NUNIT_PROJECT_LOADER)
            //        NUnitProjectSupport = true;
            //    else if (node.TypeName == VISUAL_STUDIO_PROJECT_LOADER)
            //        VisualStudioSupport = true;
            //}
        }

        public static ITestModel CreateTestModel(GuiOptions options)
        {
            return CreateTestModel(new TestEngine(), options);
        }

        // Public for testing
        public static ITestModel CreateTestModel(ITestEngine testEngine, GuiOptions options)
        {
            // Currently the InternalTraceLevel can only be set from the command-line.
            // We can't use user settings to provide a default because the settings
            // are an engine service and the engine have the internal trace level
            // set as part of its initialization.
            var traceLevel = options.InternalTraceLevel != null
                ? (InternalTraceLevel)Enum.Parse(typeof(InternalTraceLevel), options.InternalTraceLevel)
                : InternalTraceLevel.Off;

            var logFile = $"InternalTrace.{Process.GetCurrentProcess().Id}.gui.log";
            if (options.WorkDirectory != null)
                logFile = Path.Combine(options.WorkDirectory, logFile);

            InternalTrace.Initialize(logFile, traceLevel);

            testEngine.InternalTraceLevel = traceLevel;
            if (options.WorkDirectory != null)
                testEngine.WorkDirectory = options.WorkDirectory;

            return new TestModel(testEngine, options);
        }

        #endregion

        #region ITestModel Implementation

        #region General Properties

        // Command-line options
        public GuiOptions Options { get; }

        // Work Directory
        public string WorkDirectory { get { return TestEngine.WorkDirectory; } }

        // Event Dispatcher
        public ITestEvents Events { get { return _events; } }

        // Services provided either by the model itself or by the engine
        public IServiceLocator Services => TestEngine.Services;

        public IUserSettings Settings { get; }

        // Since we generate a new name when agents are wrapped,the available agent list must created dynamically.
        public IList<string> AvailableAgents =>
            [.. Services.GetService<ITestAgentProvider>().GetAvailableAgents().Select((a) => a.AgentName)];

        public IRecentFiles RecentFiles => Settings.Gui.RecentFiles;

        // Project Support
        public bool NUnitProjectSupport { get; }
        public bool VisualStudioSupport { get; }

        // Runtime Support
        private List<NUnit.Engine.IRuntimeFramework> _runtimes;
        public IList<NUnit.Engine.IRuntimeFramework> AvailableRuntimes
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
                    _resultFormats = [.. Services.GetService<IResultService>().Formats];

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

        public ITestResultManager TestResultManager { get; }

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
            HashSet<TestNode> testNodes = new HashSet<TestNode>();


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

            public bool ContainTest(TestNode testNode)
            {
                // Get list of testNodes only once
                if (testNodes.Count == 0)
                {
                    GetTestNodes(SelectedTests, SelectedTests.GetExplicitChildNodes());
                }

                return testNodes.Contains(testNode);
            }

            private void GetTestNodes(IEnumerable<TestNode> selectedTests, IList<TestNode> explicitTests)
            {
                foreach (TestNode testNode in selectedTests)
                {
                    if (explicitTests.Contains(testNode))
                        continue;

                    testNodes.Add(testNode);
                    GetTestNodes(testNode.Children, explicitTests);
                }
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

        public TestCentricProject CreateNewProject()
        {
            if (IsProjectLoaded)
                CloseProject();

            TestCentricProject = new TestCentricProject(this);
            return TestCentricProject;
        }

        public void AddTests(IEnumerable<string> fileNames)
        {
            if (!IsProjectLoaded)
                return;

            foreach( string fileName in fileNames)
                TestCentricProject.AddSubPackage(fileName);

            TestCentricProject.LoadTests();
            _events.FireTestCentricProjectLoaded();
        }

        public void RemoveTestPackage(TestPackage subPackage)
        {
            if (!IsProjectLoaded || IsTestRunning || subPackage == null || TestCentricProject.SubPackages.Count <= 1)
                return;

            TestCentricProject.RemoveSubPackage(subPackage);

            TestCentricProject.LoadTests();
            _events.FireTestCentricProjectLoaded();
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
            RecentFiles.Latest = TestCentricProject.ProjectPath;
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
                LoadedTests = new TestNode(Runner.Explore(NUnit.Engine.TestFilter.Empty));
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

            _assemblyWatcher.Setup(1000, files);
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
            if (package == null)
                package = TestCentricProject;

            if (package == null)                // no project is loaded
                return new List<string>();

            return new List<string>(
                Services.GetService<TestAgentService>().GetAgentsForPackage(package).Select(a => a.AgentName));
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
            LoadedTests = new TestNode(Runner.Explore(NUnit.Engine.TestFilter.Empty));
            AvailableCategories = GetAvailableCategories();
            BuildTestIndex();
            TestCentricTestFilter.Init();

            TestResultManager.ReloadTestResults();
#endif

            _events.FireTestReloaded(LoadedTests);
        }

        public void ReloadPackage(TestPackage package, string config)
        {
            //var originalSubPackages = new List<TestPackage>(package.SubPackages);
            //package.SubPackages.Clear();
            package.AddSetting(SettingDefinitions.DebugTests.WithValue(config));

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
            // Async to avoid blocking the main thread for incoming test events in between
            Task.Run(() => Runner.StopRun(force));
        }

        public void SaveResults(string filePath, string format = "nunit3")
        {
            log.Debug($"Saving test results to {filePath} in {format} format");

            try
            {
                var resultWriter = Services.GetService<IResultService>().GetResultWriter(format, []);
                var results = TestResultManager.GetResultForTest(LoadedTests.Id);
                log.Debug(results.Xml.OuterXml);
                resultWriter.WriteResultFile(results.Xml, filePath);
            }
            catch(Exception ex)
            {
                log.Error("Failed to save results", ex);
            }
        }

        public void TransformResults(string targetFile, string xsltFile)
        {
            var resultWriter = Services.GetService<IResultService>().GetResultWriter("user", [xsltFile]);
            var results = TestResultManager.GetResultForTest(LoadedTests.Id);
            resultWriter?.WriteResultFile(results.Xml, targetFile);
        }

        public TestNode GetTestById(string id)
        {
            return _testsById.TryGetValue(id, out var node) ? node : null;
        }

        public bool IsInTestRun(TestNode testNode)
        {
            if (_lastTestRun == null)
            {
                return false;
            }

            return _lastTestRun.ContainTest(testNode);
        }

        public NUnit.Engine.PackageSettings GetPackageSettingsForTest(string id)
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
            TestResultManager.ClearResults();
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

                Settings?.SaveSettings();
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
        private List<NUnit.Engine.IRuntimeFramework> GetAvailableRuntimes()
        {
            var runtimes = new List<NUnit.Engine.IRuntimeFramework>();

            foreach (var runtime in Services.GetService<NUnit.Engine.IAvailableRuntimes>().AvailableRuntimes)
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
            log.Debug("RunningTests");
            if (runSpec == null)
                throw new ArgumentNullException(nameof(runSpec));
            if (_lastTestRun == null)
                throw new InvalidOperationException("Field '_lastTestRun' is null");

            // Create a test filter incorporating the selected tests, the tree grouping
            // and the UI tree filter (category, outcome or duration)
            TestFilter filter = runSpec.SelectedTests.GetTestFilter(TestCentricTestFilter);

            // We need to re-create the test runner because settings such
            // as debugging have already been passed to the test runner.
            // For performance reasons, we only do this if we did run
            // in a different mode than last time.
            if (_lastRunWasDebugRun != runSpec.DebuggingRequested)
            {
                foreach (var subPackage in TestCentricProject.SubPackages)
                {
                    subPackage.AddSetting(SettingDefinitions.DebugTests.WithValue(runSpec.DebuggingRequested));
                }

                Runner = TestEngine.GetRunner(TestCentricProject);

                // It is not strictly necessary to load the tests
                // because the runner will do that automatically, however,
                // the initial test count will be incorrect causing UI crashes.
                Runner.Load();

                _lastRunWasDebugRun = runSpec.DebuggingRequested;
            }

            _lastTestRun = runSpec;
            TestResultManager.TestRunStarting();

            log.Debug("Executing RunAsync");
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
