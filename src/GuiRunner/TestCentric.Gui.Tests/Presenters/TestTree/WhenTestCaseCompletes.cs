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

    public class WhenTestCaseCompletes : TreeViewPresenterTestBase
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

        [TestCaseSource("resultData"), Ignore("Rewrite")]
        public void TreeShowsProperResult(ResultState resultState, int expectedIndex)
        {
            // Use concrete class NUnitTreeDisplayStrategy for this test case to assert SetImageIndex call
            _treeDisplayStrategyFactory.Create("NUNIT_TREE", _view, _model)
                            .Returns((x) => new NUnitTreeDisplayStrategy(x.Arg<ITestTreeView>(), x.Arg<ITestModel>()));

            _model.IsProjectLoaded.Returns(true);
            _model.HasTests.Returns(true);

            var result = resultState.Status.ToString();
            var label = resultState.Label;

            var testNode = new TestNode("<test-run id='1'><test-case id='123'/></test-run>");
            var resultNode = new ResultNode(string.IsNullOrEmpty(label)
                ? string.Format($"<test-case id='123' result='{result}'/>")
                : string.Format($"<test-case id='123' result='{result}' label='{label}'/>"));
            _model.LoadedTests.Returns(testNode);

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            _model.TestCentricProject.Returns(project);
            _model.GetTestById("123").Returns(testNode.Children.First());
            _model.TestResultManager.GetResultForTest("123").Returns(resultNode);

            //var treeNode = _adapter.MakeTreeNode(result);
            //_adapter.NodeIndex[suiteResult.Id] = treeNode;
            _model.Events.TestLoaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(testNode));
            _model.Events.TestFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(resultNode));

            _view.Received().SetImageIndex(Arg.Compat.Any<TreeNode>(), expectedIndex);
        }
    }
}
