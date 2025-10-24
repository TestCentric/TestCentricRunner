// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using TestCentric.Engine;

namespace TestCentric.Gui.Model
{
    /// <summary>
    /// TestServices caches commonly used services.
    /// </summary>
    public class TestServices : IServiceLocator
    {
        private ITestEngine _testEngine;
        private IServiceLocator _services;

        public TestServices(ITestEngine testEngine)
        {
            _testEngine = testEngine;
            _services = testEngine.Services;
        }

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
