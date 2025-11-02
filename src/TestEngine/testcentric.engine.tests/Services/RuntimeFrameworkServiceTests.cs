// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Common;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    public class RuntimeFrameworkServiceTests
    {
        private RuntimeFrameworkService _runtimeService;
        private TestPackageAnalyzer _packageAnalyzer;

        [SetUp]
        public void CreateServiceContext()
        {
            var services = new ServiceContext();
            services.Add(new Fakes.FakeProjectService());
            services.Add(new RuntimeFrameworkService());
            services.Add(new TestFrameworkService());
            services.Add(new TestPackageAnalyzer());
            services.ServiceManager.StartServices();
            _runtimeService = services.GetService<RuntimeFrameworkService>();
            _packageAnalyzer = services.GetService<TestPackageAnalyzer>();
        }

        [TearDown]
        public void StopService()
        {
            _runtimeService.StopService();
        }

        [Test]
        public void ServiceIsStarted()
        {
            Assert.That(_runtimeService.Status, Is.EqualTo(ServiceStatus.Started));
        }

        [Test]
        public void AvailableFrameworks()
        {
            var available = _runtimeService.AvailableRuntimes;
            Assert.That(available.Count, Is.GreaterThan(0));
            foreach (var framework in available)
                Console.WriteLine("Available: {0}", framework.DisplayName);
        }

        [Test]
        public void CurrentFrameworkMustBeAvailable()
        {
            var current = _runtimeService.CurrentFramework;
            Console.WriteLine("Current framework is {0} ({1})", current.DisplayName, current.Id);
            Assert.That(_runtimeService.IsAvailable(current.Id), $"{current} not available");
        }

        [Test]
        public void AvailableFrameworksListContainsNoDuplicates()
        {
            var names = new List<string>();
            foreach (var framework in _runtimeService.AvailableRuntimes)
                names.Add(framework.DisplayName);
            Assert.That(names, Is.Unique);
        }

        // TODO: Make this work
        //[TestCase("mono", 2, 0, "net-4.0")]
        //[TestCase("net", 4, 0, "net-4.0")]
        //[TestCase("net", 3, 5, "net-4.0")]
        //public void EngineOptionPreferredOverImageTarget(string framework, int majorVersion, int minorVersion, string requested)
        //{
        //    var package = new TestPackage("test");
        //    package.AddSetting(SettingDefinitions.ImageTargetFrameworkName.WithValue(framework));
        //    package.AddSetting(SettingDefinitions.ImageRuntimeVersion.WithValue(new Version(majorVersion, minorVersion)));
        //    package.AddSetting(SettingDefinitions.RequestedRuntimeFramework.WithValue(requested));

        //    _runtimeService.SelectRuntimeFramework(package);
        //    Assert.That(package.Settings.GetValueOrDefault(SettingDefinitions.TargetRuntimeFramework), Is.EqualTo(requested));
        //}

        //[Test, Platform(Exclude ="Linux")]
        //public void RuntimeFrameworkIsSetForSubpackages()
        //{
        //    var topLevelPackage = new TestPackage(new [] {"a.dll", "b.dll"});

        //    var net20Package = topLevelPackage.SubPackages[0];
        //    net20Package.Settings.Add(EnginePackageSettings.ImageRuntimeVersion, new Version("2.0.50727"));
        //    var net40Package = topLevelPackage.SubPackages[1];
        //    net40Package.Settings.Add(EnginePackageSettings.ImageRuntimeVersion, new Version("4.0.30319"));

        //    var platform = Environment.OSVersion.Platform;

        //    _packageAnalyzer.ApplyImageSettings(topLevelPackage);
        //    _runtimeService.SelectRuntimeFramework(topLevelPackage);

        //    Assert.Multiple(() =>
        //    {
        //        // UPDATE: No longer working on Linux - Excluded for now
        //        // HACK: this test will pass on a windows system with .NET 2.0 and .NET 4.0 installed or on a 
        //        // linux system with a newer version of Mono with no 2.0 profile.
        //        // TODO: Test should not depend on the availability of specific runtimes
        //        if (platform == PlatformID.Win32NT)
        //        {
        //            Assert.That(net20Package.Settings[EnginePackageSettings.TargetRuntimeFramework], Is.EqualTo("net-2.0"));
        //            Assert.That(net40Package.Settings[EnginePackageSettings.TargetRuntimeFramework], Is.EqualTo("net-4.0"));
        //        }
        //        else
        //        {
        //            Assert.That(net20Package.Settings[EnginePackageSettings.TargetRuntimeFramework], Is.EqualTo("mono-2.0"));
        //            Assert.That(net40Package.Settings[EnginePackageSettings.TargetRuntimeFramework], Is.EqualTo("mono-4.0"));
        //        }
        //    });
        //}
    }
}
