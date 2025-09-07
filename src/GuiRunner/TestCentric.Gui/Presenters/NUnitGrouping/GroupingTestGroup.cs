// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System.Collections.Generic;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Model.Filter;

    /// <summary>
    /// A specialist TestGroup class which provides the associated TestNode within the NUnit tree
    /// </summary>
    public class GroupingTestGroup : TestGroup
    {
        /// <inheritdoc />
        public GroupingTestGroup(TestNode associatedTestNode, string name) : base(name)
        {
            AssociatedTestNode = associatedTestNode;
        }

        /// <inheritdoc />
        public GroupingTestGroup(TestNode associatedTestNode, string name, int imageIndex) : base(name, imageIndex)
        {
            AssociatedTestNode = associatedTestNode;
        }

        public TestNode AssociatedTestNode { get; set; }

        public override TestFilter GetTestFilter(ITestCentricTestFilter guiFilter)
        {
            TestFilterBuilder builder = new TestFilterBuilder(guiFilter);

            foreach (TestNode test in GetNonExplicitTests())
                builder.AddSelectedTest(test);

            // Special case in which no filter can be composed and a fallback to individual IDs must be applied
            builder.AllTestCaseProvider = GetNonExplicitTests;
            return builder.Build();
        }

        protected IList<TestNode> GetNonExplicitTests()
        {
            IList<TestNode> result = new List<TestNode>();

            foreach (TestNode test in this)
                if (test.RunState != RunState.Explicit &&
                    (test.Parent.RunState != RunState.Explicit || test.Parent == AssociatedTestNode))
                    result.Add(test);

            return result;
        }
    }
}
