// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Xml.Linq;
using TestCentric.Gui.Model;
using TestCentric.Gui.Model.Filter;

namespace TestCentric.Gui.Presenters
{
    /// <summary>
    /// A TestGroup is essentially a TestSelection with a
    /// name and image index for use in the tree display.
    /// Its TreeNode property is externally set and updated.
    /// It can create a filter for running all the tests
    /// in the group.
    /// </summary>
    public class TestGroup : ITestItem
    {
        private ResultState _groupResultState;
        private bool _isResultFromLatestRun;

        #region Constructors

        public TestGroup(string name) : this(name, -1) { }

        public TestGroup(string name, int imageIndex)
        {
            Name = name;
            ImageIndex = imageIndex;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public int ImageIndex { get; set; }

        public virtual double? Duration { get; set; }

        public TestSelection Items { get; } = new TestSelection();

        public TreeNode TreeNode { get; set; }

        #endregion

        public virtual IEnumerator<TestNode> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public void Clear() => Items.Clear();

        public TestFilter GetTestFilter(ITestCentricTestFilter guiFilter)
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
        public void Add(TestNode testNode, ResultNode resultNode = null)
        {
            Items.Add(testNode);
            if (resultNode != null)
            {
                if (_groupResultState == null || TestResultManager.GetOutcome(_groupResultState) < TestResultManager.GetOutcome(resultNode.Outcome))
                    _groupResultState = resultNode.Outcome;

                _isResultFromLatestRun = _isResultFromLatestRun || resultNode.IsLatestRun;

                ImageIndex = DisplayStrategy.CalcImageIndex(_groupResultState, _isResultFromLatestRun);
            }
        }

        public void RemoveId(string id) => Items.RemoveId(id);
    }
}
