// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Common;
using TestCentric.Engine.Internal;
using TestCentric.Engine.Services;

using SettingDefinitions = NUnit.Common.SettingDefinitions;

namespace TestCentric.Engine.Runners
{
    /// <summary>
    /// AggregatingTestRunner runs tests using multiple
    /// subordinate runners and combines the results.
    /// </summary>
    public class AggregatingTestRunner : AbstractTestRunner
    {
        // AggregatingTestRunner combines the results from tests run by different
        // runners. Each file passed to it is handled by a single runner.
        private List<NUnit.Engine.TestPackage> _packageList;

        // Exceptions from unloading individual runners are caught and rethrown
        // on AggregatingTestRunner disposal, to allow TestResults to be
        // written and execution of other runners to continue.
        private readonly List<Exception> _unloadExceptions = new List<Exception>();

        private readonly ITestRunnerFactory _testRunnerFactory;

        public IList<NUnit.Engine.ITestEngineRunner> Runners { get; }

        public int LevelOfParallelism { get; }

        public AggregatingTestRunner(IServiceLocator services, NUnit.Engine.TestPackage package) : base(package)
        {
            _testRunnerFactory = services.GetService<ITestRunnerFactory>();

            _packageList = new List<NUnit.Engine.TestPackage>(package.Select(p => !p.HasSubPackages));
            Runners = new List<NUnit.Engine.ITestEngineRunner>();
            foreach (var subPackage in _packageList)
                Runners.Add(_testRunnerFactory.MakeTestRunner(subPackage));

            var maxAgentSetting = TestPackage.Settings.GetValueOrDefault(SettingDefinitions.MaxAgents);
            LevelOfParallelism = maxAgentSetting > 0
                ? Math.Min(maxAgentSetting, _packageList.Count)
                : _packageList.Count;
        }

        /// <summary>
        /// Explore a TestPackage and return information about
        /// the tests found.
        /// </summary>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>A TestEngineResult.</returns>
        public override NUnit.Engine.TestEngineResult Explore(NUnit.Engine.TestFilter filter)
        {
            var results = new List<NUnit.Engine.TestEngineResult>();

            foreach (var runner in Runners)
                results.Add(runner.Explore(filter));

            return ResultHelper.Merge(results);
        }

        /// <summary>
        /// Load a TestPackage for possible execution
        /// </summary>
        /// <returns>A TestEngineResult.</returns>
        protected override NUnit.Engine.TestEngineResult LoadPackage()
        {
            var results = new List<NUnit.Engine.TestEngineResult>();

            foreach (var runner in Runners)
                results.Add(runner.Load());

            return ResultHelper.Merge(results);
        }

        /// <summary>
        /// Unload any loaded TestPackages.
        /// </summary>
        public override void UnloadPackage()
        {
            foreach (var runner in Runners)
            {
                runner.Unload();
            }
        }

        /// <summary>
        /// Count the test cases that would be run under
        /// the specified filter.
        /// </summary>
        /// <param name="filter">A TestFilter</param>
        /// <returns>The count of test cases</returns>
        public override int CountTestCases(NUnit.Engine.TestFilter filter)
        {
            int count = 0;

            foreach (var runner in Runners)
                count += runner.CountTestCases(filter);

            return count;
        }

        /// <summary>
        /// Run the tests in a loaded TestPackage
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>
        /// A TestEngineResult giving the result of the test execution.
        /// </returns>
        protected override NUnit.Engine.TestEngineResult RunTests(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            var results = new List<NUnit.Engine.TestEngineResult>();

            bool disposeRunners = TestPackage.Settings.GetValueOrDefault(SettingDefinitions.DisposeRunners);

            if (LevelOfParallelism <= 1)
            {
                RunTestsSequentially(listener, filter, results, disposeRunners);
            }
            else
            {
                RunTestsInParallel(listener, filter, results, disposeRunners);
            }

            if (disposeRunners) Runners.Clear();

            return ResultHelper.Merge(results);
        }

        private void RunTestsSequentially(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter, List<NUnit.Engine.TestEngineResult> results, bool disposeRunners)
        {
            foreach (var runner in Runners)
            {
                var task = new TestExecutionTask(runner, listener, filter, disposeRunners);
                task.Execute();
                LogResultsFromTask(task, results);
            }
        }

        private void RunTestsInParallel(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter, List<NUnit.Engine.TestEngineResult> results, bool disposeRunners)
        {
            var workerPool = new ParallelTaskWorkerPool(LevelOfParallelism);
            var tasks = new List<TestExecutionTask>();

            foreach (var runner in Runners)
            {
                var task = new TestExecutionTask(runner, listener, filter, disposeRunners);
                tasks.Add(task);
                workerPool.Enqueue(task);
            }

            workerPool.Start();
            workerPool.WaitAll();

            foreach (var task in tasks)
                LogResultsFromTask(task, results);
        }

        /// <summary>
        /// Request the ongoing test run to stop. If no  test is running, the call is ignored.
        /// </summary>
        public override void RequestStop()
        {
            foreach (var runner in Runners)
                runner.RequestStop();
        }

        /// <summary>
        /// Force the ongoing test run to stop. If no  test is running, the call is ignored.
        /// </summary>
        public override void ForcedStop()
        {
            foreach (var runner in Runners)
                runner.ForcedStop();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            foreach (var runner in Runners)
            {
                runner.Dispose();
            }

            Runners.Clear();
        }

        private static void LogResultsFromTask(TestExecutionTask task, List<NUnit.Engine.TestEngineResult> results)
        {
            var result = task.Result;
            if (result != null)
            {
                results.Add(result);
            }

            //if (task.UnloadException != null)
            //{
            //    unloadExceptions.Add(task.UnloadException);
            //}
        }
    }
}
