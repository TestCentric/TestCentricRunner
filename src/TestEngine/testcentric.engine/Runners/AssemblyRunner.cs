// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using NUnit;
using NUnit.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Engine.Runners
{
    /// <summary>
    /// AssemblyRunner loads and runs a set of tests in a single assembly 
    /// using an agent acquired from TestAgency.
    /// </summary>
    public class AssemblyRunner : AbstractTestRunner
    {
        private static readonly Logger log = InternalTrace.GetLogger(typeof(AssemblyRunner));

        private ITestAgent _agent;
        private NUnit.Engine.ITestEngineRunner _remoteRunner;
        private TestAgentService _agentService;

        /// <summary>
        /// Construct a new AssemblyRunnerRunner
        /// </summary>
        /// <param name="services">A ServiceLocator interface for use by the runner</param>
        /// <param name="package">A TestPackage containing a single assembly</param>
        public AssemblyRunner(IServiceLocator services, NUnit.Engine.TestPackage package) : base(package)
        {
            _agentService = services.GetService<TestAgentService>();
        }

        /// <summary>
        /// Explore a TestPackage and return information about
        /// the tests found.
        /// </summary>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>A TestEngineResult.</returns>
        public override NUnit.Engine.TestEngineResult Explore(NUnit.Engine.TestFilter filter)
        {
            try
            {
                CreateAgentAndRunnerIfNeeded();
                return _remoteRunner.Explore(filter);
            }
            catch (Exception e)
            {
                log.Error("Failed to run remote tests {0}", ExceptionHelper.BuildMessageAndStackTrace(e));
                return CreateFailedResult(e);
            }
        }

        /// <summary>
        /// Load a TestPackage for possible execution
        /// </summary>
        /// <returns>A TestEngineResult.</returns>
        protected override NUnit.Engine.TestEngineResult LoadPackage()
        {
            log.Info("Loading " + TestPackage.Name);
            Unload();

            try
            {
                CreateAgentAndRunnerIfNeeded();

                return _remoteRunner.Load();
            }
            catch (Exception)
            {
                // TODO: Check if this is really needed
                // Clean up if the load failed
                Unload();
                throw;
            }
        }

        /// <summary>
        /// Unload any loaded TestPackage and clear
        /// the reference to the remote runner.
        /// </summary>
        public override void UnloadPackage()
        {
            if (_remoteRunner != null && TestPackage != null)
                try
                {
                    log.Info("Unloading " + TestPackage.Name);
                    _remoteRunner.Unload();
                }
                catch (Exception e)
                {
                    log.Warning("Failed to unload the remote runner. {0}", ExceptionHelper.BuildMessageAndStackTrace(e));
                }
                finally
                {
                    _remoteRunner = null;
                    LoadResult = null;
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
            CreateAgentAndRunnerIfNeeded();

            return _remoteRunner.CountTestCases(filter);
        }

        /// <summary>
        /// Run the tests in a loaded TestPackage
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>A TestResult giving the result of the test execution</returns>
        protected override NUnit.Engine.TestEngineResult RunTests(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            log.Info("Running " + TestPackage.Name);

            try
            {
                CreateAgentAndRunnerIfNeeded();

                var result = _remoteRunner.Run(listener, filter);
                log.Info("Done running " + TestPackage.Name);
                return result;
            }
            catch (Exception e)
            {
                log.Error("Failed to run remote tests {0}", ExceptionHelper.BuildMessageAndStackTrace(e));
                return CreateFailedResult(e);
            }
        }

        /// <summary>
        /// Start a run of the tests in the loaded TestPackage, returning immediately.
        /// The tests are run asynchronously and the listener interface is notified
        /// as it progresses.
        /// </summary>
        /// <param name="listener">An ITestEventHandler to receive events</param>
        /// <param name="filter">A TestFilter used to select tests</param>
        /// <returns>An AsyncTestRun that will provide the result of the test execution</returns>
        protected override NUnit.Engine.AsyncTestEngineResult RunTestsAsync(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            log.Info("Running " + TestPackage.Name + " (async)");

            try
            {
                CreateAgentAndRunnerIfNeeded();

                return _remoteRunner.RunAsync(listener, filter);
            }
            catch (Exception e)
            {
                log.Error("Failed to run remote tests {0}", ExceptionHelper.BuildMessageAndStackTrace(e));
                var result = new NUnit.Engine.AsyncTestEngineResult();
                result.SetResult(CreateFailedResult(e));
                return result;
            }
        }

        private bool _runCancelled;

        /// <summary>
        /// Request the ongoing test run to stop. If no  test is running, the call is ignored.
        /// </summary>
        public override void RequestStop()
        {
            log.Info("Requesting stop");
            _runCancelled = false;

            if (_remoteRunner != null)
            {
                try
                {
                    _remoteRunner.RequestStop();
                }
                catch (Exception e)
                {
                    log.Error("Failed to stop the remote run. {0}", ExceptionHelper.BuildMessageAndStackTrace(e));
                }
            }
        }

        /// <summary>
        /// Force the ongoing test run to stop. If no  test is running, the call is ignored.
        /// </summary>
        public override void ForcedStop()
        {
            log.Info("Cancelling test run");
            _runCancelled = true;

            if (_remoteRunner != null)
            {
                try
                {
                    _remoteRunner.ForcedStop();
                }
                catch (Exception e)
                {
                    log.Error("Failed to stop the remote run. {0}", ExceptionHelper.BuildMessageAndStackTrace(e));
                }

                _agent = null;
                _remoteRunner = null;
            }
        }

        protected override void Dispose(bool disposing)
        {
            // Disposal has to perform two actions, unloading the runner and
            // stopping the agent. Both must be tried even if one fails so
            // there can be up to two independent errors to be reported
            // through an NUnitEngineException. We do that by combining messages.
            if (!_disposed && disposing)
            {
                _disposed = true;

                Unload();

                if (_agent != null)
                {
                    _agentService.ReleaseAgent(_agent);
                }
            }
        }

        private void CreateAgentAndRunnerIfNeeded()
        {
            if (_agent == null)
            {
                log.Debug($"Trying to get an agent");

                if (_agentService.IsAgentAvailable(TestPackage))
                    _agent = _agentService.GetAgent(TestPackage);
                else
                    throw new EngineException($"No agent can be found for package {TestPackage.Name}.");

                log.Debug($"Got agent {_agent.Id:B}");
            }

            if (_remoteRunner == null)
            {
                log.Debug("Creating Runner");
                _remoteRunner = _agent.CreateRunner(TestPackage);
                log.Debug($"Created Runner {_remoteRunner.GetType().Name}");
            }
        }

        NUnit.Engine.TestEngineResult CreateFailedResult(Exception e)
        {
            var suite = XmlHelper.CreateTopLevelElement("test-suite");
            XmlHelper.AddAttribute(suite, "type", "Assembly");
            XmlHelper.AddAttribute(suite, "id", TestPackage.ID);
            XmlHelper.AddAttribute(suite, "name", TestPackage.Name);
            XmlHelper.AddAttribute(suite, "fullname", TestPackage.FullName);
            XmlHelper.AddAttribute(suite, "runstate", "NotRunnable");
            XmlHelper.AddAttribute(suite, "testcasecount", "1");
            XmlHelper.AddAttribute(suite, "result", "Failed");
            XmlHelper.AddAttribute(suite, "label", _runCancelled ? "Cancelled" : "Error");
            XmlHelper.AddAttribute(suite, "start-time", DateTime.UtcNow.ToString("u"));
            XmlHelper.AddAttribute(suite, "end-time", DateTime.UtcNow.ToString("u"));
            XmlHelper.AddAttribute(suite, "duration", "0.001");
            XmlHelper.AddAttribute(suite, "total", "1");
            XmlHelper.AddAttribute(suite, "passed", "0");
            XmlHelper.AddAttribute(suite, "failed", "1");
            XmlHelper.AddAttribute(suite, "inconclusive", "0");
            XmlHelper.AddAttribute(suite, "skipped", "0");
            XmlHelper.AddAttribute(suite, "asserts", "0");

            var failure = suite.AddElement("failure");
            if (_runCancelled)
                failure.AddElementWithCDataSection("message", "Run cancelled by user");
            else
            {
                failure.AddElementWithCDataSection("message", ExceptionHelper.BuildMessage(e));
                failure.AddElementWithCDataSection("stack-trace", ExceptionHelper.BuildMessageAndStackTrace(e));
            }

            return new NUnit.Engine.TestEngineResult(suite);
        }
    }
}
