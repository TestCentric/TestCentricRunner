// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;
using NUnit.Framework;
using NSubstitute;
using TestCentric.Gui.Model;
using System;
using TestCentric.Gui.Views;
using System.Collections.Generic;
using System.Linq;
using TestCentric.Gui.Elements;

namespace TestCentric.Gui.Presenters.TestTree
{
    using TestCentric.Gui.Model.Settings;

    public class TreeViewPresenterTests : TreeViewPresenterTestBase
    {
        [TestCase(true)]
        [TestCase(false)]
        public void WhenTreeConfigurationIsChanged_ShowCheckBoxes_NewValueIsApplied(bool showCheckBoxSetting)
        {
            _model.TreeConfiguration.ShowCheckBoxes.Returns(showCheckBoxSetting);

            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs(nameof(TreeConfiguration.ShowCheckBoxes)));

            Assert.That(_view.ShowCheckBoxes.Checked, Is.EqualTo(showCheckBoxSetting));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WhenTreeConfigurationIsChanged_ShowNamespace_StrategyIsReloaded(bool showNamespace)
        {
            ITreeDisplayStrategy strategy = Substitute.For<ITreeDisplayStrategy>();
            _treeDisplayStrategyFactory.Create(null, null, null).ReturnsForAnyArgs(strategy);
            _model.TreeConfiguration.DisplayFormat.Returns("NUNIT_TREE");
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs(nameof(TreeConfiguration.DisplayFormat)));

            // Act
            _model.TreeConfiguration.ShowNamespaces = showNamespace;
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs(nameof(TreeConfiguration.ShowNamespaces)));


