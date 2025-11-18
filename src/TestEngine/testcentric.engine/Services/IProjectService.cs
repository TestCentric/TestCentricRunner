// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Engine.Extensibility;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// The IProjectService interface is implemented by ProjectService.
    /// It knows how to load projects in a specific format and can expand
    /// TestPackages based on projects.
    /// </summary>
    public interface IProjectService
    {
        /// <summary>
        /// Returns true if the file indicated is one that this
        /// loader knows how to load.
        /// </summary>
        /// <param name="path">The path of the project file</param>
        /// <returns>True if the loader knows how to load this file, otherwise false</returns>
        bool CanLoadFrom(string path);

        /// <summary>
        /// Loads a project of a known format.
        /// </summary>
        /// <param name="path">The path of the project file</param>
        /// <returns>An IProject interface to the loaded project or null if the project cannot be loaded</returns>
        IProject LoadFrom(string path);

        /// <summary>
        /// Expands a TestPackage based on a known project format, populating it
        /// with the project contents and any settings the project provides. 
        /// Note that the package file path must be checked to ensure that it is
        /// a known project format before calling this method.
        /// </summary>
        /// <param name="package">The TestPackage to be expanded</param>
        void ExpandProjectPackage(NUnit.Engine.TestPackage package);
    }
}
