// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System.Collections.Generic;
    using System.Linq;
    using TestCentric.Gui.Model;

    public class TreeViewModelGroupingBase : TreeViewModelBase
    {
        /// <inheritdoc />
        public TreeViewModelGroupingBase(ITestModel model) : base(model)
        {
        }

        public override void CreateTreeModel(TestNode rootNode)
        {
            // 1. Get list of all TestNode representing test cases
            IList<TestNode> testNodes = rootNode.Select(n => !n.IsSuite && n.IsVisible).ToList();

            // 2. Create all tree nodes for one testNode one-by-one
            foreach (TestNode testNode in testNodes)
            {
                IList<string> groupNames = GetGroupNames(testNode);
                foreach (string groupName in groupNames)
                    CreateTreeNodeViewModels(testNode, groupName);
            }
        }

        protected override TreeNodeViewModel CreateTreeNodeViewModel(TestNode testNode, string name)
        {
            return new TreeNodeViewModel(Model, testNode, name) { CreateTestItemCallback = CreateTestItemCallback };
        }

        private ITestItem CreateTestItemCallback(TreeNodeViewModel viewModel)
        {
            // For leaf nodes (e.g. a single test case) return TestItem
            if (!viewModel.AssociatedTestNode.IsAssembly && !viewModel.AssociatedTestNode.IsSuite && !viewModel.AssociatedTestNode.IsProject)
                return viewModel.AssociatedTestNode;

            return new GroupingTestGroup(viewModel, viewModel.Name);
        }
    }
}
