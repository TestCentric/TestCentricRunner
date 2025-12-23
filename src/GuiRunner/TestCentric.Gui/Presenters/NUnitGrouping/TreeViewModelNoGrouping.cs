// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System.Collections.Generic;
    using System.Linq;
    using TestCentric.Gui.Model;

    /// <summary>
    /// Create a tree view model with no grouping - i.e. the standard NUnit tree
    /// </summary>
    public class TreeViewModelNoGrouping : TreeViewModelBase
    {
        public TreeViewModelNoGrouping(ITestModel model) : base(model)
        {
        }

        /// <inheritdoc />
        public override void CreateTreeModel(TestNode testNode)
        {
            foreach (var topLevelNode in testNode.Children)
                if (topLevelNode.IsVisible)
                    RootViewModels.Add(CreateNUnitTreeNode(null, topLevelNode));

            // Map 'Test-Run' TestNode to all root nodes (for RunAll tests command)
            foreach (TreeNodeViewModel rootNode in RootViewModels)
                AddTestToViewModelMapping(testNode, rootNode);
        }

        private TreeNodeViewModel CreateNUnitTreeNode(TreeNodeViewModel parentNode, TestNode testNode)
        {
            TreeNodeViewModel nodeViewModel = null;

            if (ShowTreeNodeType(testNode))
            {
                if (FoldNamespaceNodesHandler.IsNamespaceNode(testNode))
                {
                    // Get list of all namespace nodes which can be folded
                    // And get name of folded namespaces and store in dictionary for later usage
                    IList<TestNode> foldedNodes = FoldNamespaceNodesHandler.FoldNamespaceNodes(testNode);
                    string name = FoldNamespaceNodesHandler.GetFoldedNamespaceName(foldedNodes);

                    nodeViewModel = CreateAndInitTreeNodeViewModel(foldedNodes.First(), name); // Create node representing the first node
                    testNode = foldedNodes.Last();                                  // But proceed building up tree with last node
                }
                else
                    nodeViewModel = CreateAndInitTreeNodeViewModel(testNode, testNode.Name);

                parentNode?.AddChild(nodeViewModel);
                parentNode = nodeViewModel;
            }

            foreach (TestNode child in testNode.Children)
                if (child.IsVisible)
                    CreateNUnitTreeNode(parentNode, child);

            return nodeViewModel;
        }

        protected override TreeNodeViewModel CreateTreeNodeViewModel(TestNode testNode, string name)
        {
            return new TreeNodeViewModel(Model, testNode, name);
        }
    }
}
