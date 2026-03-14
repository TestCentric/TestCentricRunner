// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;

namespace TestCentric.Gui.Presenters
{
    using Model;

    /// <summary>
    /// DurationGrouping groups tests by duration. The contents
    /// of the groups change during execution and the display
    /// icon changes according to the test results.
    /// </summary>
    public class DurationGrouping : TestGrouping
    {
        public DurationGrouping(GroupDisplayStrategy displayStrategy) : base(displayStrategy)
        {
        }

        #region Overrides

        public override string ID => "DURATION";

        public override void LoadGroups(IEnumerable<TestNode> tests)
        {
            Groups.Clear();

            // Predefine all TestGroups and TreeNodes
            Groups.Add(new TestGroup("Slow > 1 sec"));
            Groups.Add(new TestGroup("Medium > 100 ms"));
            Groups.Add(new TestGroup("Fast < 100 ms"));
            Groups.Add(new TestGroup("Not Run"));

            base.LoadGroups(tests);
        }

        /// <summary>
        /// Post a test result to the tree, changing the treeNode
        /// color to reflect success or failure. Overridden here
        /// to allow for moving nodes from one group to another
        /// based on the result of running the test.
        /// </summary>
        public override void OnTestFinished(ResultNode result)
        {
            _displayStrategy.ApplyResultToGroup(result);
        }

        public override TestGroup[] SelectGroups(TestNode testNode)
        {
            // Only a single group is possible for this grouping.
            return new TestGroup[] { SelectGroup(testNode) };
        }

        #endregion

        #region Helper Methods

        private TestGroup SelectGroup(TestNode testNode)
        {
            var group = Groups[3]; // NotRun

            var result = testNode as ResultNode;
            if (result == null)
                result = _displayStrategy.GetResultForTest(testNode);

            if (result != null)
            {
                group = result.Duration > 1.0
                    ? Groups[0]
                    : result.Duration > 0.1
                        ? Groups[1]
                        : Groups[2];
            }

            return group;
        }

        #endregion
    }
}
