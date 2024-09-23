// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System.IO;
using NUnit.Framework;
using TestCentric.Tests.Assemblies;
using NUnit.Engine;

namespace TestCentric.Gui.Model
{
    public class TestModelAssemblyTests
    {
        private const string MOCK_ASSEMBLY = "mock-assembly.dll";

        private ITestModel _model;

        [SetUp]
        public void CreateTestModel()
        {
            var engine = TestEngineActivator.CreateInstance();
            Assert.That(engine, Is.Not.Null, "Unable to create engine instance for testing");

            _model = new TestModel(engine);
            _model.PackageOverrides[EnginePackageSettings.ProcessModel] = "InProcess";
            _model.LoadTests(new[] { Path.Combine(TestContext.CurrentContext.TestDirectory, MOCK_ASSEMBLY) });
        }

        [TearDown]
        public void ReleaseTestModel()
        {
            _model.Dispose();
        }

        [Test] // TODO: Move this to the engine tests
        public void EnsureAgentIsAvailable()
        {
            var testDir = TestContext.CurrentContext.TestDirectory;
            Assert.That(File.Exists(Path.Combine(testDir, "agents/net20/testcentric-agent.exe")), "Cannot find testcentric-agent - may cause other test failures");
            Assert.That(File.Exists(Path.Combine(testDir, "agents/net20/testcentric-agent-x86.exe")), "Cannot find testcentric-agent-x86 - may cause other test failures");
        }

        [Test]
        public void CheckStateAfterLoading()
        {
            Assert.That(_model.HasTests, "HasTests");
            Assert.That(_model.Tests, Is.Not.Null, "Tests");
            Assert.That(_model.HasResults, Is.False, "HasResults");

            var testRun = _model.Tests;
            Assert.That(testRun.Xml.Name, Is.EqualTo("test-run"), "Expected test-run element");
            Assert.That(testRun.RunState, Is.EqualTo(RunState.Runnable), "RunState of test-run");
            Assert.That(testRun.TestCount, Is.EqualTo(MockAssembly.Tests), "TestCount of test-run");
            Assert.That(testRun.Children.Count, Is.EqualTo(1), "Child count of test-run");

            var testAssembly = testRun.Children[0];
            Assert.That(testAssembly.RunState, Is.EqualTo(RunState.Runnable), "RunState of assembly");
            Assert.That(testAssembly.TestCount, Is.EqualTo(MockAssembly.Tests), "TestCount of assembly");
        }

        [Test]
        public void CheckThatTestsMapToPackages()
        {
            var package1 = _model.GetPackageForTest(_model.Tests.Id);
            var package2 = _model.GetPackageForTest(_model.Tests.Children[0].Id);
            var nopackage = _model.GetPackageForTest(_model.Tests.Children[0].Children[0].Id);

            Assert.That(package1, Is.Not.Null, "Package1");
            Assert.That(package2, Is.Not.Null, "Package2");
            Assert.That(nopackage, Is.Null);

            Assert.That(package1.Name, Is.Null);
            Assert.That(package1.SubPackages.Count, Is.EqualTo(1));

            Assert.That(package2.Name, Is.EqualTo(MOCK_ASSEMBLY));
            Assert.That(package2.SubPackages.Count, Is.Zero);

            Assert.That(package2, Is.SameAs(package1.SubPackages[0]));
        }

        [Test]
        public void CheckStateAfterRunningTests()
        {
            RunAllTestsAndWaitForCompletion();

            Assert.That(_model.HasTests, "HasTests");
            Assert.That(_model.Tests, Is.Not.Null, "Tests");
            Assert.That(_model.HasResults, "HasResults");
        }

        [Test]
        public void CheckStateAfterUnloading()
        {
            _model.UnloadTests();

            Assert.That(_model.HasTests, Is.False, "HasTests");
            Assert.That(_model.Tests, Is.Null, "Tests");
            Assert.That(_model.HasResults, Is.False, "HasResults");
        }

        [Test]
        public void CheckStateAfterReloading()
        {
            _model.ReloadTests();

            Assert.That(_model.HasTests, "HasTests");
            Assert.That(_model.Tests, Is.Not.Null, "Tests");
            Assert.That(_model.HasResults, Is.False, "HasResults");
        }

        [Test]
        public void TestTreeIsUnchangedByReload()
        {
            var originalTests = _model.Tests;

            _model.ReloadTests();

            Assert.Multiple(() => CheckNodesAreEqual(originalTests, _model.Tests));
        }

        private void RunAllTestsAndWaitForCompletion()
        {
            bool runComplete = false;
            _model.Events.RunFinished += (r) => runComplete = true;

            _model.RunAllTests();

            while (!runComplete)
                System.Threading.Thread.Sleep(1);
        }

        private void CheckNodesAreEqual(TestNode beforeReload, TestNode afterReload)
        {
            Assert.That(afterReload.Name, Is.EqualTo(beforeReload.Name));
            Assert.That(afterReload.FullName, Is.EqualTo(beforeReload.FullName));
            Assert.That(afterReload.Id, Is.EqualTo(beforeReload.Id), $"Different IDs for {beforeReload.Name}");
            Assert.That(afterReload.IsSuite, Is.EqualTo(beforeReload.IsSuite));

            if (afterReload.IsSuite)
            {
                Assert.That(afterReload.Children.Count, Is.EqualTo(beforeReload.Children.Count), $"Different number of children for {beforeReload.Name}");
                for (int i = 0; i < afterReload.Children.Count; i++)
                    CheckNodesAreEqual(beforeReload.Children[i], afterReload.Children[i]);
            }
        }
    }
}
