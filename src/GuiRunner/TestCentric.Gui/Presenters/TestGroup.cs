// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Text;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System;
    using Model;
    using TestCentric.Gui.Model.Filter;

    /// <summary>
    /// A TestGroup is essentially a TestSelection with a
    /// name and image index for use in the tree display.
    /// Its TreeNode property is externally set and updated.
    /// It can create a filter for running all the tests
    /// in the group.
    /// </summary>
    public class TestGroup : TestSelection, ITestItem
    {
        #region Constructors

        public TestGroup(string name) : this(name, -1) { }

        public TestGroup(string name, int imageIndex)
        {
            Name = name;
            ImageIndex = imageIndex;
        }

        #endregion

        #region Properties

        public override string Name { get; }

        public int ImageIndex { get; set; }

        public double? Duration { get; set; }

        public TreeNode TreeNode { get; set; }

        #endregion

        public override TestFilter GetTestFilter(ITestCentricTestFilter guiFilter)
        {
            TestFilterBuilder builder = new TestFilterBuilder(guiFilter);

            foreach (TestNode test in this)
                if (test.RunState != RunState.Explicit)
                    builder.AddSelectedTest(test);

            return builder.Build();
        }

        /// <summary>
        /// Add a testNode to the TestGroup and apply the testNode result to the result state of the group
        /// </summary>
        public void Add(TestNode testNode, ResultNode resultNode)
        {
            Add(testNode);
            if (resultNode != null)
            {
                int imageIndex = DisplayStrategy.CalcImageIndex(resultNode);
                ImageIndex = Math.Max(imageIndex, ImageIndex);
            }
        }
    }
}
