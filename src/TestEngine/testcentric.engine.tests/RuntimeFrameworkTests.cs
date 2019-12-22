// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

#if !NETCOREAPP1_1 && !NETCOREAPP2_1
using System;
using System.Runtime.Versioning;
using NUnit.Framework;

namespace NUnit.Engine
{
    using Helpers;

    [TestFixture]
    public class RuntimeFrameworkTests
    {
        static readonly Runtime CURRENT_RUNTIME =
            Type.GetType("Mono.Runtime", false) != null
                ? Runtime.Mono
                : Runtime.Net;

        [Test]
        public void CanGetCurrentFramework()
        {
            RuntimeFramework framework = RuntimeFramework.CurrentFramework;

            Assert.That(framework.Runtime, Is.EqualTo(CURRENT_RUNTIME));
            Assert.That(framework.ClrVersion, Is.EqualTo(Environment.Version));
        }

        [Test]
        public void CurrentFrameworkHasBuildSpecified()
        {
            Assert.That(RuntimeFramework.CurrentFramework.ClrVersion.Build, Is.GreaterThan(0));
        }

        [TestCaseSource(nameof(frameworkData))]
        public void CanCreateUsingFrameworkVersion(FrameworkData data)
        {
            RuntimeFramework framework = new RuntimeFramework(data.runtime, data.frameworkVersion);
            Assert.That(framework.Runtime, Is.EqualTo(data.runtime));
            Assert.That(framework.FrameworkVersion, Is.EqualTo(data.frameworkVersion));
            Assert.That(framework.ClrVersion, Is.EqualTo(data.clrVersion));
            Assert.That(framework.FrameworkName, Is.EqualTo(data.frameworkName));
        }

        [TestCaseSource(nameof(frameworkData))]
        public void CanCreateUsingClrVersion(FrameworkData data)
        {
            // Versions 3.x and 4.5 or higher can't be created using CLR version
            Assume.That(data.frameworkVersion.Major != 3);
            Assume.That(data.frameworkVersion.Major != 4 || data.frameworkVersion.Minor == 0);

            RuntimeFramework framework = new RuntimeFramework(data.runtime, data.clrVersion);
            Assert.That(framework.Runtime, Is.EqualTo(data.runtime));
            Assert.That(framework.FrameworkVersion, Is.EqualTo(data.frameworkVersion));
            Assert.That(framework.ClrVersion, Is.EqualTo(data.clrVersion));
            Assert.That(framework.FrameworkName, Is.EqualTo(data.frameworkName));
        }

        [TestCaseSource(nameof(frameworkData))]
        public void CanParseRuntimeFramework(FrameworkData data)
        {
            RuntimeFramework framework = RuntimeFramework.Parse(data.representation);
            Assert.That(framework.Runtime, Is.EqualTo(data.runtime));
            Assert.That(framework.ClrVersion, Is.EqualTo(data.clrVersion));
            Assert.That(framework.FrameworkName, Is.EqualTo(data.frameworkName));
        }

        [TestCaseSource(nameof(frameworkData))]
        public void CanDisplayFrameworkAsString(FrameworkData data)
        {
            RuntimeFramework framework = new RuntimeFramework(data.runtime, data.frameworkVersion);
            Assert.That(framework.ToString(), Is.EqualTo(data.representation));
            Assert.That(framework.DisplayName, Is.EqualTo(data.displayName));
        }

        [TestCaseSource(nameof(frameworkData))]
        public void CanCreateFromFrameworkName(FrameworkData data)
        {
            Assume.That(data.runtime != Runtime.Mono);

            var framework = RuntimeFramework.FromFrameworkName(data.frameworkName);
            Assert.That(framework.ToString(), Is.EqualTo(data.representation));
        }

        [TestCaseSource(nameof(matchData))]
        public bool CanMatchRuntimes(RuntimeFramework f1, RuntimeFramework f2)
        {
            return f1.Supports(f2);
        }

        [TestCaseSource(nameof(CanLoadData))]
        public bool CanLoad(RuntimeFramework f1, RuntimeFramework f2)
        {
            return f1.CanLoad(f2);
        }

#pragma warning disable 414
        static TestCaseData[] matchData = new TestCaseData[] {
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(3,5)),
                new RuntimeFramework(Runtime.Net, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0)),
                new RuntimeFramework(Runtime.Net, new Version(3,5)))
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(3,5)),
                new RuntimeFramework(Runtime.Net, new Version(3,5)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0)),
                new RuntimeFramework(Runtime.Net, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0)),
                new RuntimeFramework(Runtime.Net, new Version(2,0,50727)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0,50727)),
                new RuntimeFramework(Runtime.Net, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0,50727)),
                new RuntimeFramework(Runtime.Net, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0)),
                new RuntimeFramework(Runtime.Mono, new Version(2,0)))
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0)),
                new RuntimeFramework(Runtime.Net, new Version(1,1)))
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0,50727)),
                new RuntimeFramework(Runtime.Net, new Version(2,0,40607)))
                .Returns(false),
            };

        private static readonly TestCaseData[] CanLoadData = {
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0)),
                new RuntimeFramework(Runtime.Net, new Version(2,0)))
                .Returns(true),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(2,0)),
                new RuntimeFramework(Runtime.Net, new Version(4,0)))
                .Returns(false),
            new TestCaseData(
                new RuntimeFramework(Runtime.Net, new Version(4,0)),
                new RuntimeFramework(Runtime.Net, new Version(2,0)))
                .Returns(true)
            };
