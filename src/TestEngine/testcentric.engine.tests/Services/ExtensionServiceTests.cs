// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using NUnit.Framework;
using TestCentric.Engine.Internal;
using TestCentric.Engine.Extensibility;
using NUnit.Extensibility;

namespace TestCentric.Engine.Services
{
    public class ExtensionServiceTests
    {
        private ExtensionService _serviceClass;
        private IExtensionService _serviceInterface;

#pragma warning disable 414
        private static readonly string[] KnownExtensionPointPaths = {
            "/TestCentric/Engine/AgentLaunchers",
            "/TestCentric/Engine/ProjectLoaders",
            "/TestCentric/Engine/ResultWriters",
            "/TestCentric/Engine/TestEventListeners",
            "/TestCentric/Engine/DriverFactories"
        };

        private static readonly Type[] KnownExtensionPointTypes = {
            typeof(NUnit.Engine.Extensibility.IAgentLauncher),
            typeof(NUnit.Engine.Extensibility.IProjectLoader),
            typeof(NUnit.Engine.Extensibility.IResultWriter),
            typeof(NUnit.Engine.ITestEventListener),
            typeof(NUnit.Engine.Extensibility.IDriverFactory)
        };

        private static readonly int[] KnownExtensionPointCounts = { 1, 1, 2, 0, 1 };
#pragma warning restore 414

        [SetUp]
        public void CreateService()
        {
            // Initialize service to use actual extension points with dummy extensions installed
            _serviceInterface = _serviceClass = new ExtensionService();
            _serviceClass.ExtensionBaseDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
            _serviceClass.StartService();
        }

        [Test]
        public void AllExtensionPointsAreKnown()
        {
            Assert.That(_serviceInterface.ExtensionPoints.Select(ep => ep.Path), Is.EquivalentTo(KnownExtensionPointPaths));
        }

        [Test, Sequential]
        public void CanGetExtensionPointByPath(
            [ValueSource(nameof(KnownExtensionPointPaths))] string path,
            [ValueSource(nameof(KnownExtensionPointTypes))] Type type)
        {
            var ep = _serviceInterface.GetExtensionPoint(path);
            Assert.That(ep, Is.Not.Null);
            Assert.That(ep.Path, Is.EqualTo(path));
            Assert.That(ep.TypeName, Is.EqualTo(type.FullName));
        }

#if false // needs to be reorganized to provide access to some extension manager calls
#pragma warning disable 414
        private static readonly string[] KnownExtensions = {
            "TestCentric.Engine.DummyProjectLoaderExtension",
            "TestCentric.Engine.DummyResultWriterExtension",
            "TestCentric.Engine.DummyEventListenerExtension",
            "TestCentric.Engine.DummyServiceExtension"
        };
#pragma warning restore 414

        [TestCaseSource(nameof(KnownExtensions))]
        public void CanListExtensions(string typeName)
        {
            Assert.That(_serviceClass.Extensions,
                Has.One.Property(nameof(ExtensionNode.TypeName)).EqualTo(typeName)
                   .And.Property(nameof(ExtensionNode.Enabled)).True);
        }

        [Test, Sequential]
        public void ExtensionsAreAddedToExtensionPoint(
            [ValueSource(nameof(KnownExtensionPointPaths))] string path,
            [ValueSource(nameof(KnownExtensionPointCounts))] int extensionCount)
        {
            var ep = _serviceClass.GetExtensionPoint(path);
            Assume.That(ep, Is.Not.Null);

            Assert.That(ep.Extensions.Count, Is.EqualTo(extensionCount));
        }

        [Test]
        public void ExtensionMayBeDisabledByDefault()
        {
            Assert.That(_serviceInterface.Extensions,
                Has.One.Property(nameof(ExtensionNode.TypeName)).EqualTo("TestCentric.Engine.DummyDisabledExtension")
                   .And.Property(nameof(ExtensionNode.Enabled)).False);
        }

