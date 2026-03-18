// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using NUnit;
    using TestCentric.Gui.Elements;
    using TestCentric.Gui.Model;

    /// <summary>
    /// This class is responsible for the recent file menu entries. It populates the menu from the recent file list settings,
    /// handles the menu item events and provide a context menu to remove entries from the recent file list.
    ///
    /// The menu can either show project files or non-project files only. 
    /// </summary>
    public class RecentFileMenuController
    {
        public RecentFileMenuController(ITestModel model, IPopup menuItem, bool filterProjectFiles)
        {
            Guard.ArgumentNotNull(model, nameof(model));
            Guard.ArgumentNotNull(menuItem, nameof(menuItem));

            Model = model;
            MenuItems = menuItem.MenuItems;
            FilterProjectFiles = filterProjectFiles;
        }
        
        private ITestModel Model { get; }

        private ToolStripItemCollection MenuItems { get; }

        private bool FilterProjectFiles { get; }

        public void PopulateMenu()
        {
            // Remove all non-existing files from recent file list (includes null entries)
            var entries = Model.Settings.Gui.RecentFiles.Entries;
            for (int index = entries.Count; --index >= 0;)
                if (!File.Exists(entries[index]))
                    entries.RemoveAt(index);

            // Filter the recent file list for project files or non-project files
            var recentFiles = entries.Cast<string>().Where(Filter).ToList();

            // Create menu items
            MenuItems.Clear();
            foreach (string fileName in recentFiles)
            {
                string menuItemText = $"{MenuItems.Count + 1} {fileName}";
                ToolStripMenuItem menuItem = CreateRecentFileMenuItem(menuItemText, fileName);
                MenuItems.Add(menuItem);
            }
        }

        private bool Filter(string fileName)
        {
            return FilterProjectFiles && IsProjectFile(fileName) || 
                   !FilterProjectFiles && !IsProjectFile(fileName);
        }

        private bool IsProjectFile(string fileName)
        {
            return fileName.EndsWith("tcproj", StringComparison.InvariantCultureIgnoreCase);
        }

        private ToolStripMenuItem CreateRecentFileMenuItem(string menuText, string tag)
        {
            var menuItem = new ToolStripMenuItem(menuText);
            menuItem.Tag = tag;
            menuItem.Click += OnMenuItemClicked;
            menuItem.MouseUp += OnMenuItemMouseUp;
            return menuItem;
        }

        private void OnMenuItemMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var menuItem = new MenuItem("Remove item from list");
                menuItem.Tag = sender;
                menuItem.Click += RemoveRecentFileEntry;

                Control control = (sender as ToolStripMenuItem)?.Owner as Control;
                ContextMenu contextMenu = new ContextMenu(new[] { menuItem });
                contextMenu.Show(control, control.PointToClient(Cursor.Position));
            }
        }

        private void OnMenuItemClicked(object sender, EventArgs e)
        {
            string fileName = (string)((ToolStripMenuItem)sender).Tag;
            Model.OpenExistingFile(fileName);
        }

        private void RemoveRecentFileEntry(object sender, EventArgs e)
        {
            // Remove entry from settings
            MenuItem contextMenuItem = sender as MenuItem;
            ToolStripMenuItem selectedMenuItem = contextMenuItem.Tag as ToolStripMenuItem;
            string fileName = (string)selectedMenuItem.Tag;
            var entries = Model.Settings.Gui.RecentFiles.Entries;
            entries.Remove(fileName);

            // Remove entry from menu
            if (MenuItems.Contains(selectedMenuItem))
                MenuItems.Remove(selectedMenuItem);

            // Update text of all remaining menu items
            int index = 1;
            foreach (ToolStripMenuItem menuItem in MenuItems)
                menuItem.Text = $"{index++} {menuItem.Tag}";
        }
    }
}
