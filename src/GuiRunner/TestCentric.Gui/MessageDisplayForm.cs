// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Drawing;
using System.Security.Policy;
using System.Windows.Forms;

namespace TestCentric.Gui
{
    /// <summary>
    /// Form used by MessageDisplay to display TestCentric Messages.
    /// </summary>
    public partial class MessageDisplayForm : Form, IMessageDisplay
    {
        // This class is inspired by and partially based on FlexibleMessageBox
        // by Jörg Reichert(public@jreichert.de), which may be found at
        // http://www.codeproject.com/Articles/601900/FlexibleMessageBox.
        // Our implementation differs in not attempting to emulate Windows
        // MessageBox but instead using our IMessageDisplay interface to
        // display TestCentric messages.

        private const int SCREEN_EDGE = 20;
        private const int SCREEN_MARGIN = 2 * SCREEN_EDGE;

        // These three constants must match the Form layout
        private const int FORM_MARGIN = 7;
        private const int ICON_WIDTH = 48;
        private const int ICON_TEXT_SPACING = 10;
        private const int MIN_TEXT_HEIGHT = 55;

        private string _defaultCaption;

        public MessageDisplayForm(string defaultCaption)
        {
            InitializeComponent();

            _defaultCaption = defaultCaption;
        }

        public void Error(string text, string caption = null) =>
            Show(text, caption ?? _defaultCaption, MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);

        public void Info(string text, string caption = null) =>
            Show(text, caption ?? _defaultCaption, MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);

        public bool YesNo(string text, string caption = null) =>
            Show(text, caption ?? _defaultCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes;

        public bool OkCancel(string text, string caption = null) =>
            Show(text, caption ?? _defaultCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.OK;

        public DialogResult YesNoCancel(string text, string caption = null) =>
            Show(text, caption ?? _defaultCaption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

        public DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defaultButton)
        {
            _messageText = text;
            Text = caption ?? _defaultCaption;

            SelectIconImage(icon);
            SetupButtons(buttons);
            AdjustSizes(icon, text);

            return ShowDialog();
        }

        private void SelectIconImage(MessageBoxIcon icon)
        {
            iconPictureBox.Image = icon switch
            {
                MessageBoxIcon.Information => SystemIcons.Information.ToBitmap(),
                MessageBoxIcon.Warning => SystemIcons.Warning.ToBitmap(),
                MessageBoxIcon.Error => SystemIcons.Error.ToBitmap(),
                MessageBoxIcon.Question => SystemIcons.Question.ToBitmap(),
                _ => null // Includes MessageBoxIcon.None
            };

            iconPictureBox.Visible = (iconPictureBox.Image != null);
        }

        private void SetupButtons(MessageBoxButtons buttons)
        {
            button1.Visible = false;
            button2.Visible = false;
            button3.Visible = false;
            CancelButton = null;

            switch (buttons)
            {
                case MessageBoxButtons.AbortRetryIgnore:
                    SetUpButton(button1, "Abort", DialogResult.Abort);
                    SetUpButton(button2, "Retry", DialogResult.Retry);
                    SetUpButton(button3, "Ignore", DialogResult.Ignore);
                    ControlBox = false;
                    break;
                case MessageBoxButtons.OKCancel:
                    SetUpButton(button2, "OK", DialogResult.OK);
                    SetUpButton(button3, "Cancel", DialogResult.Cancel);
                    CancelButton = button3;
                    break;
                case MessageBoxButtons.RetryCancel:
                    SetUpButton(button2, "Retry", DialogResult.Retry);
                    SetUpButton(button3, "Cancel", DialogResult.Cancel);
                    CancelButton = button3;
                    break;
                case MessageBoxButtons.YesNo:
                    SetUpButton(button2, "Yes", DialogResult.Yes);
                    SetUpButton(button3, "No", DialogResult.No);
                    ControlBox = false;
                    break;
                case MessageBoxButtons.YesNoCancel:
                    SetUpButton(button1, "Yes", DialogResult.Yes);
                    SetUpButton(button2, "No", DialogResult.No);
                    SetUpButton(button3, "Cancel", DialogResult.Cancel);
                    CancelButton = button3;
                    break;
                case MessageBoxButtons.OK:
                default:
                    SetUpButton(button3, "OK", DialogResult.OK);
                    CancelButton = button3;
                    break;
            }
        }

        private void SetUpButton(Button button, string text, DialogResult result)
        {
            button.Visible = true;
            button.Text = text;
            button.DialogResult = result;
        }

        private void AdjustSizes(MessageBoxIcon icon, string text)
        {
            _displayRect = ClientRectangle;
            _displayRect.Height -= FORM_MARGIN * 2 + buttonPanel.Height;

            if (icon == MessageBoxIcon.None)
            {
                _displayRect.Location = new Point(FORM_MARGIN, FORM_MARGIN);
                _displayRect.Width -= FORM_MARGIN * 2;
            }
            else
            {
                _displayRect.Location = new Point(iconPictureBox.Right + ICON_TEXT_SPACING, FORM_MARGIN);
                _displayRect.Width -= FORM_MARGIN * 2 + ICON_WIDTH + ICON_TEXT_SPACING;
            }

            Graphics g = Graphics.FromHwnd(Handle);
            Screen screen = Screen.FromHandle(Handle);
            //SizeF layoutArea = new SizeF(screen.WorkingArea.Width - SCREEN_MARGIN, screen.WorkingArea.Height - SCREEN_MARGIN);
            SizeF layoutArea = new SizeF(_displayRect.Width, screen.WorkingArea.Height - SCREEN_MARGIN);
            Size sizeNeeded = Size.Ceiling(
                g.MeasureString(text, Font, layoutArea));

            _displayRect.Size = sizeNeeded;
            if (_displayRect.Height < MIN_TEXT_HEIGHT)
                _displayRect.Height = MIN_TEXT_HEIGHT;

            buttonPanel.Top = _displayRect.Bottom + FORM_MARGIN;

            this.ClientSize = new Size(buttonPanel.Right, buttonPanel.Bottom);
        }

        private Rectangle _displayRect;
        private string _messageText;
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = Graphics.FromHwnd(Handle);
            g.DrawString(_messageText, Font, Brushes.Black, _displayRect);
        }
    }
}
