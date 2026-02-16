// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Elements;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.Main
{
    using NUnit.Common;

    [TestFixture("dummy.dll")]
    [TestFixture("test1.dll", "test2.dll")]
    public class ProjectEventTests : MainPresenterTestBase
    {
        private string[] _testFiles;
        private TestCentricProject _project;

        private const string DEFAULT_TITLE_BAR = "TestCentric Runner for NUnit";

        public ProjectEventTests(params string[] testFiles)
        {
            _testFiles = testFiles;
        }

        [SetUp]
        public void CreateProject()
        {
            _project = new TestCentricProject("MyProject", _testFiles);
            _model.TestCentricProject.Returns(_project);
            _model.TopLevelPackage.Returns(_project.TopLevelPackage);
        }

        [Test]
        public void WhenProjectIsCreated_TitleBarIsSetToDefault()
        {
            FireProjectLoadedEvent();

            _view.Received().Title = "TestCentric - MyProject";
        }

        [Test]
        public void WhenProjectIsClosed_TitleBarIsSetToDefault()
        {
            FireProjectUnloadedEvent();

            _view.Received().Title = DEFAULT_TITLE_BAR; 
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WhenProjectIsLoaded_RunAsX86Command_IsUpdatedFromProjectSetting(bool runAsX86)
        {
            _project.SetTopLevelSetting(SettingDefinitions.RunAsX86.WithValue(runAsX86));

            FireProjectLoadedEvent();

            _view.RunAsX86.Received().Checked = runAsX86;
        }

        [Test]
        public void WhenTestAssemblyChanged_ReloadTests()
        {
            // Act
            FireTestAssemblyChangedEvent();

            // Assert
            _model.Received().ReloadTests();
        }
    }
}
