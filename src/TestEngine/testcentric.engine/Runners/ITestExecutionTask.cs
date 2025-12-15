// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;

namespace TestCentric.Engine.Runners
{
    public interface ITestExecutionTask
    {
        void Execute();
    }
}
