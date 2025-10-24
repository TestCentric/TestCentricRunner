// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using TestCentric.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Gui.Model
{
    /// <summary>
    /// TestServices caches commonly used services.
    /// </summary>
    public class TestServices : ITestServices
    {
        private IServiceLocator _services;
        private ITestEngine _testEngine;

        public TestServices(ITestEngine testEngine)
        {
            _testEngine = testEngine;
            _services = testEngine.Services;

            ExtensionService = GetService<IExtensionService>();
            ResultService = GetService<IResultService>();
            TestAgentService = GetService<TestAgentService>();
        }

        #region ITestServices Implementation

        public IExtensionService ExtensionService { get; }

        public IResultService ResultService { get; }

        public TestAgentService TestAgentService { get; }

        #endregion

        #region IServiceLocator Implementation

        public T GetService<T>() where T : class
        {
            return _services.GetService<T>();
        }

        public object GetService(Type serviceType)
        {
            return _services.GetService(serviceType);
        }

        #endregion
    }
}
