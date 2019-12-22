// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System.IO;
using NUnit.Framework;

namespace NUnit.Engine
{
    [NonParallelizable]
    class WorkingDirectoryTests
    {
        private string _origWorkingDir;

        [OneTimeSetUp]
        public void SetWorkingDirToTempDir()
        {
            _origWorkingDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetTempPath());
        }

        [OneTimeTearDown]
        public void ResetWorkingDir()
        {
            Directory.SetCurrentDirectory(_origWorkingDir);
        }

        [Test]
        public void EngineCanBeCreatedFromAnyWorkingDirectory()
        {
            Assert.That(() => TestEngineActivator.CreateInstance(), Throws.Nothing);
        }
    }
}
