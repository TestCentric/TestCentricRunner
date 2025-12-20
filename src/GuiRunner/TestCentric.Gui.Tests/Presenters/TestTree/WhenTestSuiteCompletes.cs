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

    public class WhenTestSuiteCompletes : TreeViewPresenterTestBase
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

        [TestCaseSource("resultData")]
        public void TreeShowsProperResult(ResultState resultState, int expectedIndex)
        {
            // Use concrete class NUnitTreeDisplayStrategy for this test case to assert SetImageIndex call
            _treeDisplayStrategyFactory.Create("NUNIT_TREE", _view, _model)
                .Returns((x) => new NUnitTreeDisplayStrategy(x.Arg<ITestTreeView>(), x.Arg<ITestModel>()));
            _model.IsProjectLoaded.Returns(true);
            _model.HasTests.Returns(true);

            var result = resultState.Status.ToString();
            var label = resultState.Label;

            var testNode = new TestNode("<test-run id='1'><test-suite id='100'><test-case id='200'/></test-suite></test-run>");
            var resultNode = new ResultNode(string.IsNullOrEmpty(label)
                ? string.Format("<test-suite id='100' result='{0}'/>", result)
                : string.Format("<test-suite id='100' result='{0}' label='{1}'/>", result, label));
            var testCaseResultNode = new ResultNode(string.IsNullOrEmpty(label)
                                                ? string.Format("<test-suite id='200' result='{0}'/>", result)
                                                : string.Format("<test-suite id='200' result='{0}' label='{1}'/>", result, label));

            _model.LoadedTests.Returns(testNode);
            _model.GetTestById("100").Returns(testNode.Children.First());
            _model.TestResultManager.GetResultForTest("100").Returns(resultNode);
            _model.TestResultManager.GetResultForTest("200").Returns(testCaseResultNode);

            var project = new TestCentricProject(_model, "dummy.dll");
            _model.TestCentricProject.Returns(project);

            _model.Events.TestLoaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(testNode));
            _model.Events.SuiteFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(resultNode));

            _view.Received().SetImageIndex(Arg.Compat.Any<TreeNode>(), expectedIndex);
        }
    }
}
