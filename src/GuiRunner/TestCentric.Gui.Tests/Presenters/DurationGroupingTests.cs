// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters
{
    using System.Collections.Generic;
    using System.Windows.Forms;
    using NSubstitute;
    using NUnit.Framework;
    using System.Linq;
    using Model;
    using Views;

    [TestFixture]
    public class DurationGroupingTests
    {
        [Test]
        public void Constructor_Groups_IsEmptyList()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            // 2. Act
            DurationGrouping grouping = new DurationGrouping(strategy);

            // 3. Assert
            Assert.That(grouping.Groups.Count, Is.EqualTo(0));
        }

        [Test]
        public void ID_Is_Duration()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            // 2. Act
            DurationGrouping grouping = new DurationGrouping(strategy);

            // 3. Assert
            Assert.That(grouping.ID, Is.EqualTo("DURATION"));
        }

        [Test]
        public void Load_DurationGroups_AreCreated()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);
            IEnumerable<TestNode> tests = new List<TestNode>();

            // 2. Act
            DurationGrouping grouping = new DurationGrouping(strategy);
            grouping.Load(tests);

            // 3. Assert
            Assert.That(grouping.Groups.Count, Is.EqualTo(4));
        }

        [Test]
        [TestCase("2.0", "Slow > 1 sec")]
        [TestCase("0.9", "Medium > 100 ms")]
        [TestCase("0.01", "Fast < 100 ms")]
        public void Load_ResultExistsForTestNode_TestNodeIsInsideGroup(string duration, string expectedGroupName)
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            var resultNode = new ResultNode($"<test-case id='1' duration='{duration}'/>");

            var testNode = new TestNode($"<test-case id='1' />");
            var tests = new List<TestNode> { testNode };

            model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            // 2. Act
            DurationGrouping grouping = new DurationGrouping(strategy);
            grouping.Load(tests);

            // 3. Assert
            var expectedGroup = grouping.Groups.FirstOrDefault(g => g.Name == expectedGroupName);
            Assert.That(expectedGroup, Contains.Item(testNode));
        }

        [Test]
        public void Load_NoResultExistsForTestNode_TestNodeIsInsideNotRunGroup()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            var testNode = new TestNode($"<test-case id='1' />");
            var tests = new List<TestNode> { testNode };

            model.TestResultManager.GetResultForTest("1").Returns((ResultNode)null);

            // 2. Act
            DurationGrouping grouping = new DurationGrouping(strategy);
            grouping.Load(tests);

            // 3. Assert
            var expectedGroup = grouping.Groups.FirstOrDefault(g => g.Name == "Not Run");
            Assert.That(expectedGroup, Contains.Item(testNode));
        }

        [Test]
        public void OnTestFinished_Calls_ApplyResultToGroup()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);
            ResultNode result = new ResultNode("<test-case id='1'/>");

            // 2. Act
            DurationGrouping grouping = new DurationGrouping(strategy);
            grouping.OnTestFinished(result);

            // 3. Assert
            strategy.Received().ApplyResultToGroup(result);
        }

        [Test]
        [TestCase("Passed", "Passed", "Passed", TestTreeView.SuccessIndex)]
        [TestCase("Passed", "Passed", "Failed", TestTreeView.FailureIndex)]
        [TestCase("Passed", "Failed", "Passed", TestTreeView.FailureIndex)]
        [TestCase("Failed", "Warning", "Passed", TestTreeView.FailureIndex)]
        [TestCase("Failed", "Failed", "Failed", TestTreeView.FailureIndex)]
        [TestCase("Warning", "Passed", "Passed", TestTreeView.WarningIndex)]
        [TestCase("Passed", "Passed", "Warning", TestTreeView.WarningIndex)]
        [TestCase("Passed", "Passed", "Inconclusive", TestTreeView.SuccessIndex)]
        [TestCase("Inconclusive", "Inconclusive", "Inconclusive", TestTreeView.InconclusiveIndex)]
        [TestCase("Skipped", "Skipped", "Skipped", -1)]
        [TestCase("Failed", "Skipped", "Skipped", TestTreeView.FailureIndex)]
        [TestCase("Skipped", "Passed", "Skipped", TestTreeView.SuccessIndex)]
        public void OnTestRunFinished(string testResult1, string testResult2, string testResult3, int expectedImageIndex)
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);
            IEnumerable<TestNode> tests = new List<TestNode>();
            ResultNode result = new ResultNode("<test-case id='1'/>");

            // Create grouping and initialize all groups
            DurationGrouping grouping = new DurationGrouping(strategy);
            grouping.Load(tests);

            foreach (TestGroup testGroup in grouping.Groups)
            {
                testGroup.TreeNode = new TreeNode();
            }

            // 2. Act
            // Only testing the first group
            TestGroup group = grouping.Groups.First();
            CreateTestAndResultInGroup(group, model, "1", testResult1);
            CreateTestAndResultInGroup(group, model, "2", testResult2);
            CreateTestAndResultInGroup(group, model, "3", testResult3);
            model.IsInTestRun(null).ReturnsForAnyArgs(true);

            grouping.OnTestRunFinished();

            // 3. Assert
            Assert.That(group.ImageIndex, Is.EqualTo(expectedImageIndex));
        }

        private void CreateTestAndResultInGroup(TestGroup group, ITestModel model, string testId, string testResult)
        {
            group.Add(new TestNode($"<test-case id='{testId}' />"));
            model.TestResultManager.GetResultForTest(testId).Returns(CreateResultNode(testId, testResult));
        }

        private ResultNode CreateResultNode(string id, string result)
        {
            return new ResultNode($"<test-case id='{id}' result='{result}'/>");
        }
    }
}
