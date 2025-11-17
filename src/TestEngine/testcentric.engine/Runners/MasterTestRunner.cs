// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
using TestCentric.Engine.Internal;
using TestCentric.Engine.Services;

using TestPackage = NUnit.Engine.TestPackage;

namespace TestCentric.Engine.Runners
{
    /// <summary>
    /// MasterTestRunner implements the ITestRunner interface, which
    /// is the user-facing representation of a test runner. It uses
    /// various internal runners to load and run tests for the user.
    /// </summary>
    public class MasterTestRunner : ITestRunner
    {
        // MasterTestRunner is the only runner that is passed back
        // to users asking for an ITestRunner. The actual details of
        // execution are handled by various internal runners, which
        // impement ITestEngineRunner.
        //
        // Explore and execution results from MasterTestRunner are
        // returned as XmlNodes, created from the internal 
        // TestEngineResult representation.
        // 
        // MasterTestRunner is responsible for creating the test-run
        // element, which wraps all the individual assembly and project
        // results.

        private NUnit.Engine.ITestEngineRunner _engineRunner;
        private readonly IServiceLocator _services;
        private readonly TestPackageAnalyzer _packageAnalyzer;
        private readonly IRuntimeFrameworkService _runtimeService;
        private readonly IProjectService _projectService;
        private ITestRunnerFactory _testRunnerFactory;
        private bool _disposed;

        private TestEventDispatcher _eventDispatcher;

        private const int WAIT_FOR_CANCEL_TO_COMPLETE = 5000;

        private static readonly Logger log = InternalTrace.GetLogger(typeof(MasterTestRunner));

        public MasterTestRunner(IServiceLocator services, TestPackage package)
        {
            if (services == null) throw new ArgumentNullException("services");
            if (package == null) throw new ArgumentNullException("package");

            _services = services;
            TestPackage = package;

            // Get references to the services we use
            _projectService = _services.GetService<IProjectService>();
            _testRunnerFactory = _services.GetService<ITestRunnerFactory>();

            _packageAnalyzer = _services.GetService<TestPackageAnalyzer>();
            _runtimeService = _services.GetService<IRuntimeFrameworkService>();

            _eventDispatcher = _services.GetService<TestEventDispatcher>();

            // Last chance to catch invalid settings in package,
            // in case the client runner missed them.
            _packageAnalyzer.ValidatePackageSettings(package);
        }

        /// <summary>
        /// The TestPackage for which this is the runner
        /// </summary>
        protected TestPackage TestPackage { get; set; }

        /// <summary>
        /// The result of the last call to LoadPackage
        /// </summary>
        protected NUnit.Engine.TestEngineResult LoadResult { get; set; }

        /// <summary>
        /// Gets an indicator of whether the package has been loaded.
        /// </summary>
        protected bool IsPackageLoaded
        {
            get { return LoadResult != null; }
        }

        /// <summary>
        /// Get a flag indicating whether a test is running
        /// </summary>
        public bool IsTestRunning { get; private set; }

        /// <summary>
        /// Load a TestPackage for possible execution. The
        /// explicit implementation returns an ITestEngineResult
        /// for consumption by clients.
        /// </summary>
        /// <returns>An XmlNode representing the loaded assembly.</returns>
        public XmlNode Load()
        {
            LoadResult = GetEngineRunner().Load()
                .MakeTestRunResult(TestPackage);

            return LoadResult.Xml;
        }

        /// <summary>
        /// Unload any loaded TestPackage. If none is loaded,
        /// the call is ignored.
        /// </summary>
        public void Unload()
        {
            UnloadPackage();
        }

        /// <summary>
        /// Reload the currently loaded test package.
        /// </summary>
        /// <returns>An XmlNode representing the loaded package</returns>
        /// <exception cref="InvalidOperationException">If no package has been loaded</exception>
        public XmlNode Reload()
        {
            LoadResult = GetEngineRunner().Reload()
                .MakeTestRunResult(TestPackage);

            return LoadResult.Xml;
        }

