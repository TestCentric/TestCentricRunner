// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters
{
    /// <summary>
    /// This factory class is responsible for the creation of TreeDisplayStrategy classes
    /// </summary>
    public class TreeDisplayStrategyFactory : ITreeDisplayStrategyFactory
    {
        public ITreeDisplayStrategy Create(string displayStrategy, ITestTreeView treeView, ITestModel testModel)
        {
            switch (displayStrategy)
            {
                case "TEST_LIST":
                    return new TestListDisplayStrategy(treeView, testModel);
                case "NUNIT_TREE":
                default:
                    return new NUnitTreeDisplayStrategy(treeView, testModel);
            }
        }
    }
}
