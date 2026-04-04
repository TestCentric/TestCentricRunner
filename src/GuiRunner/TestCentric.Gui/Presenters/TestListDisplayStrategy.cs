// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Linq;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using Model;
    using NUnit;
    using NUnit.UiException.CodeFormatters;
    using Views;

    /// <summary>
    /// TestListDisplayStrategy is used to display lists
    /// of test cases grouped in various ways.
    /// </summary>
    public class TestListDisplayStrategy : GroupDisplayStrategy
    {
        #region Construction and Initialization

        public TestListDisplayStrategy(ITestTreeView view, ITestModel model) : base(view, model)
        {
            _view.CollapseToFixturesCommand.Enabled = false;
        }

        #endregion

        #region Properties

        public override string StrategyID => "TEST_LIST";

        public override string Description
        {
            get { return "Tests By " + GroupBy; }
        }

        protected override string GroupBy => TreeConfiguration.TestListGroupBy;
        private bool ShowAssemblies => TreeConfiguration.TestListShowAssemblies;
        private bool ShowFixtures => TreeConfiguration.TestListShowFixtures;

        #endregion

        #region Method Overrides

        public override void OnTestLoaded(TestNode rootNode, VisualState visualState)
        {
            ClearTree();

            var testGroups = GroupTestCases(rootNode);

            foreach (var group in testGroups)
                if (group.TestNodes.Count() > 0)
                    _view.Nodes.Add(group.TreeNode = MakeTreeNode(group, true));

            _view.TreeView.ExpandAll();

            visualState?.ApplyTo(_view.TreeView);
            ApplyResultsToTree();

            _model.SaveProject();
        }

        public override VisualState CreateVisualState()
        {
            VisualState visualState = null;

            _view.InvokeIfRequired(() =>
            {
                visualState = new VisualState("TEST_LIST", _topLevelGrouping?.ID)
                {
                    ShowAssemblies = TreeConfiguration.TestListShowAssemblies,
                    ShowFixtures = TreeConfiguration.TestListShowFixtures
                }.LoadFrom(_view.TreeView);
            });

            return visualState;
        }

        /// <summary>
        /// Check if a tree node type should be shown or omitted.
        /// </summary>
        protected override bool ShowTreeNodeType(TestNode testNode)
        {
            if (testNode.IsAssembly)
                return TreeConfiguration.TestListShowAssemblies;

            if (testNode.IsFixture)
                return TreeConfiguration.TestListShowFixtures;

            return !testNode.IsSuite;
        }

        /// <summary>
        /// ApplyResultToGroup is called for groupings that can change on execution
        /// </summary>
        /// <param name="result">A ResultNode</param>
        public override void ApplyResultToGroup(ResultNode result)
        {
            // The result of a test does not have any connection to it's Parent,
            // so we use the TreeNodes to make the connection.
            var treeNodes = GetTreeNodesForTest(result);

            // Result may be for a TestNode not shown in the TestListStrategy,
            // e.g. a namespace or a parameterized suite.
            if (treeNodes.Count == 0)
                return;

            // Since changing of groups is currently only needed for groupings that
            // display each node only once, we can ignore all but the first node.
            var treeNode = treeNodes[0];
            
            var oldParent = treeNode.Parent;
            var oldGroup = oldParent?.Tag as TestGroup;

            // We only have to proceed for tests that are direct
            // descendants of a group node.
            if (oldGroup == null)
                return;

            var topLevelGroup = _topLevelGrouping.SelectGroups(result)[0];
            var newGroup = topLevelGroup;

            if (topLevelGroup.TreeNode == null)
            {
                topLevelGroup.TreeNode = MakeTreeNode(topLevelGroup, false);
                if (!_view.Nodes.Contains(topLevelGroup.TreeNode))
                    _view.Nodes.Add(topLevelGroup.TreeNode);
            }

            if (ShowAssemblies || ShowFixtures)
            {
                string assemblyName = null;
                string fixtureName = null;

                // Find assembly and fixture names for this node
                for (TestNode parent = _model.GetTestById(result.Id).Parent; parent != null; parent = parent.Parent)
                {
                    if (ShowAssemblies && parent.IsAssembly)
                        assemblyName = parent.Name;
                    if (ShowFixtures && parent.IsFixture)
                        fixtureName = parent.Name;
                }

                // Create the subGroups
                if (ShowAssemblies)
                {
                    newGroup = topLevelGroup.GetOrAddSubGroup(assemblyName);
                    if (ShowFixtures)
                        newGroup = newGroup.GetOrAddSubGroup(fixtureName);
                }
                else // ShowFixtures only
                {
                    newGroup = topLevelGroup.GetOrAddSubGroup(fixtureName);
                }
            }

            // If the group didn't change, we can get out of here
            if (oldGroup == newGroup)
                return;

            newGroup.Add(result);
            oldGroup.TestNodes.RemoveId(result.Id);
            var newParent = newGroup.TreeNode;

            _view.InvokeIfRequired(() =>
            {
                // Remove test from the tree.
                treeNode.Remove();
                RemoveEmptyParentNodes(oldParent);

                // Add the test back to the tree in it's new position
                newParent.Nodes.Add(treeNode);
                newParent.Text = GroupDisplayName(newGroup);
                ExpandNewParentNodes(newParent);
            });
        }

        #endregion

        #region Helper Methods

        private List<TestGroup> GroupTestCases(TestNode testNode)
        {
            if (_topLevelGrouping == null || _topLevelGrouping.ID != GroupBy)
                _topLevelGrouping = CreateTestGrouping(GroupBy);

            _topLevelGrouping.LoadGroups(GetTestCases(testNode));

            if (ShowAssemblies)
            {
                CreateSubGroups(_topLevelGrouping.Groups, "ASSEMBLY");
                if (ShowFixtures)
                    foreach (var topLevelGroup in _topLevelGrouping.Groups)
                        CreateSubGroups(topLevelGroup.SubGroups, "FIXTURE");
            }
            else if (ShowFixtures)
            {
                CreateSubGroups(_topLevelGrouping.Groups, "FIXTURE");
            }

            return _topLevelGrouping.Groups;
        }

        private void CreateSubGroups(IList<TestGroup> groupsToSubGroup, string groupBy)
        {
            foreach (var group in groupsToSubGroup)
                CreateSubGroups(group, groupBy);
        }

        private void CreateSubGroups(TestGroup group, string groupBy)
        {
            Guard.ArgumentValid(groupBy == "ASSEMBLY" || groupBy == "FIXTURE",
                "Invalid argument. Only 'ASSEMBLY' and 'FIXTURE' are accepted for subgroups", nameof(groupBy));

            var grouping = CreateTestGrouping(groupBy);
            grouping.LoadGroups(group.TestNodes);
            foreach (var subGroup in grouping.Groups)
            {
                group.SubGroups.Add(subGroup);
                subGroup.ParentGroup = group;
            }
        }

        private void RemoveEmptyParentNodes(TreeNode parentNode)
        {
            if (parentNode.Parent != null)
                RemoveEmptyParentNodes(parentNode.Parent);
            parentNode.Remove();
        }

        private void ExpandNewParentNodes(TreeNode parentNode)
        {
            parentNode.Expand();
            if (parentNode.Parent != null)
                ExpandNewParentNodes(parentNode.Parent);
        }

        private TestSelection GetTestCases(TestNode testNode)
        {
            return new TestSelection(testNode
                .Select(n => !n.IsSuite)
                .OrderBy(s => s.Name));
        }

        #endregion
    }
}
