// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    /// <summary>
    /// This class is responsible to set/reset the group's duration property before/after a test run
    /// The current approach sums up all individual durations of the test cases included in the group.
    /// Please note that this approach doesn't consider the TestFixture Setup time.
    /// </summary>
    public class TreeNodeDurationHandler
    {
        public static void ClearGroupDurations(IEnumerable<TestGroup> groups)
        {
            foreach (TestGroup group in groups)
                group.Duration = null;
        }

        public static void SetGroupDurations(ITestModel model, IList<TestGroup> groups)
        {
            foreach (TestGroup group in groups)
            {
                foreach (TestNode testNode in group)
                {
                    ResultNode result = model.TestResultManager.GetResultForTest(testNode.Id);
                    if (result == null)
                        continue;

                    if (!group.Duration.HasValue)
                        group.Duration = 0;

                    group.Duration += result.Duration;
                }
            }
        }
    }
}
