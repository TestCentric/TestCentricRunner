// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Xml;
using TestCentric.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Gui.Model.Fakes
{
    public class MockTestEngine : ITestEngine
    {
        #region Instance Variables

        private ServiceLocator _services = new ServiceLocator();

        // Simulate Engine internal services, which are always present
        private ExtensionService _extensions = new ExtensionService();
        private ResultService _resultService = new ResultService();
        private AvailableRuntimesService _availableRuntimes = new AvailableRuntimesService();
        private TestAgentInfoService _testAgentInfoService = new TestAgentInfoService();

        #endregion

        #region Constructor

        public MockTestEngine()
        {
            _services.AddService<IExtensionService>(_extensions);
            _services.AddService<IResultService>(_resultService);
            _services.AddService<NUnit.Engine.IAvailableRuntimes>(_availableRuntimes);
            _services.AddService<TestAgentInfoService>(_testAgentInfoService);
        }

        #endregion

        #region Fluent Engine Setup Methods

        public MockTestEngine WithService<TService>(TService service)
        {
            _services.AddService<TService>(service);
            return this;
        }

        public MockTestEngine WithExtension()
        {
            return this;
        }

        public MockTestEngine WithResultWriter(string name)
        {
            return this;
        }

        public MockTestEngine WithRuntimes(params RuntimeFramework[] runtimes)
        {
            _availableRuntimes.AddRuntimes(runtimes);
            return this;
        }

        #endregion

        #region ITestEngine Explicit Implementation

        InternalTraceLevel ITestEngine.InternalTraceLevel { get; set; }

        IServiceLocator ITestEngine.Services { get { return _services; } }

        string ITestEngine.WorkDirectory { get; set; }

        ITestRunner ITestEngine.GetRunner(TestPackage package)
        {
            return new MasterTestRunner();
        }

        void ITestEngine.Initialize()
        {
        }

        #endregion

        #region Fake MasterTestRunner

        public class MasterTestRunner : ITestRunner
        {
            bool ITestRunner.IsTestRunning
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            int ITestRunner.CountTestCases(NUnit.Engine.TestFilter filter)
            {
                throw new NotImplementedException();
            }

            void IDisposable.Dispose()
            {
                throw new NotImplementedException();
            }

            XmlNode ITestRunner.Explore(NUnit.Engine.TestFilter filter)
            {
                throw new NotImplementedException();
            }

            XmlNode ITestRunner.Load()
            {
                throw new NotImplementedException();
            }

            XmlNode ITestRunner.Reload()
            {
                throw new NotImplementedException();
            }

            XmlNode ITestRunner.Run(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
            {
                throw new NotImplementedException();
            }

            NUnit.Engine.ITestRun ITestRunner.RunAsync(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
            {
                throw new NotImplementedException();
            }

            void ITestRunner.StopRun(bool force)
            {
                throw new NotImplementedException();
            }

            void ITestRunner.Unload()
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IDisposable explicit implementation

        void IDisposable.Dispose()
        {
        }

        #endregion
    }
}
