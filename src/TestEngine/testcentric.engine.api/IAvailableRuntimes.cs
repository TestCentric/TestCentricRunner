﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;

namespace TestCentric.Engine
{
    /// <summary>
    /// Interface that returns a list of available runtime frameworks.
    /// </summary>
    public interface IAvailableRuntimes
    {
        /// <summary>
        /// Gets a list of available runtime frameworks.
        /// </summary>
        IList<IRuntimeFramework> AvailableRuntimes { get; }
    }
}
