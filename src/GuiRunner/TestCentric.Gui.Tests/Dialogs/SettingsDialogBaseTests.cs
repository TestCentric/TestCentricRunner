// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Dialogs
{
    using NSubstitute;
    using NUnit.Common;
    using NUnit.Engine;
    using NUnit.Framework;
    using TestCentric.Gui.Model;

    [TestFixture]
    internal class SettingsDialogBaseTests
    {
        [Test]
        public void ApplySettings_IsAppliedToProject()
        {
            // 1. Arrange
            ITestModel model = Substitute.For<ITestModel>();
            TestCentricProject project = new TestCentricProject(model);
            model.TestCentricProject.Returns(project);

            SettingsDialogBase settingsDialog = new SettingsDialogBase(null, model);
            settingsDialog.PackageSettingChanges.Add(SettingDefinitions.DebugTests.WithValue(true));

            // 2. Act
            settingsDialog.ApplySettings();

            // 3. Assert
            Assert.That(project.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.True);
        }


        [Test]
        public void ApplySettings_ProjectIsNull_SettingIsNotApplied()
        {
            // 1. Arrange
            ITestModel model = Substitute.For<ITestModel>();
            model.TestCentricProject.Returns((TestCentricProject)null);

            SettingsDialogBase settingsDialog = new SettingsDialogBase(null, model);
            settingsDialog.PackageSettingChanges.Add(SettingDefinitions.DebugTests.WithValue(true));

            // 2. Act
            settingsDialog.ApplySettings();

            // 3. Assert
            // No crash occurred
            Assert.Pass("ApplySettings worked successfully even if no project is loaded.");
        }
    }
}
