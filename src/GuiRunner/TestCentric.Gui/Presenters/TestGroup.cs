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

        public TestGroup(string name, int imageIndex = -1)
        {
            Name = name;
            ImageIndex = imageIndex;
        }

        public TestGroup(string name, IEnumerable<TestNode> tests, int imageIndex = -1)
        {
            Name = name;
            TestNodes = new TestSelection(tests);
            ImageIndex = imageIndex;
        }

        public TestGroup(string name, IEnumerable<TestGroup> subGroups, int imageIndex = -1)
        {
            Name = name;
            SubGroups = [.. subGroups];
            ImageIndex = imageIndex;
        }

        #endregion

        #region Properties

        public string Name { get; }

        public int ImageIndex { get; set; }

        public virtual double? Duration { get; set; }

        /// <summary>
        /// The selection of test cases included in this group.
        /// </summary>
        /// <remarks>
        /// A `TestGroup` always has a `TestSelection` associated with it, which
        /// includes all the test cases contained in the group. This is true
        /// even if there are sub-groups. Any sub-groups will have a selection
        /// that includes a subset of the parent group's test cases. This approach
        /// allows us to execute the tests for a group directly, by providing the
        /// group's `TestSelection` as an argument.
        /// </remarks>
        public TestSelection TestNodes { get; } = new TestSelection();

        public TestGroup ParentGroup { get; set; }

        public IList<TestGroup> SubGroups { get; } = new List<TestGroup>();

        public TreeNode TreeNode { get; set; }

        #endregion

        public virtual IEnumerator<TestNode> GetEnumerator()
        {
            return TestNodes.GetEnumerator();
        }

        public void Clear() => TestNodes.Clear();

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
            if (resultNode == null)
                resultNode = testNode as ResultNode;

            TestNodes.Add(testNode);
            if (resultNode != null)
            {
                if (_groupResultState == null || TestResultManager.GetOutcome(_groupResultState) < TestResultManager.GetOutcome(resultNode.Outcome))
                    _groupResultState = resultNode.Outcome;

                _isResultFromLatestRun = _isResultFromLatestRun || resultNode.IsLatestRun;

                ImageIndex = DisplayStrategy.CalcImageIndex(_groupResultState, _isResultFromLatestRun);
            }
        }

        public TestGroup GetOrAddSubGroup(string name)
        {
            TestGroup subGroup = null;

            foreach (TestGroup group in SubGroups)
                if (group.Name == name)
                {
                    subGroup = group;
                    break;
                }

            if (subGroup == null)
            {
                subGroup = new TestGroup(name) { ParentGroup = this };
                subGroup.TreeNode = new TreeNode(name) { Tag = subGroup, Name = name };
                SubGroups.Add(subGroup);
                TreeNode.Nodes.Add(subGroup.TreeNode);
            }

            return subGroup;
        }

        public void RemoveId(string id) => TestNodes.RemoveId(id);
    }
}
