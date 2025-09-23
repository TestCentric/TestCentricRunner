// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************
using TestCentric.Engine.Extensibility;
using TestCentric.Extensibility;

// ExtensionPoints supported by the engine
[assembly: ExtensionPoint("/TestCentric/Engine/AgentLaunchers", typeof(NUnit.Engine.Extensibility.IAgentLauncher))]
[assembly: ExtensionPoint("/TestCentric/Engine/ProjectLoaders", typeof(NUnit.Engine.Extensibility.IProjectLoader))]
[assembly: ExtensionPoint("/TestCentric/Engine/ResultWriters", typeof(NUnit.Engine.Extensibility.IResultWriter))]
[assembly: ExtensionPoint("/TestCentric/Engine/TestEventListeners", typeof(NUnit.Engine.ITestEventListener))]
[assembly: ExtensionPoint("/TestCentric/Engine/DriverFactories", typeof(NUnit.Engine.Extensibility.IDriverFactory))]
