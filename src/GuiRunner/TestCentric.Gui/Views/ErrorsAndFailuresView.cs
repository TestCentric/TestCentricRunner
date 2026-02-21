// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using NUnit;
using NUnit.UiException.Controls;

namespace TestCentric.Gui.Views
{
    using Controls;

    /// <summary>
    /// Summary description for ErrorDisplay.
    /// </summary>
    public partial class ErrorsAndFailuresView : UserControlView, IErrorsAndFailuresView
    {
        static Logger log = InternalTrace.GetLogger("ErrorsAndFailureView");

        private const int ITEM_MARGIN = 2;

        int hoverIndex = -1;
        private System.Windows.Forms.Timer hoverTimer;
        TipWindow tipWindow;

        #region Construction and Disposal

        public ErrorsAndFailuresView()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            sourceCode = new SourceCodeDisplay();
            errorBrowser.RegisterDisplay(sourceCode);
            errorBrowser.RegisterDisplay(stackTraceDisplay);

            sourceCode.SplitOrientationChanged += (s, e) =>
            {
                SourceCodeSplitOrientationChanged?.Invoke(this, new EventArgs());
            };

            sourceCode.SplitterDistanceChanged += (s, e) =>
            {
                SourceCodeSplitterDistanceChanged?.Invoke(this, new EventArgs());
            };

            errorBrowser.StackTraceDisplayChanged += (s, e) =>
            {
                SourceCodeDisplayChanged?.Invoke(this, new EventArgs());
            };

            flowLayoutPanel.Layout += FlowLayoutPanel_Layout;
        }

        #endregion

        #region IErrorsAndFailuresView Members

        public event EventHandler SplitterPositionChanged;
        public event EventHandler SourceCodeSplitterDistanceChanged;
        public event EventHandler SourceCodeSplitOrientationChanged;
        public event EventHandler SourceCodeDisplayChanged;

        public ITestResultSubView TestResultSubView => testResultSubView;

        public ITestOutputSubView TestOutputSubView => testOutputSubView;

        public string Header
        {
            get { return header.Text; }
            set { InvokeIfRequired(() => header.Text = value); }
        }

        public bool EnableToolTips { get; set; }

        public void SetFixedFont(Font font)
        {
            if (detailList.Font == font)
                return;

            InvokeIfRequired(() =>
            {
                detailList.Font = font;
                stackTraceDisplay.Font = font;
                sourceCode.CodeDisplayFont = font;
                RefillDetailList();
            });
        }

        public int SplitterPosition
        {
            get { return tabSplitter.SplitPosition; }
            set
            {
                if (value >= tabSplitter.MinSize && value < ClientSize.Height)
                    InvokeIfRequired(() =>
                    {
                        tabSplitter.SplitPosition = value;
                    });
            }
        }

        public float SourceCodeSplitterDistance
        {
            get { return sourceCode.SplitterDistance; }
            set
            {
                InvokeIfRequired(() =>
                {
                    sourceCode.SplitterDistance = value;
                });
            }
        }

        public Orientation SourceCodeSplitOrientation
        {
            get { return sourceCode.SplitOrientation; }
            set
            {
                InvokeIfRequired(() =>
                {
                    sourceCode.SplitOrientation = value;
                });
            }

        }

        public bool SourceCodeDisplay
        {
            get { return errorBrowser.SelectedDisplay == sourceCode; }
            set
            {
                InvokeIfRequired(() =>
                {
                    if (value)
                        errorBrowser.SelectedDisplay = sourceCode;
                    else
                        errorBrowser.SelectedDisplay = stackTraceDisplay;
                });
            }
        }

        public void Clear()
        {
            InvokeIfRequired(() =>
            {
                detailList.Items.Clear();
                detailList.ContextMenuStrip = null;
                errorBrowser.StackTraceSource = "";

                panel1.Visible = false;
            });
        }

        public void AddResult(string status, string testName, string message, string stackTrace)
        {
            InvokeIfRequired(() =>
            {
                InsertTestResultItem(new TestResultItem(status, testName, message, stackTrace));
                panel1.Visible = true;
            });
        }

        #endregion

