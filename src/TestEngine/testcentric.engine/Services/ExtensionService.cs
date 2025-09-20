// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TestCentric.Engine.Internal;
using TestCentric.Extensibility;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// The ExtensionService discovers ExtensionPoints and Extensions and
    /// maintains them in a database. It can return extension nodes or
    /// actual extension objects on request.
    /// </summary>
    class ExtensionService : Service, IExtensionService
    {
        static readonly Logger log = InternalTrace.GetLogger(typeof(ExtensionService));

        private const string ENGINE_TYPE_EXTENSION_PATH = "/TestCentric/Engine/TypeExtensions/";

        static readonly Assembly TESTCENTRIC_ENGINE_ASSEMBLY = Assembly.GetExecutingAssembly();
        static readonly Assembly TESTCENTRIC_ENGINE_API_ASSEMBLY = typeof(ITestEngine).Assembly;
        static readonly Assembly NUNIT_ENGINE_API_ASSEMBLY = typeof(NUnit.Engine.ITestEngine).Assembly;
        static readonly string ENGINE_DIRECTORY = Path.GetDirectoryName(AssemblyHelper.GetAssemblyPath(TESTCENTRIC_ENGINE_ASSEMBLY));

        private static readonly bool RUNNING_UNDER_CHOCOLATEY =
            File.Exists(Path.Combine(ENGINE_DIRECTORY, "VERIFICATION.txt"));
        private static readonly string[] PACKAGE_PREFIXES = RUNNING_UNDER_CHOCOLATEY
            ? ["testcentric-extension-", "nunit-extension-"]
            : ["TestCentric.Extension.", "NUnit.Extension."];

        // The Extension Manager is available internally to allow direct
        // access to ExtensionPoints and ExtensionNodes.
        internal readonly ExtensionManager _extensionManager;

        public ExtensionService()
        {
            _extensionManager = new ExtensionManager
            {
                TypeExtensionPath = ENGINE_TYPE_EXTENSION_PATH,
                PackagePrefixes = PACKAGE_PREFIXES
            };
        }

        #region IExtensionService Implementation

        /// <summary>
        /// Gets an enumeration of all ExtensionPoints in the engine.
        /// </summary>
        public IEnumerable<IExtensionPoint> ExtensionPoints => _extensionManager.ExtensionPoints;

        /// <summary>
        /// Gets an enumeration of all installed Extensions.
        /// </summary>
        public IEnumerable<IExtensionNode> Extensions => _extensionManager.Extensions;

        /// <summary>
        /// Get an ExtensionPoint based on its unique identifying path.
        /// </summary>
        IExtensionPoint IExtensionService.GetExtensionPoint(string path) => _extensionManager.GetExtensionPoint(path);

        /// <summary>
        /// Get an enumeration of ExtensionNodes based on their identifying path.
        /// </summary>
        IEnumerable<IExtensionNode> IExtensionService.GetExtensionNodes(string path) => _extensionManager.GetExtensionNodes(path);

        /// <summary>
        /// Enable or disable an extension
        /// </summary>
        public void EnableExtension(string typeName, bool enabled) => _extensionManager.EnableExtension(typeName, enabled);

        #endregion

        #region Other Public Properties and Methods

        public IList<Assembly> RootAssemblies { get; } = new List<Assembly>();

        public string ExtensionBaseDirectory { get; set; } = ENGINE_DIRECTORY;

        public IEnumerable<IExtensionNode> GetExtensionNodes<T>(bool includeDisabled = false)
        {
            return _extensionManager.GetExtensionNodes<T>(includeDisabled);
        }

        public IEnumerable<T> GetExtensions<T>()
            where T : class
        {
            var exceptions = new List<Exception>();

            foreach (var node in _extensionManager.GetExtensionNodes<T>())
            {
                T extension = (T)node.ExtensionObject;

                if (extension is not null)
                    yield return extension;
            }
        }

        #endregion

        #region StartService Override

        public override void StartService()
        {
            try
            {
                _extensionManager.FindExtensionPoints(
                    TESTCENTRIC_ENGINE_ASSEMBLY);
                    //TESTCENTRIC_ENGINE_API_ASSEMBLY);
                
                _extensionManager.FindExtensionAssemblies(
                    TESTCENTRIC_ENGINE_ASSEMBLY);
                
                Status = ServiceStatus.Started;
            }
            catch
            {
                Status = ServiceStatus.Error;
                throw;
            }
        }

        #endregion
    }
}
