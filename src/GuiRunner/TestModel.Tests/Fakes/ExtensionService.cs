// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Engine;
using NUnit.Extensibility;

namespace TestCentric.Gui.Model.Fakes
{
    public class ExtensionService : IExtensionService
    {
        private List<IExtensionPoint> _extensionPoints;
        private List<IExtensionNode> _extensions;

        public ExtensionService()
        {
            _extensionPoints = new List<IExtensionPoint>();
            _extensions = new List<IExtensionNode>();

            // ExtensionPoints are all known, so we add in constructor. Extensions
            // may vary, so we use a method to add them.
            _extensionPoints.Add(new ExtensionPoint(
                "/NUnit/Engine/TestEventListeners", "NUnit.Engine.ITestEventListener", "Allows an extension to process progress reports and other events from the test."));
            _extensionPoints.Add(new ExtensionPoint(
                "/NUnit/Engine/DriverFactories", "NUnit.Engine.Extensibility.IDriverFactory", "Supplies a driver to run tests that use a specific test framework."));
            _extensionPoints.Add(new ExtensionPoint(
                "/NUnit/Engine/ProjectLoaders", "NUnit.Engine.Extensibility.IProjectLoader", "Recognizes and loads assemblies from various types of project formats."));
            _extensionPoints.Add(new ExtensionPoint(
                "/NUnit/Engine/ResultWriters", "NUnit.Engine.Extensibility.IResultWriter", "Supplies a writer to write the result of a test to a file using a specific format."));
        }

        public void AddExtensions(params IExtensionNode[] extensions)
        {
            _extensions.AddRange(extensions);
        }

        public IEnumerable<IExtensionPoint> ExtensionPoints { get { return _extensionPoints; } }

        public IEnumerable<IExtensionNode> Extensions { get; } = new List<IExtensionNode>();

        public void InstallExtensions()
        {

        }

        public void EnableExtension(string typeName, bool enabled)
        {
        }

        public IEnumerable<IExtensionNode> GetExtensionNodes(string path)
        {
            return new IExtensionNode[0];
        }

        public IExtensionPoint GetExtensionPoint(string path)
        {
            return null;
        }

        public void FindExtensionAssemblies(string initialDirectory)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetExtensions<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IExtensionNode> GetExtensionNodes<T>()
        {
            throw new NotImplementedException();
        }

        // ExtensionPoint class is nested since the list of extension points is fixed
        public class ExtensionPoint : IExtensionPoint
        {
            public ExtensionPoint(string path, string typeName, string description)
            {
                Path = path;
                TypeName = typeName;
                Description = description;
            }

            public string Description { get; }

            public IEnumerable<IExtensionNode> Extensions { get; } = new List<IExtensionNode>();

            public string Path { get; }

            public string TypeName { get; }
        }
    }

    public class ExtensionNode : IExtensionNode
    {
        public ExtensionNode(string path, string typeName, string description)
        {
            Path = path;
            TypeName = typeName;
            Description = description;
        }

        public string Description { get; }

        public bool Enabled { get; set; }

        public ExtensionStatus Status { get; }

        public Exception Exception { get; }

        public string Path { get; }

        public IEnumerable<string> PropertyNames { get; }

        public string TypeName { get; }

        public NUnit.Engine.IRuntimeFramework TargetFramework { get; }

        public IEnumerable<string> GetValues(string name)
        {
            return Array.Empty<string>();
        }

        public string AssemblyPath { get; }

        public Version AssemblyVersion { get; }

        public object ExtensionObject { get; }
    }
}
