// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Model.Settings;
    using Views;

    /// <summary>
    /// DisplayStrategy is the abstract base for the various
    /// strategies used to display tests in the tree control.
    /// It works primarily as a traditional strategy, with methods
    /// called by the TreeViewPresenter, but may also function
    /// as a presenter in it's own right, since it is created 
    /// with references to the view and mode.
    /// 
    /// We currently support two different strategies:
    /// NunitTreeDisplay and TestListDisplay
    /// </summary>
    public abstract class DisplayStrategy : ITreeDisplayStrategy
    {
        protected ITestTreeView _view;
        protected ITestModel _model;
        protected IUserSettings _settings;
        protected TreeView _treeView;

        protected Dictionary<string, List<TreeNode>> _nodeIndex = new Dictionary<string, List<TreeNode>>();

        #region Construction and Initialization

        public DisplayStrategy(ITestTreeView view, ITestModel model)
        {
            _view = view;
            _treeView = view.TreeView;
            _model = model;
            _settings = _model.Settings;
        }

        #endregion

        #region Properties

        public bool HasResults
        {
            get { return _model.HasResults; }
        }

        public abstract string StrategyID { get; }

        public abstract string Description { get; }

        internal ITreeConfiguration TreeConfiguration => _model.TreeConfiguration;

        #endregion

        #region Public Methods

        /// <summary>
        /// Load all tests into the tree, starting from a root TestNode.
        /// </summary>
        public virtual void OnTestLoaded(TestNode rootNode, VisualState visualState)
        {
            ClearTree();

            AddTreeNodesToCollection(rootNode.Children, _view.Nodes);

            // Update tree state
            if (visualState != null)
                visualState.ApplyTo(_view.TreeView);
            else
                SetInitialExpansion();

            ApplyResultsToTree();

            _view.EnableTestFilter(true);
        }

        protected void AddTreeNodesToCollection(IEnumerable<TestNode> testNodes, TreeNodeCollection treeNodes)
        {
            foreach (var testNode in testNodes)
                AddTreeNodeToCollection(testNode, treeNodes);
        }

        protected void AddTreeNodeToCollection(TestNode testNode, TreeNodeCollection treeNodes)
        {
            if (ShowTreeNodeType(testNode))
            {
                var treeNode = MakeTreeNode(testNode, false);
                treeNodes.Add(treeNode);
                treeNodes = treeNode.Nodes;
            }

            AddTreeNodesToCollection(testNode.Children, treeNodes);
        }

        protected virtual void SetInitialExpansion() { }

        public abstract VisualState CreateVisualState();

        public void OnTestUnloaded()
        {
            ClearTree();
            _view.EnableTestFilter(false);
        }

        public virtual void OnTestStarting(TestNode testNode)
        {
            foreach (TreeNode treeNode in GetTreeNodesForTest(testNode))
                _view.SetImageIndex(treeNode, TestTreeView.RunningIndex, true);
        }

        public virtual void OnTestFinished(ResultNode result)
        {
            _view.InvokeIfRequired(() =>
            {
                int imageIndex = CalcImageIndex(result);
                foreach (TreeNode treeNode in GetTreeNodesForTest(result))
                {
                    treeNode.Text = GetTreeNodeDisplayName(result);
                    _view.SetImageIndex(treeNode, imageIndex, true);
                }
            });
        }

        public virtual void OnTestRunStarting()
        {
            _view.InvokeIfRequired(() =>
            {
                UpdateTreeIconsOnRunStart(_view.Nodes);

                if (TreeConfiguration.NUnitTreeShowTestDuration)
                    UpdateTreeNodeNames();
            });
        }

        public virtual void OnTestRunFinished()
        {
            _view.InvokeIfRequired(() =>
            {
                if (_view.SortCommand.SelectedItem == TreeViewNodeComparer.Duration)
                    _view.Sort();

                if (TreeConfiguration.NUnitTreeShowTestDuration)
                    UpdateTreeNodeNames();

                ResetTestRunningIcons(_view.Nodes);
            });
        }

        // Called when either the display strategy or the grouping
        // changes. May need to distinguish these cases.
        public void Reload(bool applyVisualState = false)
        {
            TestNode testNode = _model.LoadedTests;
            if (testNode != null)
            {
                VisualState visualState = applyVisualState ? CreateVisualState() : null;
                OnTestLoaded(testNode, visualState);
                ApplyResultsToTree();
            }
        }

        #endregion

        #region Helper Methods

        protected void ClearTree()
        {
            _view.Clear();
            _nodeIndex.Clear();
        }

        protected TreeNode MakeTreeNode(TestGroup group, bool recursive)
        {
            TreeNode treeNode = new TreeNode(GroupDisplayName(group), group.ImageIndex, group.ImageIndex);
            treeNode.Tag = group;
            group.TreeNode = treeNode;

            if (recursive)
                foreach (TestNode test in group)
                    AddTreeNodeToCollection(test, treeNode.Nodes);

            return treeNode;
        }

        public TreeNode MakeTreeNode(TestNode testNode, bool recursive)
        {
            string treeNodeName = GetTreeNodeDisplayName(testNode);
            TreeNode treeNode = new TreeNode(treeNodeName);
            treeNode.Tag = testNode;

            int imageIndex = TestTreeView.SkippedIndex;

            switch (testNode.RunState)
            {
                case RunState.Ignored:
                    imageIndex = TestTreeView.IgnoredIndex;
                    break;
                case RunState.NotRunnable:
                    imageIndex = TestTreeView.FailureIndex;
                    break;
            }

            treeNode.ImageIndex = treeNode.SelectedImageIndex = imageIndex;

            AddTestNodeMapping(testNode, treeNode);

            if (recursive)
                foreach (TestNode childNode in testNode.Children)
                    treeNode.Nodes.Add(MakeTreeNode(childNode, true));

            return treeNode;
        }

        /// <summary>
        /// Check if a tree node type should be shown or omitted.
        /// </summary>
        protected abstract bool ShowTreeNodeType(TestNode testNode);

        protected void AddTestNodeMapping(TestNode testNode, TreeNode treeNode)
        {
            string id = testNode.Id;
            if (_nodeIndex.ContainsKey(id))
                _nodeIndex[id].Add(treeNode);
            else
                _nodeIndex.Add(id, new List<TreeNode> { treeNode });
        }

        public string GroupDisplayName(TestGroup group)
        {
            return string.Format("{0} ({1})", group.Name, group.Items.Count());
        }

        protected virtual string GetTreeNodeName(TestNode testNode)
        {
            return testNode.Name;
        }

        private string GetTreeNodeDisplayName(TestNode testNode)
        {
            string treeNodeName = GetTreeNodeName(testNode);

            // Check if test result is available for this node
            ResultNode result = testNode as ResultNode ?? _model.TestResultManager.GetResultForTest(testNode.Id);
            if (TreeConfiguration.NUnitTreeShowTestDuration && result != null)
                treeNodeName += $" [{result.Duration:0.000}s]";

            return treeNodeName;
        }

        /// <summary>
        /// Update all tree node names
        /// If setting 'ShowDuration' is active and test results are available, show test duration in tree node.
        /// </summary>
        public virtual void UpdateTreeNodeNames()
        {
            UpdateTreeNodeNames(_view.Nodes);
        }

        private void UpdateTreeNodeNames(TreeNodeCollection nodes)
        {
            _treeView.BeginUpdate();
            foreach (TreeNode treeNode in nodes)
            {
                UpdateTreeNodeName(treeNode);
                UpdateTreeNodeNames(treeNode.Nodes);
            }
            _treeView.EndUpdate();
        }

        public void UpdateTreeNodeNames(IEnumerable<TestGroup> groups)
        {
            foreach (TestGroup group in groups)
                UpdateTreeNodeName(group.TreeNode);
        }

        private void UpdateTreeNodeName(TreeNode treeNode)
        {
            string treeNodeName = "";
            TestNode testNode = treeNode.Tag as TestNode; 
            if (testNode != null)
                treeNodeName = GetTreeNodeDisplayName(testNode);
            else if (treeNode.Tag is TestGroup testGroup)
            {
                treeNodeName = GroupDisplayName(testGroup);
                if (TreeConfiguration.NUnitTreeShowTestDuration && testGroup.Duration.HasValue)
                    treeNodeName += $" [{testGroup.Duration.Value:0.000}s]";
            }

            _view.InvokeIfRequired(() => treeNode.Text = treeNodeName);
        }

        public static int CalcImageIndex(ResultNode resultNode)
        {
            return CalcImageIndex(resultNode, resultNode.IsLatestRun);
        }

        public static int CalcImageIndex(ResultNode resultNode, bool latestRun)
        {
            return CalcImageIndex(resultNode.Outcome, latestRun);
        }

        public static int CalcImageIndex(ResultState resultState, bool latestRun)
        {
            switch (resultState.Status)
            {
                case TestStatus.Inconclusive:
                    return latestRun ? TestTreeView.InconclusiveIndex : TestTreeView.InconclusiveIndex_NotLatestRun;
                case TestStatus.Passed:
                    return latestRun ? TestTreeView.SuccessIndex : TestTreeView.SuccessIndex_NotLatestRun;
                case TestStatus.Failed:
                    return latestRun ? TestTreeView.FailureIndex : TestTreeView.FailureIndex_NotLatestRun;
                case TestStatus.Warning:
                    return latestRun ? TestTreeView.WarningIndex : TestTreeView.WarningIndex_NotLatestRun;
                case TestStatus.Skipped:
                default:
                    return resultState.Label == "Ignored"
                        ? latestRun ? TestTreeView.IgnoredIndex : TestTreeView.IgnoredIndex_NotLatestRun
                        : TestTreeView.SkippedIndex;
            }
        }

        protected void ApplyResultsToTree()
        {
            if (!_model.HasResults || _view.Nodes == null)
                return;

            foreach (TreeNode treeNode in _view.Nodes)
                ApplyResultsToTree(treeNode);
        }

        private void ApplyResultsToTree(TreeNode treeNode)
        {
            TestNode testNode = treeNode.Tag as TestNode;

            if (testNode != null)
            {
                ResultNode resultNode = GetResultForTest(testNode);
                if (resultNode != null)
                    treeNode.ImageIndex = treeNode.SelectedImageIndex = CalcImageIndex(resultNode);
            }

            foreach (TreeNode childNode in treeNode.Nodes)
                ApplyResultsToTree(childNode);
        }

        private void UpdateTreeIconsOnRunStart(TreeNodeCollection treeNodes)
        {
            foreach (TreeNode treeNode in treeNodes)
            {
                UpdateTreeIconsOnRunStart(treeNode.Nodes);

                bool anyChildNodeRunning = treeNode.Nodes.OfType<TreeNode>().Any(t => t.ImageIndex == TestTreeView.PendingIndex);
                int imageIndex = treeNode.ImageIndex;

                if (anyChildNodeRunning || treeNode.Tag is TestNode testNode && _model.IsInTestRun(testNode))
                    imageIndex = TestTreeView.PendingIndex;
                else
                {
                    imageIndex = UpdateTreeIconToPreviousRun(treeNode.ImageIndex);
                    if (treeNode.Tag is TestGroup testGroup)
                        testGroup.ImageIndex = imageIndex;
                }

                _view.SetImageIndex(treeNode, imageIndex);
            }
        }

        private int UpdateTreeIconToPreviousRun(int imageIndex)
        {
            switch (imageIndex)
            {
                case TestTreeView.InconclusiveIndex:
                    return TestTreeView.InconclusiveIndex_NotLatestRun;
                case TestTreeView.SuccessIndex:
                    return TestTreeView.SuccessIndex_NotLatestRun;
                case TestTreeView.WarningIndex:
                    return TestTreeView.WarningIndex_NotLatestRun;
                case TestTreeView.FailureIndex:
                    return TestTreeView.FailureIndex_NotLatestRun;
                case TestTreeView.IgnoredIndex:
                    return TestTreeView.IgnoredIndex_NotLatestRun;
            }
            return imageIndex;
        }

        protected void ResetTestRunningIcons(TreeNodeCollection treeNodes)
        {
            // Only required for exceptional use case 'force stop test run'
            foreach (TreeNode treeNode in treeNodes)
            {
                if (treeNode.ImageIndex == TestTreeView.PendingIndex || treeNode.ImageIndex == TestTreeView.RunningIndex)
                    _view.SetImageIndex(treeNode, TestTreeView.SkippedIndex);

                ResetTestRunningIcons(treeNode.Nodes);
            }
        }

        public void CollapseToFixtures()
        {
            _treeView.BeginUpdate();

            if (_view.Nodes != null) // TODO: Null when mocked
                foreach (TreeNode treeNode in _view.Nodes)
                    CollapseToFixtures(treeNode);

            _treeView.EndUpdate();
        }

        protected void CollapseToFixtures(TreeNode treeNode)
        {
            TestNode testNode = treeNode.Tag as TestNode;

            if (testNode != null && testNode.Type == "TestFixture")
                treeNode.Collapse();
            else if (testNode == null || testNode.IsSuite)
            {
                treeNode.Expand();
                foreach (TreeNode child in treeNode.Nodes)
                    CollapseToFixtures(child);
            }
        }

        public List<TreeNode> GetTreeNodesForTest(TestNode testNode)
        {
            return GetTreeNodesForTest(testNode.Id);
        }

        public List<TreeNode> GetTreeNodesForTest(string id)
        {
            List<TreeNode> treeNodes;
            if (!_nodeIndex.TryGetValue(id, out treeNodes))
                treeNodes = new List<TreeNode>();

            return treeNodes;
        }

        /// <summary>
        /// Removes one tree node from the tree
        /// </summary>
        public void RemoveTreeNode(TreeNode treeNode)
        {
            if (treeNode.Tag is TestNode testNode && _nodeIndex.TryGetValue(testNode.Id, out List<TreeNode> treeNodeList))
                treeNodeList.Remove(treeNode);

            treeNode.Remove();
        }

        public ResultNode GetResultForTest(TestNode testNode)
        {
            return _model.TestResultManager.GetResultForTest(testNode.Id);
        }

        #endregion
    }
}