        #region DetailList Events
        /// <summary>
        /// When one of the detail failure items is selected, display
        /// the stack trace and set up the tool tip for that item.
        /// </summary>
        private void detailList_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            TestResultItem resultItem = (TestResultItem)detailList.SelectedItem;
            errorBrowser.StackTraceSource = resultItem.FilteredStackTrace;
            detailList.ContextMenuStrip = detailListContextMenuStrip;
        }

        private void detailList_MeasureItem(object sender, System.Windows.Forms.MeasureItemEventArgs e)
        {
            TestResultItem item = (TestResultItem)detailList.Items[e.Index];
            SizeF size = e.Graphics.MeasureString(item.ToString(), detailList.Font);
            e.ItemHeight = (int)size.Height + 2 * ITEM_MARGIN;
            e.ItemWidth = (int)size.Width;
            if (e.ItemWidth > detailList.HorizontalExtent)
                detailList.HorizontalExtent = e.ItemWidth;
        }

        private void detailList_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
        {
            if (e.Index >= 0)
            {
                e.DrawBackground();
                TestResultItem item = (TestResultItem)detailList.Items[e.Index];
                Brush brush = (e.State & DrawItemState.Selected) == DrawItemState.Selected
                    ? SystemBrushes.HighlightText
                    : SystemBrushes.WindowText;
                e.Graphics.DrawString(item.ToString(), detailList.Font, brush, e.Bounds.Left, e.Bounds.Top + ITEM_MARGIN);
            }
        }

        private void RefillDetailList()
        {
            if (this.detailList.Items.Count > 0)
            {
                this.detailList.BeginUpdate();
                ArrayList copiedItems = new ArrayList(detailList.Items);
                this.detailList.Items.Clear();
                foreach (object item in copiedItems)
                    this.detailList.Items.Add(item);
                this.detailList.EndUpdate();
                this.detailList.Invalidate();
            }
        }

        private void copyDetailMenuItem_Click(object sender, System.EventArgs e)
        {
            if (detailList.SelectedItem != null)
                Clipboard.SetDataObject(detailList.SelectedItem.ToString());
        }

        private void OnMouseHover(object sender, System.EventArgs e)
        {
            if (tipWindow != null) tipWindow.Close();

            if (hoverIndex >= 0 && hoverIndex < detailList.Items.Count)
            {
                Graphics g = Graphics.FromHwnd(detailList.Handle);

                Rectangle itemRect = detailList.GetItemRectangle(hoverIndex);
                string text = detailList.Items[hoverIndex].ToString();

                SizeF sizeNeeded = g.MeasureString(text, detailList.Font);
                bool expansionNeeded =
                    itemRect.Width < (int)sizeNeeded.Width ||
                    itemRect.Height < (int)sizeNeeded.Height;

                if (expansionNeeded)
                {
                    tipWindow = new TipWindow(detailList, hoverIndex);
                    tipWindow.ItemBounds = itemRect;
                    tipWindow.TipText = text;
                    tipWindow.Expansion = TipWindow.ExpansionStyle.Both;
                    tipWindow.Overlay = true;
                    tipWindow.WantClicks = true;
                    tipWindow.MouseLeaveDelay = 500;
                    tipWindow.Closed += new EventHandler(tipWindow_Closed);
                    tipWindow.Show();
                }
            }
        }

        private void tipWindow_Closed(object sender, System.EventArgs e)
        {
            tipWindow = null;
            hoverIndex = -1;
            ClearTimer();
        }

        private void detailList_MouseLeave(object sender, System.EventArgs e)
        {
            hoverIndex = -1;
            ClearTimer();
        }

        private void detailList_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            ClearTimer();

            hoverIndex = detailList.IndexFromPoint(e.X, e.Y);

            if (hoverIndex >= 0 && hoverIndex < detailList.Items.Count)
            {
                // Workaround problem of IndexFromPoint returning an
                // index when mouse is over bottom part of list.
                Rectangle r = detailList.GetItemRectangle(hoverIndex);
                if (e.Y > r.Bottom)
                    hoverIndex = -1;
                else
                {
                    hoverTimer = new System.Windows.Forms.Timer();
                    hoverTimer.Interval = 800;
                    hoverTimer.Tick += new EventHandler(OnMouseHover);
                    hoverTimer.Start();
                }
            }
        }

        private void ClearTimer()
        {
            if (hoverTimer != null)
            {
                hoverTimer.Stop();
                hoverTimer.Dispose();
            }
        }

        private void tabSplitter_SplitterMoved(object sender, SplitterEventArgs e)
        {
            SplitterPositionChanged?.Invoke(this, new EventArgs());
        }

        private void OnTestResultSubViewSizeChanged(object sender, EventArgs e)
        {
            // Content and Height of the test result view changed => Place panel1 beneath the result view
            panel1.Top = testResultSubView.Bottom + testResultSubView.Margin.Bottom + panel1.Margin.Top;
        }

        private void FlowLayoutPanel_Layout(object sender, LayoutEventArgs e)
        {
            // Adjust width of all controls 
            var subViewWidth = flowLayoutPanel.ClientRectangle.Width - testResultSubView.Margin.Left - testResultSubView.Margin.Right;
            detailList.Width = subViewWidth;
            testOutputSubView.Width = subViewWidth;

            // Adjust height of detaillist view
            var subViewHeight = flowLayoutPanel.ClientRectangle.Height - 6;
            int outputViewHeight = testOutputSubView.Visible ? testOutputSubView.Height + testOutputSubView.Margin.Vertical : 0;
            detailList.Height = Math.Max(100, subViewHeight - outputViewHeight - detailList.Margin.Top);
        }

        #endregion

        #region Helper Methods

        private void InsertTestResultItem(TestResultItem item)
        {
            detailList.BeginUpdate();
            detailList.Items.Insert(detailList.Items.Count, item);
            detailList.SelectedIndex = 0;
            detailList.EndUpdate();
        }

        #endregion
    }
}
