// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;

namespace TestCentric.Gui.Views
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Elements;

    public partial class TestTreeView : UserControl, ITestTreeView
    {
        /// <summary>
        /// Image names and indices for various test states. We load the
        /// images into the image list in the specified order so that the
        /// indices are correct. These are ordered so that the higher values 
        /// are those that propagate upwards in the tree.
        /// </summary>
        private static readonly string[] IMAGE_NAMES = { "Skipped", "Pending", "Running",
            "Inconclusive_NotLatestRun", "Success_NotLatestRun", "Ignored_NotLatestRun", "Warning_NotLatestRun", "Failure_NotLatestRun", 
            "Inconclusive", "Success", "Ignored", "Warning", "Failure" };

        public const int InitIndex = 0;
        public const int SkippedIndex = 0;
        public const int PendingIndex = 1;
        public const int RunningIndex = 2;
        public const int InconclusiveIndex_NotLatestRun = 3;
        public const int SuccessIndex_NotLatestRun = 4;
        public const int IgnoredIndex_NotLatestRun = 5;
        public const int WarningIndex_NotLatestRun = 6;
        public const int FailureIndex_NotLatestRun = 7;
        public const int InconclusiveIndex = 8;
        public const int SuccessIndex = 9;
        public const int IgnoredIndex = 10;
        public const int WarningIndex = 11;
        public const int FailureIndex = 12;

        public event TreeNodeActionHandler SelectedNodeChanged;
        public event TreeNodeActionHandler AfterCheck;
        public event TreeNodeActionHandler TreeNodeDoubleClick;
        public event EventHandler ContextMenuOpening;

        private bool _suppressAfterCheckEvent = false;
        public TestTreeView()
        {
            InitializeComponent();

            RunContextCommand = new CommandMenuElement(this.runMenuItem);
            DebugContextCommand = new CommandMenuElement(this.debugMenuItem);
            ClearResultsContextCommand = new CommandMenuElement(this.clearResultsMenuItem);
            ActiveConfiguration = new PopupMenuElement(this.activeConfigMenuItem);
            ShowCheckBoxes = new CheckedMenuElement(showCheckboxesMenuItem);
            ShowTestDuration = new CheckedMenuElement(showTestDurationMenuItem);
            ExpandAllCommand = new CommandMenuElement(expandAllMenuItem);
            CollapseAllCommand = new CommandMenuElement(collapseAllMenuItem);
            CollapseToFixturesCommand = new CommandMenuElement(collapseToFixturesMenuItem);
            TestPropertiesCommand = new CommandMenuElement(testPropertiesMenuItem);
            ViewAsXmlCommand = new CommandMenuElement(viewAsXmlMenuItem);
            RemoveTestPackageCommand = new CommandMenuElement(removeTestPackageMenuItem);
            TreeViewDeleteKeyCommand = new KeyCommand(treeView, new[] { Keys.Delete, Keys.Back }, null);
            SortCommand = new CheckedToolStripMenuGroup("Sort", sortByNameMenuItem, sortByDurationMenuItem);
            SortDirectionCommand = new CheckedToolStripMenuGroup("SortDirection", sortAscendingMenuItem, sortDescendingMenuItem);
            OutcomeFilter = new MultiCheckedToolStripButtonGroup(new[] { filterOutcomePassedButton, filterOutcomeFailedButton, filterOutcomeWarningButton, filterOutcomeNotRunButton });
            TextFilter = new ToolStripTextBoxElement(filterTextBox, "Filter...");
            CategoryFilter = new ToolStripCategoryFilterButton(filterByCategory);
            ResetFilterCommand = new ToolStripButtonElement(filterResetButton);
            TreeView = treeView;

            // NOTE: We use MouseDown here rather than MouseUp because
            // the menu strip Opening event occurs before MouseUp.
            treeView.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    ContextNode = TreeView.GetNodeAt(e.X, e.Y);
                }
            };

            treeView.ContextMenuStrip.Opening += (s, e) =>
            {
                bool clickedOnNode = ContextNode != null;

                runMenuItem.Visible =
                debugMenuItem.Visible =
                contextMenuSeparator1.Visible =
                viewAsXmlMenuItem.Visible =
                contextMenuSeparator2.Visible = clickedOnNode;

                ContextMenuOpening?.Invoke(s, e);

                // In case presenter cancels
                if (!e.Cancel && clickedOnNode)
                    SelectedNode = ContextNode;
            };

            treeView.NodeMouseDoubleClick += (s, e) => TreeNodeDoubleClick?.Invoke(e.Node);

            treeView.AfterSelect += (s, e) => SelectedNodeChanged?.Invoke(e.Node);

            treeView.AfterCheck += (s, e) =>
            {
                if (_suppressAfterCheckEvent)
                    return;

                // Update checked state of all child nodes if action is triggered by user
                // But not if triggered programmatically. For example while restoring VisualState or while setting child nodes
                if (e.Action == TreeViewAction.ByMouse || e.Action == TreeViewAction.ByKeyboard)
                {
                    _suppressAfterCheckEvent = true;
                    SetCheckedStateOfChildNodes(e.Node, e.Node.Checked);
                    _suppressAfterCheckEvent = false;
                }

                AfterCheck?.Invoke(e.Node);
            };
        }

        #region Properties

        private bool _checkBoxes;
        public bool CheckBoxes
        {
            get { return _checkBoxes; }
            set
            {
                if (_checkBoxes != value)
                {
                    var expandedNodes = new List<TreeNode>();

                    // Turning off checkboxes collapses everything, so we
                    // have to save and restore the expanded nodes.
                    if (!value)
                        foreach (TreeNode node in treeView.Nodes)
                            RecordExpandedNodes(expandedNodes, node);

                    InvokeIfRequired(() => { treeView.CheckBoxes = _checkBoxes = value; });

                    if (!value)
                        foreach (var node in expandedNodes)
                            node.Expand();
                }
            }
        }

        public ICommand RunContextCommand { get; private set; }
        public ICommand DebugContextCommand { get; private set; }
        public ICommand ClearResultsContextCommand { get; private set; }
        public IToolStripMenu ActiveConfiguration { get; private set; }
        public IChecked ShowCheckBoxes { get; private set; }
        public IChecked ShowTestDuration { get; private set; }

        public ISelection SortCommand { get; private set; }
        public ISelection SortDirectionCommand { get; private set; }
        public ICommand ExpandAllCommand { get; private set; }
        public ICommand CollapseAllCommand { get; private set; }

        public ICommand RemoveTestPackageCommand { get; private set; }

        public IKeyCommand TreeViewDeleteKeyCommand { get; private set; }
        
        public ICommand CollapseToFixturesCommand { get; private set; }
        public ICommand TestPropertiesCommand { get; private set; }
        public ICommand ViewAsXmlCommand { get; private set; }

        public TreeView TreeView { get; private set; }

        public IMultiSelection OutcomeFilter { get; private set; }
        public ICategoryFilterSelection CategoryFilter { get; private set; }
        public ICommand ResetFilterCommand { get; private set; }

        public IChanged TextFilter { get; private set; }

        public TreeNode ContextNode { get; private set; }
        public ContextMenuStrip TreeContextMenu => TreeView.ContextMenuStrip;

        private OutcomeImageSet _outcomeImages;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public OutcomeImageSet OutcomeImages
        {
            get { return _outcomeImages; }
            set { LoadAlternateImages(_outcomeImages = value); }
        }

        public TreeNodeCollection Nodes => treeView.Nodes;

        public int VisibleNodeCount => treeView.VisibleCount;
        public TreeNode TopNode
        {
            get { return treeView.TopNode; }
            set { InvokeIfRequired(() => treeView.TopNode = value); }
        }

        public TreeNode SelectedNode
        {
            get { return treeView.SelectedNode; }
            set { InvokeIfRequired(() => treeView.SelectedNode = value); }
        }

        //public IList<TreeNode> SelectedNodes => treeView.SelectedNodes;

        public IList<TreeNode> CheckedNodes => GetCheckedNodes();

        #endregion

        #region Public Methods

        public void Clear() => InvokeIfRequired(() => treeView.Nodes.Clear());
        public void Add(TreeNode treeNode) => InvokeIfRequired(() =>
        {
            treeView.BeginUpdate();
            treeView.Nodes.Add(treeNode);
            if (treeView.SelectedNode == null)
                treeView.SelectedNode = treeNode;
            treeView.EndUpdate();
            treeView.Select();
        });

        public void ExpandAll() => InvokeIfRequired(() =>
        {
             treeView.BeginUpdate();
             treeView.ExpandAll();
             treeView.EndUpdate();
        }); 
        public void CollapseAll() => InvokeIfRequired(() => treeView.CollapseAll());

        public void SetImageIndex(TreeNode treeNode, int imageIndex, bool applyToParent = false) 
        {
            InvokeIfRequired(() => treeNode.ImageIndex = treeNode.SelectedImageIndex = imageIndex);
            if (applyToParent && treeNode.Parent != null)
                SetImageIndex(treeNode.Parent, imageIndex, true);
        }

        public void InvokeIfRequired(MethodInvoker _delegate)
        {
            if (treeView.InvokeRequired)
                treeView.Invoke(_delegate);
            else
                _delegate();
        }

        public void SetTestFilterVisibility(bool isVisible)
        {
            filterToolStrip.Visible = isVisible;
            filterTextToolStrip.Visible = isVisible;
        }

        public void EnableTestFilter(bool enable)
        {
            filterToolStrip.Enabled = enable;
            filterTextToolStrip.Enabled = enable;
        }

        public void LoadAlternateImages(OutcomeImageSet imageSet)
        {
            // Set tree images
            treeImages.Images.Clear();
            foreach (string imageName in IMAGE_NAMES)
                treeImages.Images.Add(imageName, imageSet.LoadImage(imageName));

            // Set filter outcome toolbar images
            filterOutcomeFailedButton.Image = imageSet.LoadImage("Failure");
            filterOutcomePassedButton.Image = imageSet.LoadImage("Success");
            filterOutcomeWarningButton.Image = imageSet.LoadImage("Warning");
            filterOutcomeNotRunButton.Image = imageSet.LoadImage("Skipped");

            this.Invalidate();
            this.Refresh();
        }

        /// <summary>
        /// Apply the current active TreeViewNodeSorter to sort the tree view
        /// </summary>
        public void Sort()
        {
            // Restore selected node after tree sorting
            var selectedNode = treeView.SelectedNode;
            treeView.Sort();
            treeView.SelectedNode = selectedNode;
        }

        /// <summary>
        /// Set a comparer as the TreeViewNodeSorter to sort the tree view
        /// </summary>
        public void Sort(IComparer comparer)
        {
            // Restore selected node after tree sorting
            var selectedNode = treeView.SelectedNode;
            treeView.TreeViewNodeSorter = comparer;
            treeView.SelectedNode = selectedNode;
        }

        #endregion

        #region Helper Methods

        private void SetCheckedStateOfChildNodes(TreeNode node, bool isChecked)
        {
            foreach (TreeNode childNode in node.Nodes)
            {
                childNode.Checked = node.Checked;
                SetCheckedStateOfChildNodes(childNode, isChecked);
            }
        }

        private IList<TreeNode> GetCheckedNodes()
        {
            var checkedNodes = new List<TreeNode>();
            foreach (TreeNode node in treeView.Nodes)
                CollectCheckedNodes(checkedNodes, node);
            return checkedNodes;
        }

        private void CollectCheckedNodes(List<TreeNode> checkedNodes, TreeNode node)
        {
            if (node.Checked)
                checkedNodes.Add(node);
            else
                foreach (TreeNode child in node.Nodes)
                    CollectCheckedNodes(checkedNodes, child);
        }

        private void RecordExpandedNodes(List<TreeNode> expanded, TreeNode startNode)
        {
            if (startNode.IsExpanded)
                expanded.Add(startNode);

            foreach (TreeNode node in startNode.Nodes)
                RecordExpandedNodes(expanded, node);
        }

        #endregion
    }
}
