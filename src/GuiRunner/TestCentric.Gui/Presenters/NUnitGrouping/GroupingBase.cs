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
    /// This class is responsible for the NUNnit tree grouping functionality 
    /// It contains the main logic for performing the grouping and building up the tree
    /// Derived classes need to provide the actual grouping information (duration, outcome, categories)
    /// </summary>
    public class GroupingBase : INUnitGrouping
    {
        private RegroupTestEventQueue _regroupQueue;

        // Mapping listing all groups in which a TestNode is included
        private IDictionary<TestNode, IList<TestGroup>> _testToGroupMap = new Dictionary<TestNode, IList<TestGroup>>();
        // List of all root groups (outcome, duration or category groups)
        private IList<TestGroup> _rootGroups = new List<TestGroup>();
        // list of all groups (include also root groups)
        private IList<TestGroup> _groups = new List<TestGroup>();

        public GroupingBase(INUnitTreeDisplayStrategy strategy, ITestModel model, ITestTreeView view)
        {
            Strategy = strategy;
            TreeView = view;
            Model = model;

            var regrouping = new ReGrouping(view, Strategy, this);
            _regroupQueue = new RegroupTestEventQueue(regrouping);
        }

        protected ITestModel Model { get; }

        protected ITestTreeView TreeView { get; }

        protected INUnitTreeDisplayStrategy Strategy { get; }

        protected string CurrentRootGroupName { get; set; }

        /// <summary>
        /// Derived classes might set this value, if test cases might need to be regrouped after a test run
        /// For example duration or outcome grouping
        /// </summary>
        protected bool SupportsRegrouping { get; set; }

        /// <summary>
        /// Creates the complete grouped tree
        /// </summary>
        public virtual void CreateTree(TestNode rootNode)
        {
            // 1. Get list of all TestNode representing test cases
            IList<TestNode> testNodes = rootNode.Select(n => !n.IsSuite && n.IsVisible).ToList();

            TreeView.TreeView.BeginUpdate();

            // 2. Create all tree nodes for one testNode one-by-one
            foreach (TestNode testNode in testNodes)
            {
                IList<string> groupNames = GetGroupNames(testNode);
                foreach (string groupName in groupNames)
                    CreateTreeNodes(testNode, groupName);
            }

            // 3. Update tree node names and images
            TreeNodeDurationHandler.SetGroupDurations(Model, _groups);
            Strategy.UpdateTreeNodeNames();
            TreeNodeImageHandler.SetTreeNodeImages(TreeView, _groups);

            if (Model.Settings.Gui.TestTree.ShowTestDuration)
                TreeView.Sort();

            TreeView.TreeView.EndUpdate();
        }

        /// <summary>
        /// Creates (or reuse) all tree nodes along the path from the group to the testNode (representing a test case)
        /// Returns the path of tree nodes from the root tree node (group) to the leaf tree node (test case)
        /// </summary>
        public IList<TreeNode> CreateTreeNodes(TestNode testNode, string groupName)
        {
            TreeNode parentNode = GetOrCreateRootTreeNode(groupName);
            IList<TestNode> path = GetTestNodePath(testNode);
            IList<TreeNode> treeNodes = CreateTreeNodes(parentNode, path);
            AddTestToGroups(treeNodes, testNode);
            return treeNodes;
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
        public void OnTestFinished(ResultNode result)
        {
            if (SupportsRegrouping && _regroupQueue.IsQueueRequired(result))
            {
                TestNode testNode = Model.GetTestById(result.Id);
                TreeView.InvokeIfRequired(() => _regroupQueue.AddToQueue(testNode));
                return;
            }

            // Update ImageIndex of all groups containing finished test
            TestNode node = Model.GetTestById(result.Id);
            if (_testToGroupMap.TryGetValue(node, out IList<TestGroup> groups))
                TreeNodeImageHandler.OnTestFinished(result, groups, TreeView);
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
            TreeNodeDurationHandler.ClearGroupDurations(_groups);
        }

        public void OnTestRunFinished()
        {
            // Force executing of any pending regroup operations
            TreeView.InvokeIfRequired(() => _regroupQueue.ForceStopTimer());

            // The images of the root group tree nodes (for example 'CategoryA' or 'Slow') cannot be set during a test run
            TreeNodeImageHandler.SetTreeNodeImages(TreeView, _rootGroups);
            TreeNodeDurationHandler.SetGroupDurations(Model, _groups);
        }

        /// <summary>
        /// Check if a root tree node for the groupName already exists
        /// If not, create new TreeNode
        /// </summary>
        private TreeNode GetOrCreateRootTreeNode(string groupName)
        {
            CurrentRootGroupName = groupName;
            foreach (TestGroup rootGroup in _rootGroups)
                if (rootGroup.Name == groupName)
                    return rootGroup.TreeNode;

            // TreeNode doesn't exist => create it
            TestGroup group = CreateTestGroup(groupName, Model.LoadedTests);

            _rootGroups.Add(group);
            _groups.Add(group);
            TreeNode groupTreeNode = Strategy.MakeTreeNode(group);
            TreeView.Add(groupTreeNode);
            return groupTreeNode;
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
        private IList<TreeNode> CreateTreeNodes(TreeNode parentTreeNode, IList<TestNode> path)
        {
            IList<TreeNode> treeNodes = new List<TreeNode>() { parentTreeNode };

            for (int i = 0; i < path.Count; i++)
            {
                TestNode testNode = path[i];
                if (!Strategy.ShowTreeNodeType(testNode))
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
                TreeNode childNode = TryGetChildNode(parentTreeNode, name);
                if (childNode == null)
                    childNode = CreateTreeNode(parentTreeNode, testNode, name);

                // Use childNode as new ParentTreeNode and proceed with next node in path
                parentTreeNode = childNode;
                treeNodes.Add(childNode);
            }

            return treeNodes;
        }

        private TreeNode TryGetChildNode(TreeNode treeNode, string name)
        {
            foreach (TreeNode childNode in treeNode.Nodes)
                if (childNode.Tag is TestGroup group && group.Name == name)
                    return childNode;

            return null;
        }

        private TreeNode CreateTreeNode(TreeNode parentTreeNode, TestNode testNode, string name)
        {
            TreeNode treeNode = !testNode.IsSuite ? Strategy.MakeTreeNode(testNode) : Strategy.MakeTreeNode(CreateTestGroup(name, testNode));
            parentTreeNode?.Nodes.Add(treeNode);

            if (treeNode.Tag is TestGroup g)
            {
                _groups.Add(g);
                AddTestToGroupMapping(testNode, g);
            }

            return treeNode;
        }

        protected virtual TestGroup CreateTestGroup(string name, TestNode testNode)
        {
            return new GroupingTestGroup(testNode, name);
        }

        private void AddTestToGroups(IList<TreeNode> treeNodes, TestNode testNode)
        {
            ResultNode resultNode = Model.TestResultManager.GetResultForTest(testNode.Id);

            IList<TestGroup> testGroups = treeNodes.Select(t => t.Tag).OfType<TestGroup>().ToList();
            foreach (TestGroup testGroup in testGroups)
            {
                testGroup.Add(testNode, resultNode);
                AddTestToGroupMapping(testNode, testGroup);
            }
        }

        private void AddTestToGroupMapping(TestNode testNode, TestGroup testGroup)
        {
            if (_testToGroupMap.TryGetValue(testNode, out IList<TestGroup> groups))
                groups.Add(testGroup);
            else
                _testToGroupMap[testNode] = new List<TestGroup>() { testGroup };
        }

        public void RemoveTestFromGroup(TestGroup testGroup, TestNode testNode)
        {
            // TreeNode is associated with a TestGroup (inner tree node) => Remove testNode from group
            // If TestGroup has no TestNodes anymore, remove the TreeNode
            testGroup.RemoveId(testNode.Id);
            if (_testToGroupMap.TryGetValue(testNode, out var groups))
                groups.Remove(testGroup);

            if (!testGroup.Any())
            {
                _groups.Remove(testGroup);
                _rootGroups.Remove(testGroup);
                testGroup.TreeNode.Remove();
            }
        }
    }
}
