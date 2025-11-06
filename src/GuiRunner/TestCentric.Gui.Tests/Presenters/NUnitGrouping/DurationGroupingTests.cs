// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;
using TestCentric.Gui.Model.Settings;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    [TestFixture]
    internal class DurationGroupingTests
    {
        private ITestModel _model;
        private INUnitTreeDisplayStrategy _strategy;
        private ITestTreeView _view;
        private TreeNodeCollection _createNodes;

        [SetUp]
        public void Setup()
        {
            _model = Substitute.For<ITestModel>();
            _strategy = Substitute.For<INUnitTreeDisplayStrategy>();
            _view = Substitute.For<ITestTreeView>();
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            IUserSettings userSettings = Substitute.For<IUserSettings>();
            userSettings.Gui.TestTree.DisplayFormat.Returns("NUNIT_TREE");
            userSettings.Gui.TestTree.ShowNamespace.Returns(true);
            _model.Settings.Returns(userSettings);

            var treeView = new TreeView();
            _view.TreeView.Returns(treeView);

            _strategy.ShowTreeNodeType(null).ReturnsForAnyArgs(true);
            _strategy.RemoveTreeNode(Arg.Do<TreeNode>(t => t.Remove()));

            // We can't construct a TreeNodeCollection, so we need to fake it
            _createNodes = new TreeNode().Nodes;
            _strategy.MakeTreeNode(Arg.Any<TestNode>()).ReturnsForAnyArgs(x => new TreeNode(x.ArgAt<TestNode>(0).Name) { Tag = x.ArgAt<TestNode>(0) });
            _strategy.MakeTreeNode(Arg.Any<TestGroup>()).ReturnsForAnyArgs(x => {
                var testGroup = x.ArgAt<TestGroup>(0);
                var treeNode = new TreeNode(testGroup.Name) { Tag = testGroup };
                testGroup.TreeNode = treeNode;
                return treeNode;
            });
            _view.When(t => t.Add(Arg.Any<TreeNode>())).Do(x => _createNodes.Add(x.ArgAt<TreeNode>(0)));
            _view.Nodes.Returns(x => _createNodes);
        }

        [TestCase("0.05", "Fast")]
        [TestCase("0.2", "Medium")]
        [TestCase("1.0", "Slow")]
        [TestCase("", "Not run")]
        public void CreateTree_AllTestsWithSameOutcome_AllTreeNodes_AreCreated(string duration, string expectedGroupName)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", duration,
                    CreateTestSuiteXml("3-1001", "NamespaceA", duration,
                        CreateTestFixtureXml("3-1010", "Fixture_1", duration,
                            CreateTestcaseXml("3-1011", "TestA", duration),
                            CreateTestcaseXml("3-1012", "TestB", duration))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", duration,
                        CreateTestFixtureXml("3-2010", "Fixture_2", duration,
                            CreateTestcaseXml("3-2011", "TestA", duration),
                            CreateTestcaseXml("3-2012", "TestB", duration)))));

            // Act
            var grouping = new DurationGrouping(_strategy, _model, _view);
            grouping.CreateTree(testNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(1));
            AssertTreeNodeGroup(_createNodes[0], expectedGroupName, 4, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 4, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[1], "TestB");
        }

        [Test]
        public void CreateTree_TestsWithDifferentDurations_AllTreeNodes_AreCreated()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "2.0",
                    CreateTestSuiteXml("3-1001", "NamespaceA", "1.5",
                        CreateTestFixtureXml("3-1010", "Fixture_1", "1.5",
                            CreateTestcaseXml("3-1011", "TestA", "0.7"),
                            CreateTestcaseXml("3-1012", "TestB", "0.8"))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", "0.5",
                        CreateTestFixtureXml("3-2010", "Fixture_2", "0.5",
                            CreateTestcaseXml("3-2011", "TestA", "0.25"),
                            CreateTestcaseXml("3-2012", "TestB", "0.25")))));

            // Act
            var grouping = new DurationGrouping(_strategy, _model, _view);
            grouping.CreateTree(testNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "Slow", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Medium", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
        }

        [Test]
        public void CreateTree_TestsWithDifferentDurations_AllTestGroups_HaveDuration()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "2.0",
                    CreateTestSuiteXml("3-1001", "NamespaceA", "1.5",
                        CreateTestFixtureXml("3-1010", "Fixture_1", "1.5",
                            CreateTestcaseXml("3-1011", "TestA", "0.7"),
                            CreateTestcaseXml("3-1012", "TestB", "0.8"))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", "0.5",
                        CreateTestFixtureXml("3-2010", "Fixture_2", "0.5",
                            CreateTestcaseXml("3-2011", "TestA", "0.25"),
                            CreateTestcaseXml("3-2012", "TestB", "0.25")))));

            // Act
            var grouping = new DurationGrouping(_strategy, _model, _view);
            grouping.CreateTree(testNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTestGroupDuration(_createNodes[0], 1.5);
            AssertTestGroupDuration(_createNodes[0].Nodes[0], 1.5);
            AssertTestGroupDuration(_createNodes[0].Nodes[0].Nodes[0], 1.5);
            AssertTestGroupDuration(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], 1.5);

            AssertTestGroupDuration(_createNodes[1], 0.5);
            AssertTestGroupDuration(_createNodes[1].Nodes[0], 0.5);
            AssertTestGroupDuration(_createNodes[1].Nodes[0].Nodes[0], 0.5);
            AssertTestGroupDuration(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], 0.5);
        }

        [Test]
        public void CreateTree_TestsWithDifferentDurations2_AllTreeNodes_AreCreated()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "0.2",
                    CreateTestSuiteXml("3-1001", "NamespaceA", "0.2",
                        CreateTestFixtureXml("3-1010", "Fixture_1", "0.2",
                            CreateTestcaseXml("3-1011", "TestA", "0.0"),
                            CreateTestcaseXml("3-1012", "TestB", "0.2"))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", "0.2",
                        CreateTestFixtureXml("3-2010", "Fixture_2", "0.2",
                            CreateTestcaseXml("3-2011", "TestA", "0.0"),
                            CreateTestcaseXml("3-2012", "TestB", "0.2")))));

            // Act
            var grouping = new DurationGrouping(_strategy, _model, _view);
            grouping.CreateTree(testNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "Fast", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 2, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[1], "Medium", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 2, 2);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestB");
        }

        [Test]
        public void OnTestFinished_Regroups_TreeNodes()
        {
            // Arrange
            TestNode rootTestNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "",
                    CreateTestSuiteXml("3-1001", "NamespaceA", "",
                        CreateTestFixtureXml("3-1010", "Fixture_1", "",
                            CreateTestcaseXml("3-1011", "TestA", ""),
                            CreateTestcaseXml("3-1012", "TestB", ""))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", "",
                        CreateTestFixtureXml("3-2010", "Fixture_2", "",
                            CreateTestcaseXml("3-2011", "TestA", ""),
                            CreateTestcaseXml("3-2012", "TestB", "")))));


            // Create initial tree with all nodes in group 'Not run'
            var grouping = new DurationGrouping(_strategy, _model, _view);
            grouping.CreateTree(rootTestNode);

            ResultNode resultNode = CreateAndPrepareResultNode("3-1011", "0.05");

            // Act
            grouping.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "Not run", 3, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 3, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[1], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Fast", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
        }

        [Test]
        public void OnTestFinished_MultipleTimes_Regroups_TreeNodes()
        {
            // Arrange
            TestNode rootTestNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "",
                    CreateTestSuiteXml("3-1001", "NamespaceA", "",
                        CreateTestFixtureXml("3-1010", "Fixture_1", "",
                            CreateTestcaseXml("3-1011", "TestA", ""),
                            CreateTestcaseXml("3-1012", "TestB", ""))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", "",
                        CreateTestFixtureXml("3-2010", "Fixture_2", "",
                            CreateTestcaseXml("3-2011", "TestA", ""),
                            CreateTestcaseXml("3-2012", "TestB", "")))));


            // Create initial tree with all nodes in group 'Not run'
            var grouping = new DurationGrouping(_strategy, _model, _view);
            grouping.CreateTree(rootTestNode);

            // Act
            ResultNode resultNode = CreateAndPrepareResultNode("3-1011", "0.2");
            grouping.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "Not run", 3, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 3, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[1], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Medium", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            // Act
            resultNode = CreateAndPrepareResultNode("3-1012", "1.2");
            grouping.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(3));
            AssertTreeNodeGroup(_createNodes[0], "Not run", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Medium", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[2], "Slow", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");

            // Act
            resultNode = CreateAndPrepareResultNode("3-2011", "0.05");
            grouping.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(4));
            AssertTreeNodeGroup(_createNodes[0], "Not run", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Medium", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[2], "Slow", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");

            AssertTreeNodeGroup(_createNodes[3], "Fast", 1, 1);
            AssertTreeNodeGroup(_createNodes[3].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[3].Nodes[0].Nodes[0], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[3].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[3].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            // Act
            resultNode = CreateAndPrepareResultNode("3-2012", "0.05");
            grouping.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(3));
            AssertTreeNodeGroup(_createNodes[0], "Medium", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[1], "Slow", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");

            AssertTreeNodeGroup(_createNodes[2], "Fast", 2, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0], "LibraryA", 2, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
        }

        private ResultNode CreateAndPrepareResultNode(string nodeId, string duration)
        {
            TreeNode treeNode = GetCreatedTreeNode(_createNodes, nodeId);
            TestNode testNode = treeNode.Tag as TestNode;
            ResultNode resultNode = new ResultNode($"<test-case id='{nodeId}' duration='{duration}'/>");
            _model.GetTestById(nodeId).Returns(testNode);
            _model.TestResultManager.GetResultForTest(nodeId).Returns(resultNode);
            _strategy.GetTreeNodesForTest(treeNode.Tag as TestNode).ReturnsForAnyArgs(new List<TreeNode>() { treeNode });
            return resultNode;
        }

        private TreeNode GetCreatedTreeNode(TreeNodeCollection treeNodes, string nodeId)
        {
            foreach (TreeNode treeNode in treeNodes)
            {
                if (treeNode.Tag is TestNode testNode && testNode.Id == nodeId)
                    return treeNode;

                TreeNode n = GetCreatedTreeNode(treeNode.Nodes, nodeId);
                if (n != null)
                    return n;
            }


            return null;
        }

        private void AssertTestCase(TreeNode treeNode, string expectedName)
        {
            Assert.That(treeNode.Nodes.Count, Is.EqualTo(0));

            TestNode testNode = treeNode.Tag as TestNode;
            Assert.That(testNode.Name, Is.EqualTo(expectedName));
        }

        private void AssertTreeNodeGroup(TreeNode treeNode, string expectedGroupName, int expectedGroupCount, int expectedTreeNodeChildren)
        {
            Assert.That(treeNode.Nodes.Count, Is.EqualTo(expectedTreeNodeChildren));
            var group = treeNode.Tag as TestGroup;
            Assert.That(group.Name, Is.EqualTo(expectedGroupName));
            Assert.That(group.Count, Is.EqualTo(expectedGroupCount));
        }

        private void AssertTestGroupDuration(TreeNode treeNode, double expectedDuration)
        {
            var group = treeNode.Tag as TestGroup;
            Assert.That(group.Duration.Value, Is.EqualTo(expectedDuration).Within(0.01));
        }

        private string CreateTestcaseXml(string testId, string testName, string duration)
        {
            return CreateTestcaseXml(testId, testName, duration, new List<string>());
        }

        private string CreateTestcaseXml(string testId, string testName, string duration, IList<string> categories)
        {
            string str = $"<test-case id='{testId}' name='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            str += "</test-case> ";

            if (!string.IsNullOrEmpty(duration))
                _model.TestResultManager.GetResultForTest(testId).Returns(new ResultNode($"<test-case id='{testId}' duration='{duration}' />"));
            return str;
        }

        private string CreateTestFixtureXml(string testId, string testName, string outcome, params string[] testCases)
        {
            return CreateTestFixtureXml(testId, testName, outcome, new List<string>(), testCases);
        }

        private string CreateTestFixtureXml(string testId, string testName, string outcome, IEnumerable<string> categories, params string[] testCases)
        {
            string str = $"<test-suite type='TestFixture' id='{testId}'  name='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            foreach (string testCase in testCases)
                str += testCase;

            str += "</test-suite>";

            if (!string.IsNullOrEmpty(outcome))
                _model.TestResultManager.GetResultForTest(testId).Returns(new ResultNode($"<test-case id='{testId}' result='{outcome}' />"));

            return str;
        }

        private string CreateTestSuiteXml(string testId, string testName, string outcome, params string[] testSuites)
        {
            string str = $"<test-suite type='TestSuite' id='{testId}' name='{testName}'> ";
            foreach (string testSuite in testSuites)
                str += testSuite;

            str += "</test-suite>";

            if (!string.IsNullOrEmpty(outcome))
                _model.TestResultManager.GetResultForTest(testId).Returns(new ResultNode($"<test-case id='{testId}' result='{outcome}' />"));

            return str;
        }
    }
}
