// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model
{
    using NSubstitute;
    using NUnit.Framework;

    [TestFixture]
    internal class TestResultManagerTests
    {
        [TestCase(null)]
        [TestCase("")]
        [TestCase("100")]
        public void GetResultForTest_NotExistingId_ReturnsNull(string testId)
        {
            // Arrange
            ITestModel model = Substitute.For<ITestModel>();

            // Act
            var manager = new TestResultManager(model);
            ResultNode result = manager.GetResultForTest(testId);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void GetResultForTest_ExistingId_ReturnsResult()
        {
            // Arrange
            ITestModel model = Substitute.For<ITestModel>();
            ResultNode resultNode = new ResultNode("<test-suite id='1' result='Passed'/>");

            var manager = new TestResultManager(model);
            manager.AddResult(resultNode);

            // Act
            ResultNode result = manager.GetResultForTest("1");

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Outcome.Status, Is.EqualTo(TestStatus.Passed));
        }

        [Test]
        public void ClearResult_ResultIsNotAvailable()
        {
            // Arrange
            ITestModel model = Substitute.For<ITestModel>();
            ResultNode resultNode = new ResultNode("<test-suite id='1' result='Passed'/>");

            var manager = new TestResultManager(model);
            manager.AddResult(resultNode);

            // Act
            manager.ClearResults();

            // Assert
            ResultNode result = manager.GetResultForTest("1");
            Assert.That(result, Is.Null);
        }

        [Test]
        public void TestRunStarting_AllResults_IsLatestRun_IsFalse()
        {
            // Arrange
            ITestModel model = Substitute.For<ITestModel>();
            ResultNode resultNode1 = new ResultNode("<test-suite id='1' result='Passed'/>");
            ResultNode resultNode2 = new ResultNode("<test-suite id='2' result='Passed'/>");

            var manager = new TestResultManager(model);
            manager.AddResult(resultNode1);
            manager.AddResult(resultNode2);

            // Act
            manager.TestRunStarting();

            // Assert
            Assert.That(resultNode1.IsLatestRun, Is.EqualTo(false));
            Assert.That(resultNode2.IsLatestRun, Is.EqualTo(false));
        }

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
        public void AddResult_ResultExistsFromPreviousRun_ReturnsMergedResultFromCurrentAndPreviousRun(
            string oldOutcome, string oldLabel, string newOutcome, string newLabel, TestStatus expectedTestStatus)
        {
            // Arrange
            ResultNode oldResult = new ResultNode($"<test-suite id='1' result='{oldOutcome}' label='{oldLabel}' />");
            ResultNode childResult1 = new ResultNode($"<test-suite id='2' result='{newOutcome}' label='{newLabel}' />");
            ResultNode childResult2 = new ResultNode($"<test-suite id='3' result='{oldOutcome}' label='{oldLabel}' />");

            TestNode testNode1 = new TestNode($"<test-suite id='1'> <test-case id='2' />  <test-case id='3' /> </test-suite>");
            
            ITestModel model = Substitute.For<ITestModel>();
            model.GetTestById("1").Returns(testNode1);
            var manager = new TestResultManager(model);
            manager.AddResult(oldResult);
            manager.AddResult(childResult2);

            // Act
            manager.TestRunStarting();
            ResultNode newResult = new ResultNode($"<test-suite id='1' result='{newOutcome}' label='{newLabel}' />");
            manager.AddResult(childResult1);
            manager.AddResult(newResult);

            // Assert
            ResultNode result = manager.GetResultForTest("1");
            Assert.That(result.Outcome.Status, Is.EqualTo(expectedTestStatus));
        }

        [TestCase("Failed", "", "Passed", "", TestStatus.Failed)]
        [TestCase("Warning", "", "Failed", "", TestStatus.Failed)]
        [TestCase("Skipped", "", "Failed", "", TestStatus.Failed)]
        [TestCase("Warning", "", "Skipped", "Ignored", TestStatus.Warning)]
        [TestCase("Passed", "", "Inconclusive", "", TestStatus.Passed)]
        [TestCase("Inconclusive", "", "Skipped", "", TestStatus.Inconclusive)]
        public void AddResult_ResultExistsFromPreviousRun_ReturnsResultMergedResultFromCurrentAndPreviousRun(
            string oldOutcome, string oldLabel, string newOutcome, string newLabel, TestStatus returnsOldResult)
        {
            // Arrange
            ResultNode oldResult = new ResultNode($"<test-suite id='1' result='{oldOutcome}' label='{oldLabel}' />");
            ResultNode childResult1 = new ResultNode($"<test-suite id='2' result='{newOutcome}' label='{newLabel}' />");
            ResultNode childResult2 = new ResultNode($"<test-suite id='3' result='{oldOutcome}' label='{oldLabel}' />");

            TestNode testNode1 = new TestNode($"<test-suite id='1'> <test-case id='2' />  <test-case id='3' /> </test-suite>");

            ITestModel model = Substitute.For<ITestModel>();
            model.GetTestById("1").Returns(testNode1);
            var manager = new TestResultManager(model);
            manager.AddResult(oldResult);
            manager.AddResult(childResult2);

            // Act
            manager.TestRunStarting();
            ResultNode newResult = new ResultNode($"<test-suite id='1' result='{newOutcome}' label='{newLabel}' />");
            manager.AddResult(childResult1);
            ResultNode addedResult = manager.AddResult(newResult);

            // Assert
            Assert.That(addedResult.Id, Is.EqualTo("1"));
            Assert.That(addedResult.Outcome.Status, Is.EqualTo(returnsOldResult));
        }

        [Test]
        public void AddResult_ExplicitResult_ReturnsResultFromPreviousRun()
        {
            // Arrange
            ResultNode oldResult = new ResultNode($"<test-suite id='1' result='Passed' />");
            ResultNode newResult = new ResultNode($"<test-suite id='1' result='Skipped' label='Explicit' />");

            ITestModel model = Substitute.For<ITestModel>();
            var manager = new TestResultManager(model);
            manager.AddResult(oldResult);

            // Act
            manager.TestRunStarting();
            var addedResult = manager.AddResult(newResult);

            // Assert
            Assert.That(addedResult.Id, Is.EqualTo("1"));
            Assert.That(addedResult.Outcome.Status, Is.EqualTo(TestStatus.Passed));
        }

        [Test]
        public void Reload()
        {
            // Arrange
            string xmlReload = "<test-run id='1'>" +
                               "<test-case id='2' fullname='Assembly.Folder1.TestB'/>" +
                               "</test-run>";

            var reloadedTestNode = new TestNode(xmlReload);

            ITestModel model = Substitute.For<ITestModel>();
            var manager = new TestResultManager(model);
            model.LoadedTests.Returns(reloadedTestNode);

            var resultXml = "<test-case id='3' fullname='Assembly.Folder1.TestB' result='Passed'/>";
            manager.AddResult(new ResultNode(resultXml));

            // Act
            manager.ReloadTestResults();

            // Assert
            var r = manager.GetResultForTest("2");

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Id, Is.EqualTo("2"));
            Assert.That(r.Outcome.Status, Is.EqualTo(TestStatus.Passed));
            Assert.That(r.IsLatestRun, Is.False);
        }
    }
}
