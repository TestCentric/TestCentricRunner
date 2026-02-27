// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;

namespace TestCentric.Gui.Elements
{
    /// <summary>
    /// ToolStripItem is a generic wrapper for ToolStripItems
    /// </summary>
    public class ToolStripElement : IViewElement, IToolTip
    {
        private ToolStripItem _toolStripItem;

        public ToolStripElement(ToolStripItem toolStripItem)
        {
            _toolStripItem = toolStripItem;
        }

        public bool Enabled
        {
            get { return _toolStripItem.Enabled; }
            set { InvokeIfRequired(() => { _toolStripItem.Enabled = value; }); }
        }

        public bool Visible
        {
            get { return _toolStripItem.Visible; }
            set { InvokeIfRequired(() => { _toolStripItem.Visible = value; }); }
        }

        public string Text
        {
            get { return _toolStripItem.Text; }
            set { InvokeIfRequired(() => { _toolStripItem.Text = value; }); }
        }

        public string ToolTipText
        {
            get { return _toolStripItem.ToolTipText; }
            set { InvokeIfRequired(() => { _toolStripItem.ToolTipText = value; }); }
        }

        public void InvokeIfRequired(MethodInvoker del)
        {
            var toolStrip = _toolStripItem.GetCurrentParent();

            if (toolStrip != null)
                if (toolStrip.InvokeRequired)
                    toolStrip.BeginInvoke(del, new object[0]);
                else
                    del();
        }
    }
}
