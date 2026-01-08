// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Collections.Generic;
    using Model;
    using TestCentric.Gui.Presenters.NUnitGrouping;
    using Views;

    /// <summary>
    /// NUnitTreeDisplayStrategy is used to display all the tests
    /// in the traditional NUnit tree format.
    /// </summary>
    public class NUnitTreeDisplayStrategy : DisplayStrategy
    {
        private ITreeViewModel _treeViewModel;

        public NUnitTreeDisplayStrategy(ITestTreeView view, ITestModel model)
            : base(view, model) 
        {
            _view.SetTestFilterVisibility(model.Settings.Gui.TestTree.ShowFilter);
            _view.CollapseToFixturesCommand.Enabled = true;
        }

        /// <summary>
        /// Intended for testing purpose only
        /// </summary>
        public NUnitTreeDisplayStrategy(ITestTreeView view, ITestModel model, ITreeViewModel treeViewModel)
            : this(view, model)
        {
            _treeViewModel = treeViewModel;
        }

        public override string StrategyID => "NUNIT_TREE";

        public override string Description => "NUnit Tree";

        private TreeViewBuilder TreeViewBuilder { get; set; }

        public override void OnTestLoaded(TestNode testNode, VisualState visualState)
        {
            ClearTree();

            // 1. Create tree view model
            switch (_model.Settings.Gui.TestTree.NUnitGroupBy)
            {
                case "CATEGORY": _treeViewModel = new TreeViewModelCategoryGrouping(_model); break;
                case "OUTCOME": _treeViewModel = new TreeViewModelOutcomeGrouping(_model); break;
                case "DURATION": _treeViewModel = new TreeViewModelDurationGrouping(_model); break;
                default: _treeViewModel = new TreeViewModelNoGrouping(_model); break;
            }

            _treeViewModel.CreateTreeModel(testNode);

            // 2. Create tree view control
            TreeViewBuilder = new TreeViewBuilder(_model, _treeViewModel, _view);
            _treeViewModel.OnUpdateTree += TreeViewBuilder.OnUpdateTree;
            _treeViewModel.OnNodeChanged += TreeViewBuilder.OnNodeChanged;

            TreeViewBuilder.Rebuild();

            // 3. Update tree state
            _view.TreeView?.BeginUpdate();
            if (visualState != null)
                visualState.ApplyTo(_view.TreeView);
            else
                SetDefaultInitialExpansion();

            ApplyResultsToTree();
            _view.TreeView?.EndUpdate();

            _view.EnableTestFilter(true);
        }

        public override void UpdateTreeNodeNames()
        {
            TreeViewBuilder.OnShowTestDurationChanged();
        }


        public override void OnTestFinished(ResultNode result)
        {
            IList<TreeNodeViewModel> viewModels = _treeViewModel?.OnTestFinished(result);
            TreeViewBuilder?.OnTestFinished(viewModels);
        }

        /// <summary>
        /// Method is intended to be called only from test code, so that the test code doesn't need to deal with the regroup Timer
        /// </summary>
        public void OnTestFinishedWithoutRegroupTimer(ResultNode result)
        {
            _treeViewModel?.OnTestFinishedWithoutRegroupTimer(result);
        }

        public override void OnTestRunStarting()
        {
            _treeViewModel?.OnTestRunStarting();
            TreeViewBuilder?.OnTestRunStarting(() => _treeViewModel.GetAllViewModelsInTestRun());
        }

        public override void OnTestRunFinished()
        {
            var nodesInRun = _treeViewModel?.GetAllViewModelsInTestRun();
            _treeViewModel?.OnTestRunFinished();

            // The images of the root group tree nodes (for example 'CategoryA' or 'Slow') cannot be set during a test run
            TreeViewBuilder?.OnTestRunFinished(nodesInRun);
        }

        protected override VisualState CreateVisualState() => new VisualState("NUNIT_TREE", _settings.Gui.TestTree.NUnitGroupBy, _settings.Gui.TestTree.ShowNamespace).LoadFrom(_view.TreeView);

        private void SetDefaultInitialExpansion()
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
    }
}
