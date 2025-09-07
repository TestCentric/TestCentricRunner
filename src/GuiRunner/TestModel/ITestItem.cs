// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model
{
    /// <summary>
    /// ITestItem is the common interface shared by TestNodes
    /// and TestGroups and allows either to be selected in
    /// the tree.
    /// </summary>
    public interface ITestItem
    {
        /// <summary>
        /// The name of this item
        /// </summary>
        string Name { get; }
    }
}
