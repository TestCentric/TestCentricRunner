// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Linq;
    using System.Security.Cryptography;
    using Model;
    using Views;

    /// <summary>
    /// GroupDisplayStrategy is the abstract base class for 
    /// DisplayStrategies that list tests in various groupings.
    /// </summary>
    public abstract class GroupDisplayStrategy : DisplayStrategy
    {
        protected TestGrouping _topLevelGrouping;

        #region Construction and Initialization

        public GroupDisplayStrategy(ITestTreeView view, ITestModel model)
            : base(view, model)
        {
            _topLevelGrouping = CreateTestGrouping(GroupBy);
            _view.SetTestFilterVisibility(false);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Post a test result to the tree, changing the treeNode
        /// color to reflect success or failure. Overridden here
        /// to allow for moving nodes from one group to another
        /// based on the result of running the test.
        /// </summary>
        public override void OnTestFinished(ResultNode result)
        {
            _view.InvokeIfRequired(() =>
            {
                _topLevelGrouping?.OnTestFinished(result);
                
                base.OnTestFinished(result);
            });
        }

        // TODO: Move this to TestGroup? Would need access to results.
        public int CalcImageIndexForGroup(TestGroup group)
        {
            var groupIndex = -1;

            bool isLatestRun = group.TestNodes.Any(t => _model.IsInTestRun(t));
            foreach (var testNode in group)
            {
                var result = GetResultForTest(testNode);
                if (result != null)
                {
                    var imageIndex = CalcImageIndex(result, isLatestRun);

                    if (imageIndex == TestTreeView.FailureIndex)
                        return TestTreeView.FailureIndex; // Early return - can't get any worse!

                    if (imageIndex >= TestTreeView.SuccessIndex_NotLatestRun) // Only those values propagate
                        groupIndex = Math.Max(groupIndex, imageIndex);
                }
            }

            return groupIndex;
        }

        public void Add(TreeNode treeNode)
        {
            _view.Add(treeNode);
        }

        /// <summary>
        /// ApplyResultToGroup takes no action in the base GroupDisplayStrategyClass.
        /// It should be overridden in derived strategies with categories based
        /// on the result of the test, which may change upon execution.
        /// </summary>
        /// <param name="result">A ResultNode</param>
        public virtual void ApplyResultToGroup(ResultNode result) { }

        #endregion

        #region Protected Members

        protected abstract string GroupBy { get; }

        protected TestGrouping CreateTestGrouping(string groupBy)
        {
            switch (groupBy)
            {
                case null: // Needed by tests that use NSubstitute
                case "UNGROUPED":
                    return new UngroupedGrouping(this);
                case "OUTCOME":
                    return new OutcomeGrouping(this);
                case "DURATION":
                    return new DurationGrouping(this);
                case "CATEGORY":
                    return new CategoryGrouping(this, true);
                case "ASSEMBLY":
                    return new AssemblyGrouping(this);
                case "FIXTURE":
                    return new TestFixtureGrouping(this);
                default:
                    throw new ArgumentException($"Unknown grouping ID: {groupBy}");
            }
        }

        protected void UpdateDisplay()
        {
            if (_topLevelGrouping != null)
            {
                this.ClearTree();
                TreeNode topNode = null;
                foreach (var group in _topLevelGrouping.Groups)
                {
                    var treeNode = MakeTreeNode(group, true);
                    group.TreeNode = treeNode;
                    treeNode.Expand();
                    if (group.TestNodes.Count() > 0)
                    {
                        _view.Add(treeNode);
                        if (topNode == null)
                            topNode = treeNode;
                    }
                }
                if (topNode != null)
                    topNode.EnsureVisible();
            }
        }

        #endregion
    }
}
