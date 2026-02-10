// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;
using NUnit.Framework;
using NSubstitute;

namespace TestCentric.Gui.Presenters.TestTree
{
    using System.Linq;
    using Model;
    using Views;

    public class WhenTestCaseCompletes : PresenterTestBase<ITestTreeView>
    {
        static object[] resultData = new object[] {
            new object[] { ResultState.Success, TestTreeView.SuccessIndex },
            new object[] { ResultState.Ignored, TestTreeView.IgnoredIndex },
            new object[] { ResultState.Failure, TestTreeView.FailureIndex },
            new object[] { ResultState.Inconclusive, TestTreeView.InconclusiveIndex },
            new object[] { ResultState.Skipped, TestTreeView.SkippedIndex },
            new object[] { ResultState.NotRunnable, TestTreeView.FailureIndex },
            new object[] { ResultState.Error, TestTreeView.FailureIndex },
            new object[] { ResultState.Cancelled, TestTreeView.FailureIndex }
        };

        private TreeViewPresenter _presenter;

        [SetUp]
        public void Setup()
        {
            _presenter = new TreeViewPresenter(_view, _model, new TreeDisplayStrategyFactory());
        }

        [TestCaseSource("resultData")]
        public void TreeShowsProperResult(ResultState resultState, int expectedIndex)
        {
            var result = resultState.Status.ToString();
            var label = resultState.Label;

            var testNode = new TestNode("<test-run id='1'><test-case id='123'/></test-run>");
            var resultNode = new ResultNode(string.IsNullOrEmpty(label)
                ? string.Format($"<test-case id='123' result='{result}'/>")
                : string.Format($"<test-case id='123' result='{result}' label='{label}'/>"));

            _model.GetTestById("123").Returns(testNode.Children.First());
            _model.TestResultManager.GetResultForTest("123").Returns(resultNode);

            // Make it look like the view loaded
            _view.Load += Raise.Event<System.EventHandler>(_view, new System.EventArgs());

            TreeView treeView = new TreeView();
            _view.TreeView.Returns(treeView);

            var nodes = new TreeNode().Nodes; // Hack to construct a TreeNode collection
            nodes.Add(new TreeNode("test.dll"));
            _view.Nodes.Returns(nodes);

            _model.Events.TestLoaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(testNode));
            _model.Events.TestFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(resultNode));

            _view.Received().SetImageIndex(Arg.Compat.Any<TreeNode>(), expectedIndex);
        }
    }
}
