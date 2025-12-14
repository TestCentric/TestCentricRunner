// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Engine;
using NUnit.Framework;
using TestCentric.Gui.Model.Fakes;

namespace TestCentric.Gui.Model
{
    [TestFixture]
    public class TestModelTests
    {
        [Test]
        public void CreateNewEmptyProject_IsDirty_IsFalse()
        {
            // Arrange
            var engine = new MockTestEngine();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            // Act
            model.CreateNewProject();

            // Assert
            Assert.That(model.TestCentricProject.IsDirty, Is.False);
        }

        [Test]
        public void AddTests_IsDirty_IsTrue()
        {
            // Arrange
            var engine = new MockTestEngine();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            // Act
            model.CreateNewProject();
            model.AddTests(new[] { "Dummy.dll" });

            // Assert
            Assert.That(model.TestCentricProject.IsDirty, Is.True);
        }

        [Test]
        public void AddTests_TestCentricProjectLoadedEvent_IsTriggered()
        {
            // Arrange
            var engine = new MockTestEngine();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            bool projectLoadedCalled = false;
            model.Events.TestCentricProjectLoaded += (t) => projectLoadedCalled = true;

            // Act
            model.CreateNewProject();
            model.AddTests(new[] { "Dummy.dll" });

            // Assert
            Assert.That(projectLoadedCalled, Is.True);
        }

        [Test]
        public void RemoveTestPackage_TestCentricProjectLoadedEvent_IsTriggered()
        {
            // Arrange
            var engine = Substitute.For<ITestEngine>();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            bool projectLoadedCalled = false;
            model.Events.TestCentricProjectLoaded += (t) => projectLoadedCalled = true;

            // Act
            model.CreateNewProject(new[] { "Dummy.dll", "Dummy2.dll" });
            var subPackage = model.TestCentricProject.SubPackages[1];
            model.RemoveTestPackage(subPackage);

            // Assert
            Assert.That(projectLoadedCalled, Is.True);
        }
      
        [Test]
        public void SaveProject_RecentFiles_ContainsProjectName()
        {
            // Arrange
            var engine = new MockTestEngine();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            // Act
            model.CreateNewProject();
            model.SaveProject("TestCentric.tcproj");

            // Assert
            Assert.That(model.Settings.Gui.RecentFiles.Latest, Is.EqualTo("TestCentric.tcproj"));
        }

        [Test]
        public void IsInTestRun_TestNode_ReturnsTrue()
        {
            // Arrange
            var xmlNode = XmlHelper.CreateXmlNode($"<test-case id='1' />");
            var testNode = new TestNode(xmlNode);

            var runner = Substitute.For<ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<ITestEngine>();
            engine.GetRunner(null).ReturnsForAnyArgs(runner);
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            model.CreateNewProject();
            model.LoadTests(new[] { "dummy.dll" });
            model.RunTests(testNode);

            // Act
            bool isRunning = model.IsInTestRun(testNode);

            // Assert
            Assert.That(isRunning, Is.True);
        }

        [Test]
        public void IsInTestRun_TestNodeWithChildren_ReturnsTrue()
        {
            // Arrange
            var xmlNode = XmlHelper.CreateXmlNode($"<test-suite type='TestFixture' id='1' name='FixtureA'> <test-case id='2' name='TestA'/> <test-case id='3' name='TestB'/> </test-suite>");
            var testNode = new TestNode(xmlNode);

            var runner = Substitute.For<ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<ITestEngine>();
            engine.GetRunner(null).ReturnsForAnyArgs(runner);
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            model.CreateNewProject();
            model.LoadTests(new[] { "dummy.dll" });
            model.RunTests(testNode);

            // Act
            bool isRunning1 = model.IsInTestRun(testNode);
            bool isRunning2 = model.IsInTestRun(testNode.Children[0]);
            bool isRunning3 = model.IsInTestRun(testNode.Children[1]);

            // Assert
            Assert.That(isRunning1, Is.True);
            Assert.That(isRunning2, Is.True);
            Assert.That(isRunning3, Is.True);
        }

        [Test]
        public void IsInTestRun_TestSelection_ReturnsTrue()
        {
            // Arrange
            var xmlNode = XmlHelper.CreateXmlNode($"<test-case id='1' name='TestA' />");
            var testNode1 = new TestNode(xmlNode);
            var testNode2 = new TestNode(xmlNode);
            TestSelection tests = new TestSelection(new[] { testNode1, testNode2 });

            var runner = Substitute.For<ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<ITestEngine>();
            engine.GetRunner(null).ReturnsForAnyArgs(runner);
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            model.CreateNewProject();
            model.LoadTests(new[] { "dummy.dll" });
            model.RunTests(tests);

            // Act
            bool isRunning1 = model.IsInTestRun(testNode1);
            bool isRunning2 = model.IsInTestRun(testNode2);

            // Assert
            Assert.That(isRunning1, Is.True);
            Assert.That(isRunning2, Is.True);
        }

        [Test]
        public void TestRun_AllPreviousTestResults_IsLatestRun_IsFalse()
        {
            // Arrange
            var xmlNode = XmlHelper.CreateXmlNode($"<test-case id='1' name='TestA' />");
            var testNode = new TestNode(xmlNode);

            var runner = Substitute.For<ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<ITestEngine>();
            engine.GetRunner(null).ReturnsForAnyArgs(runner);
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            model.CreateNewProject();
            model.LoadTests(new[] { "dummy.dll" });

            ResultNode resultNode1 = new ResultNode("<test-case id='1' />");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' />");
            model.TestResultManager.AddResult(resultNode1);
            model.TestResultManager.AddResult(resultNode2);

            // Act
            model.RunTests(testNode);

            // Assert
            Assert.That(resultNode1.IsLatestRun, Is.False);
            Assert.That(resultNode2.IsLatestRun, Is.False);
        }
    }
}
