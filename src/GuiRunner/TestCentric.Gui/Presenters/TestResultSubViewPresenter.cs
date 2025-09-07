// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Gui.Model;
using TestCentric.Gui.Model.Settings;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters
{
    public interface ITestResultSubViewPresenter
    {
        /// <summary>
        /// Clear test result sub view
        /// </summary>
        void Clear();

        /// <summary>
        /// Update test result sub view to display result of testnode/testGroup
        /// </summary>
        void Update(ITestItem testItem);
    }

    public class TestResultSubViewPresenter : ITestResultSubViewPresenter
    {
        private ITestResultSubView _view;
        private ITestModel _model;

        public TestResultSubViewPresenter(ITestResultSubView view, ITestModel model)
        {
            _view = view;
            _model = model;
        }

        /// <summary>
        /// Clear test result sub view
        /// </summary>
        public void Clear()
        {
            _view.Clear();
        }

        /// <summary>
        /// Update test result sub view to display result of testnode/testGroup
        /// </summary>
        public void Update(ITestItem testItem)
        {
            TestResultCounts summary = TestResultCounts.GetResultCounts(_model, testItem);

            bool detailSectionVisible = false;
            ResultState overallOutcome = null;
            if (testItem is TestNode testNode)
            {
                ResultNode result = _model.TestResultManager.GetResultForTest(testNode.Id);
                overallOutcome = result?.Outcome;
                detailSectionVisible = testNode.IsAssembly || testNode.IsProject || testNode.IsSuite;
            }
            else if (testItem is TestGroup testGroup)
            {
                overallOutcome = GetGroupOutcome(testGroup);
                detailSectionVisible = true;
            }

            _view.UpdateCaption(summary, overallOutcome);
            _view.UpdateDetailSectionVisibility(detailSectionVisible);
            if (!detailSectionVisible)
            {
                _view.ShrinkToCaption();
                return;
            }

            _view.UpdateDetailSection(summary);
        }

        private ResultState GetGroupOutcome(TestGroup testGroup)
        {
            ResultState state = null;
            foreach(TestNode testNode in testGroup)
            {
                ResultNode result = _model.TestResultManager.GetResultForTest(testNode.Id);
                if (result == null)
                    continue;

                if (result.Outcome.Status == TestStatus.Failed)
                    return ResultState.Failure;

                if (result.Outcome.Status == TestStatus.Warning)
                    state = ResultState.Warning;

                if (result.Outcome.Status == TestStatus.Skipped && state?.Status != TestStatus.Warning)
                    state = result.Outcome.Label == "Ignored" ? ResultState.Ignored : ResultState.Skipped;

                if (result.Outcome.Status == TestStatus.Passed && state == null)
                    state = ResultState.Success;
            }

            return state;
        }
    }

}
