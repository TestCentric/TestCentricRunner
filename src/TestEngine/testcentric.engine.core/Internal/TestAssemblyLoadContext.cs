﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

#if NETCOREAPP3_1_OR_GREATER

using System.Reflection;
using System.Runtime.Loader;
using System.IO;
using System;
using System.Linq;

namespace TestCentric.Engine.Internal
{
    internal sealed class TestAssemblyLoadContext : AssemblyLoadContext
    {
        private readonly string _testAssemblyPath;
        private readonly string _basePath;
        private readonly TestAssemblyResolver _resolver;

        public TestAssemblyLoadContext(string testAssemblyPath)
        {
            _testAssemblyPath = testAssemblyPath;
            _resolver = new TestAssemblyResolver(this, testAssemblyPath);
            _basePath = Path.GetDirectoryName(testAssemblyPath);
        }

        protected override Assembly Load(AssemblyName name)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var loadedAssembly = assemblies.FirstOrDefault(x => x.GetName().Name == name.Name);

            if (loadedAssembly != null)
                loadedAssembly = base.Load(name);

            if (loadedAssembly == null)
            {
                // Load assemblies that are dependencies, and in the same folder as the test assembly,
                // but are not fully specified in test assembly deps.json file. This happens when the
                // dependencies reference in the csproj file has CopyLocal=false, and for example, the
                // reference is a projectReference and has the same output directory as the parent.
                string assemblyPath = Path.Combine(_basePath, name.Name + ".dll");
                if (File.Exists(assemblyPath))
                    loadedAssembly = LoadFromAssemblyPath(assemblyPath);
            }

            return loadedAssembly;
        }
    }
}

#endif
