// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace TestCentric.Gui.Dialogs
{
    public partial class PinnableDisplay : TestCentricFormBase
    {
        private Image _pinnedImage;
        private Image _unpinnedImage;

        public PinnableDisplay()
        {
            InitializeComponent();

            // Most of what follows should ideally be in a presenter, for
            // ease of testing. However, it's really just boilerplate
            // funcionality and it's simpler to keep it all in the dialog.
            // serving as a proof of concept;
            _pinnedImage = new Bitmap(GetType().Assembly.GetManifestResourceStream("TestCentric.Gui.Images.pinned.gif"));
            _unpinnedImage = new Bitmap(GetType().Assembly.GetManifestResourceStream("TestCentric.Gui.Images.unpinned.gif"));

            pinButton.Image = _unpinnedImage;
            pinButton.CheckedChanged += (s, e) =>
            {
                pinButton.Image = pinButton.Checked
                    ? _pinnedImage
                    : _unpinnedImage;
            };
        }

        [Browsable(false)]
        public string TestName
        {
            get { return testName.Text; }
            set { testName.Text = value; }
        }

        [Browsable(false)]
        public bool Pinned => pinButton.Checked;

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HT_CLIENT = 0x01;
            const int HT_CAPTION = 0x02;

            if (m.Msg == WM_NCHITTEST)
            {
                DefWndProc(ref m);
                if (m.Result == (IntPtr)HT_CLIENT)
                    m.Result = (IntPtr)HT_CAPTION;
                else
                    base.WndProc(ref m);
            }
            else
                base.WndProc(ref m);
        }

        private void PinnableDisplay_Load(object sender, EventArgs e)
        {
            // Derived displays may have different widths, so set the
            // location of the controls dynamically on Load.
            exitButton.Left = ClientRectangle.Width - exitButton.Width - 4;
            pinButton.Left = exitButton.Left - pinButton.Width - 4;
            testName.Width = pinButton.Left - testName.Left;
        }
    }

    /// <summary>
    /// This Label is displayed in the caption of a pinnable form <see cref="PinnableDisplay"/>
    /// This form should support the move functionality (click+hold mouse in caption and move around) like any regular form
    /// To support this functionality it mustn't process the HitTest window message itself, but pass it to beneath form
    /// </summary>
    internal class TransparentCaptionLabel : Label
    {
        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTTRANSPARENT = -1;

            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    m.Result = (IntPtr)HTTRANSPARENT;
                    return;
            }
            base.WndProc(ref m);
        }
    }
}
