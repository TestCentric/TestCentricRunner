// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;

namespace TestCentric.Gui.Model.Filter
{
    public class TestCentricTestFilter : ITestCentricTestFilter
    {
        private List<ITestFilter> _filters = new List<ITestFilter>();

        public TestCentricTestFilter(ITestModel model, Action filterChangedEvent)
        {
            TestModel = model;
            FireFilterChangedEvent = filterChangedEvent;

            _filters.Add(new OutcomeFilter(model));
            _filters.Add(new TextFilter());
            _filters.Add(new CategoryFilter(model));
        }

        private ITestModel TestModel { get; }

        private Action FireFilterChangedEvent;

        public IEnumerable<string> OutcomeFilter
        {
            get => GetFilterCondition("OutcomeFilter");
            set => SetFilterCondition("OutcomeFilter", value);
        }

        public IEnumerable<string> CategoryFilter
        {
            get => GetFilterCondition("CategoryFilter");

            set => SetFilterCondition("CategoryFilter", value);
        }

        public string TextFilter
        {
            get => GetFilterCondition("TextFilter").First();
            set => SetFilterCondition("TextFilter", new string[] { value });
        }

        public IEnumerable<string> AllCategories
        {
            get
            {
                var categoryFilter = _filters.FirstOrDefault(f => f.FilterId == "CategoryFilter") as CategoryFilter;
                return categoryFilter?.AllCategories ?? Enumerable.Empty<string>();
            }
        }

        public bool IsActive => IsNUnitTree && _filters.Any(x => x.IsActive);

        /// <summary>
        /// Filtering is not supported yet for category list or test list tree view 
        /// </summary>
        private bool IsNUnitTree => TestModel.Settings == null || TestModel.Settings.Gui.TestTree.DisplayFormat == "NUNIT_TREE";

        public void ResetAll(bool suppressFilterChangedEvent = false)
        {
            foreach (ITestFilter filter in _filters)
            {
                filter.Reset();
            }

            if (!suppressFilterChangedEvent)
            {
                FilterNodes(TestModel.LoadedTests);
                FireFilterChangedEvent();
            }
        }

        public void Init()
        {
            foreach (ITestFilter filter in _filters)
            {
                filter.Init();
            }
        }

        private bool FilterNodes(TestNode testNode)
        {
            // 1. Check if any child is visible => parent must be visible too
            bool childIsVisible = false;
            foreach (TestNode child in testNode.Children)
                if (FilterNodes(child))
                    childIsVisible = true;

            // 2. Check if node itself is visible
            bool isVisible = _filters.All(f => f.IsMatching(testNode));
            testNode.IsVisible = isVisible || childIsVisible;
            return testNode.IsVisible;
        }

        private IEnumerable<string> GetFilterCondition(string filterId)
        {
            var testFilter = _filters.FirstOrDefault(f => f.FilterId == filterId);
            return testFilter.Condition ?? Enumerable.Empty<string>();
        }

        private void SetFilterCondition(string filterId, IEnumerable<string> filter)
        {
            // 1. Get concrete filter by ID
            var testFilter = _filters.FirstOrDefault(f => f.FilterId == filterId);
            if (testFilter == null)
                return;
                
            // 2. Set condition, apply new filter to all nodes and fire event
            testFilter.Condition = filter;
            FilterNodes(TestModel.LoadedTests);
            FireFilterChangedEvent();
        }
    }
}
