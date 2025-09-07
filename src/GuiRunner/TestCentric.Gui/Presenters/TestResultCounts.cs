// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters
{
    /// <summary>
    /// This struct represents the numbers of test cases which passed, failed, ignored... for one TestResult node
    /// </summary>
    public struct TestResultCounts
    {
        public int FailedCount { get; set; }

        public int PassedCount { get; set; }

        public int WarningCount { get; set; }

        public int NotRunCount { get; set; }

        public int InconclusiveCount { get; set; }

        public int IgnoreCount { get; set; }

        public int ExplicitCount { get; set; }

        public int TestCount => FailedCount + PassedCount + WarningCount + NotRunCount + InconclusiveCount + IgnoreCount + ExplicitCount;

        public int AssertCount { get; set; }

        public double Duration { get; set; }

        private void Add(TestResultCounts summary)
        {
            FailedCount += summary.FailedCount;
            PassedCount += summary.PassedCount;
            WarningCount += summary.WarningCount;
            NotRunCount += summary.NotRunCount;
            InconclusiveCount += summary.InconclusiveCount;
            IgnoreCount += summary.IgnoreCount;
            ExplicitCount += summary.ExplicitCount;
            AssertCount += summary.AssertCount;
            Duration += summary.Duration;
        }

        public static TestResultCounts GetResultCounts(ITestModel model, ITestItem testItem)
        {
            if (testItem is TestNode testNode)
                return GetResultCounts(model, testNode);

            if (testItem is TestGroup testGroup)
                return GetResultCounts(model, testGroup);

            return new TestResultCounts();
        }

        private static TestResultCounts GetResultCounts(ITestModel model, TestGroup testGroup)
        {
            TestResultCounts testCounts = new TestResultCounts();
            foreach (TestNode testNode in testGroup)
            {
                TestResultCounts c = GetResultCounts(model, testNode);
                testCounts.Add(c);
            }

            return testCounts;
        }

        private static TestResultCounts GetResultCounts(ITestModel model, TestNode testNode)
        {
            TestResultCounts testCounts = new TestResultCounts();
            ResultNode result = model.TestResultManager.GetResultForTest(testNode.Id);

            // Only consider outcome from test cases; and check if testNode is filtered out
            if (!testNode.IsProject && !testNode.IsSuite && !testNode.IsAssembly && testNode.IsVisible)
            {
                if (result != null)
                {
                    if (result.Outcome.Status == TestStatus.Passed)
                        testCounts.PassedCount++;
                    else if (result.Outcome.Status == TestStatus.Failed)
                        testCounts.FailedCount++;
                    else if (result.Outcome.Status == TestStatus.Inconclusive)
                        testCounts.InconclusiveCount++;
                    else if (result.Outcome.Status == TestStatus.Warning)
                        testCounts.WarningCount++;
                    else if (result.Outcome.Status == TestStatus.Skipped && result.Label == "Ignored")
                        testCounts.IgnoreCount++;
                    else if (result.Outcome.Status == TestStatus.Skipped && result.Label == "Explicit")
                        testCounts.ExplicitCount++;

                    testCounts.AssertCount += result.AssertCount;
                    testCounts.Duration += result.Duration;
                }
                else
                    testCounts.NotRunCount = 1;
            }

            // Iterate through all child nodes
            foreach (TestNode childNode in testNode.Children)
            {
                testCounts.Add(GetResultCounts(model, childNode));
            }

            return testCounts;
        }

    }
}
