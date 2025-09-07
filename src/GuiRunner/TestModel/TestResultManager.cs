// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model
{
    using System.Collections.Generic;
    using System.Linq;

    public interface ITestResultManager
    {
        /// <summary>
        /// Get result by a test ID
        /// </summary>
        ResultNode GetResultForTest(string id);

        /// <summary>
        /// Add one test result
        /// In case of a test suite we need to check if some child node with a result from a previous run exists (not all test are run; but only a subset)
        /// In this case we need to merge the current test result and the result from the previous run.
        /// </summary>
        /// <param name="resultNode"></param>
        /// <returns></returns>
        ResultNode AddResult(ResultNode resultNode);

        /// <summary>
        /// Clear all results
        /// </summary>
        void ClearResults();

        /// <summary>
        /// Get all failed tests
        /// </summary>
        IEnumerable<TestNode> GetFailedTests();

        /// <summary>
        /// When a test run is starting all results from previous run are marked with IsLatestRun = false
        /// </summary>
        void TestRunStarting();

        /// <summary>
        /// If a test assembly is reloaded all test results from previous test runs
        /// should be kept as long as the test still exists in the test assembly
        /// The test full name is used for this task
        /// </summary>
        void ReloadTestResults();
    }

    /// <summary>
    /// This class is responsible to manage the test results reported by nunit
    /// Basically it's a dictionary using the test ID as a key
    /// </summary>
    public class TestResultManager : ITestResultManager
    {
        public TestResultManager(ITestModel model)
        {
            Model = model;
        }

        private IDictionary<string, ResultNode> Results { get; } = new Dictionary<string, ResultNode>();

        private ITestModel Model { get; }

        /// <inheritdoc />
        public ResultNode GetResultForTest(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            return Results.TryGetValue(id, out ResultNode result) ? result : null;
        }


        /// <inheritdoc />
        public ResultNode AddResult(ResultNode resultNode)
        {
            if (Results.TryGetValue(resultNode.Id, out ResultNode oldResult))
            {
                if (resultNode.Outcome.Equals(ResultState.Explicit))
                    return oldResult;

                if (resultNode.IsSuite && UpdateResultFromPreviousTestRun(resultNode, out ResultState newTestResult))
                    resultNode = ResultNode.Create(resultNode.Xml, newTestResult);
            }

            resultNode.IsLatestRun = true;
            Results[resultNode.Id] = resultNode;

            return resultNode;
        }

        /// <inheritdoc />
        public void ClearResults()
        {
            Results.Clear();
        }

        /// <inheritdoc />
        public void TestRunStarting()
        {
            foreach (ResultNode resultNode in Results.Values)
                resultNode.IsLatestRun = false;
        }

        /// <inheritdoc />
        public void ReloadTestResults()
        {
            // Get all existing test results
            List<ResultNode> oldResults = Results.Values.ToList();
            ClearResults();

            // Search for TestFullName of all old results in new TestNodes
            foreach (ResultNode oldResult in oldResults)
            {
                TestNode testNode = TryGetTestNode(Model.LoadedTests, oldResult.FullName);
                if (testNode != null)
                {
                    // Test for result still exists: Create new result by keeping result content, but use current test ID
                    ResultNode newResult = ResultNode.Create(oldResult.Xml, testNode.Id);
                    Results.Add(testNode.Id, newResult);
                }
            }
        }

        private TestNode TryGetTestNode(TestNode testNode, string fullName)
        {
            if (testNode.FullName == fullName)
                return testNode;

            foreach (TestNode childNode in testNode.Children)
            {
                TestNode foundNode = TryGetTestNode(childNode, fullName);
                if (foundNode != null)
                    return foundNode;
            }

            return null;
        }


        public IEnumerable<TestNode> GetFailedTests()
        {
            var failedTests = new List<TestNode>();

            foreach (ResultNode result in Results.Values)
            {
                if (!result.IsSuite && result.Outcome.Status == TestStatus.Failed)
                {
                    TestNode testNode = Model.GetTestById(result.Id);
                    failedTests.Add(testNode);
                }
            }

            return failedTests;
        }

        /// <summary>
        /// If a TestNode was not executed entirely in the current test run
        /// Some child TestNodes might contain TestResults from a previous test run
        /// </summary>
        private IList<ResultNode> GetPreviousTestRunResults(ResultNode resultNode)
        {
            var results = new List<ResultNode>();

            TestNode node = Model.GetTestById(resultNode.Id);
            if (node == null)
                return results;

            foreach (TestNode child in node.Children)
            {
                ResultNode childResult = GetResultForTest(child.Id);
                if (childResult != null && !childResult.IsLatestRun)
                    results.Add(childResult);
            }

            return results;
        }

        private bool UpdateResultFromPreviousTestRun(ResultNode testResult, out ResultState newResultState)
        {
            newResultState = null;
            IList<ResultNode> previousChildResults = GetPreviousTestRunResults(testResult);
            if (!previousChildResults.Any())
                return false;

            ResultState previousTestRunResult = GetPreviousTestRunResultOutcome(previousChildResults);
            bool updateTestResult = GetOutcome(previousTestRunResult) > GetOutcome(testResult.Outcome);
            newResultState = updateTestResult ? previousTestRunResult : testResult.Outcome;
            return updateTestResult;
        }

        private ResultState GetPreviousTestRunResultOutcome(IList<ResultNode> previousChildResults)
        {
            ResultState resultState = ResultState.Inconclusive;
            foreach (ResultNode oldChildResult in previousChildResults)
            {
                if (GetOutcome(oldChildResult.Outcome) > GetOutcome(resultState))
                    resultState = oldChildResult.Outcome;
            }

            return resultState;
        }

        private int GetOutcome(ResultState resultState)
        {
            switch (resultState.Status)
            {
                case TestStatus.Inconclusive:
                    return 1;
                case TestStatus.Passed:
                    return 2;
                case TestStatus.Warning:
                    return 4;
                case TestStatus.Failed:
                    return 5;
                case TestStatus.Skipped:
                default:
                    return resultState.Label == "Ignored" ? 3 : 0;
            }
        }
    }
}
