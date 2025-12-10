// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Engine.Runners
{
    public abstract class NonRunnableAssemblyRunner : AbstractTestRunner
    {
        public NonRunnableAssemblyRunner(NUnit.Engine.TestPackage package) : base(package) { }

        protected abstract NotRunnableAssemblyResult Result { get; }

        protected override NUnit.Engine.TestEngineResult LoadPackage()
        {
            return new NUnit.Engine.TestEngineResult(Result.LoadResult);
        }

        public override int CountTestCases(NUnit.Engine.TestFilter filter)
        {
            return 0;
        }

        protected override NUnit.Engine.TestEngineResult RunTests(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            return new NUnit.Engine.TestEngineResult(Result.RunResult);
        }

        public override NUnit.Engine.TestEngineResult Explore(NUnit.Engine.TestFilter filter)
        {
            return new NUnit.Engine.TestEngineResult(Result.LoadResult);
        }

        public override void RequestStop()
        {
        }

        public override void ForcedStop()
        {
        }
    }

    public class InvalidAssemblyRunner : NonRunnableAssemblyRunner
    {
        private string _assemblyPath;
        private string _message;

        public InvalidAssemblyRunner(NUnit.Engine.TestPackage package, string message)
            : base(package)
        {
            _assemblyPath = package.FullName;
            _message = message;
        }

        protected override NotRunnableAssemblyResult Result =>
            new InvalidAssemblyResult(_assemblyPath, _message)
            {
                TestID = TestPackage.ID
            };
    }

    public class SkippedAssemblyRunner : NonRunnableAssemblyRunner
    {
        private string _assemblyPath;

        public SkippedAssemblyRunner(NUnit.Engine.TestPackage package)
            :base(package)
        {
            _assemblyPath = package.FullName;
        }

        protected override NotRunnableAssemblyResult Result =>
            new SkippedAssemblyResult(_assemblyPath)
            {
                TestID = TestPackage.ID
            };
    }
}
