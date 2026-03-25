// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Views;

    /// <summary>
    /// OutcomeGrouping groups tests by outcome. The contents
    /// of the groups change during execution but the display
    /// icon remains the same.
    /// </summary>
    public class OutcomeGrouping : TestGrouping
    {
        #region Constructor

        public OutcomeGrouping(GroupDisplayStrategy display) : base(display)
        {
        }

        #endregion

        #region Overrides

        public override string ID => "OUTCOME";

        public override void LoadGroups(IEnumerable<TestNode> tests)
        {
            Groups.Clear();

            // Predefine all TestGroups and TreeNodes
            Groups.Add(new TestGroup("Failed", TestTreeView.FailureIndex));
            Groups.Add(new TestGroup("Warning", TestTreeView.WarningIndex));
            Groups.Add(new TestGroup("Passed", TestTreeView.SuccessIndex));
            Groups.Add(new TestGroup("Ignored", TestTreeView.IgnoredIndex));
            Groups.Add(new TestGroup("Inconclusive", TestTreeView.InconclusiveIndex));
            Groups.Add(new TestGroup("Skipped", TestTreeView.SkippedIndex));
            Groups.Add(new TestGroup("Not Run", TestTreeView.InitIndex));

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
            // Only a single group is possible for this node
            return new TestGroup[] { SelectGroup(testNode) };
        }

        #endregion

        #region Helper Methods

        private TestGroup SelectGroup(TestNode testNode)
        {
            var result = testNode as ResultNode;
            if (result == null)
                result = _displayStrategy.GetResultForTest(testNode);

            // TODO: Eliminate reliance on constant indices
            if (result != null)
                switch (result.Outcome.Status)
                {
                    case TestStatus.Failed:
                        return Groups[0];
                    case TestStatus.Warning:
                        return Groups[1];
                    case TestStatus.Passed:
                        return Groups[2];
                    case TestStatus.Skipped:
                        return result.Outcome.Label == "Ignored" ? Groups[3] : Groups[5];
                    case TestStatus.Inconclusive:
                        return Groups[4];
                }

            return Groups[6]; // Not Run
        }

        #endregion
    }
}
