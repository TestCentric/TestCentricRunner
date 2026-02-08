// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NSubstitute;
    using NUnit.Common;
    using NUnit.Framework;
    using TestCentric.Gui.Model.Settings;

    [TestFixture]
    internal class TestCentricProjectTests
    {
        private ITestModel _model;
        private IUserSettings _settings;

        [SetUp]
        public void SetUp()
        {
            _model = Substitute.For<ITestModel>();
            _settings = Substitute.For<IUserSettings>();
            _model.Settings.Returns(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up any test files created
            if (File.Exists("TestCentricTestProject.tcproj"))
                File.Delete("TestCentricTestProject.tcproj");
            if (File.Exists("AnotherTestProject.tcproj"))
                File.Delete("AnotherTestProject.tcproj");
        }

        #region Constructor Tests

        [Test]
        public void Constructor_EmptyConstructor_InitializesProject()
        {
            // 1. Arrange & Act
            TestCentricProject project = new TestCentricProject();

            // 2. Assert
            Assert.That(project, Is.Not.Null);
            Assert.That(project.TestFiles, Is.Not.Null);
            Assert.That(project.TestFiles.Count, Is.EqualTo(0));
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void Constructor_WithSingleFilename_AddsFileToTestFiles()
        {
            // 1. Arrange & Act
            TestCentricProject project = new TestCentricProject(new GuiOptions("TestAssembly.dll"));

            // 2. Assert
            Assert.That(project.TestFiles.Count, Is.EqualTo(1));
            Assert.That(project.TestFiles[0], Is.EqualTo("TestAssembly.dll"));
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void Constructor_WithMultipleFilenames_AddsAllFilesToTestFiles()
        {
            // 1. Arrange
            string[] filenames = ["Test1.dll", "Test2.dll", "Test3.dll"];

            // 2. Act
            TestCentricProject project = new TestCentricProject(new GuiOptions(filenames));

            // 3. Assert
            Assert.That(project.TestFiles.Count, Is.EqualTo(3));
            Assert.That(project.TestFiles, Is.EquivalentTo(filenames));
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void Constructor_WithSolutionFile_SetsSkipNonTestAssemblies()
        {
            // 1. Arrange & Act
            TestCentricProject project = new TestCentricProject(new GuiOptions("Solution.sln"));

            // 2. Assert
            Assert.That(project.TopLevelPackage.SubPackages.Count, Is.EqualTo(1));
            Assert.That(project.TopLevelPackage.SubPackages[0].Settings.HasSetting(SettingDefinitions.SkipNonTestAssemblies.Name), Is.True);
        }

        [Test]
        public void Constructor_WithTcprojFile_ThrowsInvalidOperationException()
        {
            // 1. Arrange & Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                new TestCentricProject(new GuiOptions("NestedProject.tcproj")));
        }

        #endregion

        #region IsProjectFile Tests

        [Test]
        [TestCase("project.tcproj", true)]
        [TestCase("PROJECT.TCPROJ", true)]
        [TestCase("MyProject.tcproj", true)]
        [TestCase("test.dll", false)]
        [TestCase("solution.sln", false)]
        [TestCase("project.nunit", false)]
        public void IsProjectFile_VariousExtensions_ReturnsExpectedResult(string filename, bool expected)
        {
            // 1. Arrange & Act
            bool result = TestCentricProject.IsProjectFile(filename);

            // 2. Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        #endregion

        #region IsDirty Tests

        [Test]
        public void IsDirty_NewProject_IsFalse()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act & Assert
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void IsDirty_AfterSetTopLevelSetting_IsTrue()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));

            // 3. Assert
            Assert.That(project.IsDirty, Is.True);
        }

        [Test]
        public void IsDirty_AfterRemoveSetting_IsTrue()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.RemoveSetting(SettingDefinitions.DebugTests.Name);

            // 3. Assert
            Assert.That(project.IsDirty, Is.True);
        }

        [Test]
        public void IsDirty_AfterSetSubPackageSetting_IsTrue()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSetting(SettingDefinitions.DebugTests.WithValue(true));

            // 3. Assert
            Assert.That(project.IsDirty, Is.True);
        }

        [Test]
        public void IsDirty_AfterAddPackageSetting_IsTrue()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSetting(SettingDefinitions.DebugTests.WithValue(true));

            // 3. Assert
            Assert.That(project.IsDirty, Is.True);
        }

        [Test]
        public void IsDirty_AfterAddSubPackage_IsTrue()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSubPackage("NewTest.dll");

            // 3. Assert
            Assert.That(project.IsDirty, Is.True);
        }

        [Test]
        public void IsDirty_AfterRemoveSubPackage_IsTrue()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject(new GuiOptions("Test1.dll", "Test2.dll" ));
            var subPackage = project.TopLevelPackage.SubPackages[0];

            // 2. Act
            project.RemoveSubPackage(subPackage);

            // 3. Assert
            Assert.That(project.IsDirty, Is.True);
        }

        [Test]
        public void IsDirty_AfterSave_IsFalse()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));
            Assert.That(project.IsDirty, Is.True);

            // 2. Act
            project.SaveAs("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void IsDirty_AfterLoad_IsFalse()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));
            project.SaveAs("TestCentricTestProject.tcproj");
            
            project.SetTopLevelSetting(SettingDefinitions.MaxAgents.WithValue(5));
            Assert.That(project.IsDirty, Is.True);

            // 2. Act
            project.Load("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(project.IsDirty, Is.False);
        }

        #endregion

        #region AddSetting and RemoveSetting Tests

        [Test]
        public void AddSetting_AddsSettingToProject()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSetting(SettingDefinitions.DebugTests.Name, true);

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.True);
            Assert.That(project.TopLevelPackage.Settings.GetSetting(SettingDefinitions.DebugTests.Name), Is.EqualTo(true));
        }

        [Test]
        public void SetTopLevelSetting_Setting_IsSetInProject()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.True);
            Assert.That(project.TopLevelPackage.Settings.GetSetting(SettingDefinitions.DebugTests.Name), Is.EqualTo(true));
        }

        [Test]
        public void SetSubPackageSetting_Setting_IsSetInProject()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSetting(SettingDefinitions.DebugTests.WithValue(true));

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.True);
            Assert.That(project.TopLevelPackage.Settings.GetSetting(SettingDefinitions.DebugTests.Name), Is.EqualTo(true));
        }

        [Test]
        public void AddSetting_WithVariousTypes_StoresCorrectly()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSetting("BoolSetting", true);
            project.AddSetting("IntSetting", 42);
            project.AddSetting("StringSetting", "TestValue");

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.GetSetting("BoolSetting"), Is.EqualTo(true));
            Assert.That(project.TopLevelPackage.Settings.GetSetting("IntSetting"), Is.EqualTo(42));
            Assert.That(project.TopLevelPackage.Settings.GetSetting("StringSetting"), Is.EqualTo("TestValue"));
        }

        [Test]
        public void AddSetting_InvokedTwice_StoresCorrectly()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSetting("BoolSetting", true);
            project.AddSetting("BoolSetting", true);

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.GetSetting("BoolSetting"), Is.EqualTo(true));
        }

        [Test]
        public void AddSetting_InvokedTwice2_StoresCorrectly()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSetting(SettingDefinitions.DebugTests.WithValue(true));
            project.AddSetting(SettingDefinitions.DebugTests.WithValue(true));

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.GetSetting(SettingDefinitions.DebugTests.Name), Is.EqualTo(true));
        }

        [Test]
        public void RemoveSetting_ByName_RemovesSettingFromProject()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.True);

            // 2. Act
            project.RemoveSetting(SettingDefinitions.DebugTests.Name);

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.False);
        }

        [Test]
        public void RemoveSetting_ByDefinition_RemovesSettingFromProject()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.True);

            // 2. Act
            project.RemoveSetting(SettingDefinitions.DebugTests);

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests.Name), Is.False);
        }

        #endregion

        #region SubPackage Management Tests

        [Test]
        public void AddSubPackage_ByFilename_AddsToSubPackagesAndTestFiles()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.AddSubPackage("NewTest.dll");

            // 3. Assert
            Assert.That(project.TopLevelPackage.SubPackages.Count, Is.EqualTo(1));
            Assert.That(project.TestFiles.Count, Is.EqualTo(1));
            Assert.That(project.TestFiles[0], Is.EqualTo("NewTest.dll"));
        }

        [Test]
        public void AddSubPackage_ByTestPackage_AddsToSubPackages()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            var subPackage = new NUnit.Engine.TestPackage("SubTest.dll");

            // 2. Act
            project.AddSubPackage(subPackage);

            // 3. Assert
            Assert.That(project.TopLevelPackage.SubPackages.Count, Is.EqualTo(1));
            Assert.That(project.TopLevelPackage.SubPackages[0], Is.SameAs(subPackage));
        }

        [Test]
        public void RemoveSubPackage_RemovesFromSubPackagesAndTestFiles()
        {
            // 1. Arrange
            string fullPath1 = Path.GetFullPath("Test1.dll");
            string fullPath2 = Path.GetFullPath("Test2.dll");
            string fullPath3 = Path.GetFullPath("Test3.dll");

            TestCentricProject project = new TestCentricProject(new GuiOptions(fullPath1, fullPath2, fullPath3 ));
            var subPackageToRemove = project.TopLevelPackage.SubPackages[1];
            Assert.That(project.TopLevelPackage.SubPackages.Count, Is.EqualTo(3));
            Assert.That(project.TestFiles.Count, Is.EqualTo(3));
            Assert.That(project.TestFiles, Does.Contain(fullPath2));

            // 2. Act
            project.RemoveSubPackage(subPackageToRemove);

            // 3. Assert
            Assert.That(project.TopLevelPackage.SubPackages.Count, Is.EqualTo(2));
            Assert.That(project.TestFiles.Count, Is.EqualTo(2));
            Assert.That(project.TestFiles, Does.Not.Contain(fullPath2));
        }

        [Test]
        public void RemoveSubPackage_WithNullPackage_DoesNothing()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject(new GuiOptions("Test1.dll", "Test2.dll" ));
            var originalCount = project.TopLevelPackage.SubPackages.Count;

            // 2. Act
            project.RemoveSubPackage(null);

            // 3. Assert
            Assert.That(project.TopLevelPackage.SubPackages.Count, Is.EqualTo(originalCount));
        }

        #endregion

        #region Save and Load Tests

        [Test]
        public void SaveAs_CreatesFileWithProjectPath()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));

            // 2. Act
            project.SaveAs("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(File.Exists("TestCentricTestProject.tcproj"), Is.True);
            Assert.That(project.ProjectPath, Is.EqualTo("TestCentricTestProject.tcproj"));
        }

        [Test]
        public void Save_UpdatesExistingFile()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SaveAs("TestCentricTestProject.tcproj");
            
            // 2. Act
            project.SetTopLevelSetting(SettingDefinitions.MaxAgents.WithValue(5));
            project.Save();

            // 3. Assert
            Assert.That(File.Exists("TestCentricTestProject.tcproj"), Is.True);
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void Load_SavedProject_SettingsAreRestored()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SetTopLevelSetting(SettingDefinitions.DebugTests.WithValue(true));
            project.SaveAs("TestCentricTestProject.tcproj");

            // 2. Act
            TestCentricProject newProject = new TestCentricProject();
            newProject.Load("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(newProject.TopLevelPackage.Settings.HasSetting(SettingDefinitions.DebugTests), Is.True);
            var debugTests = newProject.TopLevelPackage.Settings.GetSetting(SettingDefinitions.DebugTests.Name);
            Assert.That(debugTests, Is.True);
        }

        [Test]
        public void Load_SavedProjectWithSubPackages_RestoresSubPackages()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject(new GuiOptions("Test1.dll", "Test2.dll"));
            project.SaveAs("TestCentricTestProject.tcproj");

            TestCentricProject loadedProject = new TestCentricProject();

            // 2. Act
            loadedProject.Load("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(loadedProject.TopLevelPackage.SubPackages.Count, Is.EqualTo(2));
        }

        [Test]
        public void Load_SetsProjectPath()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();
            project.SaveAs("TestCentricTestProject.tcproj");

            TestCentricProject loadedProject = new TestCentricProject();

            // 2. Act
            loadedProject.Load("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(loadedProject.ProjectPath, Is.EqualTo("TestCentricTestProject.tcproj"));
        }

        [Test]
        public void Load_SetsTestFiles()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject(new GuiOptions("Test1.dll", "Test2.dll"));
            project.SaveAs("TestCentricTestProject.tcproj");

            TestCentricProject loadedProject = new TestCentricProject();

            // 2. Act
            loadedProject.Load("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(loadedProject.TestFiles.Count, Is.EqualTo(2));
            Assert.That(loadedProject.TestFiles[0], Does.EndWith("Test1.dll"));
            Assert.That(loadedProject.TestFiles[1], Does.Contain("Test2.dll"));
        }

        #endregion

        #region FileName and ProjectPath Tests

        [Test]
        public void ProjectPath_BeforeSave_IsNull()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act & Assert
            Assert.That(project.ProjectPath, Is.Null);
        }

        [Test]
        public void ProjectPath_AfterSaveAs_IsSet()
        {
            // 1. Arrange
            TestCentricProject project = new TestCentricProject();

            // 2. Act
            project.SaveAs("TestCentricTestProject.tcproj");

            // 3. Assert
            Assert.That(project.ProjectPath, Is.EqualTo("TestCentricTestProject.tcproj"));
        }

        #endregion

        #region Options Integration Tests

        [Test]
        public void Constructor_WithOptions_AppliesWorkDirectory()
        {
            // 1. Arrange
            var options = new GuiOptions(new[] { "test.dll", "--work=C:\\WorkDir" });
            _model.Options.Returns(options);

            // 2. Act
            TestCentricProject project = new TestCentricProject(options);

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.WorkDirectory.Name), Is.True);
            Assert.That(project.TopLevelPackage.Settings.GetSetting(SettingDefinitions.WorkDirectory.Name), Is.EqualTo("C:\\WorkDir"));
        }

        [Test]
        public void Constructor_WithOptions_AppliesMaxAgents()
        {
            // 1. Arrange
            var options = new GuiOptions(new[] { "test.dll", "--agents=5" });
            _model.Options.Returns(options);

            // 2. Act
            TestCentricProject project = new TestCentricProject(options);

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.MaxAgents.Name), Is.True);
            Assert.That(project.TopLevelPackage.Settings.GetSetting(SettingDefinitions.MaxAgents.Name), Is.EqualTo(5));
        }

        [Test]
        public void Constructor_WithOptions_AppliesRunAsX86()
        {
            // 1. Arrange
            var options = new GuiOptions(new[] { "test.dll", "--x86" });
            _model.Options.Returns(options);

            // 2. Act
            TestCentricProject project = new TestCentricProject(options);

            // 3. Assert
            Assert.That(project.TopLevelPackage.Settings.HasSetting(SettingDefinitions.RunAsX86.Name), Is.True);
            Assert.That(project.TopLevelPackage.Settings.GetSetting(SettingDefinitions.RunAsX86.Name), Is.EqualTo(true));
        }

        #endregion
    }
}
