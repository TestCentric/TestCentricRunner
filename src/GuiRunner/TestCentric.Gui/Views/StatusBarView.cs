// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using NUnit;

namespace TestCentric.Gui.Views
{
    public partial class StatusBarView : UserControlView, IStatusBarView
    {
        private static Logger log = InternalTrace.GetLogger(nameof(StatusBarView));

        public StatusBarView()
        {
            InitializeComponent();
        }

        public void Initialize()
        {
            InvokeIfRequired(() =>
            {
                foreach (ToolStripStatusLabel panel in statusStrip1.Items)
                    if (panel != StatusLabel)
                        panel.Visible = false;
            });
        }

        public override string Text
        {
            set { InvokeIfRequired(() => StatusLabel.Text = value); }
        }

        public int Passed
        {
            set { UpdateCounter(passedPanel, value); }
        }

        public int Failed
        {
            set { UpdateCounter(failedPanel, value); }
        }

        public int Warnings
        {
            set { UpdateCounter(warningsPanel, value); }
        }

        public int Inconclusive
        {
            set { UpdateCounter(inconclusivePanel, value); }
        }

        public int Ignored
        {
            set { UpdateCounter(ignoredPanel, value); }
        }

        public int Skipped
        {
            set { UpdateCounter(skippedPanel, value); }
        }

        public double Duration
        {
            set 
            {
                InvokeIfRequired(() =>
                {
                    timePanel.Text = $"{value.ToString("F3")} sec";
                    timePanel.Visible = true;
                });
            }
        }

        private void UpdateCounter(ToolStripStatusLabel statusLabel, int count)
        {
            InvokeIfRequired(() =>
            {
                statusLabel.Text = count.ToString();
                statusLabel.Visible = true;
            });
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            Height = Math.Max((int)Font.GetHeight() + 10, MinimumSize.Height);
            statusStrip1.Font = Font;
        }

        public void LoadImages(OutcomeImageSet imageSet)
        {
            log.Debug($"Loading images from ImageSet {imageSet.Name}");

            // Update all images in the pictureBoxes
            try
            {
                passedPanel.Image = imageSet.LoadImage("Success");
                failedPanel.Image = imageSet.LoadImage("Failure");
                warningsPanel.Image = imageSet.LoadImage("Warning");
                inconclusivePanel.Image = imageSet.LoadImage("Inconclusive");
                ignoredPanel.Image = imageSet.LoadImage("Ignored");
                skippedPanel.Image = imageSet.LoadImage("Skipped");
            }
            catch(FileNotFoundException ex)
            {
                // We log the error but don't rethrow. This allows the CI to pass.
                // Missing images should be discovered by inspecting the GUI.
                log.Error("Image not found", ex);
            }
        }
    }
}
