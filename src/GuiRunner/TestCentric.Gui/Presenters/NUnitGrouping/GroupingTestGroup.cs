// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System.Collections.Generic;
    using System.Linq;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Model.Filter;

    /// <summary>
    /// A specialist TestGroup class which provides the associated TestNode within the NUnit tree
    /// </summary>
    public class GroupingTestGroup : TestGroup
    {
        /// <inheritdoc />
        public GroupingTestGroup(TreeNodeViewModel viewModel, string name) : base(name)
        {
            NodeViewModel = viewModel;
        }

        /// <inheritdoc />
        public GroupingTestGroup(TreeNodeViewModel viewModel, string name, int imageIndex) : base(name, imageIndex)
        {
            NodeViewModel = viewModel;
        }

        internal TreeNodeViewModel NodeViewModel { get; }

        internal TestNode AssociatedTestNode => NodeViewModel.AssociatedTestNode;

        public override double? Duration => NodeViewModel.TestDuration;

        public override TestFilter GetTestFilter(ITestCentricTestFilter guiFilter)
        {
            TestFilterBuilder builder = new TestFilterBuilder(guiFilter);

            foreach (TestNode test in NodeViewModel.GetNonExplicitTests())
                builder.AddSelectedTest(test);

            // Special case in which no filter can be composed and a fallback to individual IDs must be applied
            builder.AllTestCaseProvider = NodeViewModel.GetNonExplicitTests;
            return builder.Build();
        }

        /// <inheritdoc />
        public override IEnumerator<TestNode> GetEnumerator()
        {
            return TreeNodeViewModel.GetTestCases(NodeViewModel).Select(s => s.AssociatedTestNode).GetEnumerator();
        }
    }
}
