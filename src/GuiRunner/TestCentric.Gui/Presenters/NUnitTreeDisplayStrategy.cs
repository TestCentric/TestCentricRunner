// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using Model;
    using TestCentric.Gui.Presenters.NUnitGrouping;
    using Views;

    /// <summary>
    /// NUnitTreeDisplayStrategy is used to display a the tests
    /// in the traditional NUnit tree format.
    /// </summary>
    public class NUnitTreeDisplayStrategy : DisplayStrategy, INUnitTreeDisplayStrategy
    {
        private IDictionary<TestNode, string> _foldedNodeNames = new Dictionary<TestNode, string>();
        private INUnitGrouping _grouping;

        public NUnitTreeDisplayStrategy(ITestTreeView view, ITestModel model)
            : base(view, model) 
        {
            _view.SetTestFilterVisibility(model.Settings.Gui.TestTree.ShowFilter);
            _view.CollapseToFixturesCommand.Enabled = true;
        }


        public override string StrategyID => "NUNIT_TREE";

        public override string Description
        {
            get { return "NUnit Tree"; }
        }

        public override void OnTestLoaded(TestNode testNode, VisualState visualState)
        {
            ClearTree();
            _foldedNodeNames.Clear();

            switch (_model.Settings.Gui.TestTree.NUnitGroupBy)
            {
                case "CATEGORY": _grouping = new NUnitGrouping.CategoryGrouping(this, _model, _view); break;
                case "OUTCOME": _grouping = new NUnitGrouping.OutcomeGrouping(this, _model, _view); break;
                case "DURATION": _grouping = new NUnitGrouping.DurationGrouping(this, _model, _view); break;
                default: _grouping = null; break;
            }

            if (_grouping != null)
                _grouping.CreateTree(testNode);

            else
            {
                foreach (var topLevelNode in testNode.Children)
                    if (topLevelNode.IsVisible)
                        _view.Add(CreateNUnitTreeNode(null, topLevelNode));
            }

            _view.TreeView?.BeginUpdate();
            if (visualState != null)
                visualState.ApplyTo(_view.TreeView);
            else
                SetDefaultInitialExpansion();

            ApplyResultsToTree();
            _view.TreeView?.EndUpdate();

            _view.EnableTestFilter(true);
        }

        public override void OnTestFinished(ResultNode result)
        {
            base.OnTestFinished(result);
            _grouping?.OnTestFinished(result);
        }

        public override void OnTestRunStarting()
        {
            _grouping?.OnTestRunStarting();
            base.OnTestRunStarting();
        }

        public override void OnTestRunFinished()
        {
            _grouping?.OnTestRunFinished();
            base.OnTestRunFinished();
        }

        protected override VisualState CreateVisualState() => new VisualState("NUNIT_TREE", _settings.Gui.TestTree.NUnitGroupBy, _settings.Gui.TestTree.ShowNamespace).LoadFrom(_view.TreeView);

        protected override string GetTreeNodeName(TestNode testNode)
        {
            // For folded namespace nodes use the combined name of all folded nodes ("Library.Test.Folder")
            if (_foldedNodeNames.TryGetValue(testNode, out var name)) 
                return name;

            return base.GetTreeNodeName(testNode);
        }

        private TreeNode CreateNUnitTreeNode(TreeNode parentNode, TestNode testNode)
        {
            TreeNode treeNode = null;

            if (ShowTreeNodeType(testNode))
            {
                if (FoldNamespaceNodesHandler.IsNamespaceNode(testNode))
                {
                    // Get list of all namespace nodes which can be folded
                    // And get name of folded namespaces and store in dictionary for later usage
                    IList<TestNode> foldedNodes = FoldNamespaceNodesHandler.FoldNamespaceNodes(testNode);
                    _foldedNodeNames[foldedNodes.First()] = FoldNamespaceNodesHandler.GetFoldedNamespaceName(foldedNodes);

                    treeNode = MakeTreeNode(foldedNodes.First(), false);    // Create TreeNode representing the first node
                    testNode = foldedNodes.Last();                          // But proceed building up tree with last node
                }
                else
                    treeNode = MakeTreeNode(testNode, false);

                parentNode?.Nodes.Add(treeNode);
                parentNode = treeNode;
            }

            foreach (TestNode child in testNode.Children)
                if (child.IsVisible)
                    CreateNUnitTreeNode(parentNode, child);

            return treeNode;
        }

        /// <summary>
        /// Check if a tree node type should be shown or omitted
        /// Currently we support only omitting the namespace nodes
        /// </summary>
        public bool ShowTreeNodeType(TestNode testNode)
        {
            if (FoldNamespaceNodesHandler.IsNamespaceNode(testNode))
                return _settings.Gui.TestTree.ShowNamespace;

            return true;
        }

        /// <summary>
        /// Creates a new tree node for a TestNode
        /// </summary>
        public TreeNode MakeTreeNode(TestNode testNode)
        {
            TreeNode treeNode = MakeTreeNode(testNode, false);
            ResultNode resultNode = _model.TestResultManager.GetResultForTest(testNode.Id);
            if (resultNode != null)
                _view.SetImageIndex(treeNode, DisplayStrategy.CalcImageIndex(resultNode));

            return treeNode;
        }

        /// <summary>
        /// Creates a new tree node for a TestGroup
        /// </summary>
        public TreeNode MakeTreeNode(TestGroup testGroup)
        {
            return MakeTreeNode(testGroup, false);
        }

        private void SetDefaultInitialExpansion()
        {
            var displayStyle = (InitialTreeExpansion)_settings.Gui.TestTree.InitialTreeDisplay;

            TreeNode firstNode = null;
            foreach (TreeNode node in _view.Nodes)
            {
                SetInitialExpansion(displayStyle, node);
                if (firstNode == null)
                    firstNode = node;
            }

            firstNode?.EnsureVisible();
        }

        private void SetInitialExpansion(InitialTreeExpansion displayStyle, TreeNode treeNode)
        {
            switch (displayStyle)
            {
                case InitialTreeExpansion.Auto:
                    if (_view.VisibleNodeCount >= treeNode.GetNodeCount(true))
                        treeNode.ExpandAll();
                    else
                        CollapseToFixtures(treeNode);
                    break;
                case InitialTreeExpansion.Expand:
                    treeNode.ExpandAll();
                    break;
                case InitialTreeExpansion.Collapse:
                    treeNode.Collapse();
                    break;
                case InitialTreeExpansion.HideTests:
                    CollapseToFixtures(treeNode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(displayStyle), displayStyle, null);
            }
        }
    }
}
