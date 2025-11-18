// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using TestCentric.Engine.Runners;

using SettingDefinitions = NUnit.Common.SettingDefinitions;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// TestRunnerFactory handles creation of a suitable test
    /// runner for a given package to be loaded and run either in a
    /// separate process or within the same process.
    /// </summary>
    class TestRunnerFactory : Service, ITestRunnerFactory
    {
        /// <summary>
        /// Returns a test runner based on the settings in a TestPackage.
        /// </summary>
        /// <param name="package">The TestPackage to be loaded and run</param>
        /// <returns>A TestRunner</returns>
        public NUnit.Engine.ITestEngineRunner MakeTestRunner(NUnit.Engine.TestPackage package)
        {
            var packageList = package.Select(p => !p.HasSubPackages);

            if (packageList.Count > 1)
                return new AggregatingTestRunner(ServiceContext, package);

            return MakeRunnerForSingleTestFile(packageList[0]);
        }

        private NUnit.Engine.ITestEngineRunner MakeRunnerForSingleTestFile(NUnit.Engine.TestPackage package)
        {
            if (!File.Exists(package.FullName))
                return new InvalidAssemblyRunner(package, "File not found: " + package.FullName);

            var ext = Path.GetExtension(package.FullName).ToLowerInvariant();
            if (ext != ".dll" && ext != ".exe")
                return new InvalidAssemblyRunner(package, "File type is not supported");

            if (package.Settings.GetValueOrDefault(SettingDefinitions.SkipNonTestAssemblies))
                return new SkippedAssemblyRunner(package);

            return new AssemblyRunner(ServiceContext, package);
        }

        // TODO: Review this method once used by a gui - currently unused.
        // The current implementation doesn't allow any runners to be reused.
        public bool CanReuse(NUnit.Engine.ITestEngineRunner runner, NUnit.Engine.TestPackage package)
        {
            return false;
        }
    }
}
