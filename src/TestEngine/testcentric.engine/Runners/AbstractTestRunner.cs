// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.ComponentModel;

namespace TestCentric.Engine.Runners
{
    /// <summary>
    /// AbstractTestRunner is the base class for all internal runners
    /// within the NUnit Engine. It implements the ITestEngineRunner
    /// interface, which uses TestEngineResults to communicate back
    /// to higher level runners.
    /// </summary>
    public abstract class AbstractTestRunner : NUnit.Engine.ITestEngineRunner
    {
        public AbstractTestRunner(NUnit.Engine.TestPackage package)
        {
            Guard.ArgumentNotNull(package, nameof(package));
            TestPackage = package;
        }

        /// <summary>
        /// The TestPackage, which this runner is handling
        /// </summary>
        protected NUnit.Engine.TestPackage TestPackage { get; set; }

        /// <summary>
        /// The result of the last call to LoadPackage
        /// for this runner's TestPackage.
        /// </summary>
        protected NUnit.Engine.TestEngineResult LoadResult { get; set; }

        /// <summary>
        /// Indicates whether the runner's TestPackage is loaded.
        /// </summary>
        public bool IsPackageLoaded => LoadResult != null;

        /// <summary>
        /// Loads the TestPackage for exploration or execution.
        /// </summary>
        /// <returns>A TestEngineResult.</returns>
        protected abstract NUnit.Engine.TestEngineResult LoadPackage();

        /// <summary>
        /// Reload the currently loaded test package. Overridden
        /// in derived classes to take any additional action.
        /// </summary>
        /// <returns>A TestEngineResult.</returns>
        protected virtual NUnit.Engine.TestEngineResult ReloadPackage()
        {
            return LoadPackage();
        }

        /// <summary>
        /// Unload any loaded TestPackage. Overridden in
        /// derived classes to take any necessary action.
        /// </summary>
        public virtual void UnloadPackage()
        {
        }

        /// <summary>
        /// Run the tests in the loaded TestPackage.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>A TestEngineResult giving the result of the test execution</returns>
        protected abstract NUnit.Engine.TestEngineResult RunTests(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter);

        /// <summary>
        /// Start a run of the tests in the loaded TestPackage, returning immediately.
        /// The tests are run asynchronously and the listener interface is notified
        /// as it progresses.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>An <see cref="AsyncTestEngineResult"/> that will provide the result of the test execution</returns>
        protected virtual NUnit.Engine.AsyncTestEngineResult RunTestsAsync(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
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

        /// <summary>
        /// Request the ongoing test run to stop. If no  test is running, the call is ignored.
        /// </summary>
        public abstract void RequestStop();

        /// <summary>
        /// Force the ongoing test run to stop. If no  test is running, the call is ignored.
        /// </summary>
        public abstract void ForcedStop();

        /// <summary>
        /// Explores the TestPackage and returns information about
        /// the tests found. Loads the package if not done previously.
        /// </summary>
        /// <param name="filter">The TestFilter to be used to select tests</param>
        /// <returns>A TestEngineResult.</returns>
        public abstract NUnit.Engine.TestEngineResult Explore(NUnit.Engine.TestFilter filter);

        /// <summary>
        /// Loads the TestPackage for exploration or execution, saving the result.
        /// </summary>
        /// <returns>A TestEngineResult.</returns>
        public NUnit.Engine.TestEngineResult Load()
        {
            return LoadResult = LoadPackage();
        }

        /// <summary>
        /// Reload the currently loaded test package, saving the result.
        /// </summary>
        /// <returns>A TestEngineResult.</returns>
        /// <exception cref="InvalidOperationException">If no package has been loaded</exception>
        public NUnit.Engine.TestEngineResult Reload()
        {
            if (this.TestPackage == null)
                throw new InvalidOperationException("MasterTestRunner: Reload called before Load");

            return LoadResult = ReloadPackage();
        }

        /// <summary>
        /// Unload any loaded TestPackage.
        /// </summary>
        public void Unload()
        {
            UnloadPackage();
            LoadResult = null;
        }

        /// <summary>
        /// Count the test cases that would be run under the specified
        /// filter, loading the TestPackage if it is not already loaded.
        /// </summary>
        /// <param name="filter">A TestFilter</param>
        /// <returns>The count of test cases.</returns>
        public abstract int CountTestCases(NUnit.Engine.TestFilter filter);

        /// <summary>
        /// Run the tests in the TestPackage, loading the package
        /// if this has not already been done.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>A TestEngineResult giving the result of the test execution</returns>
        public NUnit.Engine.TestEngineResult Run(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            return RunTests(listener, filter);
        }

        /// <summary>
        /// Start a run of the tests in the loaded TestPackage. The tests are run
        /// asynchronously and the listener interface is notified as it progresses.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>An <see cref="AsyncTestEngineResult"/> that will provide the result of the test execution</returns>
        public NUnit.Engine.AsyncTestEngineResult RunAsync(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            return RunTestsAsync(listener, filter);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    Unload();

                _disposed = true;
            }
        }
    }
}
