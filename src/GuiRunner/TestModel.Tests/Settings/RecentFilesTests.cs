// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

namespace TestCentric.Gui.Model.Settings
{
    [TestFixture]
    internal class RecentFilesTests
    {
        [Test]
        public void DefaultValue_IsEmptyList()
        {
            // Arrange
            UserSettings userSettings = new UserSettings();
            IRecentFiles recentFiles = userSettings.Gui.RecentFiles;

            // Act
            var files = recentFiles.Entries;

            // Assert
            Assert.That(files.Count, Is.EqualTo(0));
        }

        [Test]
        public void SetLatest_IsAddedToEntries()
        {
            // Arrange
            UserSettings userSettings = new UserSettings();
            IRecentFiles recentFiles = userSettings.Gui.RecentFiles;

            // Act
            recentFiles.Latest = "Test.dll";

            // Assert
            var files = recentFiles.Entries;
            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files[0], Is.EqualTo("Test.dll"));
        }

        [Test]
        public void SetLatest_Again_IsAddedOnlyOnce()
        {
            // Arrange
            UserSettings userSettings = new UserSettings();
            IRecentFiles recentFiles = userSettings.Gui.RecentFiles;

            // Act
            recentFiles.Latest = "Test.dll";
            recentFiles.Latest = "Test.dll";

            // Assert
            var files = recentFiles.Entries;
            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files[0], Is.EqualTo("Test.dll"));
        }
    }
}
