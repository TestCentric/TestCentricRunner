// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Reflection;
using TestCentric.Metadata;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// The TestFrameworkService detects references to known
    /// test frameworks, which are referenced by an assembly.
    /// </summary>
    /// <remarks>
    /// Although this service is currently only used by the
    /// PackageSettingsService, it has been designed as a
    /// separate service because it will eventually allow
    /// extensions in support of new test frameworks.
    /// </remarks>
    class TestFrameworkService : Service, ITestFrameworkService
    {
        static Logger log = InternalTrace.GetLogger(typeof(TestFrameworkService));

        /// <summary>
        /// Returns a list of test frameworks known to this service.
        /// </summary>
        /// <remarks>
        /// This is currently a fixed list but will be extensible in the future.
        /// </remarks>
        public IList<string> KnownFrameworks => new [] { "nunit.framework", "nunitlite" };

        /// <summary>
        /// Returns information about the first test framework referenced
        /// by the named assembly or null if none is found.
        /// </summary>
        public ITestFrameworkReference GetFrameworkReference(string assemblyPath)
        {
            var reference = FindReferencedTestFramework(assemblyPath);
            return reference != null ? new TestFrameworkReference(reference) : null;
        }

        private AssemblyName FindReferencedTestFramework(string assemblyName)
        {
            using (var assembly = AssemblyDefinition.ReadAssembly(assemblyName))
            {

                foreach (var reference in assembly.MainModule.AssemblyReferences)
                    foreach (string name in KnownFrameworks)
                        if (reference.Name == name)
                        {
                            log.Debug($"Assembly {assemblyName} uses test framework {name}, version {reference.Version}");
                            return new AssemblyName(reference.FullName);
                        }
            }

            return null;
        }
    }

    /// <summary>
    /// The TestFrameworkReference class represents a reference
    /// to a known test framework found in a test assembly.
    /// </summary>
    public class TestFrameworkReference : ITestFrameworkReference
    {
        public TestFrameworkReference(AssemblyName frameworkReference)
        {
            Guard.ArgumentNotNull(frameworkReference, nameof(frameworkReference));

            FrameworkReference = frameworkReference;
            //if (Name == "nunit.framework")
            //    FrameworkDriver = typeof(Drivers.NUnit3FrameworkDriver).AssemblyQualifiedName;
        }

        /// <summary>
        /// Gets the name of the test framework.
        /// </summary>
        public string Name => FrameworkReference.Name;

        public string FullName => FrameworkReference.FullName;

        /// <summary>
        /// Gets a reference to a known test framework, which was found
        /// in a test assembly or null if none was found.
        /// </summary>
        public AssemblyName FrameworkReference { get; set; }

        /// <summary>
        /// Gets the AssemblyQualifiedName of the framework driver
        /// to be used for loading and running the tests.
        /// </summary>
        public string FrameworkDriver { get; }
    }
}
