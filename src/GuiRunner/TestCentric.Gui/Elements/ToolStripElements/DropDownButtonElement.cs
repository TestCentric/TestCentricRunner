// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Elements
{
    /// <summary>
    /// DropDownButtonElement extends ToolStripElement for use with a DropDownButton.
    /// </summary>
    public class DropDownButtonElement : ToolStripElement, IPopup
    {
        public DropDownButtonElement(ToolStripDropDownButton button) : base(button)
        {
            button.Click += (s, e) => Popup?.Invoke();
        }

        public ToolStripItemCollection MenuItems { get; }

        public event CommandHandler Popup;
    }
}
