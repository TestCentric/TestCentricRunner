// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Views;

    /// <summary>
    /// NUnitTreeDisplayStrategy is used to display all the tests
    /// in the traditional NUnit tree format.
    /// </summary>
    public class NUnitTreeDisplayStrategy : DisplayStrategy
    {
        #region Construction and Initialization

        public NUnitTreeDisplayStrategy(ITestTreeView view, ITestModel model)
            : base(view, model) 
        {
            _view.SetTestFilterVisibility(model.Settings.Gui.TestTree.ShowFilter);
            _view.CollapseToFixturesCommand.Enabled = true;
        }

        #endregion

        #region Properties

        public override string StrategyID => "NUNIT_TREE";

        public override string Description => "NUnit Tree";

        #endregion

        #region Methods

        public override VisualState CreateVisualState()
        {
            VisualState visualState = null;

            _view.InvokeIfRequired(() =>
            {
                visualState = new VisualState("NUNIT_TREE", null, TreeConfiguration).LoadFrom(_view.TreeView);
            });

            return visualState;
        }

        /// <summary>
        /// Check if a tree node type should be shown or omitted.
        /// </summary>
        protected override bool ShowTreeNodeType(TestNode testNode)
        {
            if (testNode.IsAssembly)
                return TreeConfiguration.NUnitTreeShowAssemblies;
            if (testNode.IsNamespace)
                return TreeConfiguration.NUnitTreeShowNamespaces;
            if (testNode.IsFixture)
                return TreeConfiguration.NUnitTreeShowFixtures;

            return true;
        }

        protected override void SetInitialExpansion()
        {
            TreeNode firstNode = null;
            foreach (TreeNode node in _view.Nodes)
            {
                if (_view.VisibleNodeCount >= node.GetNodeCount(true))
                    node.ExpandAll();
                else
                    CollapseToFixtures(node);

                if (firstNode == null)
                    firstNode = node;
            }

            firstNode?.EnsureVisible();
        }

        #endregion
    }
}