            // Assert
            strategy.Received(2).Reload();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WhenSettingsAreChanged_ShowFilter_FilterVisibilityIsCalled(bool show)
        {

            // Act
            _model.Settings.Gui.TestTree.ShowFilter.Returns(show);
            _settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.ShowFilter"));

            // Assert
            _view.Received().SetTestFilterVisibility(show);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void WhenContextMenu_ShowTestDuration_IsClicked_SettingsIsUpdated(bool showTestDuration)
        {
            // 1. Arrange
            _view.ShowTestDuration.Checked = showTestDuration;

            // 2. Act
            _view.ShowTestDuration.CheckedChanged += Raise.Event<CommandHandler>();

            // 3. Assert
            Assert.That(_model.TreeConfiguration.ShowTestDuration, Is.EqualTo(showTestDuration));
        }

        [Test]
        public void WhenContextMenuIsDisplayed_GuiMiniLayout_TestPropertiesContextMenu_IsVisible()
        {
            // 1. Arrange
            _model.Settings.Gui.GuiLayout = "Mini";

            // 2. Act
            _view.ContextMenuOpening += Raise.Event<EventHandler>();

            // 3. Assert
            Assert.That(_view.TestPropertiesCommand.Visible, Is.True);
        }

        [Test]
        public void WhenContextMenuIsDisplayed_GuiFullLayout_TestPropertiesContextMenu_IsNotVisible()
        {
            // 1. Arrange
            _model.Settings.Gui.GuiLayout = "Full";

            // 2. Act
            _view.ContextMenuOpening += Raise.Event<EventHandler>();

            // 3. Assert
            Assert.That(_view.TestPropertiesCommand.Visible, Is.False);
        }

        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void WhenContextMenuIsDisplayed_RunCommand_EnabledState_IsUpdated(bool hasTests, bool isTestRunning, bool expectedEnabled)
        {
            // 1. Arrange
            _model.HasTests.Returns(hasTests);
            _model.IsTestRunning.Returns(isTestRunning);

            // 2. Act
            _view.ContextMenuOpening += Raise.Event<EventHandler>();

            // 3. Assert
            Assert.That(_view.RunContextCommand.Enabled, Is.EqualTo(expectedEnabled));
        }

        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void WhenContextMenuIsDisplayed_DebugCommand_EnabledState_IsUpdated(bool hasTests, bool isTestRunning, bool expectedEnabled)
        {
            // 1. Arrange
            _model.HasTests.Returns(hasTests);
            _model.IsTestRunning.Returns(isTestRunning);

            // 2. Act
            _view.ContextMenuOpening += Raise.Event<EventHandler>();

            // 3. Assert
            Assert.That(_view.DebugContextCommand.Enabled, Is.EqualTo(expectedEnabled));
        }

        [TestCase(true, true, false)]
        [TestCase(true, false, true)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void WhenContextMenuIsDisplayed_ClearResultCommand_EnabledState_IsUpdated(bool hasTests, bool isTestRunning, bool expectedEnabled)
        {
            // 1. Arrange
            _model.HasResults.Returns(hasTests);
            _model.IsTestRunning.Returns(isTestRunning);

            // 2. Act
            _view.ContextMenuOpening += Raise.Event<EventHandler>();

            // 3. Assert
            Assert.That(_view.ClearResultsContextCommand.Enabled, Is.EqualTo(expectedEnabled));
        }

        [Test]
        public void TreeCheckBoxClicked_NoNodeSelected_SelectedTestsInModel_AreEmpty()
        {
            // 1. Arrange
            IList<TreeNode> checkedNodes = new List<TreeNode>();
            _view.CheckedNodes.Returns(checkedNodes);

            // 2. Act
            _view.AfterCheck += Raise.Event<TreeNodeActionHandler>((TreeNode)null);

            // 3. Assert
            TestSelection testSelection = _model.SelectedTests;
            Assert.That(testSelection, Is.Empty);
        }

        [Test]
        public void TreeCheckBoxClicked_TreeNodesAreSelected_AllTests_AreSelected()
        {
            // 1. Arrange
            var treeNode1 = new TreeNode() { Tag = new TestNode("<test-case id='1' />") };
            var treeNode2 = new TreeNode() { Tag = new TestNode("<test-case id='2' />") };

            IList<TreeNode> checkedNodes = new List<TreeNode>() { treeNode1, treeNode2 };
            _view.CheckedNodes.Returns(checkedNodes);

            // 2. Act
            _view.AfterCheck += Raise.Event<TreeNodeActionHandler>((TreeNode)null);

            // 3. Assert
            TestSelection testSelection = _model.SelectedTests;
            Assert.That(testSelection.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TreeCheckBoxClicked_GroupIsSelected_AllTestsOfGroup_AreSelected()
        {
            // 1. Arrange
            var subTestNode1 = new TestNode("<test-case id='2' />");
            var subTestNode2 = new TestNode("<test-case id='3' />");
            var testGroup = new TestGroup("Category_1") {  subTestNode1, subTestNode2 };

            var treeNode1 = new TreeNode() { Tag = testGroup };

            IList<TreeNode> checkedNodes = new List<TreeNode>() { treeNode1 };
            _view.CheckedNodes.Returns(checkedNodes);

            // 2. Act
            _view.AfterCheck += Raise.Event<TreeNodeActionHandler>((TreeNode)null);

            // 3. Assert
            TestSelection testSelection = _model.SelectedTests;
            Assert.That(testSelection.Count(), Is.EqualTo(2));
        }

        [Test]
        public void TreeCheckBoxClicked_TreeNodeAndGroupIsSelected_AllTests_AreSelected()
        {
            // 1. Arrange
            var subTestNode1 = new TestNode("<test-case id='1' />");
            var subTestNode2 = new TestNode("<test-case id='2' />");
            var testGroup = new TestGroup("Category_1") { subTestNode1, subTestNode2 };

            var treeNode1 = new TreeNode() { Tag = testGroup };
            var treeNode2 = new TreeNode() { Tag = new TestNode("<test-case id='3' />") };

            IList<TreeNode> checkedNodes = new List<TreeNode>() { treeNode1, treeNode2 };
            _view.CheckedNodes.Returns(checkedNodes);

            // 2. Act
            _view.AfterCheck += Raise.Event<TreeNodeActionHandler>((TreeNode)null);

            // 3. Assert
            TestSelection testSelection = _model.SelectedTests;
            Assert.That(testSelection.Count(), Is.EqualTo(3));
        }

        [Test]
        public void TreeCheckBoxClicked_TreeNodeInsideGroupIsSelected_AllTests_AreSelected()
        {
            // 1. Arrange
            var subTestNode1 = new TestNode("<test-case id='1' />");
            var subTestNode2 = new TestNode("<test-case id='2' />");
            var testGroup = new TestGroup("Category_1") { subTestNode1, subTestNode2 };

            var treeNode1 = new TreeNode() { Tag = testGroup };
            var treeNode2 = new TreeNode() { Tag = subTestNode1 };

            IList<TreeNode> checkNodes = new List<TreeNode>() { treeNode1, treeNode2 };
            _view.CheckedNodes.Returns(checkNodes);

            // 2. Act
            _view.AfterCheck += Raise.Event<TreeNodeActionHandler>((TreeNode)null);

            // 3. Assert
            TestSelection testSelection = _model.SelectedTests;
            Assert.That(testSelection.Count(), Is.EqualTo(2));
        }

        [Test]
        public void OutcomeFilterChanged_ApplyFilter()
        {
            // 1. Arrange
            var selectedItems = new List<string>() { "Passed" };
            _view.OutcomeFilter.SelectedItems.Returns(selectedItems);

            // 2. Act
            _view.OutcomeFilter.SelectionChanged += Raise.Event<CommandHandler>();

            // 3. Assert
            _model.TestCentricTestFilter.Received().OutcomeFilter = selectedItems;
        }

        [Test]
        public void TextFilterChanged_ApplyFilter()
        {
            // 1. Arrange
            _view.TextFilter.Text.Returns("TestA");

            // 2. Act
            _view.TextFilter.Changed += Raise.Event<CommandHandler>();

            // 3. Assert
            _model.TestCentricTestFilter.Received().TextFilter = "TestA";
        }

        [Test]
        public void FilterChanged_ReloadTree_IsInvoked()
        {
            // 1. Arrange
            ITreeDisplayStrategy strategy = Substitute.For<ITreeDisplayStrategy>();
            _treeDisplayStrategyFactory.Create(null, null, null).ReturnsForAnyArgs(strategy);
            _model.TreeConfiguration.DisplayFormat.Returns("NUNIT_TREE");
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs(nameof(TreeConfiguration.DisplayFormat)));

            // 2. Act
            _model.Events.TestFilterChanged += Raise.Event<TestEventHandler>(new TestEventArgs());

            // 3. Assert
            strategy.Received().Reload(true);
        }

        [Test]
        public void SortChanged_Sort_IsInvoked()
        {
            // 1. Arrange

            // 2. Act
            _view.SortCommand.SelectionChanged += Raise.Event<CommandHandler>();

            // 3. Assert
            _view.ReceivedWithAnyArgs().Sort(null);
        }

        [Test]
        public void SortDirectionChanged_Sort_IsInvoked()
        {
            // 1. Arrange

            // 2. Act
            _view.SortCommand.SelectionChanged += Raise.Event<CommandHandler>();

            // 3. Assert
            _view.ReceivedWithAnyArgs().Sort(null);
        }

        [Test]
        public void SortChanged_ToDuration_ShowDuration_IsSet()
        {
            // 1. Arrange
            _view.SortCommand.SelectedItem.Returns(TreeViewNodeComparer.Duration);

            // 2. Act
            _view.SortCommand.SelectionChanged += Raise.Event<CommandHandler>();

            // 3. Assert
            _view.ShowTestDuration.Received().Checked = true;
        }

        [Test]
        public void ResetFilterCommand_TestCentricTestFilter_AreReset()
        {
            // 1. Arrange
            _view.SortCommand.SelectedItem.Returns(TreeViewNodeComparer.Duration);

            // 2. Act
            _view.ResetFilterCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            _model.TestCentricTestFilter.Received().ResetAll();
        }

        [Test]
        public void ResetFilterCommand_Filter_UIElements_AreReset()
        {
            // 1. Arrange
            _view.SortCommand.SelectedItem.Returns(TreeViewNodeComparer.Duration);

            // 2. Act
            _view.ResetFilterCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            _view.TextFilter.Received().Text = "";
            _view.OutcomeFilter.ReceivedWithAnyArgs().SelectedItems = null;
            _view.CategoryFilter.ReceivedWithAnyArgs().SelectedItems = null;
        }

        [Test]
        public void ClearResultsCommand_Executed_ClearResults()
        {
            // 1. Arrange
            ITreeDisplayStrategy strategy = Substitute.For<ITreeDisplayStrategy>();
            _treeDisplayStrategyFactory.Create(null, null, null).ReturnsForAnyArgs(strategy);
            _model.TreeConfiguration.DisplayFormat.Returns("NUNIT_TREE");
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs(nameof(TreeConfiguration.DisplayFormat)));

            // 2. Act
            _view.ClearResultsContextCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            _model.Received().ClearResults();
        }

        [Test]
        public void ClearResultsCommand_Executed_ReloadTree()
        {
            // 1. Arrange
            ITreeDisplayStrategy strategy = Substitute.For<ITreeDisplayStrategy>();
            _treeDisplayStrategyFactory.Create(null, null, null).ReturnsForAnyArgs(strategy);
            _model.TreeConfiguration.DisplayFormat.Returns("NUNIT_TREE");
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs(nameof(TreeConfiguration.DisplayFormat)));

            // 2. Act
            _view.ClearResultsContextCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            strategy.Received().Reload();
        }

        [Test]
        public void RemoveTestPackageCommand_SelectedNode_IsNull_TestPackageIsNotRemoved()
        {
            // 1. Arrange
            _view.SelectedNode.Returns((TreeNode)null);

            // 2. Act
            _view.RemoveTestPackageCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            _model.DidNotReceiveWithAnyArgs().RemoveTestPackage(null);
        }

        [TestCase(false, false, "TestSuite")]
        [TestCase(true, false, "TestSuite")]
        [TestCase(true, false, "TestSuite"), Ignore("Needs work")]
        [TestCase(true, false, "Assembly")]
        public void RemoveTestPackageCommand_NotAllConditionsMeet_TestPackageIsNotRemoved(bool hasTests, bool isTestRunning, string testNodeType)
        {
            // 1. Arrange
            TestNode testNode = new TestNode($"<test-suite id='1' type='{testNodeType}' />");
            TreeNode treeNode = new TreeNode();
            treeNode.Tag = testNode; 
            _view.SelectedNode.Returns(treeNode);
            _model.HasTests.Returns(hasTests);
            _model.IsTestRunning.Returns(isTestRunning);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            _model.TestCentricProject.Returns(project);

            // 2. Act
            _view.RemoveTestPackageCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            _model.DidNotReceiveWithAnyArgs().RemoveTestPackage(null);
        }

        [Test, Ignore("Needs work")]
        public void RemoveTestPackageCommand_AllConditionsMeet_TestPackageIsRemoved()
        {
            // 1. Arrange
            TestNode testNode = new TestNode($"<test-suite id='1' type='Assembly' />");
            TreeNode treeNode = new TreeNode();
            treeNode.Tag = testNode;
            _view.SelectedNode.Returns(treeNode);
            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(false);

            var project = new TestCentricProject(new GuiOptions("dummy.dll", "dummy2.dll"));
            _model.TestCentricProject.Returns(project);

            // 2. Act
            _view.RemoveTestPackageCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            _model.ReceivedWithAnyArgs().RemoveTestPackage(null);
        }

        [Test]
        public void WhenContextMenuIsDisplayed_ContextNode_IsNull_RemoveTestPackageCommandContextMenu_IsNotVisible()
        {
            // 1. Arrange
            _view.ContextNode.Returns((TreeNode)null);

            // 2. Act
            _view.RemoveTestPackageCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            Assert.That(_view.RemoveTestPackageCommand.Visible, Is.False);
        }

        [TestCase(false, false, "TestSuite")]
        [TestCase(true, false, "TestSuite")]
        [TestCase(true, false, "TestSuite")]
        [TestCase(true, false, "Assembly")]
        public void WhenContextMenuIsDisplayed_RemoveTestPackageCommandContextMenu_IsNotVisible(bool hasTests, bool isTestRunning, string testNodeType)
        {
            // 1. Arrange
            TestNode testNode = new TestNode($"<test-suite id='1' type='{testNodeType}' />");
            TreeNode treeNode = new TreeNode();
            treeNode.Tag = testNode;
            _view.ContextNode.Returns(treeNode);
            _model.HasTests.Returns(hasTests);
            _model.IsTestRunning.Returns(isTestRunning);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            _model.TestCentricProject.Returns(project);

            // 2. Act
            _view.RemoveTestPackageCommand.Execute += Raise.Event<CommandHandler>();

            // 3. Assert
            Assert.That(_view.RemoveTestPackageCommand.Visible, Is.False);
        }

        [Test, Ignore("Needs work")]
        public void WhenContextMenuIsDisplayed_RemoveTestPackageCommandContextMenu_IsVisible()
        {
            // 1. Arrange
            TestNode testNode = new TestNode($"<test-suite id='1' type='Assembly' />");
            TreeNode treeNode = new TreeNode();
            treeNode.Tag = testNode;
            _view.ContextNode.Returns(treeNode);
            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(false);

            var project = new TestCentricProject(new GuiOptions("dummy.dll", "dummy2.dll"));
            _model.TestCentricProject.Returns(project);

            // 2. Act
            _view.ContextMenuOpening += Raise.Event<EventHandler>();

            // 3. Assert
            Assert.That(_view.RemoveTestPackageCommand.Visible, Is.True);
        }

        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //public void WhenContextNodeIsNotNull_RunCommandExecutesThatTest()
        //{
        //    var testNode = new TestNode("<test-case id='DUMMY-ID'/>");
        //    _view.ContextNode.Returns(new TestSuiteTreeNode(testNode));

        //    _view.RunCommand.Execute += Raise.Event<CommandHandler>();

        //    _model.Received().RunTests(testNode);
        //}

        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //public void WhenContextNodeIsNull_RunCommandExecutesSelectedTests()
        //{
        //    var testNodes = new[] { new TestNode("<test-case id='DUMMY-1'/>"), new TestNode("<test-case id='DUMMY-2'/>") };
        //    _view.SelectedTests.Returns(testNodes);

        //    _view.RunCommand.Execute += Raise.Event<CommandHandler>();

        //    _model.Received().RunTests(Arg.Compat.Is<TestSelection>((sel) => sel.Count == 2 && sel[0].Id == "DUMMY-1" && sel[1].Id == "DUMMY-2"));
        //}



        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //public void WhenTestIsChanged_ReloadSettingsIsEnabled()
        //{
        //    _settings.Engine.ReloadOnChange = true;
        //    _model.Events.TestChanged += Raise.Event<TestEventHandler>(new TestEventArgs());
        //    _model.Received().ReloadTests();
        //}

        // TODO: Version 1 Test - Make it work if needed.
        //[Test]
        //public void WhenTestIsChanged_ReloadSettingsIsDisabled()
        //{
        //    _settings.Engine.ReloadOnChange = false;
        //    _model.Events.TestChanged += Raise.Event<TestEventHandler>(new TestEventArgs());
        //    _model.DidNotReceive().ReloadTests();
        //}

        //[Test]
        //public void WhenDisplayFormatChanges_TreeIsReloaded()
        //{
        //    TestNode testNode = new TestNode(XmlHelper.CreateXmlNode("<test-run id='1'><test-suite id='42'/></test-run>"));
        //    _model.Tests.Returns(testNode);

        //    _view.Tree.Received().Add(Arg.Compat.Is<TreeNode>((tn) => ((TestNode)tn.Tag).Id == "42"));
        //}

        //[Test, Combinatorial]
        //public void WhenContextMenuIsDisplayed_RunCheckedCommandVisibilityIsSet(
        //    [Values] bool showCheckBoxes)
        //{

        //    var testNode = new TestNode(XmlHelper.CreateXmlNode("<test-case/>"));
        //    var treeNode = new TreeNode()
        //    {
        //        Tag = testNode
        //    };

        //    _view.CheckBoxes.Returns(showCheckBoxes);
        //    _view.ShowCheckBoxes.Checked.Returns(showCheckBoxes);
        //    _view.ContextNode.Returns(treeNode);
        //    _view.TreeContextMenu.Returns(new ContextMenuStrip());

        //    _view.ClearReceivedCalls();
        //    _view.ContextMenuOpening += Raise.Event<EventHandler>();
        //    ViewElement("RunCheckedCommand").Received().Visible = showCheckBoxes;
        //}
    }
}
