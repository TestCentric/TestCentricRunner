// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using NUnit.Engine;
using NUnit.Extensibility;
using TestCentric.Engine.Extensibility;
using TestCentric.Engine.Internal;

namespace TestCentric.Engine.Services
{
    public class AgentLauncherWrapper : IAgentLauncher
    {
        private NUnit.Engine.Extensibility.IAgentLauncher _agentLauncher;
        private TestAgentInfo _agentInfo;

        public AgentLauncherWrapper(ExtensionNode extensionNode, NUnit.Engine.Extensibility.IAgentLauncher agentLauncher)
        {
            string targetFrameworkName = agentLauncher.AgentInfo.TargetRuntime.ToString();

            if (!extensionNode.PropertyNames.Contains("AgentName"))
                extensionNode.AddProperty("AgentName", extensionNode.TypeName);
            if (!extensionNode.PropertyNames.Contains("AgentType"))
                extensionNode.AddProperty("AgentType", "LocalProcess");
            if (!extensionNode.PropertyNames.Contains("TargetFramework"))
                extensionNode.AddProperty("TargetFramework", targetFrameworkName);

            _agentLauncher = agentLauncher;
            _agentInfo = new TestAgentInfo(
                extensionNode.TypeName,
                TestAgentType.LocalProcess,
                new FrameworkName(targetFrameworkName));
        }

        public NUnit.Engine.TestAgentInfo AgentInfo => _agentInfo;

        public bool CanCreateAgent(TestPackage package)
        {
            return _agentLauncher.CanCreateAgent(package.MakeNUnitPackage());
        }

        public Process CreateAgent(Guid agentId, string agencyUrl, TestPackage package)
        {
            return _agentLauncher.CreateAgent(agentId, agencyUrl, package.MakeNUnitPackage());
        }
    }
}
