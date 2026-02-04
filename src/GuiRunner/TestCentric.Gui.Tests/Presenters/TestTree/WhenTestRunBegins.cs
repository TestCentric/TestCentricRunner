// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using System.Windows.Forms;
using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters.TestTree
{
    using System.Collections.Generic;
    using TestCentric.Gui.Model.Settings;

    public class WhenTestRunBegins : TreeViewPresenterTestBase
    {
        // Use dedicated test file name; Used for VisualState file too
        const string TestFileName = "TreeViewPresenterTestRunBegin.dll";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _treeDisplayStrategyFactory = new TreeDisplayStrategyFactory();
        }

        [TearDown]
        public void TearDown()
        {
            // Delete VisualState file to prevent any unintended side effects
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        [Test]
        public void TreeNodeImageIconsAreSet()
        {
            // Arrange
            var tv = new TreeView();
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _view.TreeView.Returns(tv);

            var testNode = new TestNode("<test-run id='0'>" +
                                        "<test-suite type='TestFixture' id='100' name='FixtureA'>" +
                                            "<test-case id='101' name='TestA'/> " +
                                        "</test-suite>" +
                                        "<test-suite type='TestFixture' id='200' name='FixtureB'>" +
                                            "<test-case id='201' name='TestB'/> " +
                                            "<test-case id='202' name='TestC'/> " +
                                            "<test-case id='203' name='TestD'/> " +
                                        "</test-suite>" +
                                        "</test-run>");


            // We can't construct a TreeNodeCollection, so we fake it
            // TreeNodeCollection nodes = new TreeNode().Nodes;
            /*TreeNode treeNode1 = new TreeNode("TestA") { Tag = testNode.Children[0] };
            TreeNode treeNode2 = new TreeNode("TestB") { Tag = testNode.Children[1]};
            TreeNode treeNode3 = new TreeNode("TestC") { Tag = testNode.Children[2], ImageIndex = TestTreeView.InitIndex };
            TreeNode treeNode4 = new TreeNode("TestD") { Tag = testNode.Children[3]};

            treeNode1.Nodes.Add(treeNode2);
            nodes.AddRange(new[] { treeNode1, treeNode3, treeNode4 } );*/
            // _view.Nodes.Returns(nodes);

            IList<TreeNode> treeNodes = new List<TreeNode>();
            _view.When(v => v.Add(Arg.Any<TreeNode>())).Do(t => treeNodes.Add(t[0] as TreeNode));

            var project = new TestCentricProject(new GuiOptions(TestFileName));
            _model.TestCentricProject.Returns(project);
            _model.LoadedTests.Returns(testNode);

            TestNode testA = testNode.Children[0].Children[0];
            TestNode testB = testNode.Children[1].Children[0];

            _model.TestsInRun.Returns(new TestSelection(new []{ testA, testB }));

            FireTestLoadedEvent(testNode);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            _view.Received().SetImageIndex(treeNodes[0], TestTreeView.RunningIndex);
            _view.Received().SetImageIndex(treeNodes[1], TestTreeView.RunningIndex);
            _view.Received().SetImageIndex(treeNodes[0].Nodes[0], TestTreeView.RunningIndex);
        }

        [TestCase("Passed", TestTreeView.SuccessIndex_NotLatestRun)]
        [TestCase("Failed", TestTreeView.FailureIndex_NotLatestRun)]
        [TestCase("Warning", TestTreeView.WarningIndex_NotLatestRun)]
        [TestCase("Skipped", TestTreeView.SkippedIndex)]
        public void TreeNodeWithResults_ImageIconsAreSet_ToPreviousOutcomeIcon(string resultState, int expectedImageIndex)
        {
            // Arrange
            var tv = new TreeView();
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _view.TreeView.Returns(tv);

            var testNode = new TestNode($"<test-suite type='TestFixture' id='0' name='FixtureA'>"+
                                        "<test-case id='1' name='TestA'/> "+
                                        "<test-case id='2' name='TestB'/> +" +
                                        "<test-case id='3' name='TestC'/> +" +
                                        "<test-case id='4' name='TestD'/> +" +
                                        "</test-suite>");


            // We can't construct a TreeNodeCollection, so we fake it
            //TreeNodeCollection nodes = new TreeNode().Nodes;
            //TreeNode treeNode1 = new TreeNode("TestA") { Tag = testNode.Children[0], ImageIndex = imageIndex };
            //TreeNode treeNode2 = new TreeNode("TestB") { Tag = testNode.Children[1], ImageIndex = imageIndex };
            //TreeNode treeNode3 = new TreeNode("TestC") { Tag = testNode.Children[2], ImageIndex = imageIndex };
            //TreeNode treeNode4 = new TreeNode("TestD") { Tag = testNode.Children[3], ImageIndex = imageIndex };

            //treeNode1.Nodes.Add(treeNode2);
            //nodes.AddRange(new[] { treeNode1, treeNode3, treeNode4 });
            //_view.Nodes.Returns(nodes);
            //_view.When(v => v.SetImageIndex(Arg.Any<TreeNode>(), Arg.Any<int>()))
            //    .Do(a => a.ArgAt<TreeNode>(0).ImageIndex = a.ArgAt<int>(1));

            IList<TreeNode> treeNodes = new List<TreeNode>();
            _view.When(v => v.Add(Arg.Any<TreeNode>())).Do(t => treeNodes.Add(t[0] as TreeNode));

            ResultNode resultNode2 = new ResultNode($"<test-case id='2' result='{resultState}'/>");
            ResultNode resultNode3 = new ResultNode($"<test-case id='3' result='{resultState}'/>");
            ResultNode resultNode4 = new ResultNode($"<test-case id='4' result='{resultState}'/>");
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode2);
            _model.TestResultManager.GetResultForTest("3").Returns(resultNode3);
            _model.TestResultManager.GetResultForTest("4").Returns(resultNode4);

            var project = new TestCentricProject(new GuiOptions(TestFileName));
            _model.TestCentricProject.Returns(project);
            _model.LoadedTests.Returns(testNode);
            _model.TestsInRun.Returns(new TestSelection() { testNode.Children[0] });

            FireTestLoadedEvent(testNode);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            _view.Received().SetImageIndex(treeNodes[0], TestTreeView.RunningIndex);
            _view.Received().SetImageIndex(treeNodes[1], expectedImageIndex);
            _view.Received().SetImageIndex(treeNodes[2], expectedImageIndex);
            _view.Received().SetImageIndex(treeNodes[3], expectedImageIndex);
        }

        [TestCase("NUNIT_TREE")]
        [TestCase("TEST_LIST")]
        public void CurrentDisplayFormat_IsSaved_InVisualFile(string displayFormat)
        {
            // Arrange
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _settings.Gui.TestTree.DisplayFormat = displayFormat;
            _model.TreeConfiguration.DisplayFormat = displayFormat;
            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            var project = new TestCentricProject(new GuiOptions(TestFileName));
            _model.TestCentricProject.Returns(project);
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestsInRun.Returns(new TestSelection());
            FireTestLoadedEvent(testNode);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            VisualState visualState = VisualState.LoadFrom(fileName);
            Assert.That(visualState.DisplayStrategy, Is.EqualTo(displayFormat));
        }

        [TestCase("NUNIT_TREE", "UNGROUPED")]
        [TestCase("NUNIT_TREE", "ASSEMBLY")]
        [TestCase("NUNIT_TREE", "CATEGORY")]
        [TestCase("NUNIT_TREE", "OUTCOME")]
        [TestCase("NUNIT_TREE", "DURATION")]
        // TODO: FIX
        //[TestCase("TEST_LIST", "UNGROUPED")]
        [TestCase("TEST_LIST", "ASSEMBLY")]
        [TestCase("TEST_LIST", "CATEGORY")]
        [TestCase("TEST_LIST", "OUTCOME")]
        [TestCase("TEST_LIST", "DURATION")]
        public void VisualStateIsSavedCorrectly(string displayFormat, string groupBy)
        {
            // Arrange
            _model.TestsInRun.Returns(new TestSelection());
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _settings.Gui.TestTree.DisplayFormat = displayFormat;
            _model.TreeConfiguration.DisplayFormat = displayFormat;
            _model.TreeConfiguration.TestListGroupBy = groupBy;
            _model.TreeConfiguration.NUnitGroupBy = groupBy;
            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            var project = new TestCentricProject(new GuiOptions(TestFileName));
            _model.TestCentricProject.Returns(project);
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestsInRun.Returns(new TestSelection());
            FireTestLoadedEvent(testNode);
            _model.TreeConfiguration.DisplayFormat = displayFormat;
            _model.TreeConfiguration.NUnitGroupBy = groupBy;

            // Act
            FireRunStartingEvent(1234);

            // Assert
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            VisualState visualState = VisualState.LoadFrom(fileName);
            Assert.That(visualState.DisplayStrategy, Is.EqualTo(displayFormat));
            Assert.That(visualState.GroupBy, Is.EqualTo(groupBy));
        }
    }
}
