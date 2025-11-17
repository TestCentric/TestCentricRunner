// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

#if !NETCOREAPP2_1
using NUnit.Common;
using NUnit.Framework;
using TestCentric.Engine.Services;
using TestCentric.Engine.Services.Fakes;

namespace TestCentric.Engine.Runners
{
    public class AggregatingTestRunnerTests
    {
        private ServiceContext _context;

        [SetUp]
        public void CreateContext()
        {
            _context = new ServiceContext();
            // Called in AggregatingTestRunner constructor. The fake returns
            // an AssemblyRunner even though the assembly doesn't actually exist.
            _context.Add(new FakeTestRunnerFactory());
        }

        [TestCase(1)]
        [TestCase(3)]
        [TestCase(8)]
        [TestCase(20)]
        public void CheckLevelOfParallelism_MaxAgentsNotSpecified(int assemblyCount)
        {
            var package = CreatePackage(assemblyCount);

            var runner = new AggregatingTestRunner(_context, package);
            Assert.That(runner.LevelOfParallelism, Is.EqualTo(assemblyCount));
        }

        [TestCase(1, 1, 1)]
        [TestCase(3, 1, 1)]
        [TestCase(3, 2, 2)]
        [TestCase(3, 3, 3)]
        [TestCase(20, 8, 8)]
        [TestCase(8, 20, 8)]
        public void CheckLevelOfParallelism_MaxAgentsSpecified(int assemblyCount, int maxAgents, int expected)
        {
            var package = CreatePackage(assemblyCount);
            package.Settings.Add(SettingDefinitions.MaxAgents.WithValue(maxAgents));

            var runner = new AggregatingTestRunner(_context, package);
            Assert.That(runner.LevelOfParallelism, Is.EqualTo(expected));
        }

        [Test]
        public void CheckLevelOfParallelism_SingleAssemblyPackageConstructor()
        {
            var package = new TestPackage("junk.dll");
            var runner = new AggregatingTestRunner(_context, package);
            Assert.That(runner.LevelOfParallelism, Is.EqualTo(1));
        }

        // Create a fake test package with a specified number of assemblies,
        // generating different names for each of them.
        private TestPackage CreatePackage(int assemblyCount)
        {
            var assemblies = new string[assemblyCount];
            for (int i = 0; i < assemblyCount; i++)
                assemblies[i] = $"test{i + 1}.dll";

            return new TestPackage(assemblies);
        }
    }
}
#endif
