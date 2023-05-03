// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using NUnit.Engine;
using TestCentric.Engine.Extensibility;
using TestCentric.Engine.Internal;

namespace TestCentric.Engine.Services
{
    public class Net462AgentLauncher : IAgentLauncher
    {
        public static bool Enabled = true;

        private const string RUNTIME_IDENTIFIER = ".NETFramework";
        private static readonly Version RUNTIME_VERSION = new Version(4, 6, 2);
        private static readonly FrameworkName TARGET_FRAMEWORK = new FrameworkName(RUNTIME_IDENTIFIER, RUNTIME_VERSION);

        public TestAgentInfo AgentInfo => new TestAgentInfo(GetType().Name, TestAgentType.LocalProcess, TARGET_FRAMEWORK);

        public bool CanCreateProcess(TestPackage package)
        {
            // Get target runtime
            string runtimeSetting = package.GetSetting(EnginePackageSettings.TargetRuntimeFramework, "");
            var framework = RuntimeFramework.Parse(runtimeSetting).FrameworkName;
            return framework.Identifier == ".NETFramework" && framework.Version.Major <= 4;
        }

        public Process CreateProcess(Guid agentId, string agencyUrl, TestPackage package)
        {
            // Should not be called unless runtime is one we can handle
            if (!CanCreateProcess(package))
                return null;

            // Access other package settings
            bool runAsX86 = package.GetSetting(EnginePackageSettings.RunAsX86, false);
            bool debugTests = package.GetSetting(EnginePackageSettings.DebugTests, false);
            bool debugAgent = package.GetSetting(EnginePackageSettings.DebugAgent, false);
            string traceLevel = package.GetSetting(EnginePackageSettings.InternalTraceLevel, "Off");
            bool loadUserProfile = package.GetSetting(EnginePackageSettings.LoadUserProfile, false);
            string workDirectory = package.GetSetting(EnginePackageSettings.WorkDirectory, string.Empty);

            var sb = new StringBuilder($"--agentId={agentId} --agencyUrl={agencyUrl} --pid={Process.GetCurrentProcess().Id}");

            // Set options that need to be in effect before the package
            // is loaded by using the command line.
            if (traceLevel != "Off")
                sb.Append(" --trace=").EscapeProcessArgument(traceLevel);
            if (debugAgent)
                sb.Append(" --debug-agent");
            if (workDirectory != string.Empty)
                sb.Append(" --work=").EscapeProcessArgument(workDirectory);

            var agentName = runAsX86 ? "testcentric-agent-x86.exe" : "testcentric-agent.exe";
            var enginePath = AssemblyHelper.GetDirectoryName(Assembly.GetExecutingAssembly());
            var agentPath = System.IO.Path.Combine(enginePath, $"agents/net462/{agentName}");
            var agentArgs = sb.ToString();

            var process = new Process();
            process.EnableRaisingEvents = true;

            var startInfo = process.StartInfo;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.LoadUserProfile = loadUserProfile;

            startInfo.FileName = agentPath;
            startInfo.Arguments = agentArgs;

            return process;
        }
    }
}
