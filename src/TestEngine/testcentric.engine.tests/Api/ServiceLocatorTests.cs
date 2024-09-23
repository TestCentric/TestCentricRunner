// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using NUnit.Engine;
using NUnit.Framework;

namespace TestCentric.Engine.Api

{
    public class ServiceLocatorTests
    {
        private ITestEngine _testEngine;

        [OneTimeSetUp]
        public void CreateEngine()
        {
            // TODO: We should really be using the actual engine for this test
            _testEngine = new TestEngine();
            _testEngine.InternalTraceLevel = InternalTraceLevel.Off;
        }

        [OneTimeTearDown]
        public void DisposeEngine()
        {
            _testEngine.Dispose();
        }

        [TestCase(typeof(IDriverService))]
        public void CanAccessService(Type serviceType)
        {
            IService service = _testEngine.Services.GetService(serviceType) as IService;
            Assert.That(service, Is.Not.Null, "GetService(Type) returned null");
            Assert.That(service, Is.InstanceOf(serviceType));
            Assert.That(service.Status, Is.EqualTo(ServiceStatus.Started));
        }
    }
}
