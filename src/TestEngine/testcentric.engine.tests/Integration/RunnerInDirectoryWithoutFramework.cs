// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

#if !NETCOREAPP1_1
using System;
using System.IO;
using System.Threading;
using NUnit.Framework;

namespace NUnit.Engine.Integration
{
    internal sealed class RunnerInDirectoryWithoutFramework : IDisposable
    {
        private readonly DirectoryWithNeededAssemblies directory;

        public string ConsoleExe => Path.Combine(directory.Directory, "nunit3-console.exe");
        public string AgentExe => Path.Combine(directory.Directory, "testcentric-agent.exe");
        public string AgentX86Exe => Path.Combine(directory.Directory, "testcentric-agent-x86.exe");

        public RunnerInDirectoryWithoutFramework()
        {
            directory = new DirectoryWithNeededAssemblies("nunit3-console", "testcentric.engine", "testcentric.engine.core", "testcentric-agent", "testcentric-agent-x86");

            Assert.That(Path.Combine(directory.Directory, "nunit.framework.dll"), Does.Not.Exist, "This test must be run without nunit.framework.dll in the same directory as the console runner.");
        }

        public void Dispose()
        {
            for (;;) try
            {
                File.Delete(AgentExe);
                File.Delete(AgentX86Exe);
                break;
            }
            catch (UnauthorizedAccessException)
            {
                Thread.Sleep(100);
            }

            directory.Dispose();
        }
    }
}
#endif
