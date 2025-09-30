// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Linq;
using TestCentric.Engine.Runners;
using NUnit.Framework;

namespace TestCentric.Engine.Services.TestRunnerFactoryTests
{
    using System.IO;
    using Fakes;

    /// <summary>
    /// Tests of ITestRunner tree structure for different combinations
    /// of TestPackage and PackageSettings. Some tests are overkill for
    /// current capabilities. For example, it's really not necessary to
    /// verify that the subrunners of a MultipleTestProcessRunner are
    /// all ProcessRunners. However, this approach will support future
    /// scenarios, where different types of runners are aggregated.
    /// </summary>
    public class RunnerSelectionTests
    {
        static readonly string MOCK_ASSEMBLY = FullPathTo("mock-assembly.dll");

        private ServiceContext _services;

        [OneTimeSetUp]
        public void SetUp()
        {
            _services = new ServiceContext();

            var projectService = new FakeProjectService();
            projectService.Add("a.nunit", "a.dll");
            projectService.Add("ab.nunit", "a.dll", "b.dll");
            projectService.Add("m1.nunit", MOCK_ASSEMBLY);
            projectService.Add("m2.nunit", MOCK_ASSEMBLY, MOCK_ASSEMBLY);
            projectService.Add("m3.nunit", MOCK_ASSEMBLY, MOCK_ASSEMBLY, MOCK_ASSEMBLY);
            projectService.Add("am.nunit", "a.dll", MOCK_ASSEMBLY);
            projectService.Add("mb.nunit", MOCK_ASSEMBLY, "b.dll");
            projectService.Add("amb.nunit", "a.dll", MOCK_ASSEMBLY, "b.dll");
            projectService.Add("mbm.nunit", MOCK_ASSEMBLY, "b.dll", MOCK_ASSEMBLY);

            _services.Add(new FakeExtensionService());
            _services.Add(projectService);
            _services.Add(new TestRunnerFactory());
            _services.Add(new FakeRuntimeService());
            _services.Add(new TestFrameworkService());
            _services.Add(new TestPackageAnalyzer());

            _services.ServiceManager.StartServices();
        }

        [TestCaseSource(nameof(TestCases))]
        public void RunnerSelectionTest(TestPackage package, RunnerResult expectedResult)
        {
            var masterRunner = new MasterTestRunner(_services, package);
            var runner = masterRunner.GetEngineRunner();
            Assert.That(GetRunnerResult(runner), Is.EqualTo(expectedResult));
        }

        private static RunnerResult GetRunnerResult(NUnit.Engine.ITestEngineRunner runner)
        {
            var result = new RunnerResult { TestRunner = runner.GetType() };

            if (runner is AggregatingTestRunner aggRunner)
                result.SubRunners = aggRunner.Runners.Select(GetRunnerResult).ToList();

            return result;
        }

        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
#if NETCOREAPP
                yield return new TestCaseData(
                    new TestPackage("a.dll"),
                    RunnerResult.LocalTestRunner).SetName("SingleAssembly_StringCtor");

                yield return new TestCaseData(
                    new TestPackage(new string[] { "a.dll" }),
                    RunnerResult.LocalTestRunner).SetName("SingleAssembly_ListCtor");

                yield return new TestCaseData(
                    new TestPackage("a.junk"),
                    RunnerResult.LocalTestRunner).SetName("SingleUnknown");

                yield return new TestCaseData(
                    new TestPackage(new string[] { "a.dll", "b.dll" }),
                    RunnerResult.AggregatingTestRunner(2)).SetName("TwoAssemblies");

                yield return new TestCaseData(
                    new TestPackage(new[] { "ab.nunit" }),
                    RunnerResult.LocalTestRunner).SetName("SingleProject_ListCtor");

                yield return new TestCaseData(
                    new TestPackage("ab.nunit"),
                    RunnerResult.AggregatingTestRunner(2)).SetName("SingleProject_StringConstructor");

