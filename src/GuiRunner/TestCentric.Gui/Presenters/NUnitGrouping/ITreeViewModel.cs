// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System;

    /// <summary>
    /// EventArgs for the ITreeViewModel.OnUpdateTree event
    /// </summary>
    public class UpdateTreeEventsArgs(IList<TreeNodeViewModel> viewModels) : EventArgs
    {
        public IList<TreeNodeViewModel> ViewModels { get; } = viewModels;
    }

    /// <summary>
    /// EventArgs for the ITreeViewModel.OnNodeChanged event
    /// </summary>
    public class NodeChangedEventsArgs(TreeNodeViewModel viewModel) : EventArgs
    {
        public TreeNodeViewModel ViewModel { get; } = viewModel;
    }

    public interface ITreeViewModel
    {
        event EventHandler<UpdateTreeEventsArgs> OnUpdateTree;
        event EventHandler<NodeChangedEventsArgs> OnNodeChanged;

        /// <summary>
        /// List of nodes representing the root nodes of the tree
        /// </summary>
        IList<TreeNodeViewModel> RootViewModels { get; }

        /// <summary>
        /// Creates the complete tree model
        /// </summary>
        void CreateTreeModel(TestNode node);

        /// <summary>
        /// Removes one node from the tree model
        /// </summary>
        void RemoveNode(TestNode testNode);

        /// <summary>
        /// Retrieves the list of groups in which a testNode is grouped
        /// </summary>
        IList<string> GetGroupNames(TestNode testNode);

        /// <summary>
        /// Insert one test node into the tree model under the specified group
        /// All not existing tree nodes are created as needed
        /// </summary>
        /// <returns>
        /// The path from the root tree node to the testNode as a list of TreeNodeViewModels
        /// </returns>
        IList<TreeNodeViewModel> CreateTreeNodeViewModels(TestNode testNode, string groupName);

        /// <summary>
        /// Updates the tree model for a list of finished tests
        /// Used when tests need to be regrouped into a different group
        /// </summary>
        void UpdateTreeModel(IList<TestNode> finishedTests);

        /// <summary>
        /// Get the viewmodel for one specific TestNode
        /// </summary>
        TreeNodeViewModel GetTreeNodeViewModel(TestNode testNode);

        /// <summary>
        /// Returns all view models in the current test run
        /// </summary>
        IList<TreeNodeViewModel> GetAllViewModelsInTestRun();

        /// <summary>
        /// Called when one test case is finished
        /// </summary>
        IList<TreeNodeViewModel> OnTestFinished(ResultNode result);

        /// <summary>
        /// This method is intended to be called only from test code,
        /// so that the test code doesn't need to deal with the regroup Timer
        /// </summary>
        void OnTestFinishedWithoutRegroupTimer(ResultNode result);

        /// <summary>
        /// Called when the entire test run is finished
        /// </summary>
        void OnTestRunFinished();

        /// <summary>
        /// Called when the entire test run is starting
        /// </summary>
        void OnTestRunStarting();
    }
}
