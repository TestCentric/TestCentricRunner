// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using NUnit.Framework;
using NUnit.Engine;
using TestCentric.Tests.Assemblies;

namespace TestCentric.Gui.Model
{
    [Ignore("Unable to load assemblies in unit tests because agents are not yet installed")]
    public class TestModelAssemblyTests
    {
        private const string MOCK_ASSEMBLY = "mock-assembly.dll";

        private ITestModel _model;

        [SetUp]
        public void CreateTestModel()
        {
            _model = new TestModel(new TestEngine());
            var mockAssemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, MOCK_ASSEMBLY);
            Assert.That(File.Exists(mockAssemblyPath));
            var project = _model.CreateNewProject("MyProject", mockAssemblyPath);
            _model.LoadTests(project.TestFiles);
        }

        [TearDown]
        public void ReleaseTestModel()
        {
            _model.Dispose();
        }

        [Test]
        public void CheckStateAfterLoading()
        {
            Assert.That(_model.HasTests, "HasTests");
            Assert.That(_model.LoadedTests, Is.Not.Null, "Tests");
            Assert.That(_model.HasResults, Is.False, "HasResults");

            var testRun = _model.LoadedTests;
            Assert.That(testRun.Xml.Name, Is.EqualTo("test-run"), "Expected test-run element");
            Assert.That(testRun.RunState, Is.EqualTo(RunState.Runnable), "RunState of test-run");
            Assert.That(testRun.TestCount, Is.EqualTo(MockAssembly.Tests), "TestCount of test-run");
            Assert.That(testRun.Children.Count, Is.EqualTo(1), "Child count of test-run");

            var testAssembly = testRun.Children[0];
            Assert.That(testAssembly.RunState, Is.EqualTo(RunState.Runnable), "RunState of assembly");
            Assert.That(testAssembly.TestCount, Is.EqualTo(MockAssembly.Tests), "TestCount of assembly");
        }

        [TestCase("MockTestFixture")]
        [TestCase("FailingTest")]
        public void CheckGetTestById(string testName)
        {
            var testNode = FindTestByName(_model.LoadedTests, testName);
            Assert.That(testNode, Is.Not.Null, $"Internal Test Error: Can't find {testName} in mock-assembly");

            var foundTest = _model.GetTestById(testNode.Id);
            Assert.That(foundTest, Is.Not.Null, $"No test found with id {testNode.Id}");
            Assert.That(foundTest.Name, Is.EqualTo(testName), "Found the test but name is wrong");
        }

        private TestNode FindTestByName(TestNode parent, string name)
        {
            if (parent.Name == name)
                return parent;

            foreach (var child in parent.Children)
            {
                var result = FindTestByName(child, name);
                if (result != null) return result;
            }

            return null;
        }

        [Test]
        public void CheckThatTestsMapToPackages()
        {
            var package1 = _model.GetPackageForTest(_model.LoadedTests.Id);
            var package2 = _model.GetPackageForTest(_model.LoadedTests.Children[0].Id);
            var nopackage = _model.GetPackageForTest(_model.LoadedTests.Children[0].Children[0].Id);

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
            Assert.That(_model.LoadedTests, Is.Not.Null, "Tests");
            Assert.That(_model.HasResults, "HasResults");
        }

        [Test]
        public void CheckStateAfterUnloading()
        {
            _model.UnloadTests();

            Assert.That(_model.HasTests, Is.False, "HasTests");
            Assert.That(_model.LoadedTests, Is.Null, "Tests");
            Assert.That(_model.HasResults, Is.False, "HasResults");
        }

        [Test]
        public void CheckStateAfterReloading()
        {
            _model.ReloadTests();

            Assert.That(_model.HasTests, "HasTests");
            Assert.That(_model.LoadedTests, Is.Not.Null, "Tests");
            Assert.That(_model.HasResults, Is.False, "HasResults");
        }

        [Test]
        public void TestTreeIsUnchangedByReload()
        {
            var originalTests = _model.LoadedTests;

            _model.ReloadTests();

            Assert.Multiple(() => CheckNodesAreEqual(originalTests, _model.LoadedTests));
        }

        private void RunAllTestsAndWaitForCompletion()
        {
            bool runComplete = false;
            _model.Events.RunFinished += (r) => runComplete = true;

            _model.RunTests(_model.LoadedTests);

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
