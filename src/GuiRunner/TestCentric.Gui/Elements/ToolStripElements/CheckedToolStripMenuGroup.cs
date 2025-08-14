﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Windows.Forms;

namespace TestCentric.Gui.Elements
{
    /// <summary>
    /// CheckedMenuGroup wraps a set of checked menu items as an ISelection.
    /// </summary>
    public class CheckedToolStripMenuGroup : ISelection
    {
        // Fields are public for testing only
        public IList<ToolStripMenuItem> MenuItems = new List<ToolStripMenuItem>();
        public ToolStripMenuItem TopMenu;

        public event CommandHandler SelectionChanged;

        public CheckedToolStripMenuGroup(ToolStripMenuItem topMenu)
        {
            TopMenu = topMenu;
            foreach (ToolStripMenuItem menuItem in topMenu.DropDown.Items)
                MenuItems.Add(menuItem);

            InitializeMenuItems();
        }

        public CheckedToolStripMenuGroup(string name, params ToolStripMenuItem[] menuItems)
        {
             foreach (var menuItem in menuItems)
                MenuItems.Add(menuItem);

            InitializeMenuItems();
        }

        public void Refresh()
        {
            if (TopMenu != null)
            {
                MenuItems.Clear();
                foreach (ToolStripMenuItem menuItem in TopMenu.DropDown.Items)
                    MenuItems.Add(menuItem);
            }

            InitializeMenuItems();
        }

        private void InitializeMenuItems()
        {
            SelectedIndex = -1;

            for (int index = 0; index < MenuItems.Count; index++)
            {
                ToolStripMenuItem menuItem = MenuItems[index];

                // We need to handle this ourselves
                menuItem.CheckOnClick = false;

                if (menuItem.Checked)
                    // Select first menu item checked in designer
                    // and uncheck any others.
                    if (SelectedIndex == -1)
                        SelectedIndex = index;
                    else
                        menuItem.Checked = false;

                // Handle click by user
                menuItem.Click += menuItem_Click;
            }

            // If no items were checked, select first one
            if (SelectedIndex == -1 && MenuItems.Count > 0)
            {
                MenuItems[0].Checked = true;
                SelectedIndex = 0;
            }
        }

        // Note that all items must be on the same toolstrip
        private ToolStrip _toolStrip;
        public ToolStrip ToolStrip
        {
            get
            {
                if (_toolStrip == null && MenuItems.Count > 0)
                    _toolStrip = MenuItems[0].GetCurrentParent();

                return _toolStrip;
            }
        }

        public string Text { get; set; }

        public int SelectedIndex
        {
            get
            {
                for (int i = 0; i < MenuItems.Count; i++)
                    if (MenuItems[i].Checked)
                        return i;

                return -1;
            }
            set
            {
                InvokeIfRequired(() =>
                {
                    for (int i = 0; i < MenuItems.Count; i++)
                        MenuItems[i].Checked = value == i;
                });
            }
        }

        public string SelectedItem
        {
            get { return (string)MenuItems[SelectedIndex].Tag; }
            set
            {
                for (int i = 0; i < MenuItems.Count; i++)
                    if ((string)MenuItems[i].Tag == value)
                    {
                        SelectedIndex = i;
                        break;
                    }
            }
        }

        private bool _enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;

                foreach (ToolStripMenuItem menuItem in MenuItems)
                    menuItem.Enabled = value;
            }
        }

        private bool _visible;
        public bool Visible
        {
            get { return _visible; }
            set
            {
                _visible = value;

                foreach (ToolStripMenuItem menuItem in MenuItems)
                    menuItem.Visible = value;
            }
        }

        void menuItem_Click(object sender, System.EventArgs e)
        {
            ToolStripMenuItem clicked = (ToolStripMenuItem)sender;

            // If user clicks selected item, ignore it
            if (!clicked.Checked)
            {
                for (int index = 0; index < MenuItems.Count; index++)
                {
                    ToolStripMenuItem item = MenuItems[index];

                    if (item == clicked)
                    {
                        item.Checked = true;
                        SelectedIndex = index;
                    }
                    else
                    {
                        item.Checked = false;
                    }
                }

                if (SelectionChanged != null)
                    SelectionChanged();
            }
        }

        public void EnableItem(string tag, bool enabled)
        {
            foreach (ToolStripMenuItem item in MenuItems)
                if ((string)item.Tag == tag)
                    item.Enabled = enabled;
        }

        public void InvokeIfRequired(MethodInvoker _delegate)
        {
            if (ToolStrip.InvokeRequired)
                ToolStrip.BeginInvoke(_delegate);
            else
                _delegate();
        }
    }
}
