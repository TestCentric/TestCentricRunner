// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Engine.Runners;

namespace TestCentric.Engine.Services.Fakes
{
    public class FakeTestRunnerFactory : Service, ITestRunnerFactory
    {
        public bool CanReuse(NUnit.Engine.ITestEngineRunner runner, TestPackage package)
        {
            return true;
        }

        public NUnit.Engine.ITestEngineRunner MakeTestRunner(TestPackage package)
        {
            return new AssemblyRunner(ServiceContext, package);
        }
    }
}