#pragma warning restore 414

        public struct FrameworkData
        {
            public Runtime runtime;
            public Version frameworkVersion;
            public Version clrVersion;
            public string representation;
            public string displayName;
            public FrameworkName frameworkName;

            public FrameworkData(Runtime runtime, Version frameworkVersion, Version clrVersion,
                string representation, string displayName, string frameworkName)
            {
                this.runtime = runtime;
                this.frameworkVersion = frameworkVersion;
                this.clrVersion = clrVersion;
                this.representation = representation;
                this.displayName = displayName;
                this.frameworkName = frameworkName != null
                    ? new FrameworkName(frameworkName)
                    : null;
            }

            public override string ToString()
            {
                return string.Format("<{0}-{1}>", this.runtime, this.frameworkVersion);
            }
        }

#pragma warning disable 414
        static FrameworkData[] frameworkData = new FrameworkData[] {
            new FrameworkData(Runtime.Net, new Version(1,0), new Version(1,0,3705), "net-1.0", ".NET 1.0", ".NETFramework,Version=v1.0"),
            new FrameworkData(Runtime.Net, new Version(1,1), new Version(1,1,4322), "net-1.1", ".NET 1.1", ".NETFramework,Version=v1.1"),
            new FrameworkData(Runtime.Net, new Version(2,0), new Version(2,0,50727), "net-2.0", ".NET 2.0", ".NETFramework,Version=2.0"),
            new FrameworkData(Runtime.Net, new Version(3,0), new Version(2,0,50727), "net-3.0", ".NET 3.0", ".NETFramework,Version=3.0"),
            new FrameworkData(Runtime.Net, new Version(3,5), new Version(2,0,50727), "net-3.5", ".NET 3.5", ".NETFramework,Version=3.5"),
            new FrameworkData(Runtime.Net, new Version(4,0), new Version(4,0,30319), "net-4.0", ".NET 4.0", ".NETFramework,Version=4.0"),
            new FrameworkData(Runtime.Net, new Version(4,5), new Version(4,0,30319), "net-4.5", ".NET 4.5", ".NETFramework,Version=4.5"),
            new FrameworkData(Runtime.Net, new Version(4,5,1), new Version(4,0,30319), "net-4.5.1", ".NET 4.5.1", ".NETFramework,Version=4.5.1"),
            new FrameworkData(Runtime.Net, new Version(4,5,2), new Version(4,0,30319), "net-4.5.2", ".NET 4.5.2", ".NETFramework,Version=4.5.2"),
            new FrameworkData(Runtime.Net, new Version(4,6), new Version(4,0,30319), "net-4.6", ".NET 4.6", ".NETFramework,Version=4.6"),
            new FrameworkData(Runtime.Net, new Version(4,6,1), new Version(4,0,30319), "net-4.6.1", ".NET 4.6.1", ".NETFramework,Version=4.6.1"),
            new FrameworkData(Runtime.Net, new Version(4,6,2), new Version(4,0,30319), "net-4.6.2", ".NET 4.6.2", ".NETFramework,Version=4.6.2"),
            new FrameworkData(Runtime.Net, new Version(4,7), new Version(4,0,30319), "net-4.7", ".NET 4.7", ".NETFramework,Version=4.7"),
            new FrameworkData(Runtime.Net, new Version(4,7,1), new Version(4,0,30319), "net-4.7.1", ".NET 4.7.1", ".NETFramework,Version=4.7.1"),
            new FrameworkData(Runtime.Net, new Version(4,7,2), new Version(4,0,30319), "net-4.7.2", ".NET 4.7.2", ".NETFramework,Version=4.7.2"),
            new FrameworkData(Runtime.Net, new Version(4,8), new Version(4,0,30319), "net-4.8", ".NET 4.8", ".NETFramework,Version=4.8"),
            new FrameworkData(Runtime.Mono, new Version(1,0), new Version(1,1,4322), "mono-1.0", "Mono 1.0", ".NETFramework,Version=1.0"),
            new FrameworkData(Runtime.Mono, new Version(2,0), new Version(2,0,50727), "mono-2.0", "Mono 2.0", ".NETFramework,Version=2.0"),
            new FrameworkData(Runtime.Mono, new Version(3,5), new Version(2,0,50727), "mono-3.5", "Mono 3.5", ".NETFramework,Version=3.5"),
            new FrameworkData(Runtime.Mono, new Version(4,0), new Version(4,0,30319), "mono-4.0", "Mono 4.0", ".NETFramework,Version=4.0"),
            new FrameworkData(Runtime.NetCore, new Version(1,0), new Version(1,0,1234), "netcore-1.0", ".NETCore 1.0", ".NETCoreApp,Version=1.0"),
            new FrameworkData(Runtime.NetCore, new Version(1,1), new Version(1,1,1234), "netcore-1.1", ".NETCore 1.1", ".NETCoreApp,Version=1.1"),
            new FrameworkData(Runtime.NetCore, new Version(2,0), new Version(2,0,1234), "netcore-2.0", ".NETCore 2.0", ".NETCoreApp,Version=2.0"),
            new FrameworkData(Runtime.NetCore, new Version(2,1), new Version(2,1,1234), "netcore-2.1", ".NETCore 2.1", ".NETCoreApp,Version=2.1"),
            new FrameworkData(Runtime.NetCore, new Version(3,0), new Version(3,0,1234), "netcore-3.0", ".NETCore 3.0", ".NETCoreApp,Version=3.0"),
        };
#pragma warning restore 414
    }
}
#endif
