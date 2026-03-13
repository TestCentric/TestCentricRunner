// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Linq;
    using Model;
    using Views;

    /// <summary>
    /// TestGrouping is the base class of all groupings. It represents a
    /// set of TestGroups containing tests. In the base class, the contents
    /// of each group is stable once created.
    /// </summary>
    public abstract class TestGrouping
    {
        protected GroupDisplayStrategy _displayStrategy;

        #region Constructor

        /// <summary>
        /// Construct a TestGrouping for a given GroupDisplayStrategy
        /// </summary>
        /// <param name="displayStrategy"></param>
        public TestGrouping(GroupDisplayStrategy displayStrategy)
        {
            _displayStrategy = displayStrategy;
            Groups = new List<TestGroup>();
        }

        #endregion

        #region Public Members

        public abstract string ID { get; }

        public List<TestGroup> Groups { get; private set; }

        /// <summary>
        /// Load tests into the grouping
        /// </summary>
        /// <param name="tests">The TestNodes to be loaded</param>
        public virtual void LoadGroups(IEnumerable<TestNode> tests)
        {
            foreach (TestNode testNode in tests)
                foreach (var group in SelectGroups(testNode))
                    group.Add(testNode);

            // If there are test results, show the result for the group
            if (_displayStrategy.HasResults)
                foreach (var group in Groups)
                    group.ImageIndex = _displayStrategy.CalcImageIndexForGroup(group);
        }

        /// <summary>
        /// Post a test result to the tree, changing the treeNode
        /// color to reflect success or failure.
        /// </summary>
        public virtual void OnTestFinished(ResultNode result)
        {
            // Override to take any necessary action
        }

        public void OnTestRunFinished()
        {
            // Update tree node icon for all group nodes at the end of a test run
            // The icon state can be finally determined for duration and outcome grouping only at this point in time
            // For category grouping, the status can already be partially determined in OnTestFinished event; But finally only at the end of a test run
            foreach (TestGroup group in Groups)
            {
                int imageIndex = _displayStrategy.CalcImageIndexForGroup(group);
                group.TreeNode.ImageIndex = group.TreeNode.SelectedImageIndex = group.ImageIndex = imageIndex;
            }
        }

        /// <summary>
        /// Returns an array of groups in which the TestNode is included.
        /// </summary>
        public abstract TestGroup[] SelectGroups(TestNode testNode);

        #endregion
    }
}
