// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using TestCentric.Engine.Extensibility;

using NUnitPackage = NUnit.Engine.TestPackage;

namespace TestCentric.Engine.Services
{
    public class AgentLauncherWrapper : IAgentLauncher
    {
        private NUnit.Engine.Extensibility.IAgentLauncher _agentLauncher;

        public AgentLauncherWrapper(NUnit.Engine.Extensibility.IAgentLauncher agentLauncher)
        {
            _agentLauncher = agentLauncher;
        }

        public TestAgentInfo AgentInfo
        {
            get
            {
                return new TestAgentInfo(
                    _agentLauncher.AgentInfo.AgentName,
                    (TestAgentType)Enum.Parse(typeof(TestAgentType), _agentLauncher.AgentInfo.AgentType.ToString()),
                    _agentLauncher.AgentInfo.TargetRuntime.ToString());
            }
        }

        public bool CanCreateProcess(TestPackage package)
        {
            return _agentLauncher.CanCreateAgent(MakeNUnitPackage(package));
        }

        public Process CreateProcess(Guid agentId, string agencyUrl, TestPackage package)
        {
            return _agentLauncher.CreateAgent(agentId, agencyUrl, MakeNUnitPackage(package));
        }

        private static NUnitPackage MakeNUnitPackage(TestPackage package)
        {
            var nunitPackage = new NUnitPackage();

            foreach (var subPackage in package.SubPackages)
                nunitPackage.AddSubPackage(MakeNUnitPackage(subPackage));

            foreach (var setting in package.Settings)
            {
                var name = setting.Name;
                var value = setting.Value;

                if (value is int)
                    nunitPackage.AddSetting(setting.Name, (int)value);
                else if (value is bool)
                    nunitPackage.AddSetting(setting.Name, (bool)value);
                else if (value is string)
                    nunitPackage.AddSetting(setting.Name, (string)value);
            }

            // HACK: We still use TargetRuntimeFramework, of Type RuntimeFramework.
            // NUnit uses TargetFrameworkName. This is needed until our API is updated.
            var targetRuntime = package.Settings.GetValueOrDefault(SettingDefinitions.TargetRuntimeFramework);
            nunitPackage.AddSetting("TargetFrameworkName", RuntimeFramework.Parse(targetRuntime).FrameworkName.ToString());

            return nunitPackage;
        }
    }
}
