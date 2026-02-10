// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using NSubstitute;

namespace TestCentric.Gui.Presenters.TestTree
{
    using Model;

    public class WhenTestsAreReloaded : TreeViewPresenterTestBase
    {
        [SetUp]
        public void Setup()
        {
            ClearAllReceivedCalls();

            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(false);
        }

        [Test]
        public void TestFilters_AreReset()
        {
            // Arrange
            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestCentricProject.Returns(project);

            // Act
            FireTestReloadedEvent(testNode);

            // Assert
            _view.TextFilter.Received().Text = "";
            _view.OutcomeFilter.ReceivedWithAnyArgs().SelectedItems = null;
            _view.CategoryFilter.ReceivedWithAnyArgs().SelectedItems = null;
        }

        [Test]
        public void CategoryFilter_IsClosed_And_Init()
        {
            // Arrange
            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestCentricProject.Returns(project);

            // Act
            FireTestReloadedEvent(testNode);

            // Assert
            _view.CategoryFilter.Received().Close();
            _view.CategoryFilter.Received().Init(_model);
        }

        [Test]
        public void VisualState_IsAppliedToTree()
        {
            // Arrange
            ITreeDisplayStrategy strategy = Substitute.For<ITreeDisplayStrategy>();
            _treeDisplayStrategyFactory.Create(null, null, null).ReturnsForAnyArgs(strategy);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestCentricProject.Returns(project);

            // Act
            FireTestReloadedEvent(testNode);

            // Assert
            strategy.Received().OnTestLoaded(testNode, null);
        }

        //[Test]
        public void Reloading_VisualState_IsSaved()
        {
            // Arrange
            ITreeDisplayStrategy strategy = Substitute.For<ITreeDisplayStrategy>();
            _treeDisplayStrategyFactory.Create(null, null, null).ReturnsForAnyArgs(strategy);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.TestCentricProject.Returns(project);
            FireTestLoadedEvent(testNode);

            // Act
            FireTestsReloadingEvent();

            // Assert
            strategy.Received().SaveVisualState();
        }
    }
}
