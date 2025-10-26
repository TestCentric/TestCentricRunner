// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;

namespace TestCentric.Engine.Extensibility
{
    public interface IAgentLauncher
    {
        NUnit.Engine.TestAgentInfo AgentInfo { get; }
        bool CanCreateAgent(TestPackage package);
        Process CreateAgent(Guid agentId, string agencyUrl, TestPackage package);
    }
}
