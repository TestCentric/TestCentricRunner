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

    public class WhenTestSuiteCompletes : PresenterTestBase<ITestTreeView>
    {
        private TreeViewPresenter _presenter;

        [SetUp]
        public void Setup()
        {
            _presenter = new TreeViewPresenter(_view, _model, new TreeDisplayStrategyFactory());
        }

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

        [TestCaseSource("resultData")]
        public void TreeShowsProperResult(ResultState resultState, int expectedIndex)
        {
            var result = resultState.Status.ToString();
            var label = resultState.Label;

            var testNode = new TestNode("<test-run id='1'><test-suite id='100'><test-case id='200'/></test-suite></test-run>");
            var resultNode = new ResultNode(string.IsNullOrEmpty(label)
                ? string.Format("<test-suite id='100' result='{0}'/>", result)
                : string.Format("<test-suite id='100' result='{0}' label='{1}'/>", result, label));
            var testCaseResultNode = new ResultNode(string.IsNullOrEmpty(label)
                                                ? string.Format("<test-suite id='200' result='{0}'/>", result)
                                                : string.Format("<test-suite id='200' result='{0}' label='{1}'/>", result, label));

            _model.GetTestById("100").Returns(testNode.Children.First());
            _model.TestResultManager.GetResultForTest("100").Returns(resultNode);
            _model.TestResultManager.GetResultForTest("200").Returns(testCaseResultNode);

            // Make it look like the view loaded
            _view.Load += Raise.Event<System.EventHandler>(_view, new System.EventArgs());

            TreeView treeView = new TreeView();
            _view.TreeView.Returns(treeView);

            // We can't construct a TreeNodeCollection, so we fake it
            var nodes = new TreeNode().Nodes;
            nodes.Add(new TreeNode("test.dll"));
            _view.Nodes.Returns(nodes);

            _model.Events.TestLoaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(testNode));
            _model.Events.SuiteFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(resultNode));

            _view.Received().SetImageIndex(Arg.Compat.Any<TreeNode>(), expectedIndex);
        }
    }
}
