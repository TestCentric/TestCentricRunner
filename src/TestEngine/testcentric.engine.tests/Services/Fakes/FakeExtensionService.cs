// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using NUnit.Engine;
using NUnit.Extensibility;

namespace TestCentric.Engine.Services.Fakes
{
    public class FakeExtensionService : FakeService, IExtensionService
    {
        public IEnumerable<IExtensionPoint> ExtensionPoints
        {
            get
            {
                return new IExtensionPoint[0];
            }
        }

        public IEnumerable<IExtensionNode> Extensions
        {
            get
            {
                return new IExtensionNode[0];
            }
        }

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
            throw new System.NotImplementedException();
        }

        public IEnumerable<T> GetExtensions<T>()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<IExtensionNode> GetExtensionNodes<T>()
        {
            throw new System.NotImplementedException();
        }
    }
}
