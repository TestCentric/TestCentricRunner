// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model
{
    using System;
    using System.Threading.Tasks;
    using NUnit;
    using NUnit.Engine;

    /// <summary>
    /// The TestCentricRunner class wraps the NUnit Engine's ITestRunner interface to explore and run tests.
    /// While an assembly is loaded by the NUnit ITestRunner, it's blocked by the file system and the assembly cannot be modified by the user.
    /// However, the user should be able to continue developing their tests while the tests are already shown in the GUI runner.
    /// Therefore, the TestCentricRunner minimizes the time that the ITestRunner is active by creating and disposing it as needed for each test run.
    /// </summary>
    internal class TestCentricRunner : IDisposable
    {
        static Logger log = InternalTrace.GetLogger(typeof(TestModel));

        internal TestCentricRunner(ITestEngine testEngine, TestEventDispatcher testEvents)
        {
            Guard.ArgumentNotNull(testEngine, nameof(testEngine));
            Guard.ArgumentNotNull(testEvents, nameof(testEvents));

            TestEngine = testEngine;
            TestEvents = testEvents;
            testEvents.RunFinished += OnTestRunFinished;
        }

        private ITestEngine TestEngine { get; }

        private TestEventDispatcher TestEvents { get; }

        private ITestRunner ActiveTestRun { get; set; }

        public bool IsTestRunning => ActiveTestRun != null && ActiveTestRun.IsTestRunning;

        public void Dispose()
        {
            ResetActiveTestRun();
        }

        /// <summary>
        /// Explore tests from a package
        /// </summary>
        public TestNode Explore(TestPackage package)
        {
            Guard.ArgumentNotNull(package, nameof(package));

            // Create a runner to explore the tests, but dispose it immediately after. 
            var runner = TestEngine.GetRunner(package);
            log.Debug($"Got {runner.GetType().Name} for package");

            TestNode loadedTests = null;
            try
            {
                log.Debug("Loading tests");
                loadedTests = new TestNode(runner.Explore(NUnit.Engine.TestFilter.Empty));
                log.Debug($"Loaded {loadedTests.Xml.GetAttribute("TestCaseCount")} tests");
            }
            catch (Exception ex)
            {
                log.Error("Failed to load tests", ex);
                TestEvents.FireTestLoadFailure(ex);
                return null;
            }
            finally
            {
                runner.Dispose();
            }

            return loadedTests;
        }

        /// <summary>
        /// Runs the tests in the specified package.
        /// </summary>
        public void RunAsync(TestPackage package, NUnit.Engine.TestFilter filter)
        {
            Guard.ArgumentNotNull(package, nameof(package));
            Guard.ArgumentNotNull(filter, nameof(filter));

            log.Debug("Executing RunAsync");
            ActiveTestRun = TestEngine.GetRunner(package);
            ActiveTestRun.Load();
            ActiveTestRun.RunAsync(TestEvents, filter);
        }

        private void OnTestRunFinished(TestResultEventArgs args)
        {
            // Dispose runner object after the run is finished
            ResetActiveTestRun();
        }

        public void StopRun(bool force)
        {
            // Async to avoid blocking the main thread for incoming test events in between
            Task.Run(() =>
            {
                ActiveTestRun.StopRun(force);
                ResetActiveTestRun();
            });
        }

        private void ResetActiveTestRun()
        {
            ActiveTestRun?.Dispose();
            ActiveTestRun = null;
        }
    }
}
