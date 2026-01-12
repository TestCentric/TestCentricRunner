// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    /// <summary>
    /// This class is responsible to provide a ViewModel for a tree
    /// It contains the main logic for performing the grouping and building up the ViewModel
    /// Derived classes need to provide the actual grouping information (duration, outcome, categories)
    /// </summary>
    public abstract class TreeViewModelBase : ITreeViewModel
    {
        // Mapping from a TestNode to all ViewModels in which is included
        // (in common this is 1:1 mapping; 1:N mapping only if multiple categories are used for a TestNode)
        private IDictionary<TestNode, IList<TreeNodeViewModel>> _testNodeToViewModels = new Dictionary<TestNode, IList<TreeNodeViewModel>>();
        public event EventHandler<NodeChangedEventsArgs> OnNodeChanged;
        public event EventHandler<UpdateTreeEventsArgs> OnUpdateTree;
        private RegroupTestEventQueue _regroupQueue;

        public TreeViewModelBase(ITestModel model)
        {
            Model = model;
            TreeConfiguration = model.TreeConfiguration;
            RootViewModels = new List<TreeNodeViewModel>();

            var regrouping = new ReGrouping(this);
            _regroupQueue = new RegroupTestEventQueue(regrouping);
        }

        protected ITestModel Model { get; }

        protected ITreeConfiguration TreeConfiguration { get; }

        /// <summary>
        /// List of all root groups (outcome, duration or category groups)
        /// </summary>
        public IList<TreeNodeViewModel> RootViewModels { get; }

        protected string CurrentRootGroupName { get; set; }

        /// <summary>
        /// Derived classes might set this value, if test cases might need to be regrouped after a test run
        /// For example duration or outcome grouping
        /// </summary>
        protected bool SupportsRegrouping { get; set; }

        /// <summary>
        /// Creates the complete grouped tree
        /// </summary>
        public abstract void CreateTreeModel(TestNode rootNode);

        public IList<TreeNodeViewModel> CreateTreeNodeViewModels(TestNode testNode, string groupName)
        {
            IList<TestNode> path = GetTestNodePath(testNode);
            var parentNodeViewModel = GetOrCreateRootNode(groupName);
            IList<TreeNodeViewModel> nodes = CreateTreeNodeViewModels(parentNodeViewModel, path);
            return nodes;
        }

        /// <summary>
        /// Retrieves the list of groups in which a testNode is grouped
        /// Derived classes need to override this method
        /// </summary>
        public virtual IList<string> GetGroupNames(TestNode testNode)
        {
            return new List<string>();
        }

        /// <summary>
        /// Called when one test case is finished
        /// </summary>
        public IList<TreeNodeViewModel> OnTestFinished(ResultNode result)
        {
            TestNode testNode = Model.GetTestById(result.Id);
            if (SupportsRegrouping && _regroupQueue.IsQueueRequired(testNode))
            {
                _regroupQueue.AddToQueue(testNode);
                return new List<TreeNodeViewModel>();
            }

            // Update ImageIndex of all groups containing finished test
            if (_testNodeToViewModels.TryGetValue(testNode, out IList<TreeNodeViewModel> viewModels))
            {
                foreach (TreeNodeViewModel viewModel in viewModels)
                {
                    viewModel.OnTestFinished(result);
                }
            }

            return viewModels ?? new List<TreeNodeViewModel>();
        }

        public TreeNodeViewModel GetTreeNodeViewModel(TestNode testNode)
        {
            // This method is currently only used for regrouping and TestNodes of type test-case => returning first is OK here
            return _testNodeToViewModels[testNode].First();
        }

        public void UpdateTreeModel(IList<TestNode> finishedTests)
        {
            IList<TreeNodeViewModel> finishedTestViewModels = new List<TreeNodeViewModel>();

            // 1. Update all node models 
            foreach (TestNode testNode in finishedTests)
            {
                ResultNode result = Model.TestResultManager.GetResultForTest(testNode.Id);
                if (_testNodeToViewModels.TryGetValue(testNode, out IList<TreeNodeViewModel> list))
                {
                    foreach (TreeNodeViewModel viewModel in list)
                    {
                        viewModel.OnTestFinished(result);
                        finishedTestViewModels.Add(viewModel);
                    }
                }
            }

            // 2. Invoke update of tree 
            OnUpdateTree?.Invoke(this, new UpdateTreeEventsArgs(finishedTestViewModels));
        }

        public void RemoveNode(TestNode testNode)
        {
            TreeNodeViewModel viewModel = _testNodeToViewModels[testNode].First();
            RemoveNode(viewModel);
        }

        private void RemoveNode(TreeNodeViewModel nodeViewModel)
        {
            if (RootViewModels.Contains(nodeViewModel))
                RootViewModels.Remove(nodeViewModel);

            if (_testNodeToViewModels.TryGetValue(nodeViewModel.AssociatedTestNode, out IList<TreeNodeViewModel> viewModels) && viewModels.Contains(nodeViewModel))
                viewModels.Remove(nodeViewModel);

            TreeNodeViewModel parentNode = nodeViewModel.Parent;
            if (parentNode != null)
            {
                parentNode.RemoveChild(nodeViewModel);
                OnNodeChanged?.Invoke(this, new NodeChangedEventsArgs(parentNode));

                if (parentNode.Children.Count == 0)
                    RemoveNode(parentNode);
            }
        }

        /// <summary>
        /// Method is intended to be called only from test code, so that the test code doesn't need to deal with the regroup Timer
        /// </summary>
        public void OnTestFinishedWithoutRegroupTimer(ResultNode result)
        {
            OnTestFinished(result);
            _regroupQueue.ForceStopTimer();
        }

        public void OnTestRunStarting()
        {
            IList<TestNode> tests = Model.TestsInRun.ToList();
            if (Model.TestsInRun is GroupingTestGroup g)
            {
                tests = g.NodeViewModel.GetNonExplicitTests().ToList();
            }

            foreach (TestNode testNode in tests)
            {
                if (!_testNodeToViewModels.TryGetValue(testNode, out IList<TreeNodeViewModel> viewModels))
                    continue;

                foreach (TreeNodeViewModel treeNodeViewModel in viewModels)
                    treeNodeViewModel.SetInTestRun(true);
            }

            // 1. Update ViewModel
            IList<TreeNodeViewModel> allViewModels = _testNodeToViewModels.Values.SelectMany(x => x).ToList();

            foreach (TreeNodeViewModel nodes in allViewModels)
                nodes.OnTestRunStarted();

            // 2. Update TreeView from ViewModel

        }

        public void OnTestRunFinished()
        {
            // Force executing of any pending regroup operations
            _regroupQueue.ForceStopTimer();

            IList<TreeNodeViewModel> allViewModels = _testNodeToViewModels.Values.SelectMany(x => x).ToList();
            var nodesInRun = allViewModels.Where(n => n.IsInTestRun).ToList();
            foreach (TreeNodeViewModel node in nodesInRun)
                node.SetInTestRun(false);
        }

        public IList<TreeNodeViewModel> GetAllViewModelsInTestRun()
        {
            IList<TreeNodeViewModel> allViewModels = _testNodeToViewModels.Values.SelectMany(x => x).ToList();
            return allViewModels.Where(n => n.IsInTestRun).ToList();
        }


        /// <summary>
        /// Check if a root tree node for the groupName already exists
        /// If not, create new TreeNode
        /// </summary>
        private TreeNodeViewModel GetOrCreateRootNode(string name)
        {
            CurrentRootGroupName = name;
            foreach (TreeNodeViewModel rootGroup in RootViewModels)
                if (rootGroup.Name == name)
                    return rootGroup;

            // TreeNode doesn't exist => create it
            var viewModel = CreateAndInitTreeNodeViewModel(Model.LoadedTests, name);
            RootViewModels.Add(viewModel);

            return RootViewModels.Last();
        }


        /// <summary>
        /// Get a path in the NUnit tree (list of TestNodes) from the root node to one TestNode 
        /// </summary>
        private IList<TestNode> GetTestNodePath(TestNode testNode)
        {
            IList<TestNode> path = new List<TestNode>();
            while (testNode != null && testNode.Type != "TestRun")
            {
                path.Add(testNode);
                testNode = testNode.Parent;
            }

            return path.Reverse().ToList();
        }

        /// <summary>
        /// Creates TreeNodes for all TestNodes within the path.
        /// If a TreeNode already exists, it will be reused.
        /// Returns the list of TreeNodes representing the TestNode path.
        /// </summary>
        private IList<TreeNodeViewModel> CreateTreeNodeViewModels(TreeNodeViewModel parentTreeNode, IList<TestNode> path)
        {
            IList<TreeNodeViewModel> treeNodes = new List<TreeNodeViewModel>() { parentTreeNode };

            for (int i = 0; i < path.Count; i++)
            {
                TestNode testNode = path[i];
                if (!ShowTreeNodeType(testNode))
                    continue;

                string name = testNode.Name;

                // Check if namespace nodes must be folded into one single tree node
                if (FoldNamespaceNodesHandler.IsNamespaceNode(testNode))
                {
                    IList<TestNode> foldedNodes = FoldNamespaceNodesHandler.FoldNamespaceNodes(testNode);
                    name = FoldNamespaceNodesHandler.GetFoldedNamespaceName(foldedNodes);
                    i += foldedNodes.Count - 1;
                }

                // Try to get child TreeNode => create not exists create TreeNode
                TreeNodeViewModel childNode = TryGetChildTreeNodeViewModel(parentTreeNode, name);
                if (childNode == null)
                {
                    childNode = CreateAndInitTreeNodeViewModel(testNode, name);
                    parentTreeNode.AddChild(childNode);
                    OnNodeChanged?.Invoke(this, new NodeChangedEventsArgs(parentTreeNode));
                }

                // Use childNode as new ParentTreeNode and proceed with next node in path
                parentTreeNode = childNode;
                treeNodes.Add(childNode);
            }

            return treeNodes;
        }

        private TreeNodeViewModel TryGetChildTreeNodeViewModel(TreeNodeViewModel treeNode, string name)
        {
            foreach (TreeNodeViewModel childNode in treeNode.Children)
                if (childNode.Name == name)
                    return childNode;

            return null;
        }

        protected TreeNodeViewModel CreateAndInitTreeNodeViewModel(TestNode testNode, string name)
        {
            TreeNodeViewModel viewModel = CreateTreeNodeViewModel(testNode, name);
            AddTestToViewModelMapping(testNode, viewModel);
            return viewModel;
        }

        protected abstract TreeNodeViewModel CreateTreeNodeViewModel(TestNode testNode, string name);

        protected void AddTestToViewModelMapping(TestNode testNode, TreeNodeViewModel viewModel)
        {
            if (_testNodeToViewModels.TryGetValue(testNode, out IList<TreeNodeViewModel> viewModels))
                viewModels.Add(viewModel);
            else
                _testNodeToViewModels[testNode] = new List<TreeNodeViewModel>() { viewModel };
        }

        /// <summary>
        /// Check if a tree node type should be shown or omitted
        /// Currently we support only omitting the namespace nodes
        /// </summary>
        protected bool ShowTreeNodeType(TestNode testNode)
        {
            if (FoldNamespaceNodesHandler.IsNamespaceNode(testNode))
                return TreeConfiguration.ShowNamespaces;

            return true;
        }
    }
}
