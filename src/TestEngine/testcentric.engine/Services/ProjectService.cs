// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.IO;
using NUnit;
using NUnit.Common;
using NUnit.Engine;
using NUnit.Extensibility;
using NUnit.Engine.Extensibility;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// Summary description for ProjectService.
    /// </summary>
    /// <remarks>
    /// Depends on ExtensionService (optional)
    /// </remarks>
    class ProjectService : Service, IProjectService
    {
        Dictionary<string, ExtensionNode> _extensionIndex = new Dictionary<string, ExtensionNode>();

        public bool CanLoadFrom(string path)
        {
            ExtensionNode node = GetNodeForPath(path);
            if (node != null)
            {
                var loader = node.ExtensionObject as IProjectLoader;
                if (loader.CanLoadFrom(path))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Expands a TestPackage based on a known project format, populating it
        /// with the project contents and any settings the project provides.
        /// Note that the package file path must be checked to ensure that it is
        /// a known project format before calling this method.
        /// </summary>
        /// <param name="package">The TestPackage to be expanded</param>
        public void ExpandProjectPackage(NUnit.Engine.TestPackage package)
        {
            Guard.ArgumentNotNull(package, "package");
            Guard.ArgumentValid(package.SubPackages.Count == 0, "Package is already expanded", "package");

            string path = package.FullName;
            if (!File.Exists(path))
                return;

            IProject project = LoadFrom(path);
            Guard.ArgumentValid(project != null, "Unable to load project " + path, "package");

            string activeConfig = package.Settings.GetValueOrDefault(SettingDefinitions.ActiveConfig); // Need RunnerSetting
            if (activeConfig == null)
                activeConfig = project.ActiveConfigName;
            else
                Guard.ArgumentValid(project.ConfigNames.Contains(activeConfig), $"Requested configuration {activeConfig} was not found", "package");

            TestPackage tempPackage = project.GetTestPackage(activeConfig);

            // Add info about the configurations to the project package
            tempPackage.AddSetting(SettingDefinitions.ActiveConfig.WithValue(activeConfig));
            tempPackage.AddSetting(SettingDefinitions.ConfigNames.WithValue(new List<string>(project.ConfigNames).ToArray()));

            // The original package held overrides, so don't change them, but
            // do apply any settings specified within the project itself.
            foreach (var setting in tempPackage.Settings)
            {
                if (package.Settings.HasSetting(setting.Name)) // Don't override settings from command line
                    continue;

                package.Settings.Add(setting);
            }

            foreach (var subPackage in tempPackage.SubPackages)
                package.AddSubPackage(subPackage);

            // If no config file is specified (by user or by the project loader) check
            // to see if one exists in same directory as the package. If so, we
            // use it. If not, each assembly will use its own config, if present.
            if (!package.Settings.HasSetting(SettingDefinitions.ConfigurationFile))
            {
                var packageConfig = Path.ChangeExtension(path, ".config");
                if (File.Exists(packageConfig))
                    package.Settings.Add(SettingDefinitions.ConfigurationFile.WithValue(packageConfig));
            }
        }

        public override void StartService()
        {
            if (Status == ServiceStatus.Stopped)
            {
                try
                {
                    var extensionService = ServiceContext.GetService<ExtensionService>();

                    if (extensionService == null)
                        Status = ServiceStatus.Started;
                    else if (extensionService.Status != ServiceStatus.Started)
                        Status = ServiceStatus.Error;
                    else
                    {
                        Status = ServiceStatus.Started;

                        var extensionNodes = new List<IExtensionNode>();
                        extensionNodes.AddRange(extensionService.GetExtensionNodes<IProjectLoader>());
                        extensionNodes.AddRange(extensionService.GetExtensionNodes<NUnit.Engine.Extensibility.IProjectLoader>());

                        foreach (var node in extensionNodes)
                        {
                            foreach (string ext in node.GetValues("FileExtension"))
                            {
                                if (ext != null)
                                {
                                    if (_extensionIndex.ContainsKey(ext))
                                        throw new NUnitEngineException(string.Format("ProjectLoader extension {0} is already handled by another extension.", ext));

                                    // HACK
                                    _extensionIndex.Add(ext, (ExtensionNode)node);
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // TODO: Should we just ignore any addin that doesn't load?
                    Status = ServiceStatus.Error;
                    throw;
                }
            }
        }

        public IProject LoadFrom(string path)
        {
            if (File.Exists(path))
            {
                ExtensionNode node = GetNodeForPath(path);
                if (node != null)
                {
                    var loader = node.ExtensionObject as IProjectLoader;
                    if (loader.CanLoadFrom(path))
                        return loader.LoadFrom(path);
                }
            }

            return null;
        }

        private ExtensionNode GetNodeForPath(string path)
        {
            var ext = Path.GetExtension(path);

            if (string.IsNullOrEmpty(ext) || !_extensionIndex.ContainsKey(ext))
                return null;

            return _extensionIndex[ext];
        }
    }
}
