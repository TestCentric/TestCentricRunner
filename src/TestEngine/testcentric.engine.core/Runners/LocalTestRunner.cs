// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;

namespace TestCentric.Engine.Runners
{
    /// <summary>
    /// LocalTestRunner runs tests in the current application domain.
    /// </summary>
    public class LocalTestRunner : TestAgentRunner
    {
        public LocalTestRunner(TestPackage package) : base(package)
        {
            this.TestDomain = AppDomain.CurrentDomain;
        }
    }
}
