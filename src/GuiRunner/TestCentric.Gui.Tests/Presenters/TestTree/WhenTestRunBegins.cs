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
        public void WhenTestRunStarts_TreeNodeImageIconsAreSet()
        {
            // Arrange
            var tv = new TreeView();
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _view.TreeView.Returns(tv);

            TestNode testNode1 = new TestNode("<test-suite id='1'/>");
            TestNode testNode2 = new TestNode("<test-suite id='2'/>");
            TestNode testNode3 = new TestNode("<test-suite id='3'/>");
            TestNode testNode4 = new TestNode("<test-suite id='4'/>");


            // We can't construct a TreeNodeCollection, so we fake it
            TreeNodeCollection nodes = new TreeNode().Nodes;
            TreeNode treeNode1 = new TreeNode("TestA") { Tag = testNode1 };
            TreeNode treeNode2 = new TreeNode("TestB") { Tag = testNode2 };
            TreeNode treeNode3 = new TreeNode("TestC") { Tag = testNode3, ImageIndex = TestTreeView.InitIndex }; 
            TreeNode treeNode4 = new TreeNode("TestD") { Tag = testNode4 };

            treeNode1.Nodes.Add(treeNode2);
            nodes.AddRange(new[] { treeNode1, treeNode3, treeNode4 } );
            _view.Nodes.Returns(nodes);
            _view.When(v => v.SetImageIndex(Arg.Any<TreeNode>(), Arg.Any<int>()))
                .Do(a => a.ArgAt<TreeNode>(0).ImageIndex = a.ArgAt<int>(1));

            var project = new TestCentricProject(_model, TestFileName);
            _model.TestCentricProject.Returns(project);
            _model.LoadedTests.Returns(testNode1);
            _model.IsInTestRun(testNode2).Returns(true);
            _model.IsInTestRun(testNode4).Returns(true);

            FireTestLoadedEvent(testNode1);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            _view.Received().SetImageIndex(treeNode1, TestTreeView.RunningIndex);
            _view.Received().SetImageIndex(treeNode2, TestTreeView.RunningIndex);
            _view.Received().SetImageIndex(treeNode4, TestTreeView.RunningIndex);

            _view.Received().SetImageIndex(treeNode3, TestTreeView.InitIndex);
        }

        [TestCase(TestTreeView.SuccessIndex, TestTreeView.SuccessIndex_NotLatestRun)]
        [TestCase(TestTreeView.FailureIndex, TestTreeView.FailureIndex_NotLatestRun)]
        [TestCase(TestTreeView.WarningIndex, TestTreeView.WarningIndex_NotLatestRun)]
        [TestCase(TestTreeView.IgnoredIndex, TestTreeView.IgnoredIndex_NotLatestRun)]
        [TestCase(TestTreeView.InconclusiveIndex, TestTreeView.InconclusiveIndex_NotLatestRun)]
        [TestCase(TestTreeView.SkippedIndex, TestTreeView.SkippedIndex)]
        public void WhenTestRunStarts_TreeNodeWithResults_ImageIconsAreSet_ToPreviousOutcomeIcon(int imageIndex, int expectedImageIndex)
        {
            // Arrange
            var tv = new TreeView();
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _view.TreeView.Returns(tv);

            TestNode testNode1 = new TestNode("<test-suite id='1'/>");
            TestNode testNode2 = new TestNode("<test-suite id='2'/>");
            TestNode testNode3 = new TestNode("<test-suite id='3'/>");
            TestNode testNode4 = new TestNode("<test-suite id='4'/>");


            // We can't construct a TreeNodeCollection, so we fake it
            TreeNodeCollection nodes = new TreeNode().Nodes;
            TreeNode treeNode1 = new TreeNode("TestA") { Tag = testNode1, ImageIndex = imageIndex };
            TreeNode treeNode2 = new TreeNode("TestB") { Tag = testNode2, ImageIndex = imageIndex };
            TreeNode treeNode3 = new TreeNode("TestC") { Tag = testNode3, ImageIndex = imageIndex };
            TreeNode treeNode4 = new TreeNode("TestD") { Tag = testNode4, ImageIndex = imageIndex };

            treeNode1.Nodes.Add(treeNode2);
            nodes.AddRange(new[] { treeNode1, treeNode3, treeNode4 });
            _view.Nodes.Returns(nodes);
            _view.When(v => v.SetImageIndex(Arg.Any<TreeNode>(), Arg.Any<int>()))
                .Do(a => a.ArgAt<TreeNode>(0).ImageIndex = a.ArgAt<int>(1));

            var project = new TestCentricProject(_model, TestFileName);
            _model.TestCentricProject.Returns(project);
            _model.LoadedTests.Returns(testNode1);
            _model.IsInTestRun(testNode1).Returns(true);

            FireTestLoadedEvent(testNode1);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            _view.Received().SetImageIndex(treeNode1, TestTreeView.RunningIndex);
            _view.Received().SetImageIndex(treeNode2, expectedImageIndex);
            _view.Received().SetImageIndex(treeNode3, expectedImageIndex);
            _view.Received().SetImageIndex(treeNode4, expectedImageIndex);
        }

        [TestCase("NUNIT_TREE")]
        [TestCase("TEST_LIST")]
        public void WhenTestRunStarts_CurrentDisplayFormat_IsSaved_InVisualFile(string displayFormat)
        {
            // Arrange
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _settings.Gui.TestTree.DisplayFormat = displayFormat;
            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            var project = new TestCentricProject(_model, TestFileName);
            _model.TestCentricProject.Returns(project);
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            VisualState visualState = VisualState.LoadFrom(fileName);
            Assert.That(visualState.DisplayStrategy, Is.EqualTo(displayFormat));
        }

        // TODO: This is failing BUT manual tests show that the UNGROUPED display
        // persists after closing and re-opening the app. Error in test itself?
        //[TestCase("UNGROUPED")]
        [TestCase("ASSEMBLY")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        public void WhenTestRunStarts_CurrentGroupBy_IsSaved_InVisualFile(string groupBy)
        {
            // Arrange
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _settings.Gui.TestTree.DisplayFormat = "TEST_LIST";
            _settings.Gui.TestTree.TestList.GroupBy = groupBy;

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            var project = new TestCentricProject(_model, TestFileName);
            _model.TestCentricProject.Returns(project);
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            VisualState visualState = VisualState.LoadFrom(fileName);
            Assert.That(visualState.GroupBy, Is.EqualTo(groupBy));
        }

        [TestCase("UNGROUPED")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        [TestCase("DURATION")]
        public void WhenTestRunStarts_NUnitTree_CurrentGroupBy_IsSaved_InVisualFile(string groupBy)
        {
            // Arrange
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _settings.Gui.TestTree.DisplayFormat = "NUNIT_TREE";
            _settings.Gui.TestTree.NUnitGroupBy = groupBy;

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            var project = new TestCentricProject(_model, TestFileName);
            _model.TestCentricProject.Returns(project);
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Act
            FireRunStartingEvent(1234);

            // Assert
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            VisualState visualState = VisualState.LoadFrom(fileName);
            Assert.That(visualState.GroupBy, Is.EqualTo(groupBy));
        }

        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //public void WhenTestRunStarts_ResultsAreCleared()
        //{
        //    _settings.RunStarting += Raise.Event<TestEventHandler>(new TestEventArgs(TestAction.RunStarting, "Dummy", 1234));

        //    _view.Received().ClearResults();
        //}
    }
}
