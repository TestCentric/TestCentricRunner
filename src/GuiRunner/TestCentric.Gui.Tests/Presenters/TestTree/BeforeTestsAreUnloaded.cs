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

    public class BeforeTestsAreUnloaded : TreeViewPresenterTestBase
    {
        [OneTimeSetUp]
        public void Setup()
        {
            _treeDisplayStrategyFactory = new TreeDisplayStrategyFactory();
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
        public void VisualState_IsSaved(string displayFormat, string groupBy)
        {
            // Arrange
            var strategy = _treeDisplayStrategyFactory.Create(displayFormat, _view, _model);
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));
            _settings.Gui.TestTree.DisplayFormat = displayFormat;
            _model.TreeConfiguration.DisplayFormat = displayFormat;
            _model.TreeConfiguration.TestListGroupBy = groupBy;
            _model.TreeConfiguration.NUnitGroupBy = groupBy;
            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            _model.TestCentricProject.Returns(project);
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestsInRun.Returns(new TestSelection());
            FireTestLoadedEvent(testNode);

            // Act
            FireTestsUnloadingEvent();

            // Assert
            //strategy.Received().SaveVisualState();
            string fileName = VisualState.GetVisualStateFileName("dummy.dll");
            VisualState visualState = VisualState.LoadFrom(fileName);
            Assert.That(visualState.DisplayStrategy, Is.EqualTo(displayFormat));
            Assert.That(visualState.GroupBy, Is.EqualTo(groupBy));
        }
    }
}
