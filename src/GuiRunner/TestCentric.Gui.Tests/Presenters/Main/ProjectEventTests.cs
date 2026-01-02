// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Elements;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.Main
{
    using NUnit.Common;

    public class ProjectEventTests : MainPresenterTestBase
    {
        [Test]
        public void WhenProjectIsCreated_TitleBarIsSet()
        {
            var project = new TestCentricProject(_model, "dummy.dll");
            _model.TestCentricProject.Returns(project);

            FireProjectLoadedEvent();

            _view.Received().Title = "TestCentric - UNNAMED.tcproj";
        }

        [Test]
        public void WhenProjectIsClosed_TitleBarIsSet()
        {
            FireProjectUnloadedEvent();

            _view.Received().Title = "TestCentric Runner for NUnit"; 
        }


        [Test]
        public void WhenProjectIsSaved_TitleBarIsSet()
        {
            // Arrange
            var project = new TestCentricProject(_model, "dummy.dll");
            _model.TestCentricProject.Returns(project);
            _view.DialogManager.GetFileSavePath(null, null, null, null).ReturnsForAnyArgs("TestCentric.tcproj");

            // Act
            project.SaveAs("TestCentric.tcproj");
            _view.SaveProjectCommand.Execute += Raise.Event<CommandHandler>();

            // Assert
            _model.Received().SaveProject("TestCentric.tcproj");
            _view.Received().Title = "TestCentric - TestCentric.tcproj";
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]

        public void WhenProjectIsLoaded_RunAsX86Command_IsUpdatedFromProjectSetting(bool runAsX86)
        {
            var project = new TestCentricProject(_model, "dummy.dll");
            project.SetTopLevelSetting(SettingDefinitions.RunAsX86.WithValue(runAsX86));
            _model.TestCentricProject.Returns(project);

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