        [Test]
        public void DisabledExtensionMayBeEnabled()
        {
            _serviceInterface.EnableExtension("TestCentric.Engine.DummyDisabledExtension", true);

            Assert.That(_serviceInterface.Extensions,
                Has.One.Property(nameof(ExtensionNode.TypeName)).EqualTo("TestCentric.Engine.DummyDisabledExtension")
                   .And.Property(nameof(ExtensionNode.Enabled)).True);
        }

        [Test]
        public void SkipsGracefullyLoadingOtherFrameworkExtensionAssembly()
        {
            //May be null on mono
            Assume.That(Assembly.GetEntryAssembly(), Is.Not.Null, "Entry assembly is null, framework loading validation will be skipped.");

#if NETCOREAPP2_1
            var assemblyName = Path.Combine(GetNetFrameworkSiblingDirectory(), "testcentric.engine.core.tests.exe");
#elif NET462
            var assemblyName = Path.Combine(GetNetCoreSiblingDirectory(), "testcentric.engine.core.tests.dll");
#endif
            Assert.That(assemblyName, Does.Exist);

            var service = new ExtensionService();
            service.FindExtensionPoints(typeof(TestEngine).Assembly);
            service.FindExtensionPoints(typeof(ITestEngine).Assembly);
            var extensionAssembly = new ExtensionAssembly(assemblyName, false);

            Assert.That(() => service.FindExtensionsInAssembly(extensionAssembly), Throws.Nothing);
        }

        [TestCaseSource(nameof(ValidCombos))]
        public void ValidTargetFrameworkCombinations(FrameworkCombo combo)
        {
            Assert.That(() => ExtensionService.CanLoadTargetFramework(combo.RunnerAssembly, combo.ExtensionAssembly),
                Is.True);
        }

        [TestCaseSource(nameof(InvalidTargetFrameworkCombos))]
        public void InvalidTargetFrameworkCombinations(FrameworkCombo combo)
        {
            Assert.That(() => ExtensionService.CanLoadTargetFramework(combo.RunnerAssembly, combo.ExtensionAssembly),
                Is.False);
        }

        [TestCaseSource(nameof(InvalidRunnerCombos))]
        public void InvalidRunnerTargetFrameworkCombinations(FrameworkCombo combo)
        {
            Assert.That(() => ExtensionService.CanLoadTargetFramework(combo.RunnerAssembly, combo.ExtensionAssembly),
                Throws.Exception.TypeOf<NUnitEngineException>().And.Message.Contains("not .NET Standard"));
        }

        // ExtensionAssembly is internal, so cannot be part of the public test parameters
        public struct FrameworkCombo
        {
            internal Assembly RunnerAssembly { get; }
            internal ExtensionAssembly ExtensionAssembly { get; }

            internal FrameworkCombo(Assembly runnerAsm, ExtensionAssembly extensionAsm)
            {
                RunnerAssembly = runnerAsm;
                ExtensionAssembly = extensionAsm;
            }

            public override string ToString() =>
                $"{RunnerAssembly.GetName()}:{ExtensionAssembly.AssemblyName}";
        }

        public static IEnumerable<TestCaseData> ValidCombos()
        {
#if NETCOREAPP2_1
            Assembly netstandard = typeof(ExtensionService).Assembly;
            Assembly netcore = Assembly.GetExecutingAssembly();

            var extNetStandard = new ExtensionAssembly(netstandard.Location, false);
            var extNetCore = new ExtensionAssembly(netcore.Location, false);

            yield return new TestCaseData(new FrameworkCombo(netcore, extNetStandard)).SetName("ValidCombo(.NET Core, .NET Standard)");
            yield return new TestCaseData(new FrameworkCombo(netcore, extNetCore)).SetName("ValidCombo(.NET Core, .Net Core)");
#else
            Assembly netFramework = typeof(ExtensionService).Assembly;

            var extNetFramework = new ExtensionAssembly(netFramework.Location, false);
            var extNetStandard = new ExtensionAssembly(Path.Combine(TestContext.CurrentContext.TestDirectory, "testcentric.engine.core.dll"), false);

            yield return new TestCaseData(new FrameworkCombo(netFramework, extNetFramework)).SetName("ValidCombo(.NET Framework, .NET Framework)");
            yield return new TestCaseData(new FrameworkCombo(netFramework, extNetStandard)).SetName("ValidCombo(.NET Framework, .NET Standard)");
#endif
        }

