// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Engine.Extensibility;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// Wrapper class for project loaders which use the NUnit Engine API
    /// </summary>
    public class NUnitProjectLoaderWrapper : IProjectLoader
    {
        private NUnit.Engine.Extensibility.IProjectLoader _projectLoader;

        public NUnitProjectLoaderWrapper(NUnit.Engine.Extensibility.IProjectLoader projectLoader)
        {
            _projectLoader = projectLoader;
        }

        public bool CanLoadFrom(string path)
        {
            return _projectLoader.CanLoadFrom(path);
        }

        public IProject LoadFrom(string path)
        {
            return new Projects.NUnitProject(_projectLoader.LoadFrom(path));
        }
    }
}