        /// <summary>
        /// Count the test cases that would be run under the specified
        /// filter, loading the TestPackage if it is not already loaded.
        /// </summary>
        /// <param name="filter">A TestFilter</param>
        /// <returns>The count of test cases.</returns>
        public int CountTestCases(NUnit.Engine.TestFilter filter)
        {
            return GetEngineRunner().CountTestCases(filter);
        }

        /// <summary>
        /// Run the tests in a loaded TestPackage. The explicit
        /// implementation returns an ITestEngineResult for use
        /// by external clients.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>An XmlNode giving the result of the test execution</returns>
        public XmlNode Run(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            return RunTests(listener, filter).Xml;
        }

        /// <summary>
        /// Start a run of the tests in the loaded TestPackage. The tests are run
        /// asynchronously and the listener interface is notified as it progresses.
        /// </summary>
        /// <param name="listener">The listener that is notified as the run progresses</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns></returns>
        public NUnit.Engine.ITestRun RunAsync(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            return RunTestsAsync(listener, filter);
        }

        /// <summary>
        /// Cancel the ongoing test run. If no  test is running, the call is ignored.
        /// </summary>
        /// <param name="force">If true, cancel any ongoing test threads, otherwise wait for them to complete.</param>
        public void StopRun(bool force)
        {
            if (force)
            {
                log.Info("Cancelling test run");

                // When running under .NET Core, the test framework will not be able to kill the
                // threads currently running tests. We handle cleanup here in case that happens.
                _engineRunner.ForcedStop();

                // Send completion events for any tests, which were still running
                _eventDispatcher.IssuePendingNotifications();

                IsTestRunning = false;

                // Signal completion of the run
                _eventDispatcher.DispatchEvent($"<test-run id='{TestPackage.ID}' result='Failed' label='Cancelled' />");
            }
            else
            {
                log.Info("Requesting stop");

                _engineRunner.RequestStop();
            }
        }

        /// <summary>
        /// Explore a loaded TestPackage and return information about
        /// the tests found.
        /// </summary>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>An XmlNode representing the tests found.</returns>
        public XmlNode Explore(NUnit.Engine.TestFilter filter)
        {
            LoadResult = GetEngineRunner().Explore(filter)
                .MakeTestRunResult(TestPackage);

            return LoadResult.Xml;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of this object.
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing && _engineRunner != null)
                    _engineRunner.Dispose();

