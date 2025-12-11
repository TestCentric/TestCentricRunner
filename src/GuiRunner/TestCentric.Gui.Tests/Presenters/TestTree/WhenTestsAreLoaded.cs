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

    public class WhenTestsAreLoaded : TreeViewPresenterTestBase
    {
        // Use dedicated test file name; Used for VisualState file too
        const string TestFileName = "TreeViewPresenterTestsLoaded.dll";

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _treeDisplayStrategyFactory = new TreeDisplayStrategyFactory();
        }

        [SetUp]
        public void SimulateTestLoad()
        {
            ClearAllReceivedCalls();

            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(false);
            var project = new TestCentricProject(_model, TestFileName);
            _model.TestCentricProject.Returns(project);

            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);
        }

        [TearDown]
        public void TearDown()
        {
            // Delete VisualState file to prevent any unintended side effects
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            if (File.Exists(fileName))
                File.Delete(fileName);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestLoaded_NoVisualState_ShowCheckBox_IsAppliedFromSettings(bool showCheckBoxSetting)
        {
            // Arrange: adapt settings
            _model.Settings.Gui.TestTree.ShowCheckBoxes = showCheckBoxSetting;

            // Act: load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_view.ShowCheckBoxes.Checked, Is.EqualTo(showCheckBoxSetting));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestLoaded_WithVisualState_ShowCheckBox_IsAppliedFromVisualState(bool showCheckBox)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState();
            visualState.ShowCheckBoxes = showCheckBox;
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            visualState.Save(fileName);

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_view.ShowCheckBoxes.Checked, Is.EqualTo(showCheckBox));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestLoaded_NoVisualState_ShowNamespace_IsAppliedFromSettings(bool showNamespace)
        {
            // Arrange: adapt settings
            _model.Settings.Gui.TestTree.ShowNamespace = showNamespace;

            // Act: load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_model.Settings.Gui.TestTree.ShowNamespace, Is.EqualTo(showNamespace));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestLoaded_WithVisualState_ShowNamespace_IsAppliedFromVisualState(bool showNamespace)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState();
            visualState.ShowNamespace = showNamespace;
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            visualState.Save(fileName);

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_settings.Gui.TestTree.ShowNamespace, Is.EqualTo(showNamespace));
        }

        [TestCase("NUNIT_TREE", typeof(NUnitTreeDisplayStrategy))]
        [TestCase("TEST_LIST", typeof(TestListDisplayStrategy))]
        public void TestLoaded_WithVisualState_TreeStrategy_IsCreatedFromVisualState(string displayFormat, Type expectedStrategy)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState();
            visualState.DisplayStrategy = displayFormat;
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            visualState.Save(fileName);

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_presenter.Strategy, Is.TypeOf(expectedStrategy));
        }

        [TestCase("NUNIT_TREE", typeof(NUnitTreeDisplayStrategy))]
        [TestCase("TEST_LIST", typeof(TestListDisplayStrategy))]
        public void TestLoaded_NoVisualState_TreeStrategy_IsCreatedFromSettings(string displayFormat, Type expectedStrategy)
        {
            // Arrange: adapt settings
            _model.Settings.Gui.TestTree.DisplayFormat = displayFormat;

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_presenter.Strategy, Is.TypeOf(expectedStrategy));
        }

        [TestCase("NUNIT_TREE")]
        [TestCase("TEST_LIST")]
        public void TestLoaded_WithVisualState_DisplayFormatSetting_IsUpdatedFromVisualState(string displayFormat)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState();
            visualState.DisplayStrategy = displayFormat;
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            visualState.Save(fileName);

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_model.Settings.Gui.TestTree.DisplayFormat, Is.EqualTo(displayFormat));
        }

        [TestCase("UNGROUPED")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        [TestCase("DURATION")]
        public void TestLoaded_WithVisualState_NUnitTreeGroupBySetting_IsUpdatedFromVisualState(string groupBy)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState();
            visualState.DisplayStrategy = "NUNIT_TREE";
            visualState.GroupBy = groupBy;
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            visualState.Save(fileName);

            _model.Settings.Gui.TestTree.TestList.GroupBy = "DURATION";

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_model.Settings.Gui.TestTree.NUnitGroupBy, Is.EqualTo(groupBy));
            Assert.That(_model.Settings.Gui.TestTree.TestList.GroupBy, Is.EqualTo("DURATION"));     // Assert that testList groupBy was not changed accidently
        }

        [TestCase("ASSEMBLY")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        public void TestLoaded_WithVisualState_TestListGroupBySetting_IsUpdatedFromVisualState(string groupBy)
        {
            // Arrange: Create and save VisualState file
            VisualState visualState = new VisualState();
            visualState.DisplayStrategy = "TEST_LIST";
            visualState.GroupBy = groupBy;
            string fileName = VisualState.GetVisualStateFileName(TestFileName);
            visualState.Save(fileName);

            _model.Settings.Gui.TestTree.NUnitGroupBy = "DURATION";

            var tv = new TreeView();
            _view.TreeView.Returns(tv);

            // Act: Load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            Assert.That(_model.Settings.Gui.TestTree.TestList.GroupBy, Is.EqualTo(groupBy));
            Assert.That(_model.Settings.Gui.TestTree.NUnitGroupBy, Is.EqualTo("DURATION"));     // Assert that NUnit groupBy was not changed accidently
        }

        [Test]
        public void TestLoaded_CategoryFilter_IsInitialized()
        {
            // Act: load tests
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            FireTestLoadedEvent(testNode);

            // Assert
            _view.CategoryFilter.Received().Init(_model);
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
        //    TestNode testNode = new TestNode("<test-run id='2'><test-suite id='101' name='test.dll'/><test-suite id='102' name='another.dll'/></test-run>");
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
        //    TestNode testNode = new TestNode("<test-run><test-suite id='1' name='another.dll'/></test-run>");
        //    ClearAllReceivedCalls();
        //    _model.TestFiles.Returns(new List<string>(new[] { "test.dll" }));
        //    FireTestLoadedEvent(testNode);

        //    _view.Tree.Received().Load(Arg.Compat.Is<TreeNode>(tn => tn.Text == "another.dll"));
        //}
    }
}
