// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCentric.Gui.Model.Filter
{
    /// <summary>
    /// This class is responsible to build up a TestFilter object using the different inputs:
    /// - the selected tree node(s)
    /// - the GUI tree filter (outcome, category or name)
    /// - the tree grouping
    /// </summary>
    public class TestFilterBuilder
    {
        public TestFilterBuilder(ITestCentricTestFilter guiFilter)
        {
            OutcomeFilter = new List<string>();
            CategoryFilter = new List<string>();
            TestFilter = string.Empty;

            SelectedTests = new TestSelection();

            ApplyGuiFilter(guiFilter);
        }

        private IList<string> OutcomeFilter { get; }

        private IList<string> CategoryFilter { get; }

        private string TestFilter { get; set; }

        private TestSelection SelectedTests { get; }

        public Func<IEnumerable<TestNode>> AllTestCaseProvider { get; set; }

        private bool EmptyCategorySelected { get; set; }


        public TestFilter Build()
        {
            // There's no matching NUnit filter for outcome and tests without category.
            // Therefore we cannot build up an enhanced filter =>
            // Create ID filter for all visible child nodes of the SelectedTests
            if (OutcomeFilter.Any() || EmptyCategorySelected)
            {
                IList<TestNode> childNodes = GetAllTestcaseNodes();
                return Model.TestFilter.MakeIdFilter(childNodes);
            }

            // Create category filter
            TestFilter categoryFilter = Model.TestFilter.Empty;
            if (CategoryFilter.Any())
                categoryFilter = Model.TestFilter.MakeCategoryFilter(CategoryFilter);

            // Create test filter
            TestFilter testFilter = Model.TestFilter.Empty;
            if (!string.IsNullOrEmpty(TestFilter))
                testFilter = Model.TestFilter.MakeTestFilter(TestFilter);

            // Create ID filter for selected tests
            TestFilter idFilter = Model.TestFilter.MakeIdFilter(SelectedTests);
            if (idFilter.IsEmpty && (CategoryFilter.Any() || !string.IsNullOrEmpty(TestFilter)))
            {
                // Exceptional case: category/test filter will match also all explicit child tests => add explicit tests as NOT filter
                TestFilter explicitFilter = Model.TestFilter.MakeIdFilter(SelectedTests.GetExplicitChildNodes());
                idFilter = Model.TestFilter.MakeNotFilter(explicitFilter);
            }

            // Combine all filters into a single one
            return Model.TestFilter.MakeAndFilter(idFilter, categoryFilter, testFilter);
        }

        private void ApplyGuiFilter(ITestCentricTestFilter guiFilter)
        {
            if (!guiFilter.IsActive)
                return;

            if (guiFilter.AllCategories.Except(guiFilter.CategoryFilter).Any())
                foreach (string category in guiFilter.CategoryFilter)
                    AddCategory(category);

            foreach (string outcome in guiFilter.OutcomeFilter)
                AddOutcome(outcome);

            if (!string.IsNullOrEmpty(guiFilter.TextFilter))
                AddTestFilter(guiFilter.TextFilter);
        }


        public void AddSelectedTest(TestNode test)
        {
            SelectedTests.Add(test);
        }


        public void AddCategory(string category)
        {
            if (category == "None" || category == Filter.CategoryFilter.NoCategory)
                EmptyCategorySelected = true;
            else
                CategoryFilter.Add(category);
        }

        private void AddOutcome(string outcome)
        {
            OutcomeFilter.Add(outcome);
        }

        private void AddTestFilter(string testFilter)
        {
            TestFilter = testFilter;
        }

        /// <summary>
        /// This list of test cases is only required in case an outcome filter is active in the UI
        /// Get list of all test case IDs
        /// </summary>
        private IList<TestNode> GetAllTestcaseNodes()
        {
            IList<TestNode> result = new List<TestNode>();

            // Use registered callback (if) to get all test cases
            if (AllTestCaseProvider != null)
                return AllTestCaseProvider().ToList();

            var selection = SelectedTests.ToList();
            foreach (TestNode testNode in selection)
                GetAllTestcaseNodes(testNode, result);

            return result;
        }

        private void GetAllTestcaseNodes(TestNode testNode, IList<TestNode> result)
        {
            // Only consider Visible nodes (e.g. nodes not filtered out by the UI filter)
            if (!testNode.IsVisible)
                return;

            // Ignore explicit tests except those which were selected by the user actively 
            if (!SelectedTests.Contains(testNode) && testNode.RunState == RunState.Explicit)
                return;

            if (!testNode.IsProject && !testNode.IsSuite && testNode.Children.Count == 0)
                result.Add(testNode);

            foreach (TestNode testNodeChild in testNode.Children)
                GetAllTestcaseNodes(testNodeChild, result);
        }
    }
}
