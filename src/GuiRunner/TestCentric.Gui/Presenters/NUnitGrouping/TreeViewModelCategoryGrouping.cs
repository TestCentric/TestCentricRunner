// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    public class TreeViewModelCategoryGrouping : TreeViewModelGroupingBase
    {
        public TreeViewModelCategoryGrouping(ITestModel model) : base(model)
        {
        }

        public override IList<string> GetGroupNames(TestNode testNode)
        {
            return GetAllCategories(testNode);
        }


        private List<string> GetAllCategories(TestNode testNode)
        {
            string xpathExpression = "ancestor-or-self::*/properties/property[@name='Category']";

            // Get list of available categories of the TestNode
            List<string> categories = new List<string>();
            foreach (XmlNode node in testNode.Xml.SelectNodes(xpathExpression))
            {
                var groupName = node.Attributes["value"].Value;
                if (!string.IsNullOrEmpty(groupName) && !categories.Contains(groupName))
                    categories.Add(groupName);
            }

            if (categories.Any() == false)
                categories.Add("None");

            return categories;
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

            return new CategoryGroupingTestGroup(viewModel, CurrentRootGroupName, viewModel.Name);
        }
    }
}
