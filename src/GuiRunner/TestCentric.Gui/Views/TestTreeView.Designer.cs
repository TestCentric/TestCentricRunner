namespace TestCentric.Gui.Views
{
    using TestCentric.Gui.Controls;
    using TestCentric.Gui.Properties;

    partial class TestTreeView
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TestTreeView));
            this.treeView = new System.Windows.Forms.TreeView();
            this.testTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.runMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearResultsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.testPropertiesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewAsXmlMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeTestPackageMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activeConfigMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.showCheckboxesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showTestDurationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.sortByMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortByNameMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortByDurationMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.sortAscendingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sortDescendingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.expandAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseAllMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.collapseToFixturesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeImages = new System.Windows.Forms.ImageList(this.components);
            this.filterToolStrip = new System.Windows.Forms.ToolStrip();
            this.filterOutcomeLabel = new System.Windows.Forms.ToolStripLabel();
            this.filterOutcomePassedButton = new System.Windows.Forms.ToolStripButton();
            this.filterOutcomeFailedButton = new System.Windows.Forms.ToolStripButton();
            this.filterOutcomeWarningButton = new System.Windows.Forms.ToolStripButton();
            this.filterOutcomeNotRunButton = new System.Windows.Forms.ToolStripButton();
            this.filterSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.filterByCategory = new System.Windows.Forms.ToolStripDropDownButton();
            this.filterResetButton = new System.Windows.Forms.ToolStripButton();
            this.filterTextToolStrip = new System.Windows.Forms.ToolStrip();
            this.filterTextBox = new TestCentric.Gui.Controls.StretchToolStripTextBox();
            this.testTreeContextMenu.SuspendLayout();
            this.filterToolStrip.SuspendLayout();
            this.filterTextToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView
            // 
            this.treeView.ContextMenuStrip = this.testTreeContextMenu;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.HideSelection = false;
            this.treeView.ImageIndex = 0;
            this.treeView.ImageList = this.treeImages;
            this.treeView.Location = new System.Drawing.Point(0, 0);
            this.treeView.Name = "treeView";
            this.treeView.SelectedImageIndex = 0;
            this.treeView.Size = new System.Drawing.Size(191, 246);
            this.treeView.TabIndex = 1;
            // 
            // testTreeContextMenu
            // 
            this.testTreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.runMenuItem,
            this.debugMenuItem,
            this.clearResultsMenuItem,
            this.contextMenuSeparator1,
            this.testPropertiesMenuItem,
            this.viewAsXmlMenuItem,
            this.removeTestPackageMenuItem,
            this.activeConfigMenuItem,
            this.contextMenuSeparator2,
            this.showCheckboxesMenuItem,
            this.showTestDurationMenuItem,
            this.contextMenuSeparator3,
            this.sortByMenuItem,
            this.contextMenuSeparator4,
            this.expandAllMenuItem,
            this.collapseAllMenuItem,
            this.collapseToFixturesMenuItem});
            this.testTreeContextMenu.Name = "testTreeContextMenu";
            this.testTreeContextMenu.Size = new System.Drawing.Size(181, 336);
            // 
            // runMenuItem
            // 
            this.runMenuItem.Name = "runMenuItem";
            this.runMenuItem.Size = new System.Drawing.Size(180, 22);
            this.runMenuItem.Text = "Run";
            // 
            // debugMenuItem
            // 
            this.debugMenuItem.Name = "debugMenuItem";
            this.debugMenuItem.Size = new System.Drawing.Size(180, 22);
            this.debugMenuItem.Text = "Debug";
            // 
            // clearResultsMenuItem
            // 
            this.clearResultsMenuItem.Name = "clearResultsMenuItem";
            this.clearResultsMenuItem.Size = new System.Drawing.Size(180, 22);
            this.clearResultsMenuItem.Text = "Clear test results";
            // 
            // contextMenuSeparator1
            // 
            this.contextMenuSeparator1.Name = "contextMenuSeparator1";
            this.contextMenuSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // testPropertiesMenuItem
            // 
            this.testPropertiesMenuItem.Name = "testPropertiesMenuItem";
            this.testPropertiesMenuItem.Size = new System.Drawing.Size(180, 22);
            this.testPropertiesMenuItem.Text = "Properties...";
            // 
            // viewAsXmlMenuItem
            // 
            this.viewAsXmlMenuItem.Name = "viewAsXmlMenuItem";
            this.viewAsXmlMenuItem.Size = new System.Drawing.Size(180, 22);
            this.viewAsXmlMenuItem.Text = "View as XML...";
            // 
            // removeTestPackageMenuItem
            // 
            this.removeTestPackageMenuItem.Name = "removeTestPackageMenuItem";
            this.removeTestPackageMenuItem.Size = new System.Drawing.Size(180, 22);
            this.removeTestPackageMenuItem.Text = "Remove test file";
            // 
            // activeConfigMenuItem
            // 
            this.activeConfigMenuItem.Name = "activeConfigMenuItem";
            this.activeConfigMenuItem.Size = new System.Drawing.Size(180, 22);
            this.activeConfigMenuItem.Text = "Active Config";
            // 
            // contextMenuSeparator2
            // 
            this.contextMenuSeparator2.Name = "contextMenuSeparator2";
            this.contextMenuSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // showCheckboxesMenuItem
            // 
            this.showCheckboxesMenuItem.CheckOnClick = true;
            this.showCheckboxesMenuItem.Name = "showCheckboxesMenuItem";
            this.showCheckboxesMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showCheckboxesMenuItem.Text = "Show Checkboxes";
            // 
            // showTestDurationMenuItem
            // 
            this.showTestDurationMenuItem.CheckOnClick = true;
            this.showTestDurationMenuItem.Name = "showTestDurationMenuItem";
            this.showTestDurationMenuItem.Size = new System.Drawing.Size(180, 22);
            this.showTestDurationMenuItem.Text = "Show Test Duration";
            // 
            // contextMenuSeparator3
            // 
            this.contextMenuSeparator3.Name = "contextMenuSeparator3";
            this.contextMenuSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // sortByMenuItem
            // 
            this.sortByMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.sortByNameMenuItem,
            this.sortByDurationMenuItem,
            this.sortMenuSeparator,
            this.sortAscendingMenuItem,
            this.sortDescendingMenuItem});
            this.sortByMenuItem.Name = "sortByMenuItem";
            this.sortByMenuItem.Size = new System.Drawing.Size(180, 22);
            this.sortByMenuItem.Text = "Sort by ...";
            // 
            // sortByNameMenuItem
            // 
            this.sortByNameMenuItem.CheckOnClick = true;
            this.sortByNameMenuItem.Name = "sortByNameMenuItem";
            this.sortByNameMenuItem.Size = new System.Drawing.Size(136, 22);
            this.sortByNameMenuItem.Tag = "Name";
            this.sortByNameMenuItem.Text = "Name";
            // 
            // sortByDurationMenuItem
            // 
            this.sortByDurationMenuItem.CheckOnClick = true;
            this.sortByDurationMenuItem.Name = "sortByDurationMenuItem";
            this.sortByDurationMenuItem.Size = new System.Drawing.Size(136, 22);
            this.sortByDurationMenuItem.Tag = "Duration";
            this.sortByDurationMenuItem.Text = "Duration";
            // 
            // sortMenuSeparator
            // 
            this.sortMenuSeparator.Name = "sortMenuSeparator";
            this.sortMenuSeparator.Size = new System.Drawing.Size(133, 6);
            // 
            // sortAscendingMenuItem
            // 
            this.sortAscendingMenuItem.CheckOnClick = true;
            this.sortAscendingMenuItem.Name = "sortAscendingMenuItem";
            this.sortAscendingMenuItem.Size = new System.Drawing.Size(136, 22);
            this.sortAscendingMenuItem.Tag = "Ascending";
            this.sortAscendingMenuItem.Text = "Ascending";
            // 
            // sortDescendingMenuItem
            // 
            this.sortDescendingMenuItem.CheckOnClick = true;
            this.sortDescendingMenuItem.Name = "sortDescendingMenuItem";
            this.sortDescendingMenuItem.Size = new System.Drawing.Size(136, 22);
            this.sortDescendingMenuItem.Tag = "Descending";
            this.sortDescendingMenuItem.Text = "Descending";
            // 
            // contextMenuSeparator4
            // 
            this.contextMenuSeparator4.Name = "contextMenuSeparator4";
            this.contextMenuSeparator4.Size = new System.Drawing.Size(177, 6);
            // 
            // expandAllMenuItem
            // 
            this.expandAllMenuItem.Name = "expandAllMenuItem";
            this.expandAllMenuItem.Size = new System.Drawing.Size(180, 22);
            this.expandAllMenuItem.Text = "Expand All";
            // 
            // collapseAllMenuItem
            // 
            this.collapseAllMenuItem.Name = "collapseAllMenuItem";
            this.collapseAllMenuItem.Size = new System.Drawing.Size(180, 22);
            this.collapseAllMenuItem.Text = "Collapse All";
            // 
            // collapseToFixturesMenuItem
            // 
            this.collapseToFixturesMenuItem.Name = "collapseToFixturesMenuItem";
            this.collapseToFixturesMenuItem.Size = new System.Drawing.Size(180, 22);
            this.collapseToFixturesMenuItem.Text = "Display Fixtures";
            // 
            // treeImages
            // 
            this.treeImages.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.treeImages.ImageSize = new System.Drawing.Size(16, 16);
            this.treeImages.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // filterToolStrip
            // 
            this.filterToolStrip.Enabled = false;
            this.filterToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterOutcomeLabel,
            this.filterOutcomePassedButton,
            this.filterOutcomeFailedButton,
            this.filterOutcomeWarningButton,
            this.filterOutcomeNotRunButton,
            this.filterSeparator1,
            this.filterByCategory,
            this.filterResetButton});
            this.filterToolStrip.Location = new System.Drawing.Point(0, 0);
            this.filterToolStrip.Name = "filterToolStrip";
            this.filterToolStrip.Size = new System.Drawing.Size(191, 25);
            this.filterToolStrip.TabIndex = 0;
            this.filterToolStrip.Visible = false;
            // 
            // filterOutcomeLabel
            // 
            this.filterOutcomeLabel.Name = "filterOutcomeLabel";
            this.filterOutcomeLabel.Size = new System.Drawing.Size(36, 22);
            this.filterOutcomeLabel.Text = "Filter:";
            // 
            // filterOutcomePassedButton
            // 
            this.filterOutcomePassedButton.CheckOnClick = true;
            this.filterOutcomePassedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.filterOutcomePassedButton.Name = "filterOutcomePassedButton";
            this.filterOutcomePassedButton.Size = new System.Drawing.Size(23, 22);
            this.filterOutcomePassedButton.Tag = "Passed";
            this.filterOutcomePassedButton.Text = "Passed";
            this.filterOutcomePassedButton.ToolTipText = "Show all passed tests";
            // 
            // filterOutcomeFailedButton
            // 
            this.filterOutcomeFailedButton.CheckOnClick = true;
            this.filterOutcomeFailedButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.filterOutcomeFailedButton.Name = "filterOutcomeFailedButton";
            this.filterOutcomeFailedButton.Size = new System.Drawing.Size(23, 22);
            this.filterOutcomeFailedButton.Tag = "Failed";
            this.filterOutcomeFailedButton.Text = "Failed";
            this.filterOutcomeFailedButton.ToolTipText = "Show all failed tests";
            // 
            // filterOutcomeWarningButton
            // 
            this.filterOutcomeWarningButton.CheckOnClick = true;
            this.filterOutcomeWarningButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.filterOutcomeWarningButton.Name = "filterOutcomeWarningButton";
            this.filterOutcomeWarningButton.Size = new System.Drawing.Size(23, 22);
            this.filterOutcomeWarningButton.Tag = "Warning";
            this.filterOutcomeWarningButton.Text = "Warning";
            this.filterOutcomeWarningButton.ToolTipText = "Show all inconclusive, skipped or ignored tests";
            // 
            // filterOutcomeNotRunButton
            // 
            this.filterOutcomeNotRunButton.CheckOnClick = true;
            this.filterOutcomeNotRunButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.filterOutcomeNotRunButton.Name = "filterOutcomeNotRunButton";
            this.filterOutcomeNotRunButton.Size = new System.Drawing.Size(23, 22);
            this.filterOutcomeNotRunButton.Tag = "Not Run";
            this.filterOutcomeNotRunButton.Text = "Not Run";
            this.filterOutcomeNotRunButton.ToolTipText = "Show all tests not run yet";
            // 
            // filterSeparator1
            // 
            this.filterSeparator1.Name = "filterSeparator1";
            this.filterSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // filterByCategory
            // 
            this.filterByCategory.Name = "filterByCategory";
            this.filterByCategory.Size = new System.Drawing.Size(111, 19);
            this.filterByCategory.Text = "Filter by category";
            this.filterByCategory.ToolTipText = "Filter tests by category";
            // 
            // filterResetButton
            // 
            this.filterResetButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.filterResetButton.Image = ((System.Drawing.Image)(resources.GetObject("filterResetButton.Image")));
            this.filterResetButton.Name = "filterResetButton";
            this.filterResetButton.Size = new System.Drawing.Size(23, 20);
            this.filterResetButton.ToolTipText = "Reset all filters";
            // 
            // filterTextToolStrip
            // 
            this.filterTextToolStrip.Enabled = false;
            this.filterTextToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.filterTextBox});
            this.filterTextToolStrip.Location = new System.Drawing.Point(0, 0);
            this.filterTextToolStrip.Name = "filterTextToolStrip";
            this.filterTextToolStrip.Size = new System.Drawing.Size(191, 25);
            this.filterTextToolStrip.Stretch = true;
            this.filterTextToolStrip.TabIndex = 1;
            this.filterTextToolStrip.Visible = false;
            // 
            // filterTextBox
            // 
            this.filterTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(179, 25);
            // 
            // TestTreeView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.filterTextToolStrip);
            this.Controls.Add(this.filterToolStrip);
            this.Name = "TestTreeView";
            this.Size = new System.Drawing.Size(191, 246);
            this.testTreeContextMenu.ResumeLayout(false);
            this.filterToolStrip.ResumeLayout(false);
            this.filterToolStrip.PerformLayout();
            this.filterTextToolStrip.ResumeLayout(false);
            this.filterTextToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView;
        private System.Windows.Forms.ContextMenuStrip testTreeContextMenu;
        private System.Windows.Forms.ToolStrip filterToolStrip;
        private System.Windows.Forms.ToolStripLabel filterOutcomeLabel;
        private System.Windows.Forms.ToolStripButton filterOutcomePassedButton;
        private System.Windows.Forms.ToolStripButton filterOutcomeFailedButton;
        private System.Windows.Forms.ToolStripButton filterOutcomeWarningButton;
        private System.Windows.Forms.ToolStripButton filterOutcomeNotRunButton;
        private System.Windows.Forms.ToolStripSeparator filterSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton filterByCategory;
        private System.Windows.Forms.ToolStripButton filterResetButton;
        private System.Windows.Forms.ToolStrip filterTextToolStrip;
        private System.Windows.Forms.ToolStripMenuItem runMenuItem;
        private System.Windows.Forms.ToolStripMenuItem expandAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortByMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortByNameMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortByDurationMenuItem;
        private System.Windows.Forms.ToolStripSeparator sortMenuSeparator;
        private System.Windows.Forms.ToolStripMenuItem sortAscendingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem sortDescendingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseAllMenuItem;
        private System.Windows.Forms.ToolStripMenuItem collapseToFixturesMenuItem;
        private System.Windows.Forms.ImageList treeImages;
        private System.Windows.Forms.ToolStripMenuItem showCheckboxesMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showTestDurationMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearResultsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem activeConfigMenuItem;
        private System.Windows.Forms.ToolStripSeparator contextMenuSeparator2;
        private System.Windows.Forms.ToolStripMenuItem testPropertiesMenuItem;
        private System.Windows.Forms.ToolStripSeparator contextMenuSeparator1;
        private System.Windows.Forms.ToolStripSeparator contextMenuSeparator3;
        private System.Windows.Forms.ToolStripSeparator contextMenuSeparator4;
        private System.Windows.Forms.ToolStripMenuItem viewAsXmlMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeTestPackageMenuItem;
        private StretchToolStripTextBox filterTextBox;
    }
}
