// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Views
{
    using System.Collections;
    using System.Collections.Generic;
    using Elements;

    public delegate void TreeNodeActionHandler(TreeNode treeNode);
    public delegate void MultipleTreeNodeActionHandler(IList<TreeNode> treeNodes);

    // Interface used for testing
    public interface ITestTreeView : IView
    {
        // Events
        event TreeNodeActionHandler SelectedNodeChanged;
        event TreeNodeActionHandler AfterCheck;
        event TreeNodeActionHandler TreeNodeDoubleClick;
        event EventHandler ContextMenuOpening;

        // Commands
        ICommand RunContextCommand { get; }
        ICommand DebugContextCommand { get; }
        ICommand ClearResultsContextCommand { get; }
        IToolStripMenu ActiveConfiguration { get; }
        IChecked ShowCheckBoxes { get; }
        IChecked ShowTestDuration { get; }
        ISelection SortCommand { get; }
        ISelection SortDirectionCommand { get; }

        ICommand ExpandAllCommand { get; }
        ICommand CollapseAllCommand { get; }
        ICommand CollapseToFixturesCommand { get; }
        ICommand TestPropertiesCommand { get; }
        ICommand ViewAsXmlCommand { get; }
        ICommand RemoveTestPackageCommand { get; }
        IKeyCommand TreeViewDeleteKeyCommand { get; }

        // Tree Properties
        ContextMenuStrip TreeContextMenu { get; }

        bool CheckBoxes { get; set; }

        OutcomeImageSet OutcomeImages { get; set; }
        int VisibleNodeCount { get; }
        TreeNode TopNode { get; set; }

        TreeView TreeView { get; }
        TreeNodeCollection Nodes { get; }
        TreeNode ContextNode { get; }
        TreeNode SelectedNode { get; set; }
        IList<TreeNode> CheckedNodes { get; }

        // Test Filter related properties / methods
        IMultiSelection OutcomeFilter { get; }

        IChanged TextFilter { get; }

        ICategoryFilterSelection CategoryFilter { get; }

        ICommand ResetFilterCommand { get; }

        void SetTestFilterVisibility(bool visible);

        void EnableTestFilter(bool enable);

        // Tree-related Methods
        void Clear();
        void Add(TreeNode treeNode);
        void ExpandAll();
        void CollapseAll();

        void Sort();
        void Sort(IComparer comparer);
        void SetImageIndex(TreeNode treeNode, int imageIndex, bool applyToParent=false);

        /// <summary>
        /// Invoke a delegate if necessary, otherwise just call it
        /// </summary>
        void InvokeIfRequired(MethodInvoker _delegate);
    }
}
