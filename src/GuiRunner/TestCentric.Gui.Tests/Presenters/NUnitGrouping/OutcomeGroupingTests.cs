// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
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
    internal class OutcomeGroupingTests
    {
        private ITestModel _model;
        private ITestTreeView _view;
        private TreeNodeCollection _createNodes;

        [SetUp]
        public void Setup()
        {
            _model = Substitute.For<ITestModel>();
            _view = Substitute.For<ITestTreeView>();
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            IUserSettings userSettings = Substitute.For<IUserSettings>();
            _model.TreeConfiguration.DisplayFormat = "NUNIT_TREE";
            _model.TreeConfiguration.NUnitGroupBy = "OUTCOME";
            _model.TreeConfiguration.ShowNamespaces = true;
            _model.Settings.Returns(userSettings);

            var treeView = new TreeView();
            _view.TreeView.Returns(treeView);

            // We can't construct a TreeNodeCollection, so we need to fake it
            _createNodes = new TreeNode().Nodes;
            _view.When(t => t.Add(Arg.Any<TreeNode>())).Do(x => _createNodes.Add(x.ArgAt<TreeNode>(0)));
            _view.Nodes.Returns(x => _createNodes);
        }


        [TestCase("Passed", "Passed")]
        [TestCase("", "Not run")]
        [TestCase("Failed", "Failed")]
        public void CreateTree_AllTestsWithSameOutcome_AllTreeNodes_AreCreated(string outcome, string expectedGroupName)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", outcome,
                    CreateTestSuiteXml("3-1001", "NamespaceA", outcome,
                        CreateTestFixtureXml("3-1010", "Fixture_1", outcome,
                            CreateTestcaseXml("3-1011", "TestA", outcome),
                            CreateTestcaseXml("3-1012", "TestB", outcome))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", outcome,
                        CreateTestFixtureXml("3-2010", "Fixture_2", outcome,
                            CreateTestcaseXml("3-2011", "TestA", outcome),
                            CreateTestcaseXml("3-2012", "TestB", outcome)))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

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
        public void CreateTree_TestsWithDifferentOutcome_AllTreeNodes_AreCreated()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "Failed",
                    CreateTestSuiteXml("3-1001", "NamespaceA", "Failed",
                        CreateTestFixtureXml("3-1010", "Fixture_1", "Failed",
                            CreateTestcaseXml("3-1011", "TestA", "Passed"),
                            CreateTestcaseXml("3-1012", "TestB", "Failed"))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", "Passed",
                        CreateTestFixtureXml("3-2010", "Fixture_2", "Passed",
                            CreateTestcaseXml("3-2011", "TestA", "Passed"),
                            CreateTestcaseXml("3-2012", "TestB", "Passed")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "Passed", 3, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 3, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[1], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Failed", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
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
            _model.LoadedTests.Returns(rootTestNode);

            // Create initial tree with all nodes in group 'Not run'
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(rootTestNode, null);

            TreeNode treeNode = GetCreatedTreeNode(_createNodes, "3-1011");
            TestNode testNode = treeNode.Tag as TestNode;
            ResultNode resultNode = new ResultNode($"<test-case id='3-1011' result='Passed'/>");
            _model.GetTestById("3-1011").Returns(testNode);
            _model.TestResultManager.GetResultForTest("3-1011").Returns(resultNode);

            // Act
            strategy.OnTestFinishedWithoutRegroupTimer(resultNode);

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

            AssertTreeNodeGroup(_createNodes[1], "Passed", 1, 1);
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
            _model.LoadedTests.Returns(rootTestNode);

            // Create initial tree with all nodes in group 'Not run'
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(rootTestNode, null);

            // Act
            ResultNode resultNode = CreateAndPrepareResultNode("3-1011", "Failed");
            strategy.OnTestFinishedWithoutRegroupTimer(resultNode);

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

            AssertTreeNodeGroup(_createNodes[1], "Failed", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            // Act
            resultNode = CreateAndPrepareResultNode("3-1012", "Passed");
            strategy.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(3));
            AssertTreeNodeGroup(_createNodes[0], "Not run", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Failed", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[2], "Passed", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");

            // Act
            resultNode = CreateAndPrepareResultNode("3-2011", "Passed");
            strategy.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(3));
            AssertTreeNodeGroup(_createNodes[0], "Not run", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Failed", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[2], "Passed", 2, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0], "LibraryA", 2, 2);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");

            // Act
            resultNode = CreateAndPrepareResultNode("3-2012", "Failed");
            strategy.OnTestFinishedWithoutRegroupTimer(resultNode);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "Failed", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 2, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "Passed", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 2, 2);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
        }

        [Test]
        public void CreateTree_AllNodesInSameOutcome_NamespaceNodes_AreFolded()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "",
                    CreateTestSuiteXml("3-1001", "Test", "",
                        CreateTestSuiteXml("3-2001", "NamespaceA", "",
                            CreateTestSuiteXml("3-2005", "FolderA", "",
                                CreateTestFixtureXml("3-2010", "Fixture_1", "",
                                    CreateTestcaseXml("3-2011", "TestA", "Passed"),
                                    CreateTestcaseXml("3-2012", "TestB", "Passed")))),
                        CreateTestSuiteXml("3-3001", "NamespaceB", "",
                            CreateTestSuiteXml("3-3005", "Folder1", "",
                                CreateTestFixtureXml("3-3010", "Fixture_2", "",
                                    CreateTestcaseXml("3-3011", "TestA", "Passed"))),
                            CreateTestSuiteXml("3-3020", "Folder2", "",
                                CreateTestSuiteXml("3-3025", "Folder3", "",
                                    CreateTestFixtureXml("3-3026", "Fixture_3", "",
                                        CreateTestcaseXml("3-3027", "TestB", "Passed"))))))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(1));
            AssertTreeNodeGroup(_createNodes[0], "Passed", 4, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA.Test", 4, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA.FolderA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 2, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Folder1", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[1], "Folder2.Folder3", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[1].Nodes[0], "Fixture_3", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[1].Nodes[0].Nodes[0], "TestB");
        }

        [Test]
        public void CreateTree_AllNodesInDifferentOutcomes_NamespaceNodes_AreFolded()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "",
                    CreateTestSuiteXml("3-1001", "Test", "",
                        CreateTestSuiteXml("3-2001", "NamespaceA", "",
                            CreateTestSuiteXml("3-2005", "FolderA", "",
                                CreateTestFixtureXml("3-2010", "Fixture_1", "",
                                    CreateTestcaseXml("3-2011", "TestA", "Passed"),
                                    CreateTestcaseXml("3-2012", "TestB", "Failed")))),
                        CreateTestSuiteXml("3-3001", "NamespaceB", "",
                            CreateTestSuiteXml("3-3005", "Folder1", "",
                                CreateTestFixtureXml("3-3010", "Fixture_2", "",
                                    CreateTestcaseXml("3-3011", "TestA", "Passed"))),
                            CreateTestSuiteXml("3-3020", "Folder2", "",
                                CreateTestSuiteXml("3-3025", "Folder3", "",
                                    CreateTestFixtureXml("3-3026", "Fixture_3", "",
                                        CreateTestcaseXml("3-3027", "TestB", "Failed"))))))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "Passed", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA.Test", 2, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA.FolderA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Folder1", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[1], "Failed", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA.Test", 2, 2);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA.FolderA",1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1].Nodes[0], "Folder2.Folder3", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "Fixture_3", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[0].Nodes[0], "TestB");
        }

        private ResultNode CreateAndPrepareResultNode(string nodeId, string outcome)
        {
            TreeNode treeNode = GetCreatedTreeNode(_createNodes, nodeId);
            TestNode testNode = treeNode.Tag as TestNode;
            ResultNode resultNode = new ResultNode($"<test-case id='{nodeId}' result='{outcome}'/>");
            _model.GetTestById(nodeId).Returns(testNode);
            _model.TestResultManager.GetResultForTest(nodeId).Returns(resultNode);
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

        private string CreateTestcaseXml(string testId, string testName, string outcome)
        {
            return CreateTestcaseXml(testId, testName, outcome, new List<string>());
        }

        private string CreateTestcaseXml(string testId, string testName, string outcome, IList<string> categories)
        {
            string str = $"<test-case id='{testId}' name='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            str += "</test-case> ";

            if (!string.IsNullOrEmpty(outcome))
                _model.TestResultManager.GetResultForTest(testId).Returns(new ResultNode($"<test-case id='{testId}' result='{outcome}' />"));
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
