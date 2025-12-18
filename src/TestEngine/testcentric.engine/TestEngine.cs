// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using System.IO;
using NUnit;
using NUnit.Engine;
using TestCentric.Engine.Services;

using ServiceContext = TestCentric.Engine.Services.ServiceContext;
using ExtensionService = NUnit.Engine.Services.ExtensionService;

namespace TestCentric.Engine
{
    /// <summary>
    /// The TestEngine provides services that allow a client
    /// program to interact with NUnit in order to explore,
    /// load and run tests.
    /// </summary>
    public class TestEngine : ITestEngine
    {
        private ServiceContext _services = new ServiceContext();

        /// <summary>
        /// Access the public IServiceLocator, first initializing
        /// the services if that has not already been done.
        /// </summary>
        public IServiceLocator Services
        {
            get
            {
                if(!_services.ServiceManager.ServicesInitialized)
                    Initialize();

                return _services;
            }
        }

        public string WorkDirectory { get; set; } = Environment.CurrentDirectory;

        public NUnit.Engine.InternalTraceLevel InternalTraceLevel { get; set; }       

        /// <summary>
        /// Initialize the engine. This includes initializing mono addins,
        /// setting the trace level and creating the standard set of services
        /// used in the Engine.
        ///
        /// This interface is not normally called by user code. Programs linking
        /// only to the testcentric.engine.api assembly are given a
        /// pre-initialized instance of TestEngine. Programs
        /// that link directly to testcentric.engine usually do so
        /// in order to perform custom initialization.
        /// </summary>
        public void Initialize()
        {
            var logName = string.Format("InternalTrace.{0}.log", Process.GetCurrentProcess().Id);
            InternalTrace.Initialize(Path.Combine(WorkDirectory, logName), InternalTraceLevel);

            // If caller added services beforehand, we don't add any
            if (_services.ServiceCount == 0)
            {
                // Services that depend on other services must be added after their dependencies
                _services.Add(new NUnit.Engine.Services.TestFilterService());
                _services.Add(new ExtensionService());
                _services.Add(new TestEventDispatcher());
                _services.Add(new NUnit.Engine.Services.ProjectService());
                _services.Add(new NUnit.Engine.Services.RuntimeFrameworkService());
                _services.Add(new NUnit.Engine.Services.TestAgency());
                //_services.Add(new TestAgentService());
                _services.Add(new NUnit.Engine.Services.ResultService());
                _services.Add(new NUnit.Engine.Services.TestRunnerFactory());
            }

            _services.ServiceManager.StartServices();
        }

        /// <summary>
        /// Returns a test runner for use by clients that need to load the
        /// tests once and run them multiple times. If necessary, the
        /// services are initialized first.
        /// </summary>
        /// <returns>An ITestRunner.</returns>
        public ITestRunner GetRunner(NUnit.Engine.TestPackage package)
        {
            if(!_services.ServiceManager.ServicesInitialized)
                Initialize();

            return new Runners.MasterTestRunner(Services, package);
        }

        #region IDisposable Members

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            _services.ServiceManager.StopServices();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                    _services.ServiceManager.Dispose();

                _disposed = true;
            }
        }

        #endregion
    }
}
