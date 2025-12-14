// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Engine;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    // TODO: More tests needed!
    public class ProjectServiceTests
    {
        private ServiceContext _services;

        [SetUp]
        public void CreateServiceContext()
        {
            _services = new ServiceContext();
            _services.Add(new Fakes.FakeExtensionService());
            _services.Add(new ProjectService());
            _services.ServiceManager.StartServices();
        }

        [Test]
        public void ServiceIsStarted()
        {
            var projectService = _services.ServiceManager.GetService(typeof(IProjectService));
            Assert.That(projectService.Status, Is.EqualTo(ServiceStatus.Started));
        }
    }
}
