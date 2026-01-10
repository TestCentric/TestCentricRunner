// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;
using NUnit.Framework;
using NSubstitute;

namespace TestCentric.Gui.Presenters.TestTree
{
    using Model;
    using TestCentric.Gui.Presenters.NUnitGrouping;
    using Views;

    public abstract class DisplayStrategyTests
    {
        protected ITestTreeView _view;
        protected ITestModel _model;
        protected DisplayStrategy _strategy;

        [SetUp]
        public void CreateDisplayStrategy()
        {
            _view = Substitute.For<ITestTreeView>();
            _model = Substitute.For<ITestModel>();
            _model.Settings.Gui.TestTree.ShowNamespace.Returns(true);

            // We can't construct a TreeNodeCollection, so we fake it
            var nodes = new TreeNode().Nodes;
            nodes.Add(new TreeNode("test.dll"));
            _view.Nodes.Returns(nodes);

            var treeView = new TreeView();
            _view.TreeView.Returns(treeView);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            _model.TestCentricProject.Returns(project);

            _strategy = GetDisplayStrategy();
        }

        protected abstract DisplayStrategy GetDisplayStrategy();

        [Test]
        public void WhenTestsAreLoaded_TreeViewIsLoaded()
        {
            _strategy.OnTestLoaded(
                new TestNode("<test-run id='1'><test-suite id='42'/><test-suite id='99'/></test-run>"),
                null);

            _view.Received().Clear();
            _view.Received().Add(Arg.Compat.Is<TreeNode>((tn) => (tn.Tag as TestNode).Id == "42"));
            _view.Received().Add(Arg.Compat.Is<TreeNode>((tn) => (tn.Tag as TestNode).Id == "99"));
        }

        [Test]
        public void WhenTestsAreUnloaded_TreeViewIsCleared()
        {
            _strategy.OnTestUnloaded();

            _view.Received().Clear();
        }
    }

    public class NUnitTreeDisplayStrategyTests : DisplayStrategyTests
    {
        private ITreeViewModel _grouping;

