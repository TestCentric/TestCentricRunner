// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters
{
    [TestFixture]
    public class TestResultCountsTests
    {
        [Test]
        [TestCase("Passed", 1, 0, 0, 0)]
        [TestCase("Failed", 0, 1, 0, 0)]
        [TestCase("Warning", 0, 0, 1, 0)]
        [TestCase("Inconclusive", 0, 0, 0, 1)]
        public void TestCaseNode_WithOutcome_ReturnsExpectedCount(string resultState, int expectedPassed, int expectedFailed, int expectedWarning, int expectedInconclusive)
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' />");
            ResultNode resultNode = new ResultNode($"<test-case id='1' result='{resultState}' />");

            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testNode);

            // 3. Assert
            Assert.That(resultCounts.PassedCount, Is.EqualTo(expectedPassed));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(expectedFailed));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(expectedWarning));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(expectedInconclusive));

            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(0));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(0));
            Assert.That(resultCounts.TestCount, Is.EqualTo(1));
        }

        [Test]
        [TestCase("Ignored", 1, 0)]
        [TestCase("Explicit", 0, 1)]
        public void TestCaseNode_WithSkippedOutcome_ReturnsExpectedCount(string label, int expectedIgnore, int expectedExplicit)
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' />");
            ResultNode resultNode = new ResultNode($"<test-case id='1' result='Skipped' label='{label}' />");

            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testNode);

            // 3. Assert
            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(expectedIgnore));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(expectedExplicit));

            Assert.That(resultCounts.PassedCount, Is.EqualTo(0));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(0));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(0));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(0));
            Assert.That(resultCounts.TestCount, Is.EqualTo(1));
        }

        [Test]
        public void TestCaseNode_WithoutResult_ReturnsExpectedCount()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' />");
            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns((ResultNode)null);

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testNode);

            // 3. Assert
            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(0));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(0));

            Assert.That(resultCounts.PassedCount, Is.EqualTo(0));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(0));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(0));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(1));
            Assert.That(resultCounts.TestCount, Is.EqualTo(1));
        }

        [Test]
        public void TestCaseNode_NotVisible_ReturnsExpectedCount()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' />");
            testNode.IsVisible = false;
            ITestModel model = Substitute.For<ITestModel>();

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testNode);

            // 3. Assert
            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(0));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(0));

            Assert.That(resultCounts.PassedCount, Is.EqualTo(0));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(0));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(0));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(0));
            Assert.That(resultCounts.TestCount, Is.EqualTo(0));
        }

        [Test]
        [TestCase("Passed", "Passed", "Passed", 3, 0, 0, 0)]
        [TestCase("Failed", "Failed", "Passed", 1, 2, 0, 0)]
        [TestCase("Warning", "Failed", "Warning", 0, 1, 2, 0)]
        [TestCase("Inconclusive", "Inconclusive", "Passed", 1, 0, 0, 2)]
        public void TestSuiteNode_WithOutcome_ReturnsExpectedCount(string resultState1, string resultState2, string resultState3, int expectedPassed, int expectedFailed, int expectedWarning, int expectedInconclusive)
        {
            // 1. Arrange
            TestNode testNode = new TestNode(
                "<test-suite type='TestSuite'> " +
                    "<test-suite type='TestFixture' id='2-1000'> " +
                        "<test-case id='2-1001' />" +
                    "</test-suite>" +
                    "<test-suite type='TestFixture' id='3-1000'> " +
                        "<test-case id='3-1001' />" +
                        "<test-case id='3-1002' />" +
                    "</test-suite>" +
            "</test-suite>");

            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("2-1001").Returns(string.IsNullOrEmpty(resultState1) ? null : new ResultNode($"<test-case id='2-1001' result='{resultState1}' />"));
            model.TestResultManager.GetResultForTest("3-1001").Returns(string.IsNullOrEmpty(resultState2) ? null : new ResultNode($"<test-case id='3-1001' result='{resultState2}' />"));
            model.TestResultManager.GetResultForTest("3-1002").Returns(string.IsNullOrEmpty(resultState3) ? null : new ResultNode($"<test-case id='3-1002' result='{resultState3}' />"));

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testNode);

            // 3. Assert
            Assert.That(resultCounts.PassedCount, Is.EqualTo(expectedPassed));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(expectedFailed));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(expectedWarning));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(expectedInconclusive));

            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(0));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(0));
            Assert.That(resultCounts.TestCount, Is.EqualTo(3));
        }

        [Test]
        public void TestGroupNode_WithoutResult_ReturnsExpectedCount()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' />");
            TestGroup testGroup = new TestGroup("TestGroup") { testNode };
            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns((ResultNode)null);

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testGroup);

            // 3. Assert
            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(0));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(0));

            Assert.That(resultCounts.PassedCount, Is.EqualTo(0));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(0));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(0));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(1));
            Assert.That(resultCounts.TestCount, Is.EqualTo(1));
            Assert.That(resultCounts.AssertCount, Is.EqualTo(0));
            Assert.That(resultCounts.Duration, Is.EqualTo(0.0).Within(0.001));
        }

        [Test]
        public void TestGroupNode_WithoutChildNodes_ReturnsExpectedCount()
        {
            // 1. Arrange
            TestGroup testGroup = new TestGroup("TestGroup");
            ITestModel model = Substitute.For<ITestModel>();

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testGroup);

            // 3. Assert
            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(0));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(0));

            Assert.That(resultCounts.PassedCount, Is.EqualTo(0));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(0));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(0));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(0));
            Assert.That(resultCounts.TestCount, Is.EqualTo(0));
            Assert.That(resultCounts.AssertCount, Is.EqualTo(0));
            Assert.That(resultCounts.Duration, Is.EqualTo(0.0).Within(0.001));
        }

        [Test]
        public void TestGroupNode_WithOutcome_ReturnsExpectedCount()
        {
            // 1. Arrange
            TestNode testNode1 = new TestNode("<test-case id='1' />");
            ResultNode resultNode1 = new ResultNode($"<test-case id='1' result='Passed' asserts='2' duration='1.0'/>");

            TestNode testNode2 = new TestNode("<test-case id='2' />");
            ResultNode resultNode2 = new ResultNode($"<test-case id='2' result='Failed' asserts='3' duration='1.0'/>");

            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            model.TestResultManager.GetResultForTest("2").Returns(resultNode2);

            TestGroup testGroup = new TestGroup("TestGroup") { testNode1, testNode2 };

            // 2. Act
            TestResultCounts resultCounts = TestResultCounts.GetResultCounts(model, testGroup);

            // 3. Assert
            Assert.That(resultCounts.PassedCount, Is.EqualTo(1));
            Assert.That(resultCounts.FailedCount, Is.EqualTo(1));
            Assert.That(resultCounts.WarningCount, Is.EqualTo(0));
            Assert.That(resultCounts.InconclusiveCount, Is.EqualTo(0));

            Assert.That(resultCounts.IgnoreCount, Is.EqualTo(0));
            Assert.That(resultCounts.ExplicitCount, Is.EqualTo(0));
            Assert.That(resultCounts.NotRunCount, Is.EqualTo(0));
            Assert.That(resultCounts.TestCount, Is.EqualTo(2));

            Assert.That(resultCounts.AssertCount, Is.EqualTo(5));
            Assert.That(resultCounts.Duration, Is.EqualTo(2.0).Within(0.001));

        }
    }
}
