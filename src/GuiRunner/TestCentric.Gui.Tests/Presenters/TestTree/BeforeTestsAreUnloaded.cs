// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.TestTree
{
    using System.Windows.Forms;
    using Model;
    using TestCentric.Gui.Views;

    public class BeforeTestsAreUnloaded : PresenterTestBase<ITestTreeView>
    {
        private TreeViewPresenter _presenter;

        [SetUp]
        public void Setup()
        {
            _presenter = new TreeViewPresenter(_view, _model, new TreeDisplayStrategyFactory());
        }

        // TODO: FIX
        //[TestCase("NUNIT_TREE", "UNGROUPED")]
        //[TestCase("NUNIT_TREE", "ASSEMBLY")]
        //[TestCase("NUNIT_TREE", "CATEGORY")]
        //[TestCase("NUNIT_TREE", "OUTCOME")]
        //[TestCase("NUNIT_TREE", "DURATION")]
        // TODO: Determine why UNGROUPED fails for TestList
        //[TestCase("TEST_LIST", "UNGROUPED")]
        //[TestCase("TEST_LIST", "ASSEMBLY")]
        //[TestCase("TEST_LIST", "CATEGORY")]
        //[TestCase("TEST_LIST", "OUTCOME")]
        //[TestCase("TEST_LIST", "DURATION")]
        public void VisualState_IsSaved(string displayFormat, string groupBy)
        {
            // Arrange
            _settings.Gui.TestTree.DisplayFormat = displayFormat;
            var treeConfig = new TreeConfiguration()
            {
                DisplayFormat = displayFormat,
                NUnitGroupBy = groupBy,
                TestListGroupBy = groupBy
            };
            _model.TreeConfiguration.Returns(treeConfig);
            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            var nodes = new TreeNode().Nodes; // Hack to construct a TreeNode collection
            nodes.Add(new TreeNode("dummy.dll"));
            _view.Nodes.Returns(nodes);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            _model.TestCentricProject.Returns(project);
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestsInRun.Returns(new TestSelection());
            
            FireTestLoadedEvent(testNode);

            // Act
            FireTestsUnloadingEvent();

            // Assert
            string fileName = VisualState.GetVisualStateFileName("dummy.dll");
            VisualState visualState = VisualState.LoadFrom(fileName);
            Assert.That(visualState.DisplayStrategy, Is.EqualTo(displayFormat));
            Assert.That(visualState.GroupBy, Is.EqualTo(groupBy));
        }
    }
}
