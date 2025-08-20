// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Diagnostics;
using TestCentric.Extensibility;

namespace TestCentric.Engine.Extensibility
{
    [TypeExtensionPoint(
        Description = "Launches an Agent Process for supported target runtimes")]
    public interface IAgentLauncher
    {
        TestAgentInfo AgentInfo { get; }
        bool CanCreateProcess(TestPackage package);
        Process CreateProcess(Guid agentId, string agencyUrl, TestPackage package);
    }
}
