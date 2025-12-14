// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Windows.Forms;
using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.TestTree
{
    using Elements;
    using TestCentric.Gui.Model;
    using Views;

    public class TestExecutionTests : TreeViewPresenterTestBase
    {
        private static TestNode TEST_CASE_NODE = new TestNode(XmlHelper.CreateXmlNode("<test-case/>"));
        private static TreeNode TEST_CASE_TREE_NODE = new TreeNode()
        {
            Tag = TEST_CASE_NODE
        };

        private static TestNode TEST_SUITE_NODE = new TestNode(XmlHelper.CreateXmlNode("<test-suite/>"));
        private static TreeNode TEST_SUITE_TREE_NODE = new TreeNode()
        {
            Tag = TEST_SUITE_NODE
        };

        [Test]
        public void RunContextCommandOnTestCaseRunsTest()
        {
            _view.ContextNode.Returns(TEST_CASE_TREE_NODE);

            _view.RunContextCommand.Execute += Raise.Event<CommandHandler>();
            _model.Received().RunTests(Arg.Is(TEST_CASE_NODE));
        }

        [Test]
        public void RunContextCommandOnTestSuiteRunsTest()
        {
            _view.ContextNode.Returns(TEST_SUITE_TREE_NODE);

            _view.RunContextCommand.Execute += Raise.Event<CommandHandler>();
            _model.Received().RunTests(Arg.Is(TEST_SUITE_NODE));
        }

        [Test]
        public void DebugContextCommand_DebugsTest()
        {
            _view.ContextNode.Returns(TEST_SUITE_TREE_NODE);

            _view.DebugContextCommand.Execute += Raise.Event<CommandHandler>();
            _model.Received().DebugTests(Arg.Is(TEST_SUITE_NODE));
        }

        [Test]
        public void DebugContextCommand_TestGroup_DebugsTest()
        {
            // Arrange
            TestGroup testGroup = new TestGroup("TestGroup");
            var treeNode = new TreeNode() { Tag = testGroup };

            _view.ContextNode.Returns(treeNode);

            // Act
            _view.DebugContextCommand.Execute += Raise.Event<CommandHandler>();

            // Assert
            _model.Received().DebugTests(Arg.Is(testGroup));
        }

        [Test]
        public void DoubleClickOnTestCaseRunsTest()
        {
            _view.TreeNodeDoubleClick += Raise.Event<TreeNodeActionHandler>(TEST_CASE_TREE_NODE);
            _model.Received().RunTests(Arg.Is(TEST_CASE_NODE));
        }

        [Test]
        public void DoubleClickOnTestCase_ATestIsRunning_DoesNotRunTest()
        {
            _model.IsTestRunning.Returns(true);
            _view.TreeNodeDoubleClick += Raise.Event<TreeNodeActionHandler>(TEST_CASE_TREE_NODE);
            _model.DidNotReceive().RunTests(Arg.Is(TEST_CASE_NODE));
        }

        [Test]
        public void DoubleClickOnTestSuiteDoesNotRunTest()
        {
            _view.TreeNodeDoubleClick += Raise.Event<TreeNodeActionHandler>(TEST_SUITE_TREE_NODE);
            _model.DidNotReceive().RunTests(Arg.Is(TEST_SUITE_NODE));
        }
    }
}
