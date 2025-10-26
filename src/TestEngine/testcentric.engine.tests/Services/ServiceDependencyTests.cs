// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    using Fakes;

    public class ServiceDependencyTests
    {
        private ServiceContext _services;

        [SetUp]
        public void CreateServiceContext()
        {
            _services = new ServiceContext();
        }

        // NOTE: Initialization of services was at one time dependent
        // on the order services are added to the ServiceContext.
        // This is no longer the case. These tests verify order-independence
        // in the case of two services, one of which depends on the other.

        [Test]
        public void BothServicesStart_DependencyFirst()
        {
            _services.Add(new FakeService1());
            _services.Add(new FakeService2());
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            Assert.That(fake1.Status, Is.EqualTo(ServiceStatus.Started));
            Assert.That(fake2.Status, Is.EqualTo(ServiceStatus.Started));
        }

        [Test]
        public void BothServicesStart_DependencyLast()
        {
            _services.Add(new FakeService2());
            _services.Add(new FakeService1());
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            Assert.That(fake1.Status, Is.EqualTo(ServiceStatus.Started));
            Assert.That(fake2.Status, Is.EqualTo(ServiceStatus.Started));
        }

        [Test]
        public void FakeService1FailsToStart_DependencyFirst()
        {
            _services.Add(new FakeService1() { FailToStart = true });
            _services.Add(new FakeService2());
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            // Both show error status, which is expected
            Assert.That(fake1.Status, Is.EqualTo(ServiceStatus.Error));
            Assert.That(fake2.Status, Is.EqualTo(ServiceStatus.Error));
        }

        [Test]
        public void FakeService1FailsToStart_DependencyLast()
        {
            _services.Add(new FakeService2());
            _services.Add(new FakeService1() { FailToStart = true });
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            Assert.That(fake1.Status, Is.EqualTo(ServiceStatus.Error));
            Assert.That(fake2.Status, Is.EqualTo(ServiceStatus.Error));
        }

        [Test]
        public void FakeService2FailsToStart_DependencyFirst()
        {
            _services.Add(new FakeService1());
            _services.Add(new FakeService2() { FailToStart = true });
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            Assert.That(fake1.Status, Is.EqualTo(ServiceStatus.Started));
            Assert.That(fake2.Status, Is.EqualTo(ServiceStatus.Error));
        }

        [Test]
        public void FakeService2FailsToStart_DependencyLast()
        {
            _services.Add(new FakeService2() { FailToStart = true });
            _services.Add(new FakeService1());
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            Assert.That(fake1.Status, Is.EqualTo(ServiceStatus.Started));
            Assert.That(fake2.Status, Is.EqualTo(ServiceStatus.Error));
        }

        [Test]
        public void FakeService1NotAdded()
        {
            _services.Add(new FakeService2());
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            Assert.That(fake1, Is.Null);
            Assert.That(fake2.Status, Is.EqualTo(ServiceStatus.Error));
        }

        [Test]
        public void FakeService2NotAdded()
        {
            _services.Add(new FakeService1());
            _services.ServiceManager.StartServices();
            var fake1 = _services.GetService<FakeService1>();
            var fake2 = _services.GetService<FakeService2>();
            Assert.That(fake1.Status, Is.EqualTo(ServiceStatus.Started));
            Assert.That(fake2, Is.Null);
        }

        private class FakeService1 : FakeService { }

        private class FakeService2 : FakeService
        {
            public override void StartService()
            {
                // Hard dependency on FakeService1
                var fake1 = ServiceContext.GetService<FakeService1>();
                if (fake1 == null || fake1.Status != ServiceStatus.Started)
                    FailToStart = true;

                base.StartService();
            }
        }
    }
}
