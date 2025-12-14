// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Engine;
using NUnit.Framework;
using TestCentric.Engine.Services;

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
            _testEngine.InternalTraceLevel = NUnit.Engine.InternalTraceLevel.Off;
        }

        [OneTimeTearDown]
        public void DisposeEngine()
        {
            _testEngine.Dispose();
        }

        [TestCase(TypeArgs=[typeof(IProjectService)])]
        public void CanAccessService<TSERVICE>() where TSERVICE : class
        {
            IService service = _testEngine.Services.GetService<TSERVICE>() as IService;
            Assert.That(service, Is.Not.Null, "GetService(Type) returned null");
            Assert.That(service, Is.InstanceOf<TSERVICE>());
            Assert.That(service.Status, Is.EqualTo(ServiceStatus.Started));
        }
    }
}
