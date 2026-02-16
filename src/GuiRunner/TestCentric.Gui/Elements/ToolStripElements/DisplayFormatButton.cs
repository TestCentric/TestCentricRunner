// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;

namespace TestCentric.Gui.Elements.ToolStripElements
{
    /// <summary>
    /// DisplayFormatButton extends DropDownButtonElement to provide implementation
    /// details for the toolbar button used by TestCentric to select the display format
    /// strategy and optional details for each strategy.
    /// </summary>
    public class DisplayFormatButton : DropDownButtonElement
    {
        public DisplayFormatButton(ToolStripDropDownButton button) : base(button)
        {
            foreach (ToolStripMenuItem item in button.DropDown.Items)
            {
                item.DropDown.Opening += (s, e) => e.Cancel = !item.Checked;
                item.CheckedChanged += (s, e) =>
                {
                    if (item.Checked)
                        item.DropDown.Show();
                };
            }
        }
    }
}
