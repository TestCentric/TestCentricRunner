// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************


namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Views;

    /// <summary>
    /// This class is responsible for building and updating a Windows Forms TreeView given a TreeViewModel.
    /// </summary>
    public class TreeViewBuilder
    {
        private IDictionary<TreeNodeViewModel, TreeNode> _viewModelToTreeNodeMap = new Dictionary<TreeNodeViewModel, TreeNode>();
        private IDictionary<TreeNode, TreeNodeViewModel> _treeNodeToViewModelMap = new Dictionary<TreeNode, TreeNodeViewModel>();

        private HashSet<TreeNodeViewModel> _changedNodes = new HashSet<TreeNodeViewModel>();

        private ITestTreeView TreeView { get; }

        public TreeViewBuilder(ITestModel testModel, ITreeViewModel treeViewModel, ITestTreeView treeView)
        {
            TreeView = treeView;
            Model = testModel;
            TreeViewModel = treeViewModel;
        }

        private ITestModel Model { get; }

        private ITreeViewModel TreeViewModel { get; }

        /// <summary>
        /// Rebuild an entire TreeView from the TreeViewModel.
        /// </summary>
        internal void Rebuild()
        {
            TreeView.TreeView.BeginUpdate();
            foreach (TreeNodeViewModel viewModel in TreeViewModel.RootViewModels)
            {
                TreeNode treeNode = CreateTreeNode(viewModel);
                TreeView.Add(treeNode);
                BuildTreeFromViewModel(treeNode, viewModel);
            }

            if (Model.TreeConfiguration.ShowTestDuration)
                TreeView.Sort();

            _changedNodes.Clear();
            TreeView.TreeView.EndUpdate();
        }

        /// <summary>
        /// Updates the TreeView to reflect changes in the TreeViewModel.
        /// Invoked during a test run whenever tests finished and needs to regrouped
        /// </summary>
        internal void OnUpdateTree(object sender, UpdateTreeEventsArgs args)
        {
            TreeView.InvokeIfRequired(() =>
            {
                TreeView.TreeView.BeginUpdate();

                UpdateRootNodes(TreeViewModel.RootViewModels);

                foreach (TreeNodeViewModel viewModel in TreeViewModel.RootViewModels)
                    UpdateTreeNodeChildren(viewModel);

                UpdateTreeNodeContent(args.ViewModels);

                _changedNodes.Clear();

                TreeView.TreeView.EndUpdate();
            });
        }

        private void UpdateRootNodes(IList<TreeNodeViewModel> rootNodes)
        {
            RemoveOutdatedChildTreeNode(TreeView.Nodes, rootNodes);

            foreach (TreeNodeViewModel viewModel in rootNodes)
            {
                if (!_viewModelToTreeNodeMap.TryGetValue(viewModel, out TreeNode treeNode))
                {
                    treeNode = CreateTreeNode(viewModel);
                    TreeView.Add(treeNode);
                }
            }
        }

        private TreeNode CreateTreeNode(TreeNodeViewModel viewModelNode)
        {
            TreeNode treeNode = new TreeNode(viewModelNode.DisplayName);
            treeNode.Tag = viewModelNode.TestItem;
            if (viewModelNode.ResultState != null)
                TreeView.SetImageIndex(treeNode, DisplayStrategy.CalcImageIndex(viewModelNode.ResultState, viewModelNode.IsResultFromLastRun));

            _viewModelToTreeNodeMap.Add(viewModelNode, treeNode);
            _treeNodeToViewModelMap.Add(treeNode, viewModelNode);
            return treeNode;
        }

        internal void OnNodeChanged(object sender, NodeChangedEventsArgs args)
        {
            if (!_changedNodes.Contains(args.ViewModel))
                _changedNodes.Add(args.ViewModel);
        }

        private void BuildTreeFromViewModel(TreeNode treeNode, TreeNodeViewModel viewModel)
        {
            foreach (TreeNodeViewModel child in viewModel.Children)
            {
                TreeNode childTreeNode = CreateTreeNode(child);
                treeNode.Nodes.Add(childTreeNode);
                BuildTreeFromViewModel(childTreeNode, child);
            }
        }

        private IList<TreeNodeViewModel> AddParentNodes(HashSet<TreeNodeViewModel> changedNodes)
        {
            IList<TreeNodeViewModel> nodes = new List<TreeNodeViewModel>(changedNodes);
            foreach (TreeNodeViewModel node in changedNodes)
                AddParent(node, nodes);

            return nodes;
        }

        private void AddParent(TreeNodeViewModel node, IList<TreeNodeViewModel> nodes)
        {
            if (node?.Parent == null || nodes.Contains(node.Parent))
            {
                return;
            }

            nodes.Add(node.Parent);
            AddParent(node.Parent, nodes);
        }

        private void UpdateTreeNodeChildren(TreeNodeViewModel viewModel)
        {
            var treeNode = _viewModelToTreeNodeMap[viewModel];

            if (_changedNodes.Contains(viewModel))
            {
                CheckAndAddNewChildTreeNodes(treeNode, viewModel);
                RemoveOutdatedChildTreeNode(treeNode.Nodes, viewModel.Children);
            }

            foreach (TreeNodeViewModel modelChild in viewModel.Children)
            {
                UpdateTreeNodeChildren(modelChild);
            }
        }

        private void RemoveOutdatedChildTreeNode(TreeNodeCollection treeNodes, IList<TreeNodeViewModel> viewModels)
        {
            IList<TreeNode> removeNodes = new List<TreeNode>();
            foreach (TreeNode childTreeNode in treeNodes)
            {
                if (!ViewModelExists(viewModels, childTreeNode))
                {
                    TreeNodeViewModel childNode = _treeNodeToViewModelMap[childTreeNode];
                    _viewModelToTreeNodeMap.Remove(childNode);
                    _treeNodeToViewModelMap.Remove(childTreeNode);
                    removeNodes.Add(childTreeNode);
                }
            }

            foreach (TreeNode node in removeNodes)
                treeNodes.Remove(node);
        }

        private void CheckAndAddNewChildTreeNodes(TreeNode treeNode, TreeNodeViewModel viewModel)
        {
            foreach (TreeNodeViewModel child in viewModel.Children)
            {
                if (!TreeChildNodeExists(treeNode, child))
                {
                    var childTreeNode = CreateTreeNode(child);
                    treeNode.Nodes.Add(childTreeNode);
                }
            }
        }

        private bool ViewModelExists(IList<TreeNodeViewModel> viewModels, TreeNode childTreeNode)
        {
            TreeNodeViewModel node = _treeNodeToViewModelMap[childTreeNode];

            return viewModels.Any(c => c == node);
        }

        private bool TreeChildNodeExists(TreeNode treeNode, TreeNodeViewModel viewModel)
        {
            foreach (TreeNode childNode in treeNode.Nodes)
                if (_treeNodeToViewModelMap[childNode] == viewModel)
                    return true;

            return false;
        }

        private void UpdateTreeNodeContent(IList<TreeNodeViewModel> finishedTests)
        {
            var modifiedNodes = AddParentNodes(_changedNodes);

            foreach (TreeNodeViewModel node in modifiedNodes)
            {
                if (!_viewModelToTreeNodeMap.TryGetValue(node, out TreeNode treeNode))
                    continue;

                treeNode.Text = node.DisplayName;
            }

            foreach (TreeNodeViewModel node in finishedTests)
            {
                if (!_viewModelToTreeNodeMap.TryGetValue(node, out TreeNode treeNode))
                    continue;

                int imageIndex = node.ResultState != null ? DisplayStrategy.CalcImageIndex(node.ResultState, node.IsResultFromLastRun) : TestTreeView.InitIndex;
                if (imageIndex != treeNode.ImageIndex)
                    TreeView.SetImageIndex(treeNode, imageIndex);
            }

        }

        public void OnTestStarting(TreeNodeViewModel viewModel)
        {
            if (_viewModelToTreeNodeMap.TryGetValue(viewModel, out TreeNode treeNode))
                TreeView.SetImageIndex(treeNode, TestTreeView.RunningIndex, true);
        }

        public void OnTestFinished(IList<TreeNodeViewModel> viewModels)
        {
            foreach (TreeNodeViewModel viewModel in viewModels)
            {
                if (_viewModelToTreeNodeMap.TryGetValue(viewModel, out TreeNode treeNode) && viewModel.ResultState != null)
                    TreeView.SetImageIndex(treeNode, DisplayStrategy.CalcImageIndex(viewModel.ResultState, viewModel.IsResultFromLastRun));
            }
        }

        public void OnTestRunStarting(Func<IList<TreeNodeViewModel>> GetNodesInTestRunFunc)
        {
            TreeView.InvokeIfRequired(() =>
            {
                TreeView.TreeView.BeginUpdate();

                UpdateTreeIconsOnRunStart(TreeViewModel.RootViewModels);

                if (Model.TreeConfiguration.ShowTestDuration)
                {
                    var nodesInRun = GetNodesInTestRunFunc();
                    foreach (var node in nodesInRun)
                    {
                        if (_viewModelToTreeNodeMap.TryGetValue(node, out TreeNode treeNode))
                            treeNode.Text = node.DisplayName;
                    }
                }

                TreeView.TreeView.EndUpdate();
            });
        }

        private void UpdateTreeIconsOnRunStart(IList<TreeNodeViewModel> nodes)
        {
            foreach (TreeNodeViewModel node in nodes)
            {
                if (!_viewModelToTreeNodeMap.TryGetValue(node, out TreeNode treeNode))
                {
                    continue;
                }

                if (node.IsInTestRun)
                    TreeView.SetImageIndex(treeNode, TestTreeView.InTestRunIndex);
                else
                {
                    if (node.ResultState != null)
                        TreeView.SetImageIndex(treeNode, DisplayStrategy.CalcImageIndex(node.ResultState, node.IsResultFromLastRun));
                }

                UpdateTreeIconsOnRunStart(node.Children);
            }
        }

        public void OnTestRunFinished(IList<TreeNodeViewModel> nodesInTestRun)
        {
            TreeView.InvokeIfRequired(() =>
            {
                TreeView.TreeView.BeginUpdate();
                foreach (TreeNodeViewModel viewModel in TreeViewModel.RootViewModels)
                {
                    viewModel.OnTestFinished(null);
                    int imageIndex = viewModel.ResultState != null ? DisplayStrategy.CalcImageIndex(viewModel.ResultState, viewModel.IsResultFromLastRun) : TestTreeView.InitIndex;
                    if (_viewModelToTreeNodeMap.TryGetValue(viewModel, out TreeNode treeNode))
                        TreeView.SetImageIndex(treeNode, imageIndex);
                }

                if (Model.TreeConfiguration.ShowTestDuration)
                {
                    foreach (TreeNodeViewModel node in nodesInTestRun)
                    {
                        if (_viewModelToTreeNodeMap.TryGetValue(node, out TreeNode treeNode))
                            treeNode.Text = node.DisplayName;
                    }

                    TreeView.Sort();
                }

                TreeView.TreeView.EndUpdate();
            });
        }

        public void OnShowTestDurationChanged()
        {
            TreeView.TreeView.BeginUpdate();
            foreach (var keyValuePair in _viewModelToTreeNodeMap)
            {
                TreeNode treeNode = keyValuePair.Value;
                treeNode.Text = keyValuePair.Key.DisplayName;
            }

            TreeView.TreeView.EndUpdate();
        }
    }
}
