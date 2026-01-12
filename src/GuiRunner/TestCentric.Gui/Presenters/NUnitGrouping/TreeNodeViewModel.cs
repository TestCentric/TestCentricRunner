// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using TestCentric.Gui.Model;

    /// <summary>
    /// This class is responsible to provide a ViewModel for a TestNode/TreeNode
    /// This model extends a Nunit TestNode to support filtering and grouping.
    /// This includes mainly the child nodes, but also for example the result state.
    /// </summary>
    public class TreeNodeViewModel
    {
        private bool _resultUptodate;
        private ResultState _resultState;
        private int? _testCaseCount;
        private static CultureInfo DurationCultureInfo = new CultureInfo("en-US"); // Fixed culture to avoid culture specific comma separator

        public TreeNodeViewModel(ITestModel model, TestNode testNode, string name)
        {
            TestModel = model;
            AssociatedTestNode = testNode;
            Children = new List<TreeNodeViewModel>();
            Name = name;
            _resultState = null;
            _resultUptodate = false;
        }

        public string Name { get; }

        /// <summary>
        /// The Display name of a TestNode
        /// It includes the number of test cases and (if) the test duration
        /// Examples: "Feature_1 (5)" or "TestA [1.200s]"
        /// </summary>
        public string DisplayName
        {
            get
            {
                string name = Name;
                if (AssociatedTestNode.IsSuite)
                    name = $"{Name} ({TestCaseCount()})";

                if (!_resultUptodate)
                    CalculateResultState();

                if (TestModel.TreeConfiguration.ShowTestDuration && TestDuration.HasValue)
                    name += ((FormattableString)$" [{TestDuration.Value:0.000}s]").ToString(DurationCultureInfo);

                return name;
            }
        }

        public TestNode AssociatedTestNode { get; }

        public TreeNodeViewModel Parent { get; private set; }

        public IList<TreeNodeViewModel> Children { get; set; }

        public RunState RunState => AssociatedTestNode.RunState;

        public bool IsResultFromLastRun { get; private set; }

        private ITestModel TestModel { get; }

        public double? TestDuration { get; private set; }

        public virtual ITestItem TestItem => CreateTestItemCallback != null ? CreateTestItemCallback(this) : AssociatedTestNode;

        public Func<TreeNodeViewModel, ITestItem> CreateTestItemCallback { get; set; }

        /// <summary>
        /// Get all "test-case" nodes contained in the ViewModel node (recursive)
        /// </summary>
        public static IList<TreeNodeViewModel> GetTestCases(TreeNodeViewModel node)
        {
            List<TreeNodeViewModel> testNodes = new List<TreeNodeViewModel>();

            if (!node.AssociatedTestNode.IsSuite)
                testNodes.Add(node);
            else
                foreach (var child in node.Children)
                    testNodes.AddRange(GetTestCases(child));

            return testNodes;
        }

        /// <summary>
        /// Get all "test-case" nodes contained in the ViewModel node (recursive)
        /// - Omitting explicit child nodes
        /// - Include if ViewModel node is explicit itself
        /// </summary>
        public IList<TestNode> GetNonExplicitTests()
        {
            IList<TreeNodeViewModel> testNodes = GetTestCases(this);
            IList<TestNode> result = new List<TestNode>();

            foreach (TreeNodeViewModel test in testNodes)
                if ((test.RunState == RunState.Explicit && test.AssociatedTestNode == AssociatedTestNode) ||
                    (test.RunState != RunState.Explicit &&
                     (test.Parent.RunState != RunState.Explicit || test.Parent.AssociatedTestNode == AssociatedTestNode)))
                    result.Add(test.AssociatedTestNode);

            return result;
        }

        /// <summary>
        /// The result state of a ViewModel node is determined by considering and summing up the
        /// result of all child nodes
        /// </summary>
        public ResultState ResultState
        {
            get
            {
                if (!_resultUptodate)
                    CalculateResultState();

                return _resultState;
            }
        }

        private void CalculateResultState()
        {
            // Only 'test-case' nodes are contributing to the result state
            if (!AssociatedTestNode.IsSuite)
            {
                ResultNode r = TestModel.TestResultManager.GetResultForTest(AssociatedTestNode.Id);
                if (r != null)
                {
                    IsResultFromLastRun = r.IsLatestRun;
                    _resultState = r.Outcome;
                    if (!_resultState.Equals(ResultState.Explicit))
                        TestDuration = r.Duration;
                }

                _resultUptodate = true;
                return;
            }

            foreach (TreeNodeViewModel child in Children)
            {
                ResultState childResultState = child.ResultState;
                if (childResultState == null)
                    continue;

                if (_resultState == null || TestResultManager.GetOutcome(_resultState) < TestResultManager.GetOutcome(childResultState))
                    _resultState = childResultState;

                IsResultFromLastRun = IsResultFromLastRun || child.IsResultFromLastRun;

                if (!TestDuration.HasValue)
                    TestDuration = 0;

                // For example explicit tests with result state but no duration
                if (child.TestDuration.HasValue)
                    TestDuration += child.TestDuration;
            }

            _resultUptodate = true;
        }

        public bool IsInTestRun { get; private set; }

        public void SetInTestRun(bool value)
        {
            IsInTestRun = value;

            var parent = Parent;
            while (parent != null)
            {
                parent.IsInTestRun = value;
                parent = parent.Parent;
            }

            SetInTestRunForChildren(value);
        }

        private void SetInTestRunForChildren(bool value)
        {
            foreach (TreeNodeViewModel child in Children)
                if (!value || child.RunState != RunState.Explicit)
                {
                    child.IsInTestRun = value;
                    child.SetInTestRunForChildren(value);
                }
        }

        public void AddChild(TreeNodeViewModel childNode)
        {
            Children.Add(childNode);
            childNode.Parent = this;

            // Test result needs to be recalculated
            ResetTestResult();
            ResetTestCaseCount();
        }

        public void RemoveChild(TreeNodeViewModel childNode)
        {
            Children.Remove(childNode);

            // Test result needs to be recalculated
            ResetTestResult();
            ResetTestCaseCount();
        }

        public void OnTestRunStarted()
        {
            IsResultFromLastRun = false;

            if (IsInTestRun)
                TestDuration = null;
        }

        public void OnTestFinished(ResultNode resultNode)
        {
            ResetTestResult();
        }

        private void ResetTestResult()
        {
            IsResultFromLastRun = false;
            _resultUptodate = false;
            _resultState = null;
            TestDuration = null;

            Parent?.ResetTestResult();
        }

        private int TestCaseCount()
        {
            if (_testCaseCount == null)
                _testCaseCount = CalculateTestCaseCount();

            return _testCaseCount.Value;
        }

        private void ResetTestCaseCount()
        {
            _testCaseCount = null;
            Parent?.ResetTestCaseCount();
        }

        private int CalculateTestCaseCount()
        {
            int count = 0;

            if (!AssociatedTestNode.IsSuite && AssociatedTestNode.IsVisible)
                count += 1;

            foreach (TreeNodeViewModel child in Children)
            {
                count += child.TestCaseCount();
            }

            return count;
        }
    }
}
