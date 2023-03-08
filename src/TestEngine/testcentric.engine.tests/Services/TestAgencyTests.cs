// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

#if !NETCOREAPP2_1
using System.Linq;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    using NUnit.Engine;

    public class TestAgencyTests
    {
        private ServiceContext _services;
        private TestAgency _testAgency;

        public static readonly string[] BUILTIN_AGENTS = new string[]
        {
            "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher", "Net462AgentLauncher"
        };

        [SetUp]
        public void CreateTestAgency()
        {
            _services = new ServiceContext();
            _services.Add(new TestAgency("TestAgencyTest", 0));
            _services.ServiceManager.StartServices();
            _testAgency = _services.GetService<TestAgency>();
            Assert.That(_testAgency.Status, Is.EqualTo(ServiceStatus.Started));
        }

        [TearDown]
        public void StopServices()
        {
            _testAgency.StopService();
            Assert.That(_testAgency.Status, Is.EqualTo(ServiceStatus.Stopped));
        }

        [Test]
        public void AvailableAgents()
        {
            Assert.That(_testAgency.GetAvailableAgents().Select((info) => info.AgentName),
                Is.EqualTo(BUILTIN_AGENTS));
        }

        [TestCase("net-2.0", "Net462AgentLauncher")]
        [TestCase("net-3.5", "Net462AgentLauncher")]
        [TestCase("net-4.0", "Net462AgentLauncher")]
        [TestCase("net-4.8", "Net462AgentLauncher")]
        [TestCase("netcore-1.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-2.0", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-2.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-3.0", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-3.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-5.0", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-6.0", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-7.0", "Net70AgentLauncher")]
        public void GetAgentsForAssemblySubPackage(string targetRuntime, params string[] expectedAgents)
        {
            // Using the string constructor, which is how a subpackage is created
            var package = new TestPackage("some.dll");
            package.AddSetting(EnginePackageSettings.TargetRuntimeFramework, targetRuntime);

            Assert.That(_testAgency.GetAgentsForPackage(package).Select((info) => info.AgentName),
                Is.EqualTo(expectedAgents));
        }

        [TestCase("net-2.0", "Net462AgentLauncher")]
        [TestCase("net-3.5", "Net462AgentLauncher")]
        [TestCase("net-4.0", "Net462AgentLauncher")]
        [TestCase("net-4.8", "Net462AgentLauncher")]
        [TestCase("netcore-1.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-2.0", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-2.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-3.0", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-3.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-5.0", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-6.0", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-7.0", "Net70AgentLauncher")]
        public void GetAgentsForPackageWithOneAssembly(string targetRuntime, params string[] expectedAgents)
        {
            var package = new TestPackage(new string[] { "some.dll" });
            package.SubPackages[0].AddSetting(EnginePackageSettings.TargetRuntimeFramework, targetRuntime);

            Assert.That(_testAgency.GetAgentsForPackage(package).Select((info) => info.AgentName),
                Is.EqualTo(expectedAgents));
        }

        [TestCase("net-2.0", "net-2.0", "Net462AgentLauncher")]
        [TestCase("net-4.0", "net-4.0", "Net462AgentLauncher")]
        [TestCase("net-4.8", "net-4.5", "Net462AgentLauncher")]
        [TestCase("net-4.8", "net-2.0", "Net462AgentLauncher")]
        [TestCase("netcore-1.1", "netcore-2.0", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-2.1", "netcore-2.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-3.0", "netcore-3.1", "NetCore31AgentLauncher", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-3.1", "netcore-5.0", "Net50AgentLauncher", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-5.0", "netcore-6.0", "Net60AgentLauncher", "Net70AgentLauncher")]
        [TestCase("netcore-5.0", "netcore-7.0", "Net70AgentLauncher")]
        [TestCase("netcore-5.0", "net-4.8")] // No agents in common
        public void GetAgentsForPackageWithTwoAssemblies(string targetRuntime1, string targetRuntime2, params string[] expectedAgents)
        {
            var package = new TestPackage(new string[] { "some.dll", "another.dll" });
            package.SubPackages[0].AddSetting(EnginePackageSettings.TargetRuntimeFramework, targetRuntime1);
            package.SubPackages[1].AddSetting(EnginePackageSettings.TargetRuntimeFramework, targetRuntime2);

            Assert.That(_testAgency.GetAgentsForPackage(package).Select((info) => info.AgentName),
                Is.EqualTo(expectedAgents));
        }
    }
}
#endif
