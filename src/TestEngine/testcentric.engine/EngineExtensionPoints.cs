// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************
using NUnit.Extensibility;

// ExtensionPoints supported by the engine
[assembly: ExtensionPoint("/TestCentric/Engine/AgentLaunchers", typeof(NUnit.Engine.Extensibility.IAgentLauncher),
    Description = "Launches an Agent Process for supported target runtimes")]
[assembly: ExtensionPoint("/TestCentric/Engine/ProjectLoaders", typeof(NUnit.Engine.Extensibility.IProjectLoader),
    Description = "Recognizes and loads assemblies from various types of project formats.")]
[assembly: ExtensionPoint("/TestCentric/Engine/ResultWriters", typeof(NUnit.Engine.Extensibility.IResultWriter),
    Description = "Supplies a writer to write the result of a test to a file using a specific format.")]
[assembly: ExtensionPoint("/TestCentric/Engine/TestEventListeners", typeof(NUnit.Engine.ITestEventListener),
    Description = "Allows an extension to process progress reports and other events from the test.")]
[assembly: ExtensionPoint("/TestCentric/Engine/DriverFactories", typeof(NUnit.Engine.Extensibility.IDriverFactory),
    Description = "Supplies a driver to run tests that use a specific test framework.")]
