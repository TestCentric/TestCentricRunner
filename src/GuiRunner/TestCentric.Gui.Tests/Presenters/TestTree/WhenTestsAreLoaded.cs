// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using NSubstitute;

namespace TestCentric.Gui.Presenters.TestTree
{
    using Model;
    using System;
    using System.IO;
    using System.Windows.Forms;
    using TestCentric.Gui.Views;

    // TODO: FIX
    [Ignore("Rewrite")]
    public class WhenTestsAreLoaded : PresenterTestBase<ITestTreeView>
    {
        // Use dedicated test file name; Used for VisualState file too
        const string TEST_FILE_NAME = "TreeViewPresenterTestsLoaded.dll";
        static readonly string VISUAL_STATE_FILE_NAME = VisualState.GetVisualStateFileName(TEST_FILE_NAME);
        static readonly TestNode TEST_NODE = new TestNode("<test-suite id='1'/>");
        static readonly GuiOptions OPTIONS = new GuiOptions(TEST_FILE_NAME);

        private TreeViewPresenter _presenter;

        [SetUp]
        public void SimulateTestLoad()
        {
            ClearAllReceivedCalls();

            _presenter = new TreeViewPresenter(_view, _model, new TreeDisplayStrategyFactory());
            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(false);
            _model.Options.Returns(OPTIONS);
            _model.TestCentricProject.Returns(new TestCentricProject(OPTIONS));
            _model.TreeConfiguration.Returns(new TreeConfiguration());
            _model.LoadedTests.Returns(TEST_NODE);

            var tv = new TreeView();
            _view.TreeView.Returns(tv);
            _view.Nodes.Returns(tv.Nodes);
        }

        [TearDown]
        public void TearDown()
        {
            // Delete VisualState file to prevent any unintended side effects
            string fileName = VisualState.GetVisualStateFileName(TEST_FILE_NAME);
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        [TestCase("NUNIT_TREE", typeof(NUnitTreeDisplayStrategy), false)]
        [TestCase("NUNIT_TREE", typeof(NUnitTreeDisplayStrategy), true)]
        [TestCase("TEST_LIST", typeof(TestListDisplayStrategy), false)]
        [TestCase("TEST_LIST", typeof(TestListDisplayStrategy), true)]
        public void WithNoVisualState_DefaultsAreUsedForAllSettings(string displayFormat, Type expectedStrategy, bool showCheckBoxes)
        {
            // Arrange: adapt settings
            _model.Settings.Gui.TestTree.DisplayFormat = displayFormat;

            // Act: Load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_model.Settings.Gui.TestTree.DisplayFormat, Is.EqualTo(displayFormat));
            Assert.That(_presenter.TreeConfiguration.DisplayFormat, Is.EqualTo(displayFormat));
            Assert.That(_presenter.Strategy, Is.TypeOf(expectedStrategy));
            if (displayFormat == "NUnit_TREE")
                Assert.That(_presenter.TreeConfiguration.ShowNamespaces, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WithNoVisualState_ShowCheckBoxesComesFromUserSetting(bool showCheckBoxSetting)
        {
            // Arrange
            _model.Settings.Gui.TestTree.ShowCheckBoxes = showCheckBoxSetting;

            // Act: load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_view.ShowCheckBoxes.Checked, Is.EqualTo(showCheckBoxSetting));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WithVisualState_ShowCheckBoxIsAppliedFromVisualState(bool showCheckBox)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState()
            {
                ShowCheckBoxes = showCheckBox
            };
            visualState.Save(VISUAL_STATE_FILE_NAME);

            TryLoadVisualStateReturns(visualState);

            // Act: Load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_view.ShowCheckBoxes.Checked, Is.EqualTo(showCheckBox));
        }

        [Ignore("Must be rewritten")]
        [TestCase(true)]
        [TestCase(false)]
        public void WithVisualState_ShowNamespaceIsAppliedFromVisualState(bool showNamespace)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState()
            {
                ShowNamespace = showNamespace
            };
            visualState.Save(VISUAL_STATE_FILE_NAME);

            // Act: Load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_model.TreeConfiguration.ShowNamespaces, Is.EqualTo(showNamespace));
        }

        [TestCase("NUNIT_TREE", typeof(NUnitTreeDisplayStrategy))]
        [TestCase("TEST_LIST", typeof(TestListDisplayStrategy))]
        public void WithVisualState_TreeStrategy_IsCreatedFromVisualState(string displayFormat, Type expectedStrategy)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState()
            {
                DisplayStrategy = displayFormat
            };
            visualState.Save(VISUAL_STATE_FILE_NAME);

            TryLoadVisualStateReturns(visualState);

            // Act: Load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_presenter.Strategy, Is.TypeOf(expectedStrategy));
        }

        [Ignore("Rewrite")]
        [TestCase("NUNIT_TREE")]
        [TestCase("TEST_LIST")]
        public void WithVisualState_DisplayFormatSettingIsUpdatedFromVisualState(string displayFormat)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState()
            {
                DisplayStrategy = displayFormat
            };
            visualState.Save(VISUAL_STATE_FILE_NAME);

            TryLoadVisualStateReturns(visualState);

            // Act: Load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_model.TreeConfiguration.DisplayFormat, Is.EqualTo(displayFormat));
        }