                yield return new TestCaseData(
                    new TestPackage(new[] { "ab.nunit", "ab.nunit" }),
                    RunnerResult.AggregatingTestRunner(4)).SetName("TwoProjects");

                yield return new TestCaseData(
                    new TestPackage(new string[] { "ab.nunit", "b.dll" }),
                    RunnerResult.AggregatingTestRunner(3)).SetName("OneProjectOneAssembly");

                yield return new TestCaseData(
                    new TestPackage(new[] { "ab.nunit", "ab.nunit", "a.dll" }),
                    RunnerResult.AggregatingTestRunner(3)).SetName("TwoProjectsOneAssembly");

                yield return new TestCaseData(
                    new TestPackage(new[] { "a.dll", "b.dll", "ab.nunit" }),
                    RunnerResult.AggregatingTestRunner(4)).SetName("TwoAssembliesOneProject");

                //yield return new TestCaseData(
                //    new TestPackage(new[] { "a.junk", "b.junk" }),
                //    RunnerResult.AggregatingTestRunner(2)).SetName("TwoUnknowns");

                //yield return new TestCaseData(
                //    new TestPackage(new[] { "a.junk", "a.dll", "ab.nunit" }),
                //    RunnerResult.AggregatingTestRunner(4)).SetName("OneAssemblyOneProjectOneUnknown");
#else
                yield return MakeTestCase("mock-assembly.dll",
                    RunnerResult.ProcessRunner);

                yield return MakeTestCase(
                    "a.dll", RunnerResult.InvalidAssemblyRunner);

                yield return MakeTestCase("a.junk",
                    RunnerResult.InvalidAssemblyRunner);

                yield return MakeTestCase("mock-assembly.dll mock-assembly.dll",
                    RunnerResult.AggregatingTestRunner(RunnerResult.ProcessRunner, 2));

                yield return MakeTestCase("a.dll b.dll",
                    RunnerResult.AggregatingTestRunner(RunnerResult.InvalidAssemblyRunner, 2));

                yield return MakeTestCase("mock-assembly.dll missing.dll",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner));

                yield return MakeTestCase("missing.dll mock-assembly.dll",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.ProcessRunner));

                yield return MakeTestCase("a.junk b.junk c.junk",
                    RunnerResult.AggregatingTestRunner(RunnerResult.InvalidAssemblyRunner, 3));

                yield return MakeTestCase("m1.nunit",
                    RunnerResult.ProcessRunner);

                yield return MakeTestCase("m2.nunit",
                    RunnerResult.AggregatingTestRunner(RunnerResult.ProcessRunner, 2));

                yield return MakeTestCase("m3.nunit",
                    RunnerResult.AggregatingTestRunner(RunnerResult.ProcessRunner, 3));

                yield return MakeTestCase("am.nunit",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.ProcessRunner));

                yield return MakeTestCase("mb.nunit",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner));

                yield return MakeTestCase("amb.nunit",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner));

                yield return MakeTestCase("mbm.nunit",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.ProcessRunner));

                yield return MakeTestCase("ab.nunit",
                    RunnerResult.AggregatingTestRunner(RunnerResult.InvalidAssemblyRunner, 2));

                yield return MakeTestCase("am.nunit mb.nunit",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner));

                yield return MakeTestCase("m2.nunit b.dll",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.ProcessRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner));

                yield return MakeTestCase("ab.nunit m2.nunit b.dll",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner));

                yield return MakeTestCase("a.dll m2.nunit a.junk",
                    RunnerResult.AggregatingTestRunner(
                        RunnerResult.InvalidAssemblyRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.ProcessRunner,
                        RunnerResult.InvalidAssemblyRunner));
#endif
            }
        }

        private static TestCaseData MakeTestCase(string files, RunnerResult result)
        {
            var paths = new List<string>();
            foreach (var file in files.Split(new[] { ' ' }))
                paths.Add(FullPathTo(file));

            return new TestCaseData(
                new TestPackage(paths), result)
                    .SetArgDisplayNames(files);
        }

        private static string FullPathTo(string file)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, file);
        }
    }
}
