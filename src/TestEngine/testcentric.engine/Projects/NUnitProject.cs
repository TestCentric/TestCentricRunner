// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestCentric.Engine.Extensibility;

namespace TestCentric.Engine.Projects
{
    /// <summary>
    /// Represents an NUnit Project, as created by the NUnit project loader extension.
    /// </summary>
    public class NUnitProject : IProject
    {
        private NUnit.Engine.Extensibility.IProject _nunitProject;

        /// <summary>
        /// Construct from an IProject provided by the NUnit extension.
        /// </summary>
        /// <param name="nunitProject">An NUnit.Engine.Extensibility.IProject interface</param>
        public NUnitProject(NUnit.Engine.Extensibility.IProject nunitProject)
        {
            _nunitProject = nunitProject;

            ProjectPath = nunitProject.ProjectPath;
            ActiveConfigName = nunitProject.ActiveConfigName;
            ConfigNames = nunitProject.ConfigNames;

            if (ActiveConfigName == null && ConfigNames.Count > 0) 
            { 
                ActiveConfigName = ConfigNames[0]; // Default unless release is present
                foreach (var name in ConfigNames)
                    if (name.Equals("release", StringComparison.OrdinalIgnoreCase))
                    {
                        ActiveConfigName = name;
                        break;
                    }
            }

            TestPackage = MakeTestPackageFromNUnitPackage(nunitProject.GetTestPackage());
        }

        #region IProject Members

        /// <summary>
        /// Gets the path to the file storing this project, if any.
        /// If the project has not been saved, this is null.
        /// </summary>
        public string ProjectPath { get; private set; }

        /// <summary>
        /// Gets the active configuration, as defined by the particular project.
        /// </summary>
        /// <remarks>
        /// If not set by the loader extension, we use Release if present, or
        /// the first config if Release is not found. Note that version 3.7.1
        /// of the externsion already sets ActiveConfig to the first value found,
        /// so our check for release currently has no effect. Also, note that the
        /// config may still be null if there are no config nodes present in the
        /// original project. This is a feature of the format.
        /// </remarks>
        public string ActiveConfigName { get; private set; }

        /// <summary>
        /// Gets a list of all config names defined in the project or an empty
        /// list if none are defined.
        /// </summary>
        public IList<string> ConfigNames { get; private set; }

        /// <summary>
        /// Gets the top-level TestPackage associated with this proeject.
        /// </summary>
        public TestPackage TestPackage { get; private set; }

        /// <summary>
        /// Gets the top-level TestPackage associated with this proeject.
        /// </summary>
        // TODO: Drop this from the interface and add the property
        public TestPackage GetTestPackage()
        {
            return TestPackage;
        }

        // TODO: THIS should allow for all types of settings
        public TestPackage GetTestPackage(string configName)
        {
            if (configName == null)
                throw new ArgumentNullException(nameof(configName));

           return MakeTestPackageFromNUnitPackage(_nunitProject.GetTestPackage(configName));
        }

        private TestPackage MakeTestPackageFromNUnitPackage(NUnit.Engine.TestPackage nunitPackage)
        {
            var package = new TestPackage(nunitPackage.SubPackages.Select(p => p.FullName).ToArray());
            foreach (var setting in nunitPackage.Settings)
                package.Settings.Add(setting.Name, (string)setting.Value);

            return package;
        }

        #endregion
    }
}
