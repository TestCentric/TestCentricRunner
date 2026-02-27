// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Views;

    /// <summary>
    /// NUnitTreeDisplayStrategy is used to display all the tests
    /// in the traditional NUnit tree format.
    /// </summary>
    public class NUnitTreeDisplayStrategy : DisplayStrategy
    {
        public NUnitTreeDisplayStrategy(ITestTreeView view, ITestModel model)
            : base(view, model) 
        {
            _view.SetTestFilterVisibility(model.Settings.Gui.TestTree.ShowFilter);
            _view.CollapseToFixturesCommand.Enabled = true;
        }

        public override string StrategyID => "NUNIT_TREE";

        public override string Description => "NUnit Tree";

        public override void OnTestLoaded(TestNode testNode, VisualState visualState)
        {
            ClearTree();

            foreach (var topLevelTestNode in testNode.Children)
                AddTreeNodeToCollection(topLevelTestNode, _view.Nodes);

            // Update tree state
            if (visualState != null)
                    visualState.ApplyTo(_view.TreeView);
                else
                    SetDefaultInitialExpansion();

            ApplyResultsToTree();

            _view.EnableTestFilter(true);
        }

        private void AddTreeNodeToCollection(TestNode testNode, TreeNodeCollection treeNodes)
        {
            if (ShowTreeNodeType(testNode))
            {
                var treeNode = MakeTreeNode(testNode, false);
                treeNodes.Add(treeNode);

                foreach (var childNode in testNode.Children)
                    AddTreeNodeToCollection(childNode, treeNode.Nodes);
            }
            else
            {
                foreach (var childNode in testNode.Children)
                    AddTreeNodeToCollection(childNode, treeNodes);
            }
        }

        public override VisualState CreateVisualState()
        {
            VisualState visualState = null;

            _view.InvokeIfRequired(() =>
            {
                visualState = new VisualState("NUNIT_TREE", null, _model.TreeConfiguration.ShowNamespaces).LoadFrom(_view.TreeView);
            });

            return visualState;
        }

        private void SetDefaultInitialExpansion()
        {
            TreeNode firstNode = null;
            foreach (TreeNode node in _view.Nodes)
            {
                if (_view.VisibleNodeCount >= node.GetNodeCount(true))
                    node.ExpandAll();
                else
                    CollapseToFixtures(node);

                if (firstNode == null)
                    firstNode = node;
            }

            firstNode?.EnsureVisible();
        }
    }
}
