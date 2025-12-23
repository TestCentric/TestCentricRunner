// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    /// <summary>
    /// Class is responsible for regrouping treeNodes from one group to another group
    /// A bulk processing approach is used to improve the treeview performance.
    /// This class is getting invoked whenever the bulk processing is triggered, and receives a list of test nodes to be regrouped. 
    /// 1. The regroup logic will remove the TestNode from all TestGroups along the current path.
    /// 2. If a test group no longer contains any test nodes, the associated tree node can be removed.
    /// 3. The test node is inserted into the new group building up a new tree path.
    /// </summary>
    internal class ReGrouping
    {
        public ReGrouping(ITreeViewModel treeViewModel)
        {
            TreeViewModel = treeViewModel;
        }

        private ITreeViewModel TreeViewModel { get; }

        /// <summary>
        /// Move the TreeNode path of a TestNode to a new group
        /// This method gets a list of TestNodes either representing test cases or test suites. 
        /// </summary>
        public void Regroup(IList<TestNode> testNodes)
        {
            foreach (TestNode testNode in testNodes)
            {
                if (testNode.IsSuite)
                    continue;

                // 1. Determine new group
                string newGroupName = TreeViewModel.GetGroupNames(testNode).First();

                // 2. Remove TestCase from groups and remove TreeNodes if required
                TreeViewModel.RemoveNode(testNode);

                // 3. Create new TreeNode path
                TreeViewModel.CreateTreeNodeViewModels(testNode, newGroupName);

                // 4. Expand newly created treeNodes
                // ExpandTreeNodes(newTreeNodes);
            }

            // 5. Update all tree nodes to reflect changed number of containing tests in group
            TreeViewModel.UpdateTreeModel(testNodes);
        }

        /// <summary>
        /// Checks if a TestNode will be grouped into a different group than the current one
        /// </summary>
        public bool IsRegroupRequired(TestNode testNode)
        {
            TreeNodeViewModel viewModelNode = TreeViewModel.GetTreeNodeViewModel(testNode);

            string newGroupName = TreeViewModel.GetGroupNames(testNode).First();
            string oldGroupName = GetOldGroupName(viewModelNode);
            return oldGroupName != newGroupName;
        }

        private void ExpandTreeNodes(IList<TreeNode> newTreeNodes)
        {
            newTreeNodes.FirstOrDefault()?.Expand();
        }

        private string GetOldGroupName(TreeNodeViewModel viewModel)
        {
            while (viewModel.Parent != null)
                viewModel = viewModel.Parent;

            return viewModel.Name;
        }
    }
}
