// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Linq;
    using Model;
    using Views;

    /// <summary>
    /// GroupDisplayStrategy is the abstract base class for 
    /// DisplayStrategies that list tests in various groupings.
    /// </summary>
    public abstract class GroupDisplayStrategy : DisplayStrategy
    {
        protected TestGrouping _grouping;

        #region Construction and Initialization

        public GroupDisplayStrategy(ITestTreeView view, ITestModel model)
            : base(view, model)
        {
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
            base.OnTestFinished(result);
            _view.InvokeIfRequired(() => _grouping?.OnTestFinished(result));
        }

        public override void OnTestRunFinished()
        {
            _view.InvokeIfRequired(() =>
            {
                _grouping?.OnTestRunFinished();
                ResetTestRunningIcons(_view.Nodes);
            });
        }

        // TODO: Move this to TestGroup? Would need access to results.
        public int CalcImageIndexForGroup(TestGroup group)
        {
            var groupIndex = -1;

            bool isLatestRun = group.Any(t => _model.IsInTestRun(t));
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

        public void ApplyResultToGroup(ResultNode result)
        {
            var treeNodes = GetTreeNodesForTest(result);

            // Result may be for a TestNode not shown in the tree
            if (treeNodes.Count == 0)
                return;

            // This implementation ignores any but the first node
            // since changing of groups is currently only needed
            // for groupings that display each node once.
            var treeNode = treeNodes[0];
            var oldParent = treeNode.Parent;
            var oldGroup = oldParent?.Tag as TestGroup;

            // We only have to proceed for tests that are direct
            // descendants of a group node.
            if (oldGroup == null)
                return;

            var newGroup = _grouping.SelectGroups(result)[0];

            // If the group didn't change, we can get out of here
            if (oldGroup == newGroup)
                return;

            var newParent = newGroup.TreeNode;

            _view.InvokeIfRequired(() =>
            {
                oldGroup.RemoveId(result.Id);
                // TODO: Insert in order

                TestNode testNode = _model.GetTestById(result.Id);
                newGroup.Add(testNode);

                // Remove test from tree
                treeNode.Remove();

                // If it was last test in group, remove group
                if (oldGroup.Count() == 0)
                    oldParent.Remove();
                else // update old group
                {
                    oldParent.Text = GroupDisplayName(oldGroup);
                }

                newParent.Nodes.Add(treeNode);
                newParent.Text = GroupDisplayName(newGroup);
                newParent.Expand();

                if (newGroup.Count() == 1)
                {
                    _view.Clear();
                    newParent.ImageIndex = newParent.SelectedImageIndex = TestTreeView.PendingIndex;

                    TreeNode topNode = null;
                    foreach (var group in _grouping.Groups)
                        if (group.Count() > 0)
                        {
                            Add(group.TreeNode);
                            if (topNode == null)
                                topNode = group.TreeNode;
                        }

                    if (topNode != null)
                        topNode.EnsureVisible();
                }
            });
        }

        #endregion

        #region Protected Members

        protected void SetDefaultTestGrouping()
        {
            _grouping = CreateTestGrouping(DefaultGroupSetting);
        }

        protected abstract string DefaultGroupSetting { get; }

        protected TestGrouping CreateTestGrouping(string groupBy)
        {
            switch (groupBy)
            {
                default:
                case "UNGROUPED":
                    return null;
                case "OUTCOME":
                    return new OutcomeGrouping(this);
                case "DURATION":
                    return new DurationGrouping(this);
                case "CATEGORY":
                    // Tree display format 'Test_List' should consider categories on test fixtures and test cases
                    return new CategoryGrouping(this, StrategyID == "TEST_LIST");
            }
        }

        protected void UpdateDisplay()
        {
            if (_grouping != null)
            {
                this.ClearTree();
                TreeNode topNode = null;
                foreach (var group in _grouping.Groups)
                {
                    var treeNode = MakeTreeNode(group, true);
                    group.TreeNode = treeNode;
                    treeNode.Expand();
                    if (group.Count() > 0)
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
