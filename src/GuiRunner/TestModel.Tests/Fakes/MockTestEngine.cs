// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Xml;
using NUnit.Engine;
using TestCentric.Engine.Services;

using ITestRunner = NUnit.Engine.ITestRunner;

namespace TestCentric.Gui.Model.Fakes
{
    public class MockTestEngine : ITestEngine
    {
        private ServiceLocator _services = new ServiceLocator();
        private AvailableRuntimesService _availableRuntimes = new AvailableRuntimesService();
        //private TestAgentService _testAgentService = new TestAgentService();

        #region Constructor

        public MockTestEngine()
        {
            _services.AddService<NUnit.Engine.IExtensionService>(new ExtensionService());
            _services.AddService<NUnit.Engine.IResultService>(new ResultService());
            //_services.AddService<ITestAgentProvider>(_testAgentService);
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
            //_availableRuntimes.AddRuntimes(runtimes);
            //_testAgentService.AddRuntimes(runtimes);
            return this;
        }

        #endregion

        #region ITestEngine Explicit Implementation

        NUnit.Engine.InternalTraceLevel ITestEngine.InternalTraceLevel { get; set; }

        NUnit.Engine.IServiceLocator ITestEngine.Services { get { return _services; } }

        string ITestEngine.WorkDirectory { get; set; }

        ITestRunner ITestEngine.GetRunner(NUnit.Engine.TestPackage package)
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
