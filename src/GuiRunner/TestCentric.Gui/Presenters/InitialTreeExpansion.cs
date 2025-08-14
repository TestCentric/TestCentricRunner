// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters
{
    /// <summary>
    /// Indicates how a tree should be displayed
    /// </summary>
    public enum InitialTreeExpansion
    {
        Auto,		// Select based on space available
        Expand,		// Expand fully
        Collapse,	// Collapse fully
        HideTests	// Expand all but the fixtures, leaving
        // leaf nodes hidden
    }
}
