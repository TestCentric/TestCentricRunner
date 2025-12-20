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
    internal class CategoryGroupingTests
    {
        private ITestModel _model;
        private ITestTreeView _view;
        private TreeNodeCollection _createNodes;

        [SetUp]
        public void Setup()
        {
            _model = Substitute.For<ITestModel>();
            IUserSettings userSettings = Substitute.For<IUserSettings>();
            userSettings.Gui.TestTree.DisplayFormat.Returns("NUNIT_TREE");
            userSettings.Gui.TestTree.NUnitGroupBy.Returns("CATEGORY");
            userSettings.Gui.TestTree.ShowNamespace.Returns(true);
            _model.Settings.Returns(userSettings);
            _view = Substitute.For<ITestTreeView>();

            var treeView = new TreeView();
            _view.TreeView.Returns(treeView);

            // We can't construct a TreeNodeCollection, so we need to fake it
            _createNodes = new TreeNode().Nodes;
            _view.When(t => t.Add(Arg.Any<TreeNode>())).Do(x => _createNodes.Add(x.ArgAt<TreeNode>(0)));
            _view.Nodes.Returns(x => _createNodes);
        }

        [TestCase("CategoryA", "CategoryA")]
        [TestCase("", "None")]
        public void CreateTree_AllTestsWithSameCategory_OnFixtureLevel_AllTreeNodes_AreCreated(string category, string expectedGroupName)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "NamespaceA",
                        CreateTestFixtureXml("3-1010", "Fixture_1", new List<string>() { category },
                            CreateTestcaseXml("3-1011", "TestA", ""),
                            CreateTestcaseXml("3-1012", "TestB", ""))),
                    CreateTestSuiteXml("3-2001", "NamespaceB",
                        CreateTestFixtureXml("3-2010", "Fixture_2", new List<string>() { category },
                            CreateTestcaseXml("3-2011", "TestA", ""),
                            CreateTestcaseXml("3-2012", "TestB", "")))));

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

        [TestCase("CategoryA", "CategoryA")]
        [TestCase("", "None")]
        public void CreateTree_AllTestsWithSameCategory_OnTestCaseLevel_AllTreeNodes_AreCreated(string category, string expectedGroupName)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "NamespaceA",
                        CreateTestFixtureXml("3-1010", "Fixture_1", new List<string>(),
                            CreateTestcaseXml("3-1011", "TestA", category),
                            CreateTestcaseXml("3-1012", "TestB", category))),
                    CreateTestSuiteXml("3-2001", "NamespaceB", 
                        CreateTestFixtureXml("3-2010", "Fixture_2", new List<string>(),
                            CreateTestcaseXml("3-2011", "TestA", category),
                            CreateTestcaseXml("3-2012", "TestB", category)))));

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
        public void CreateTree_MultipleCategories_OnFixtureLevel_AllTreeNodes_AreCreated()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "NamespaceA",
                        CreateTestFixtureXml("3-1010", "Fixture_1", new List<string>() { "CategoryA", "CategoryB" },
                            CreateTestcaseXml("3-1011", "TestA", ""),
                            CreateTestcaseXml("3-1012", "TestB", ""))),
                    CreateTestSuiteXml("3-2001", "NamespaceB",
                        CreateTestFixtureXml("3-2010", "Fixture_2", new List<string>() { "CategoryA", "CategoryB" },
                            CreateTestcaseXml("3-2011", "TestA", ""),
                            CreateTestcaseXml("3-2012", "TestB", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "CategoryA", 4, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 4, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[1], "TestB");

            AssertTreeNodeGroup(_createNodes[1], "CategoryB", 4, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 4, 2);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 2, 2);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1], "NamespaceB", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[1], "TestB");
        }

        [Test]
        public void CreateTree_MultipleCategories_OnTestCaseLevel_AllTreeNodes_AreCreated()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "NamespaceA",
                        CreateTestFixtureXml("3-1010", "Fixture_1", new List<string>(),
                            CreateTestcaseXml("3-1011", "TestA", new List<string>() { "CategoryA"}),
                            CreateTestcaseXml("3-1012", "TestB", new List<string>() { "CategoryB" }))),
                    CreateTestSuiteXml("3-2001", "NamespaceB",
                        CreateTestFixtureXml("3-2010", "Fixture_2", new List<string>(),
                            CreateTestcaseXml("3-2011", "TestA", new List<string>() { "CategoryA", "CategoryB" }),
                            CreateTestcaseXml("3-2012", "TestB", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(3));
            AssertTreeNodeGroup(_createNodes[0], "CategoryA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA", 2, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[1], "CategoryB", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA", 2, 2);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[2], "None", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0], "LibraryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0], "NamespaceB", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
        }

        [Test]
        public void CreateTree_AllNodesInSameCategory_NamespaceNodes_AreFolded()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "Test",
                        CreateTestSuiteXml("3-2001", "NamespaceA",
                            CreateTestSuiteXml("3-2005", "Folder_1",
                                CreateTestFixtureXml("3-2010", "Fixture_1", new List<string>() { "CategoryA" },
                                    CreateTestcaseXml("3-2011", "TestA", ""),
                                    CreateTestcaseXml("3-2012", "TestB", "")))),
                        CreateTestSuiteXml("3-3001", "NamespaceB",
                            CreateTestSuiteXml("3-3005", "Folder_1",
                                CreateTestFixtureXml("3-3010", "Fixture_2", new List<string>() { "CategoryA" },
                                    CreateTestcaseXml("3-3011", "TestA", ""),
                                    CreateTestcaseXml("3-3012", "TestB", "")))))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(1));
            AssertTreeNodeGroup(_createNodes[0], "CategoryA", 4, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA.Test", 4, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA.Folder_1", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB.Folder_1", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[1], "TestB");
        }

        [Test]
        public void CreateTree_AllNodesInDifferentCategories_NamespaceNodes_AreFolded()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "Test",
                        CreateTestSuiteXml("3-2001", "NamespaceA",
                            CreateTestSuiteXml("3-2005", "Folder_1",
                                CreateTestFixtureXml("3-2010", "Fixture_1", new List<string>(),
                                    CreateTestcaseXml("3-2011", "TestA", "CategoryA"),
                                    CreateTestcaseXml("3-2012", "TestB", "CategoryB")))),
                        CreateTestSuiteXml("3-3001", "NamespaceB",
                            CreateTestSuiteXml("3-3005", "Folder_1",
                                CreateTestFixtureXml("3-3010", "Fixture_2", new List<string>(),
                                    CreateTestcaseXml("3-3011", "TestA", "CategoryC"),
                                    CreateTestcaseXml("3-3012", "TestB", "CategoryD")))))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(4));
            AssertTreeNodeGroup(_createNodes[0], "CategoryA", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA.Test", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[1], "CategoryB", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA.Test", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");

            AssertTreeNodeGroup(_createNodes[2], "CategoryC", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0], "LibraryA.Test", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0], "NamespaceB.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[2].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[2].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[3], "CategoryD", 1, 1);
            AssertTreeNodeGroup(_createNodes[3].Nodes[0], "LibraryA.Test", 1, 1);
            AssertTreeNodeGroup(_createNodes[3].Nodes[0].Nodes[0], "NamespaceB.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[3].Nodes[0].Nodes[0].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[3].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
        }

        [Test]
        public void CreateTree_SomeNodesShareCategories_NamespaceNodes_AreFolded()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "Test",
                        CreateTestSuiteXml("3-2001", "NamespaceA",
                            CreateTestSuiteXml("3-2005", "Folder_1",
                                CreateTestFixtureXml("3-2010", "Fixture_1", new List<string>(),
                                    CreateTestcaseXml("3-2011", "TestA", "CategoryA"),
                                    CreateTestcaseXml("3-2012", "TestB", "CategoryB")))),
                        CreateTestSuiteXml("3-3001", "NamespaceB",
                            CreateTestSuiteXml("3-3005", "Folder_1",
                                CreateTestFixtureXml("3-3010", "Fixture_2", new List<string>(),
                                    CreateTestcaseXml("3-3011", "TestA", "CategoryA"),
                                    CreateTestcaseXml("3-3012", "TestB", "CategoryB")))))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(2));
            AssertTreeNodeGroup(_createNodes[0], "CategoryA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA.Test", 2, 2);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "NamespaceA.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1], "NamespaceB.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestA");

            AssertTreeNodeGroup(_createNodes[1], "CategoryB", 2, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0], "LibraryA.Test", 2, 2);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0], "NamespaceA.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[0].Nodes[0], "Fixture_1", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[0].Nodes[0].Nodes[0], "TestB");
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1], "NamespaceB.Folder_1", 1, 1);
            AssertTreeNodeGroup(_createNodes[1].Nodes[0].Nodes[1].Nodes[0], "Fixture_2", 1, 1);
            AssertTestCase(_createNodes[1].Nodes[0].Nodes[1].Nodes[0].Nodes[0], "TestB");
        }

        [Test]
        public void CreateTree_SameCategory_DefinedMultipleTimes_AllTreeNodes_AreCreated()
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA",
                    CreateTestSuiteXml("3-1001", "NamespaceA",
                        CreateTestFixtureXml("3-1010", "Fixture_1", new List<string>() { "CategoryA", "CategoryA" },
                            CreateTestcaseXml("3-1011", "TestA", "CategoryA"),
                            CreateTestcaseXml("3-1012", "TestB", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(_view, _model);
            strategy.OnTestLoaded(testNode, null);

            // Assert tree nodes
            Assert.That(_createNodes.Count, Is.EqualTo(1));
            AssertTreeNodeGroup(_createNodes[0], "CategoryA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0], "LibraryA.NamespaceA", 2, 1);
            AssertTreeNodeGroup(_createNodes[0].Nodes[0].Nodes[0], "Fixture_1", 2, 2);
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[0], "TestA");
            AssertTestCase(_createNodes[0].Nodes[0].Nodes[0].Nodes[1], "TestB");
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

        private string CreateTestcaseXml(string testId, string testName, string category)
        {
            return CreateTestcaseXml(testId, testName, new List<string>() { category });
        }

        private string CreateTestcaseXml(string testId, string testName, IList<string> categories)
        {
            string str = $"<test-case id='{testId}' name='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            str += "</test-case> ";

            return str;
        }

        private string CreateTestFixtureXml(string testId, string testName, IEnumerable<string> categories, params string[] testCases)
        {
            string str = $"<test-suite type='TestFixture' id='{testId}'  name='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            foreach (string testCase in testCases)
                str += testCase;

            str += "</test-suite>";

            return str;
        }

        private string CreateTestSuiteXml(string testId, string testName, params string[] testSuites)
        {
            string str = $"<test-suite type='TestSuite' id='{testId}' name='{testName}'> ";
            foreach (string testSuite in testSuites)
                str += testSuite;

            str += "</test-suite>";

            return str;
        }
    }
}
