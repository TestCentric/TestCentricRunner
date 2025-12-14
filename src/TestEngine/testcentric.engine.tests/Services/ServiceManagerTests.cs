// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Engine;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    using Fakes;

    public class ServiceManagerTests
    {
        private IService _service1;
        private ServiceManager _serviceManager;

        private IService _service2;

        [SetUp]
        public void SetUpServiceManager()
        {
            _serviceManager = new ServiceManager();

            _service1 = new DummyService1();
            _serviceManager.AddService(_service1);

            _service2 = new DummyService2();
            _serviceManager.AddService(_service2);
        }

        [Test]
        public void InitializeServices()
        {
            _serviceManager.StartServices();

            IService service = _serviceManager.GetService(typeof(IDummy1));
            Assert.That(service.Status, Is.EqualTo(ServiceStatus.Started));
            service = _serviceManager.GetService(typeof(IDummy2));
            Assert.That(service.Status, Is.EqualTo(ServiceStatus.Started));
        }

        [Test]
        public void InitializationFailure()
        {
            ((FakeService)_service1).FailToStart = true;
            Assert.DoesNotThrow(() => _serviceManager.StartServices());
        }

        [Test]
        public void TerminationFailure()
        {
            ((FakeService)_service1).FailedToStop = true;
            _service1.StartService();

            Assert.DoesNotThrow(() => _serviceManager.StopServices());
        }

        [Test]
        public void AccessServiceByClass()
        {
            IService service = _serviceManager.GetService(typeof(DummyService1));
            Assert.That(service, Is.SameAs(_service1));
        }

        [Test]
        public void AccessServiceByInterface()
        {
            IService service = _serviceManager.GetService(typeof(IDummy1));
            Assert.That(service, Is.SameAs(_service1));
        }

        private interface IDummy1 { }
        private interface IDummy2 { }
        private class DummyService1 : FakeService, IDummy1 { }
        private class DummyService2 : FakeService, IDummy2 { }
    }
}
