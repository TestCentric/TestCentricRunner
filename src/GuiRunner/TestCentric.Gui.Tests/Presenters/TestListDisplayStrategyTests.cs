// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using NSubstitute;
    using NUnit.Framework;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Views;

    [TestFixture]
    internal class TestListDisplayStrategyTests
    {
        [Test]
        public void OnStrategyCreated_OutcomeFilter_IsInvisible()
        {
            // 1. Arrange
            ITestTreeView view = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();

            // 2. Act           
            TestListDisplayStrategy strategy = new TestListDisplayStrategy(view, model);

            // 3. Assert
            view.Received().SetTestFilterVisibility(false);
        }

        [Test]
        [TestCase("", "", "", 1, 3, 0, 0, 0)]
        [TestCase("Category_1", "", "", 2, 2, 1, 0, 0)]
        [TestCase("Category_1", "Category_1", "", 2, 1, 2, 0, 0)]
        [TestCase("Category_1", "", "Category_1", 2, 1, 2, 0, 0)]
        [TestCase("Category_1", "Category_1", "Category_1", 1, 0, 3, 0, 0)]
        [TestCase("Category_1", "", "Category_2", 3, 1, 1, 1, 0)]
        [TestCase("Category_1", "Category_2", "", 3, 1, 1, 1, 0)]
        [TestCase("Category_1", "Category_2", "Category_2", 2, 0, 1, 2, 0)]
        [TestCase("Category_1", "Category_2", "Category_3", 3, 0, 1, 1, 1)]
        public void OnTestLoaded_CategoryGrouping_AllGroupNodesAreCreated(string categoryTestcase1, string categoryTestcase2, string categoryTestcase3, int expectedNodes, int expectedInNonCategory, int expectedInCategory1, int expectedInCategory2, int expectedInCategory3)
        {
            // 1. Arrange
            ITestTreeView view = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();

            List<TreeNode> treeNodes = new List<TreeNode>();
            view.Add(Arg.Do<TreeNode>(x => treeNodes.Add(x)));

            model.Settings.Gui.TestTree.TestList.GroupBy.Returns("CATEGORY");

            TestNode testNode = new TestNode(
                "<test-suite type='TestSuite'> " +
                    "<test-suite type='TestFixture'>" +
                        CreateTestcaseXml("3-1000", categoryTestcase1) +
                        CreateTestcaseXml("3-1001", categoryTestcase2) +
                        CreateTestcaseXml("3-1002", categoryTestcase3) +
                    "</test-suite>" +
                "</test-suite>");

            // 2. Act           
            TestListDisplayStrategy strategy = new TestListDisplayStrategy(view, model);
            strategy.OnTestLoaded(testNode, null);

            // 3. Assert
            Assert.That(treeNodes.Count, Is.EqualTo(expectedNodes));
            AssertTreeNodeAndTestGroup(treeNodes, "None", expectedInNonCategory);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_1", expectedInCategory1);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_2", expectedInCategory2);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_3", expectedInCategory3);
        }

        [Test]
        [TestCase("Category_1", "", "", "", 1, 0, 3, 0, 0)]
        [TestCase("Category_1", "Category_1", "", "", 1, 0, 3, 0, 0)]
        [TestCase("Category_1", "Category_1", "Category_1", "Category_1", 1, 0, 3, 0, 0)]
        [TestCase("Category_1", "Category_2", "Category_2", "Category_2", 2, 0, 3, 3, 0)]
        [TestCase("Category_1", "Category_3", "Category_2", "Category_1", 3, 0, 3, 1, 1)]
        public void OnTestLoaded_CategoryGrouping_CategoryAtTestFixture_AllGroupNodesAreCreated(string categoryTestFixture, string categoryTestcase1, string categoryTestcase2, string categoryTestcase3, int expectedNodes, int expectedInNonCategory, int expectedInCategory1, int expectedInCategory2, int expectedInCategory3)
        {
            // 1. Arrange
            ITestTreeView view = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();

            List<TreeNode> treeNodes = new List<TreeNode>();
            view.Add(Arg.Do<TreeNode>(x => treeNodes.Add(x)));

            model.Settings.Gui.TestTree.TestList.GroupBy.Returns("CATEGORY");

            string xmlText = "<test-suite type='TestSuite'> " + $"<properties> <property name='Category' value='{categoryTestFixture}' /> </properties> " +
                                "<test-suite type='TestFixture'>" +
                                    CreateTestcaseXml("3-1000", categoryTestcase1) +
                                    CreateTestcaseXml("3-1001", categoryTestcase2) +
                                    CreateTestcaseXml("3-1002", categoryTestcase3) +
                                "</test-suite>" +
                            "</test-suite>";
            TestNode testNode = new TestNode(XmlHelper.CreateXmlNode(xmlText));
            
            // 2. Act           
            TestListDisplayStrategy strategy = new TestListDisplayStrategy(view, model);
            strategy.OnTestLoaded(testNode, null);

            // 3. Assert
            Assert.That(treeNodes.Count, Is.EqualTo(expectedNodes));
            AssertTreeNodeAndTestGroup(treeNodes, "None", expectedInNonCategory);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_1", expectedInCategory1);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_2", expectedInCategory2);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_3", expectedInCategory3);
        }

        [Test]
        public void OnTestLoaded_CategoryGrouping_MultipleCategoriesAtOneFixture_AllGroupNodesAreCreated()
        {
            // 1. Arrange
            ITestTreeView view = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();

            List<TreeNode> treeNodes = new List<TreeNode>();
            view.Add(Arg.Do<TreeNode>(x => treeNodes.Add(x)));

            model.Settings.Gui.TestTree.TestList.GroupBy.Returns("CATEGORY");

            TestNode testNode = new TestNode(
                        "<test-suite type='TestFixture' id='3-1000'> " +
                            "<properties> " +
                                "<property name='Category' value='Category_1' /> " +
                                "<property name='Category' value='Category_2' /> " +
                            "</properties> " +
                            CreateTestcaseXml("3-1000", "") +
                            CreateTestcaseXml("3-1001", "") +
                            CreateTestcaseXml("3-1002", "") +
                        "</test-suite>");

            // 2. Act           
            TestListDisplayStrategy strategy = new TestListDisplayStrategy(view, model);
            strategy.OnTestLoaded(testNode, null);

            // 3. Assert
            Assert.That(treeNodes.Count, Is.EqualTo(2));
            AssertTreeNodeAndTestGroup(treeNodes, "None", 0);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_1", 3);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_2", 3);
        }

        [Test]
        public void OnTestLoaded_CategoryGrouping_MultipleCategoriesAtOneTestcase_AllGroupNodesAreCreated()
        {
            // 1. Arrange
            ITestTreeView view = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();

            List<TreeNode> treeNodes = new List<TreeNode>();
            view.Add(Arg.Do<TreeNode>(x => treeNodes.Add(x)));

            model.Settings.Gui.TestTree.TestList.GroupBy.Returns("CATEGORY");

            TestNode testNode = new TestNode(
                "<test-suite type='TestFixture'> " +
                    "<test-case  id='3-1000'> " +
                        "<properties> " +
                            "<property name='Category' value='Category_1' /> " +
                            "<property name='Category' value='Category_2' /> " +
                        "</properties> " +
                    "</test-case>" +
                "</test-suite>");

            // 2. Act           
            TestListDisplayStrategy strategy = new TestListDisplayStrategy(view, model);
            strategy.OnTestLoaded(testNode, null);

            // 3. Assert
            Assert.That(treeNodes.Count, Is.EqualTo(2));
            AssertTreeNodeAndTestGroup(treeNodes, "None", 0);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_1", 1);
            AssertTreeNodeAndTestGroup(treeNodes, "Category_2", 1);
        }

        [Test]
        [TestCase("2.0", "2.0", "2.0", 1, 3, 0, 0, 0)]
        [TestCase("0.5", "2.0", "2.0", 2, 2, 1, 0, 0)]
        [TestCase("0.5", "0.5", "2.0", 2, 1, 2, 0, 0)]
        [TestCase("0.1", "0.5", "2.0", 3, 1, 1, 1, 0)]
        [TestCase("", "0.5", "", 2, 0, 1, 0, 2)]
        [TestCase("", "0.1", "4", 3, 1, 0, 1, 1)]
        [TestCase("", "", "", 1, 0, 0, 0, 3)]
        public void OnTestLoaded_DurationGrouping_AllGroupNodesAreCreated(string duration1, string duration2, string duration3, int expectedNodes, int expectedInSlow, int expectedInMedium, int expectedInFast, int expectedNotRun)
        {
            // 1. Arrange
            ITestTreeView view = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();

            List<TreeNode> treeNodes = new List<TreeNode>();
            view.Add(Arg.Do<TreeNode>(x => treeNodes.Add(x)));

            model.Settings.Gui.TestTree.TestList.GroupBy.Returns("DURATION");

            TestNode testNode = new TestNode(
                "<test-suite type='TestSuite'> " +
                    "<test-suite type='TestFixture'>" +
                        CreateTestcaseXml("3-1000", "") +
                        CreateTestcaseXml("3-1001", "") +
                        CreateTestcaseXml("3-1002", "") +
                    "</test-suite>" +
                "</test-suite>");

            model.TestResultManager.GetResultForTest("3-1000").Returns(string.IsNullOrEmpty(duration1) ? null : new ResultNode($"<test-case id='3-1000' duration='{duration1}' />"));
            model.TestResultManager.GetResultForTest("3-1001").Returns(string.IsNullOrEmpty(duration2) ? null : new ResultNode($"<test-case id='3-1001' duration='{duration2}' />"));
            model.TestResultManager.GetResultForTest("3-1002").Returns(string.IsNullOrEmpty(duration3) ? null : new ResultNode($"<test-case id='3-1002' duration='{duration3}' />"));

            // 2. Act           
            TestListDisplayStrategy strategy = new TestListDisplayStrategy(view, model);
            strategy.OnTestLoaded(testNode, null);

            // 3. Assert
            Assert.That(treeNodes.Count, Is.EqualTo(expectedNodes));
            AssertTreeNodeAndTestGroup(treeNodes, "Slow > 1 sec", expectedInSlow);
            AssertTreeNodeAndTestGroup(treeNodes, "Medium > 100 ms", expectedInMedium);
            AssertTreeNodeAndTestGroup(treeNodes, "Fast < 100 ms", expectedInFast);
            AssertTreeNodeAndTestGroup(treeNodes, "Not Run", expectedNotRun);
        }

        private static object[] TestCaseSourceOutcomeGrouping =
    {
                new object[] { "Passed", "Passed", "Passed", 1, new Dictionary<string, int>() { {"Passed", 3} } },
                new object[] { "Passed", "Failed", "Passed", 2, new Dictionary<string, int>() { {"Passed", 2}, { "Failed", 1 } } },
                new object[] { "Passed", "Failed", "Skipped", 3, new Dictionary<string, int>() { {"Passed", 1}, { "Failed", 1 }, { "Skipped", 1 } } },
                new object[] { "Inconclusive", "", "", 2, new Dictionary<string, int>() { { "Inconclusive", 1}, { "Not Run", 2 }, { "Passed", 0 } } },
                new object[] { "", "", "", 1, new Dictionary<string, int>() { { "Passed", 0}, { "Failed", 0 }, { "Not Run", 3 } } },
            };

        [Test]
        [TestCaseSource(nameof(TestCaseSourceOutcomeGrouping))]
        public void OnTestLoaded_OutcomeGrouping_AllGroupNodesAreCreated(string resultState1, string resultState2, string resultState3, int expectedNodes, Dictionary<string, int> expectedInGroup)
        {
            // 1. Arrange
            ITestTreeView view = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();

            List<TreeNode> treeNodes = new List<TreeNode>();
            view.Add(Arg.Do<TreeNode>(x => treeNodes.Add(x)));

            model.Settings.Gui.TestTree.TestList.GroupBy.Returns("OUTCOME");

            TestNode testNode = new TestNode(
                "<test-suite type='TestSuite'> " +
                    "<test-suite type='TestFixture'>" +
                        CreateTestcaseXml("3-1000", "") +
                        CreateTestcaseXml("3-1001", "") +
                        CreateTestcaseXml("3-1002", "") +
                    "</test-suite>" +
                "</test-suite>");

            model.TestResultManager.GetResultForTest("3-1000").Returns(string.IsNullOrEmpty(resultState1) ? null : new ResultNode($"<test-case id='3-1000' result='{resultState1}' />"));
            model.TestResultManager.GetResultForTest("3-1001").Returns(string.IsNullOrEmpty(resultState2) ? null : new ResultNode($"<test-case id='3-1001' result='{resultState2}' />"));
            model.TestResultManager.GetResultForTest("3-1002").Returns(string.IsNullOrEmpty(resultState3) ? null : new ResultNode($"<test-case id='3-1002' result='{resultState3}' />"));

            // 2. Act           
            TestListDisplayStrategy strategy = new TestListDisplayStrategy(view, model);
            strategy.OnTestLoaded(testNode, null);

            // 3. Assert
            Assert.That(treeNodes.Count, Is.EqualTo(expectedNodes));

            foreach (var exp in expectedInGroup)
            {
                AssertTreeNodeAndTestGroup(treeNodes, exp.Key, exp.Value);
            }
        }

        private static void AssertTreeNodeAndTestGroup(List<TreeNode> treeNodes, string testGroupName, int expectedInGroup)
        {
            TreeNode treeNode = treeNodes.Find(x => (x.Tag as TestGroup)?.Name == testGroupName);
            if (expectedInGroup == 0)
            {
                Assert.That(treeNode, Is.Null, $"TreeNode {testGroupName} exists in tree");
                return;
            }

            Assert.That(treeNode, Is.Not.Null, $"Failed to find node {testGroupName} in tree");

            // Assert treeNodes
            Assert.That(treeNode.Nodes.Count, Is.EqualTo(expectedInGroup));
            Assert.That(treeNode.Text, Does.StartWith(testGroupName));

            // Assert testGroup
            TestGroup testGroup = treeNode.Tag as TestGroup;
            Assert.That(testGroup, Is.Not.Null);
            Assert.That(testGroup.Count(), Is.EqualTo(expectedInGroup));
        }

        private string CreateTestcaseXml(string testId, string category)
        {
            string str = $"<test-case id='{testId}'> ";
            if (!string.IsNullOrEmpty(category))
                str += $"<properties> <property name='Category' value='{category}' /> </properties> ";

            str += "</test-case>";

            return str;
        }
    }
}
