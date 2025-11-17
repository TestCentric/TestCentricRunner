// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Engine;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// A Test Runner factory can supply a suitable test runner for a given package
    /// </summary>
    internal interface ITestRunnerFactory
    {
        /// <summary>
        /// Return a suitable runner for the package provided as an argument
        /// </summary>
        /// <param name="package">The test package to be loaded by the runner</param>
        /// <returns>A TestRunner</returns>
        NUnit.Engine.ITestEngineRunner MakeTestRunner(TestPackage package);

        /// <summary>
        /// Return true if the provided runner is suitable for reuse in loading
        /// the test package provided. Otherwise, return false. Runners that
        /// cannot be reused must always return false.
        /// </summary>
        /// <param name="runner">An ITestRunner to possibly be used.</param>
        /// <param name="package">The TestPackage to be loaded.</param>
        /// <returns>True if the runner may be reused for the provided package.</returns>
        bool CanReuse(ITestEngineRunner runner, TestPackage package);
    }
}
