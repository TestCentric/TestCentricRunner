// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestCentric.Gui.Elements
{
    /// <summary>
    /// PopupMenuElement represents a menu item with subitems,
    /// which may need to be populated when they are about to
    /// to be displayed.
    /// </summary>
    public class PopupMenuElement : ToolStripMenuElement, IPopup
    {
        public event CommandHandler Popup;

        public PopupMenuElement(ToolStripMenuItem menuItem)
            : base(menuItem)
        {
            menuItem.DropDownOpening += (s, e) => Popup?.Invoke();
        }
    }
}