        protected override DisplayStrategy GetDisplayStrategy()
        {
            _grouping = Substitute.For<ITreeViewModel>();
            return new NUnitTreeDisplayStrategy(_view, _model, _grouping);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void OnStrategyCreated_OutcomeFilter_IsVisible(bool isVisible)
        {
            // Arrange
            var view = Substitute.For<ITestTreeView>();
            var model = Substitute.For<ITestModel>();
            model.Settings.Gui.TestTree.ShowFilter.Returns(isVisible);

            // Act
            var strategy = new NUnitTreeDisplayStrategy(view, model);

            // Assert
            view.Received().SetTestFilterVisibility(isVisible);
        }

        [Test]
        public void OnTestRunStarting_Grouping_IsInvoked()
        {
            // Act
            _strategy.OnTestRunStarting();

            // Assert
            _grouping.Received().OnTestRunStarting();
        }


        [Test]
        public void OnTestFinished_Grouping_IsInvoked()
        {
            ResultNode result = new ResultNode($"<test-case id='1' result='Passed' />");

            // Act
            _strategy.OnTestFinished(result);

            // Assert
            _grouping.Received().OnTestFinished(result);

        }

        [Test]
        public void OnTestRunFinished_Grouping_IsInvoked()
        {
            // Act
            _strategy.OnTestRunFinished();

            // Assert
            _grouping.Received().OnTestRunFinished();
        }

        [Test]
        public void OnTestLoaded_Namespaces_AreShown_NamespaceNodes_AreCreated()
        {
            // Arrange
            _model.Settings.Gui.TestTree.ShowNamespace.Returns(true);
            string xml = 
                "<test-suite type='Assembly' id='1-1030' name='Library.Test.dll'>" +
                    "<test-suite type='TestSuite' id='1-1031' name='Library'>" +
                    "</test-suite>" +
                "</test-suite>";

            // Act
            _strategy.OnTestLoaded(new TestNode(xml), null);

            // Assert
            _view.Received().Add(Arg.Compat.Is<TreeNode>(tn => (tn.Tag as TestNode).Id == "1-1031"));
        }

        [Test]
        public void OnTestLoaded_Namespaces_AreNotShown_NamespaceNodes_AreNotCreated()
        {
            // Arrange
            _model.Settings.Gui.TestTree.ShowNamespace.Returns(false);
            string xml =
                "<test-run> <test-suite type='Assembly' id='1-1030' name='Library.Test.dll'>" +
                    "<test-suite type='TestSuite' id='1-1031' name='Library'>" +
                    "</test-suite>" +
                "</test-suite> </test-run>";

            // Act
            _strategy.OnTestLoaded(new TestNode(xml), null);

            // Assert
            _view.DidNotReceive().Add(Arg.Compat.Is<TreeNode>(tn => (tn.Tag as TestNode).Id == "1-1031"));
        }

        [Test]
        public void OnTestLoaded_NamespacesNodes_ContainingOneNamespace_AreFolded()
        {
            // Arrange
            string xml =
                "<test-suite type='Assembly' id='1-1030' name='Library.Test.dll'>" +
                    "<test-suite type='TestSuite' id='1-1031' name='Library'>" +
                        "<test-suite type='TestSuite' id='1-1032' name='Test'>" +
                            "<test-suite type='TestSuite' id='1-1033' name='Folder'>" +
                            "</test-suite>" +
                        "</test-suite>" +
                    "</test-suite>" +
                "</test-suite>";

            TreeNode treeNode = null;
            _view.Add(Arg.Do<TreeNode>(tn => treeNode = tn));

            // Act
            _strategy.OnTestLoaded(new TestNode(xml), null);

            // Assert
            Assert.That((treeNode.Tag as TestNode).Id, Is.EqualTo("1-1031"));
            Assert.That(treeNode.Text, Is.EqualTo("Library.Test.Folder (0)"));
            Assert.That(treeNode.Nodes.Count, Is.EqualTo(0));
        }

        [Test]
        public void OnTestLoaded_NamespacesNode_ContainingTwoNamespaces_AreNotFolded()
        {
            // Arrange
            string xml =
                "<test-suite type='Assembly' id='1-1030' name='Library.Test.dll'>" +
                    "<test-suite type='TestSuite' id='1-1031' name='Library'>" +
                        "<test-suite type='TestSuite' id='1-1032' name='Test' >" +
                            "<test-case id='1-1040' name='Test1'/>" +
                        "</test-suite>" +
                        "<test-suite type='TestSuite' id='1-1033' name='Folder' >" +
                            "<test-case id='1-1050' name='Test1'/>" +
                        "</test-suite>" +
                    "</test-suite>" +
                "</test-suite>";

            TreeNode treeNode = null;
            _view.Add(Arg.Do<TreeNode>(tn => treeNode = tn));

            // Act
            _strategy.OnTestLoaded(new TestNode(xml), null);

            // Assert
            Assert.That((treeNode.Tag as TestNode).Id, Is.EqualTo("1-1031"));
            Assert.That(treeNode.Text, Is.EqualTo("Library (2)"));

            var child1 = treeNode.Nodes[0];
            Assert.That((child1.Tag as TestNode).Id, Is.EqualTo("1-1032"));
            Assert.That(child1.Text, Is.EqualTo("Test (1)"));

            var child2 = treeNode.Nodes[1];
            Assert.That((child2.Tag as TestNode).Id, Is.EqualTo("1-1033"));
            Assert.That(child2.Text, Is.EqualTo("Folder (1)"));
        }

        [Test]
        public void OnTestLoaded_SetupFixtureNode_ContainingOneNamespace_AreFolded()
        {
            // Arrange
            string xml =
                "<test-suite type='Assembly' id='1-1030' name='Library.Test.dll'>" +
                    "<test-suite type='SetUpFixture' id='1-1031' name='Library'>" +
                        "<test-suite type='TestSuite' id='1-1032' name='Test'>" +
                            "<test-suite type='SetUpFixture' id='1-1033' name='Folder'>" +
                            "</test-suite>" +
                        "</test-suite>" +
                    "</test-suite>" +
                "</test-suite>";

            TreeNode treeNode = null;
            _view.Add(Arg.Do<TreeNode>(tn => treeNode = tn));

            // Act
            _strategy.OnTestLoaded(new TestNode(xml), null);

            // Assert
            Assert.That((treeNode.Tag as TestNode).Id, Is.EqualTo("1-1031"));
            Assert.That(treeNode.Text, Is.EqualTo("Library.Test.Folder (0)"));
            Assert.That(treeNode.Nodes.Count, Is.EqualTo(0));
        }

        [Test]
        public void OnTestLoaded_SetupFixtureNode_ContainingTwoNamespaces_AreNotFolded()
        {
            // Arrange
            string xml =
                "<test-suite type='Assembly' id='1-1030' name='Library.Test.dll'>" +
                    "<test-suite type='SetUpFixture' id='1-1031' name='Library'>" +
                        "<test-suite type='TestSuite' id='1-1032' name='Test'>" +
                            "<test-case id='1-1040' name='Test1'/>" +
                        "</test-suite>" +
                        "<test-suite type='TestSuite' id='1-1033' name='Folder'>" +
                            "<test-case id='1-1050' name='Test1'/>" +
                        "</test-suite>" +
                    "</test-suite>" +
                "</test-suite>";

            TreeNode treeNode = null;
            _view.Add(Arg.Do<TreeNode>(tn => treeNode = tn));

            // Act
            _strategy.OnTestLoaded(new TestNode(xml), null);

            // Assert
            Assert.That((treeNode.Tag as TestNode).Id, Is.EqualTo("1-1031"));
            Assert.That(treeNode.Text, Is.EqualTo("Library (2)"));

            var child1 = treeNode.Nodes[0];
            Assert.That((child1.Tag as TestNode).Id, Is.EqualTo("1-1032"));
            Assert.That(child1.Text, Is.EqualTo("Test (1)"));

            var child2 = treeNode.Nodes[1];
            Assert.That((child2.Tag as TestNode).Id, Is.EqualTo("1-1033"));
            Assert.That(child2.Text, Is.EqualTo("Folder (1)"));
        }

        [Test]
        public void OnTestLoaded_TestFilters_AreEnabled()
        {
            // Arrange
            string xml =
                "<test-suite type='Assembly' id='1-1030' name='Library.Test.dll'>" +
                "</test-suite>";

            // Act
            _strategy.OnTestLoaded(new TestNode(xml), null);

            // Assert
            _view.Received().EnableTestFilter(true);
        }

        [Test]
        public void OnTestUnloaded_TestFilters_AreDisabled()
        {
            // Arrange + Act
            _strategy.OnTestUnloaded();

            // Assert
            _view.Received().EnableTestFilter(false);
        }

        [Test]
        public void CollapseToFixtures_AllFixtureNodes_AreShown()
        {
            // Arrange
            var treeView = new TreeView();
            _view.TreeView.Returns(treeView);

            var rootNode = _view.Nodes[0];
            var fixtureNode1 = CreateTreeNode("<test-suite type='TestFixture' id='2' name='FixtureA'/>");
            var fixtureNode2 = CreateTreeNode("<test-suite type='TestFixture' id='3' name='FixtureB'/>");
            rootNode.Nodes.AddRange(new []{ fixtureNode1, fixtureNode2 });

            var testcase1 = CreateTreeNode("<test-case id='10' name='Test1'/>");
            var testcase2 = CreateTreeNode("<test-case id='20' name='Test2'/>");
            fixtureNode1.Nodes.AddRange(new[] { testcase1, testcase2 });

            var testcase3 = CreateTreeNode("<test-case id='30' name='Test3'/>");
            var testcase4 = CreateTreeNode("<test-case id='40' name='Test4'/>");
            fixtureNode2.Nodes.AddRange(new[] { testcase3, testcase4 });

            rootNode.ExpandAll();

            // Act
            _strategy.CollapseToFixtures();

            // Assert
            Assert.That(rootNode.IsExpanded, Is.EqualTo(true));
            Assert.That(fixtureNode1.IsExpanded, Is.EqualTo(false));
            Assert.That(fixtureNode2.IsExpanded, Is.EqualTo(false));
        }

        TreeNode CreateTreeNode(string testNodeXml)
        {
            var testNode = new TestNode(testNodeXml);
            return new TreeNode() { Tag = testNode };

        }
    }


    //public class NUnitTestListStrategyTests : DisplayStrategyTests
    //{
    //    protected override DisplayStrategy GetDisplayStrategy()
    //    {
    //        return new TestListDisplayStrategy(_view, _model);
    //    }
    //}
}