        public static IEnumerable<TestCaseData> InvalidTargetFrameworkCombos()
        {
#if NETCOREAPP2_1
            Assembly netstandard = typeof(ExtensionService).Assembly;
            Assembly netcore = Assembly.GetExecutingAssembly();

            var extNetStandard = new ExtensionAssembly(netstandard.Location, false);
            var extNetCore = new ExtensionAssembly(netcore.Location, false);
            var extNetFramework = new ExtensionAssembly(Path.Combine(GetNetFrameworkSiblingDirectory(), "testcentric.engine.core.dll"), false);

            yield return new TestCaseData(new FrameworkCombo(netcore, extNetFramework)).SetName("InvalidCombo(.NET Core, .NET Framework)");
#else
            Assembly netFramework = typeof(ExtensionService).Assembly;


            var netCoreAppDir = GetNetCoreSiblingDirectory();
            var extNetStandard = new ExtensionAssembly(Path.Combine(netCoreAppDir, "testcentric.engine.core.dll"), false);
            var extNetCoreApp = new ExtensionAssembly(Path.Combine(netCoreAppDir, "testcentric.engine.core.tests.dll"), false);

            yield return new TestCaseData(new FrameworkCombo(netFramework, extNetCoreApp)).SetName("InvalidCombo(.NET Framework, .NET Core)");
#endif

        }

        public static IEnumerable<TestCaseData> InvalidRunnerCombos()
        {
#if NETCOREAPP2_1
            Assembly netstandard = typeof(ExtensionService).Assembly;
            Assembly netcore = Assembly.GetExecutingAssembly();

            var extNetStandard = new ExtensionAssembly(netstandard.Location, false);
            var extNetCore = new ExtensionAssembly(netcore.Location, false);
            var extNetFramework = new ExtensionAssembly(Path.Combine(GetNetFrameworkSiblingDirectory(), "testcentric.engine.dll"), false);

            yield return new TestCaseData(new FrameworkCombo(netstandard, extNetStandard)).SetName("InvalidCombo(.NET Standard, .NET Standard)");
            yield return new TestCaseData(new FrameworkCombo(netstandard, extNetCore)).SetName("InvalidCombo(.NET Standard, .NET Core)");
            yield return new TestCaseData(new FrameworkCombo(netstandard, extNetFramework)).SetName("InvalidCombo(.NET Standard, .NET Framework)");
#else
            return new List<TestCaseData>();
#endif
        }

        /// <summary>
        /// Returns a directory in the parent directory that the current test assembly is in. This
        /// is used to load assemblies that target different frameworks than the current tests. So
        /// if these tests are in bin\release\net462 and dir is netstandard2.0, this will return
        /// bin\release\netstandard2.0.
        /// </summary>
        /// <param name="dir">The sibling directory</param>
        /// <returns></returns>
        private static string GetSiblingDirectory(string dir)
        {
            var file = new FileInfo(AssemblyHelper.GetAssemblyPath(typeof(ExtensionServiceTests).Assembly));
            return Path.Combine(file.Directory.Parent.FullName, dir);
        }

        private static string GetNetFrameworkSiblingDirectory()
        {
            return GetSiblingDirectory("net462");
        }

        private static string GetNetCoreSiblingDirectory()
        {
            return GetSiblingDirectory("netcoreapp2.1");
        }
#endif
    }
}
