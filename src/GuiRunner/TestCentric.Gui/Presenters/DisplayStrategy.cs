// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Model.Settings;
    using Views;
    using System.Linq;
    using TestCentric.Gui.Presenters.NUnitGrouping;

    /// <summary>
    /// DisplayStrategy is the abstract base for the various
    /// strategies used to display tests in the tree control.
    /// It works primarily as a traditional strategy, with methods
    /// called by the TreeViewPresenter, but may also function
    /// as a presenter in it's own right, since it is created 
    /// with references to the view and mode.
    /// 
    /// We currently support three different strategies:
    /// NunitTreeDisplay, TestListDisplay and FixtureListDisplay.
    /// </summary>
    public abstract class DisplayStrategy : ITreeDisplayStrategy
    {
        // TODO: This class is temporarily using image index values
        // from TestSuiteTreeNode rather than TestTreeView.

        protected ITestTreeView _view;
        protected ITestModel _model;
        protected IUserSettings _settings;

        protected Dictionary<string, List<TreeNode>> _nodeIndex = new Dictionary<string, List<TreeNode>>();

        #region Construction and Initialization

        public DisplayStrategy(ITestTreeView view, ITestModel model)
        {
            _view = view;
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Load all tests into the tree, starting from a root TestNode.
        /// </summary>
        public abstract void OnTestLoaded(TestNode testNode, VisualState visualState);

        public void SaveVisualState() => _view.InvokeIfRequired(() => CreateVisualState().Save(VisualState.GetVisualStateFileName(_model.TestCentricProject.TestFiles[0])));

        protected abstract VisualState CreateVisualState();

        public void OnTestUnloaded()
        {
            ClearTree();
            _view.EnableTestFilter(false);
        }

        public virtual void OnTestFinished(ResultNode result)
        {
            int imageIndex = CalcImageIndex(result);
            foreach (TreeNode treeNode in GetTreeNodesForTest(result))
            {
                _view.SetImageIndex(treeNode, imageIndex);
            }
        }

        public virtual void OnTestRunStarting()
        {
            _view.InvokeIfRequired(() =>
            {
                UpdateTreeIconsOnRunStart(_view.Nodes);
            });

            if (_settings.Gui.TestTree.ShowTestDuration)
                _view.InvokeIfRequired(() => UpdateTreeNodeNames());
        }

        public virtual void OnTestRunFinished()
        {
            if (_view.SortCommand.SelectedItem == TreeViewNodeComparer.Duration)
            {
                _view.InvokeIfRequired(() => _view.Sort());
            }
            if (_settings.Gui.TestTree.ShowTestDuration)
                _view.InvokeIfRequired(() => UpdateTreeNodeNames());

            _view.InvokeIfRequired(() =>
            {
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
                    treeNode.Nodes.Add(MakeTreeNode(test, true));

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
            return string.Format("{0} ({1})", group.Name, group.Count());
        }

        protected virtual string GetTreeNodeName(TestNode testNode)
        {
            return testNode.Name;
        }

        private string GetTreeNodeDisplayName(TestNode testNode)
        {
            string treeNodeName = GetTreeNodeName(testNode);

            // Check if test result is available for this node
            ResultNode result = _model.TestResultManager.GetResultForTest(testNode.Id);
            if (_settings.Gui.TestTree.ShowTestDuration && result != null)
                treeNodeName += $" [{result.Duration:0.000}s]";

            return treeNodeName;
        }

        /// <summary>
        /// Update all tree node names
        /// If setting 'ShowDuration' is active and test results are available, show test duration in tree node.
        /// </summary>
        public void UpdateTreeNodeNames()
        {
            UpdateTreeNodeNames(_view.Nodes);
        }

        private void UpdateTreeNodeNames(TreeNodeCollection nodes)
        {
            _view.TreeView.BeginUpdate();
            foreach (TreeNode treeNode in nodes)
            {
                UpdateTreeNodeName(treeNode);
                UpdateTreeNodeNames(treeNode.Nodes);
            }
            _view.TreeView.EndUpdate();
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
                if (_settings.Gui.TestTree.ShowTestDuration && testGroup.Duration.HasValue)
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

                bool anyChildNodeRunning = treeNode.Nodes.OfType<TreeNode>().Any(t => t.ImageIndex == TestTreeView.RunningIndex);
                int imageIndex = treeNode.ImageIndex;

                if (anyChildNodeRunning || treeNode.Tag is TestNode testNode && _model.IsInTestRun(testNode))
                    imageIndex = TestTreeView.RunningIndex;
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
                if (treeNode.ImageIndex == TestTreeView.RunningIndex)
                    _view.SetImageIndex(treeNode, TestTreeView.SkippedIndex);

                ResetTestRunningIcons(treeNode.Nodes);
            }
        }

        public void CollapseToFixtures()
        {
            _view.TreeView.BeginUpdate();

            if (_view.Nodes != null) // TODO: Null when mocked
                foreach (TreeNode treeNode in _view.Nodes)
                    CollapseToFixtures(treeNode);

            _view.TreeView.EndUpdate();
        }

        protected void CollapseToFixtures(TreeNode treeNode)
        {
            TestNode testNode = treeNode.Tag as TestNode;
            if (testNode == null && treeNode.Tag is GroupingTestGroup group)
                testNode = group.AssociatedTestNode;

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
