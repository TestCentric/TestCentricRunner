// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model.Fakes;

namespace TestCentric.Gui.Model
{
    using System.Xml;
    using NUnit.Common;
    using TestCentric.Engine;

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
            var engine = Substitute.For<TestCentric.Engine.ITestEngine>();
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
            Assert.That(model.RecentFiles.Latest, Is.EqualTo("TestCentric.tcproj"));
        }

        [Test]
        public void IsInTestRun_TestNode_ReturnsTrue()
        {
            // Arrange
            var xmlNode = XmlHelper.CreateXmlNode($"<test-case id='1' />");
            var testNode = new TestNode(xmlNode);

            var runner = Substitute.For<TestCentric.Engine.ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<TestCentric.Engine.ITestEngine>();
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

            var runner = Substitute.For<TestCentric.Engine.ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<TestCentric.Engine.ITestEngine>();
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

            var runner = Substitute.For<TestCentric.Engine.ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<TestCentric.Engine.ITestEngine>();
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

            var runner = Substitute.For<TestCentric.Engine.ITestRunner>();
            runner.Explore(null).ReturnsForAnyArgs(xmlNode);
            var engine = Substitute.For<TestCentric.Engine.ITestEngine>();
            engine.GetRunner(null).ReturnsForAnyArgs(runner);
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options);

            model.CreateNewProject();
            model.LoadTests(new[] { "dummy.dll" });

            ResultNode resultNode1 = new ResultNode("<test-case id='1' />");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' />");
            model.Results.Add("1", resultNode1);
            model.Results.Add("2", resultNode2);

            // Act
            model.RunTests(testNode);

            // Assert
            Assert.That(resultNode1.IsLatestRun, Is.False);
            Assert.That(resultNode2.IsLatestRun, Is.False);
        }

        [Ignore("Needs refactor")]
        [TestCase("Failed", "", "Passed", "", TestStatus.Failed)]
        [TestCase("Failed", "", "Warning", "", TestStatus.Failed)]
        [TestCase("Failed", "", "Skipped", "Ignored", TestStatus.Failed)]
        [TestCase("Failed", "", "Failed", "", TestStatus.Failed)]
        [TestCase("Warning", "", "Failed", "", TestStatus.Failed)]
        [TestCase("Skipped", "", "Failed", "", TestStatus.Failed)]
        [TestCase("Skipped", "Ignored", "Failed", "", TestStatus.Failed)]
        [TestCase("Warning", "", "Passed", "", TestStatus.Warning)]
        [TestCase("Warning", "", "Inconclusive", "", TestStatus.Warning)]
        [TestCase("Warning", "", "Skipped", "Ignored", TestStatus.Warning)]
        [TestCase("Inconclusive", "", "Warning", "", TestStatus.Warning)]
        [TestCase("Warning", "", "Warning", "", TestStatus.Warning)]
        [TestCase("Passed", "", "Warning", "", TestStatus.Warning)]
        [TestCase("Skipped", "Ignored", "Warning", "", TestStatus.Warning)]
        [TestCase("Skipped", "Ignored", "Passed", "", TestStatus.Skipped)]
        [TestCase("Skipped", "Ignored", "Inconclusive", "", TestStatus.Skipped)]
        [TestCase("Passed", "", "Skipped", "Ignored", TestStatus.Skipped)]
        [TestCase("Passed", "", "Inconclusive", "", TestStatus.Passed)]
        [TestCase("Passed", "", "Skipped", "", TestStatus.Passed)]
        [TestCase("Skipped", "", "Passed", "", TestStatus.Passed)]
        [TestCase("Inconclusive", "", "Skipped", "", TestStatus.Inconclusive)]
        public void AddResult_OldResultExists_KeepWorstResult(
            string oldOutcome, string oldLabel, string newOutcome, string newLabel, TestStatus expectedTestStatus)
        {
            // Arrange
            ResultNode oldResult = new ResultNode($"<test-suite id='1' result='{oldOutcome}' label='{oldLabel}' />");

            var engine = Substitute.For<TestCentric.Engine.ITestEngine>();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options) as TestModel;

            model.AddResult(oldResult);

            // Act
            ResultNode newResult = new ResultNode($"<test-suite id='1' result='{newOutcome}' label='{newLabel}' />");
            model.AddResult(newResult);

            // Assert
            ResultNode result = model.GetResultForTest("1");
            Assert.That(result.Outcome.Status, Is.EqualTo(expectedTestStatus));
        }

        [Ignore("Needs refactor")]
        [TestCase("Failed", "", "Passed", "", true)]
        [TestCase("Warning", "", "Failed", "", false)]
        [TestCase("Skipped", "", "Failed", "", false)]
        [TestCase("Warning", "", "Skipped", "Ignored", true)]
        [TestCase("Passed", "", "Inconclusive", "", true)]
        [TestCase("Inconclusive", "", "Skipped", "", true)]
        public void AddResult_OldResultExists_ReturnsResultWithWorstOutcome(
            string oldOutcome, string oldLabel, string newOutcome, string newLabel, bool returnsOldResult)
        {
            // Arrange
            ResultNode oldResult = new ResultNode($"<test-suite id='1' result='{oldOutcome}' label='{oldLabel}' />");

            var engine = Substitute.For<TestCentric.Engine.ITestEngine>();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options) as TestModel;

            model.AddResult(oldResult);

            // Act
            ResultNode newResult = new ResultNode($"<test-suite id='1' result='{newOutcome}' label='{newLabel}' />");
            ResultNode addedResult = model.AddResult(newResult);

            // Assert
            ResultNode expectedResult = returnsOldResult ? oldResult : newResult;
            Assert.That(addedResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void Reload()
        {
            // Arrange
            var engine = Substitute.For<ITestEngine>();
            var options = new GuiOptions("dummy.dll");
            var model = TestModel.CreateTestModel(engine, options) as TestModel;

            var runner = Substitute.For<ITestRunner>();
            string xmlOriginal = "<test-run id='1'>" +
                                 "<test-case id='2' fullname='Assembly.Folder1.TestA'/>" +
                                 "<test-case id='3' fullname='Assembly.Folder1.TestB'/>" +
                                 "</test-run>";
            string xmlReload = "<test-run id='1'>" +
                               "<test-case id='2' fullname='Assembly.Folder1.TestB'/>" +
                               "</test-run>";

            XmlNode xmlNode1 = XmlHelper.CreateXmlNode(xmlOriginal);
            XmlNode xmlNode2 = XmlHelper.CreateXmlNode(xmlReload);

            runner.Explore(null).ReturnsForAnyArgs(xmlNode1, xmlNode2);
            engine.GetRunner(null).ReturnsForAnyArgs(runner);
            model.CreateNewProject(new[] { "MyFile.dll" });

            var resultXml = "<test-case id='3' fullname='Assembly.Folder1.TestB' result='Passed'/>";
            model.Results.Add("3", new ResultNode(resultXml));

            // Act
            model.ReloadTests();

            // Assert
            var r = model.GetResultForTest("2");

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Id, Is.EqualTo("2"));
            Assert.That(r.Outcome.Status, Is.EqualTo(TestStatus.Passed));
            Assert.That(r.IsLatestRun, Is.False);
        }
    }
}
