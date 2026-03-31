// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;
using System.Windows.Forms;

namespace TestCentric.Gui.Views
{
    using System;
    using System.ComponentModel;
    using Elements;

    public partial class TestPropertiesView : UserControlView, ITestPropertiesView
    {
        public event CommandHandler DisplayHiddenPropertiesChanged;

        public TestPropertiesView()
        {
            InitializeComponent();

            // Re-raise the event from the panel here
            testPropertiesSubView.DisplayHiddenPropertiesChanged += () =>
            {
                DisplayHiddenPropertiesChanged?.Invoke();
            };

            flowLayoutPanel1.Layout += FlowLayoutPanel1_Layout;
        }

        private void FlowLayoutPanel1_Layout(object sender, LayoutEventArgs e)
        {
            var subViewWidth = flowLayoutPanel1.ClientRectangle.Width - 4;
            header.Width = subViewWidth;
            testPackageSubView.Width = subViewWidth;
            testPropertiesSubView.Width = subViewWidth;
            testGroupPropertiesSubView.Width = subViewWidth;
        }

        public int ClientHeight => ClientRectangle.Height - TestPackageSubView.Top - 40; // Value of 40 allows for non-client areas and spacing

        public string Header
        {
            get { return header.Text; }
            set { InvokeIfRequired(() => { header.Text = value; }); }
        }

        public TestPackageSubView TestPackageSubView => testPackageSubView;

        public TestPropertiesSubView TestPropertiesSubView => testPropertiesSubView;

        public TestGroupPropertiesSubView TestGroupPropertiesSubView => testGroupPropertiesSubView;


        public SubView[] SubViews => new SubView[] { TestPackageSubView, TestPropertiesSubView, TestGroupPropertiesSubView }; 

        public string TestType
        {
            get { return testPropertiesSubView.TestType; }
            set { testPropertiesSubView.TestType = value; }
        }

        public string FullName
        {
            get { return testPropertiesSubView.FullName; }
            set { testPropertiesSubView.FullName = value; }
        }

        public string Description
        {
            get { return testPropertiesSubView.Description; }
            set { testPropertiesSubView.Description = value; }
        }

        public string Categories
        {
            get { return testPropertiesSubView.Categories; }
            set { testPropertiesSubView.Categories = value; }
        }

        public string TestCount
        {
            get { return testPropertiesSubView.TestCount; }
            set { testPropertiesSubView.TestCount = value; }
        }

        public string RunState
        {
            get { return testPropertiesSubView.RunState; }
            set { testPropertiesSubView.RunState = value; }
        }

        public string SkipReason
        {
            get { return testPropertiesSubView.SkipReason; }
            set { testPropertiesSubView.SkipReason = value; }
        }

        public bool DisplayHiddenProperties
        {
            get { return testPropertiesSubView.DisplayHiddenProperties; }
        }

        public string Properties
        {
            get { return testPropertiesSubView.Properties; }
            set { testPropertiesSubView.Properties = value; }
        }

        public string PackageSettings
        {
            get { return testPackageSubView.PackageSettings; }
            set { testPackageSubView.PackageSettings = value; }
        }

        #region Nested Subview class

        /// <summary>
        /// Each sub-view class used by TestPropertyView must inherit from
        /// this class. Ideally, the class would be abstract but the Windows
        /// Forms designer does no handle abstract classes. Therefore, we
        /// use virtual messages which throw instead.
        /// </summary>
        public class SubView : UserControlView
        {
            protected const int SUBVIEW_SPACING = 4;
            protected const int EXPANDABLE_FIELD_MINIMUM_HEIGHT = 40;

            public virtual int FullHeight => throw new System.NotImplementedException();

            protected int HeightNeededForControl(Control ctrl)
            {
                Graphics g = this.CreateGraphics();
                var sizeNeeded = g.MeasureString(ctrl.Text, ctrl.Font, ctrl.Width);
                int heightNeeded = (int)System.Math.Ceiling(sizeNeeded.Height);
                return System.Math.Max(EXPANDABLE_FIELD_MINIMUM_HEIGHT, heightNeeded);
            }
        }

        #endregion
    }
}
