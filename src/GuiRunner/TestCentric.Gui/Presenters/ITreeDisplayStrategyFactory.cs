// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters
{
    /// <summary>
    /// This factory interface supports the creation of TreeDisplayStrategy classes
    /// </summary>
    public interface ITreeDisplayStrategyFactory
    {
        /// <summary>
        /// Creates a concrete class implementing interface ITreeDisplayStrategy
        /// Supported displayStrategy:
        /// - "NUNIT_TREE" (also default case) 
        /// - "TEST_LIST"
        /// </summary>
        ITreeDisplayStrategy Create(string displayStrategy, ITestTreeView treeView, ITestModel testModel);
    }
}