                _disposed = true;
            }
        }

        //Exposed for testing
        internal NUnit.Engine.ITestEngineRunner GetEngineRunner()
        {
            if (_engineRunner == null)
            {
                // Expand any project subpackages
                _packageAnalyzer.ExpandProjectPackages(TestPackage);

                // Add package settings to reflect the target runtime
                // and test framework usage of each assembly.
                foreach (var package in TestPackage.Select(p => p.IsAssemblyPackage))
                    if (File.Exists(package.FullName))
                        _packageAnalyzer.ApplyImageSettings(package);

                // Use SelectRuntimeFramework for its side effects.
                // Info will be left behind in the package about
                // each contained assembly, which will subsequently
                // be used to determine how to run the assembly.
                _runtimeService.SelectRuntimeFramework(TestPackage);

                _engineRunner = _testRunnerFactory.MakeTestRunner(TestPackage);
            }

            return _engineRunner;
        }

        /// <summary>
        /// Unload any loaded TestPackage.
        /// </summary>
        private void UnloadPackage()
        {
            if (_engineRunner != null)
                _engineRunner.Unload();
            LoadResult = null;
        }

        /// <summary>
        /// Count the test cases that would be run under
        /// the specified filter. Returns zero if the
        /// package has not yet been loaded.
        /// </summary>
        /// <param name="filter">A TestFilter</param>
        /// <returns>The count of test cases</returns>
        private int CountTests(NUnit.Engine.TestFilter filter)
        {
            if (!IsPackageLoaded) return 0;

            return GetEngineRunner().CountTestCases(filter);
        }

        /// <summary>
        /// Run the tests in the loaded TestPackage and return a test result. The tests
        /// are run synchronously and the listener interface is notified as it progresses.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>A TestEngineResult giving the result of the test execution</returns>
        private NUnit.Engine.TestEngineResult RunTests(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            _eventDispatcher.InitializeForRun();

            if (listener != null)
                _eventDispatcher.Listeners.Add(listener);

            IsTestRunning = true;
            
            string clrVersion;
            string engineVersion;

            clrVersion = Environment.Version.ToString();
            engineVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var startTime = DateTime.UtcNow;
            long startTicks = Stopwatch.GetTimestamp();

            try
            {
                var startRunNode = XmlHelper.CreateTopLevelElement("start-run")
                    .AddAttribute("count", CountTests(filter).ToString())
                    .AddAttribute("start-time", XmlConvert.ToString(startTime, "u"))
                    .AddAttribute("engine-version", engineVersion)
                    .AddAttribute("clr-version", clrVersion);

                startRunNode.AddElementWithCDataSection("command-line", Environment.CommandLine);

                _eventDispatcher.OnTestEvent(startRunNode.OuterXml);

                // Insertions are done in reverse order, since each is added as the first child.
                NUnit.Engine.TestEngineResult result = GetEngineRunner().Run(_eventDispatcher, filter)
                    .MakeTestRunResult(TestPackage)
                    .InsertFilterElement(filter)
                    .InsertCommandLineElement(Environment.CommandLine);

                result.Xml.AddAttribute("engine-version", engineVersion);
                result.Xml.AddAttribute("clr-version", clrVersion);
                double duration = (double)(Stopwatch.GetTimestamp() - startTicks) / Stopwatch.Frequency;
                result.Xml.AddAttribute("start-time", XmlConvert.ToString(startTime, "u"));
                result.Xml.AddAttribute("end-time", XmlConvert.ToString(DateTime.UtcNow, "u"));
                result.Xml.AddAttribute("duration", duration.ToString("0.000000", NumberFormatInfo.InvariantInfo));

                IsTestRunning = false;

                _eventDispatcher.OnTestEvent(result.Xml.OuterXml);

                return result;
            }
            catch(Exception ex)
            {
                IsTestRunning = false;

                var result = CreateErrorResult(TestPackage);
                result.Xml.AddAttribute("engine-version", engineVersion);
                result.Xml.AddAttribute("clr-version", clrVersion);
                double duration = (double)(Stopwatch.GetTimestamp() - startTicks) / Stopwatch.Frequency;
                result.Xml.AddAttribute("start-time", XmlConvert.ToString(startTime, "u"));
                result.Xml.AddAttribute("end-time", XmlConvert.ToString(DateTime.UtcNow, "u"));
                result.Xml.AddAttribute("duration", duration.ToString("0.000000", NumberFormatInfo.InvariantInfo));

                _eventDispatcher.OnTestEvent(result.Xml.OuterXml);

                _eventDispatcher.OnTestEvent($"<unhandled-exception message=\"{ex.Message}\" />");

                return result;
            }
        }

        private NUnit.Engine.TestEngineResult CreateErrorResult(TestPackage package)
        {
            return new NUnit.Engine.TestEngineResult($"<test-run id='{package.ID}' result='Failed' label = 'Error' />");
        }

        private NUnit.Engine.AsyncTestEngineResult RunTestsAsync(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            var testRun = new NUnit.Engine.AsyncTestEngineResult();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += (s, ea) =>
                {
                    var result = RunTests(listener, filter);
                    testRun.SetResult(result);
                };

                worker.RunWorkerAsync();
            }

            return testRun;
        }
    }
}