        [TestCase("UNGROUPED")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        [TestCase("DURATION")]
        public void WithVisualState_NUnitTreeGroupBySettingIsUpdatedFromVisualState(string groupBy)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState()
            {
                DisplayStrategy = "NUNIT_TREE",
                GroupBy = groupBy
            };
            //visualState.Save(VISUAL_STATE_FILE_NAME);
            _model.TryLoadVisualState(out Arg.Any<VisualState>()).Returns(x =>
            {
                x[0] = visualState;
                return true;
            });

            var treeConfig = new TreeConfiguration()
            {
                DisplayFormat = "NUNIT_TREE",
                NUnitGroupBy = groupBy,
                TestListGroupBy = groupBy
            };
            _model.TreeConfiguration.Returns(treeConfig);
            //var tv = new TreeView();
            //_view.TreeView.Returns(tv);

            // Act: Load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_model.TreeConfiguration.NUnitGroupBy, Is.EqualTo(groupBy));
            //Assert.That(_model.TreeConfiguration.TestListGroupBy, Is.EqualTo("UNGROUPED"));     // Assert that testList groupBy is reset
        }

        [TestCase("ASSEMBLY")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        public void WithVisualState_TestListGroupBySettingIsUpdatedFromVisualState(string groupBy)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState()
            {
                DisplayStrategy = "TEST_LIST",
                GroupBy = groupBy
            };
            visualState.Save(VISUAL_STATE_FILE_NAME);

            var treeConfig = new TreeConfiguration()
            {
                DisplayFormat = "TEST_LIST",
                NUnitGroupBy = groupBy,
                TestListGroupBy = groupBy
            };
            _model.TreeConfiguration.Returns(treeConfig);
            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            Assert.That(_model.TreeConfiguration.TestListGroupBy, Is.EqualTo(groupBy));
            //Assert.That(_model.TreeConfiguration.NUnitGroupBy, Is.EqualTo("UNGROUPED"));     // Assert that NUnit groupBy is reset
        }

        [Test]
        public void CategoryFilterIsInitialized()
        {
            // Act: load tests
            FireTestLoadedEvent(TEST_NODE);

            // Assert
            _view.CategoryFilter.Received().Init(_model);
        }

        private void TryLoadVisualStateReturns(VisualState visualState)
        {
            _model.TryLoadVisualState(out Arg.Any<VisualState>())
                .Returns(x => {
                    x[0] = visualState;
                    return true;
                });
        }

        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //[Platform(Exclude = "Linux", Reason = "Display issues")]
        //public void WhenTestLoadCompletes_PropertyDialogIsClosed()
        //{
        //    ClearAllReceivedCalls();
        //    _model.TestFiles.Returns(new List<string>(new[] { "test.dll" }));
        //    FireTestLoadedEvent(new TestNode("<test-run id='2'/>"));

        //    _view.Received().CheckPropertiesDialog();
        //}

        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //[Platform(Exclude = "Linux", Reason = "Display issues")]
        //public void WhenTestLoadCompletes_MultipleAssemblies_TopNodeIsTestRun()
        //{
        //    ClearAllReceivedCalls();
        //    _model.TestFiles.Returns(new List<string>(new[] { "test.dll", "another.dll" }));
        //    FireTestLoadedEvent(testNode);

        //    _view.Tree.Received().Load(Arg.Compat.Is<TreeNode>((tn) => tn.Text == "TestRun" && tn.Nodes.Count == 2));
        //}

        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //[Platform(Exclude = "Linux", Reason = "Display issues")]
        //public void WhenTestLoadCompletes_SingleAssembly_TopNodeIsAssembly()
        //{
        //    ClearAllReceivedCalls();
        //    _model.TestFiles.Returns(new List<string>(new[] { "test.dll" }));
        //    FireTestLoadedEvent(testNode);

        //    _view.Tree.Received().Load(Arg.Compat.Is<TreeNode>(tn => tn.Text == "another.dll"));
        //}
    }
}
