// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.Main
{
    using Elements;
    using Views;
    using Model;
    using NSubstitute.Core.Arguments;
    using System.Windows.Forms;

    public class CommandTests : MainPresenterTestBase
    {
        private static string[] NO_FILES_SELECTED = new string[0];
        private static string NO_FILE_PATH = null;

        // TODO: Because the presenter opens dialogs for these commands,
        // they can't be tested directly. This could be fixed if the
        // presenter asked the view to open dialogs.

        //[Test]
        //public void NewProjectCommand_CallsNewProject()
        //{
        //    View.NewProjectCommand.Execute += Raise.Event<CommandHandler>();
        //    // This is NYI, change when we implement it
        //    Model.DidNotReceive().NewProject();
        //}

        [TestCase(false, false, "Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        [TestCase(true, false, "Projects & Assemblies (*.nunit,*.dll,*.exe)|*.nunit;*.dll;*.exe|NUnit Projects (*.nunit)|*.nunit|Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        [TestCase(false, true, "Projects & Assemblies (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln,*.dll,*.exe)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln;*.dll;*.exe|Visual Studio Projects (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln|Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        [TestCase(true, true, "Projects & Assemblies (*.nunit,*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln,*.dll,*.exe)|*.nunit;*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln;*.dll;*.exe|NUnit Projects (*.nunit)|*.nunit|Visual Studio Projects (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln|Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        public void OpenCommand_DisplaysDialogCorrectly(bool nunitSupport, bool vsSupport, string filter)
        {
            // Return no files so model is not called
            _view.DialogManager.SelectMultipleFiles(null, null).ReturnsForAnyArgs(NO_FILES_SELECTED);
            _model.NUnitProjectSupport.Returns(nunitSupport);
            _model.VisualStudioSupport.Returns(vsSupport);

            _view.NewProjectCommand.Execute += Raise.Event<CommandHandler>();

            _view.DialogManager.Received().SelectMultipleFiles("New Project", filter);
        }

        [Test]
        public void OpenCommand_FileSelected_LoadsTests()
        {
            var files = new string[] { Path.GetFullPath("/path/to/test.dll") };
            _view.DialogManager.SelectMultipleFiles(null, null).ReturnsForAnyArgs(files);

            _view.NewProjectCommand.Execute += Raise.Event<CommandHandler>();

            _model.Received().CreateNewProject();
            _model.Received().AddTests(files);
        }

        [Test]
        public void NewProjectCommand_NoFileSelected_DoesNotCreateProject()
        {
            _view.DialogManager.SelectMultipleFiles(null, null).ReturnsForAnyArgs(NO_FILES_SELECTED);

            _view.NewProjectCommand.Execute += Raise.Event<CommandHandler>();

            _model.DidNotReceiveWithAnyArgs().CreateNewProject(null);
        }

        [TestCase(false, false, "Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        [TestCase(true, false, "Projects & Assemblies (*.nunit,*.dll,*.exe)|*.nunit;*.dll;*.exe|NUnit Projects (*.nunit)|*.nunit|Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        [TestCase(false, true, "Projects & Assemblies (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln,*.dll,*.exe)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln;*.dll;*.exe|Visual Studio Projects (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln|Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        [TestCase(true, true, "Projects & Assemblies (*.nunit,*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln,*.dll,*.exe)|*.nunit;*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln;*.dll;*.exe|NUnit Projects (*.nunit)|*.nunit|Visual Studio Projects (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln|Assemblies (*.dll,*.exe)|*.dll;*.exe|All Files (*.*)|*.*")]
        public void AddTestFilesCommand_DisplaysDialogCorrectly(bool nunitSupport, bool vsSupport, string filter)
        {
            // Return no files so model is not called
            _view.DialogManager.SelectMultipleFiles(null, null).ReturnsForAnyArgs(NO_FILES_SELECTED);
            _model.NUnitProjectSupport.Returns(nunitSupport);
            _model.VisualStudioSupport.Returns(vsSupport);

            _view.AddTestFilesCommand.Execute += Raise.Event<CommandHandler>();

            _view.DialogManager.Received().SelectMultipleFiles("Add Test Files", filter);
        }

        [Test]
        public void AddTestFilesCommand_TellsModelToLoadTests()
        {
            var testFiles = new List<string>();
            testFiles.Add("FILE1");
            testFiles.Add("FILE2");
            var project = new TestCentricProject(_model, testFiles);
            _model.TestCentricProject.Returns(project);

            var filesToAdd = new string[] { Path.GetFullPath("/path/to/test.dll") };
            _view.DialogManager.SelectMultipleFiles(null, null).ReturnsForAnyArgs(filesToAdd);

            _view.AddTestFilesCommand.Execute += Raise.Event<CommandHandler>();

            _model.Received().AddTests(filesToAdd);
        }

        [Test]
        public void AddTestFilesCommand_WhenNothingIsSelected_DoesNotCreateProject()
        {
            _view.DialogManager.SelectMultipleFiles(null, null).ReturnsForAnyArgs(NO_FILES_SELECTED);

            _view.AddTestFilesCommand.Execute += Raise.Event<CommandHandler>();

            _model.DidNotReceiveWithAnyArgs().CreateNewProject(null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CloseProjectCommand_CallsCloseProject(bool dirty)
        {
            var project = new TestCentricProject(_model, "dummy.dll");
            if (dirty) project.AddSetting("SomeSetting", "VALUE");
            _model.TestCentricProject.Returns(project);
            _view.MessageDisplay.YesNo(Arg.Any<string>()).Returns(false);

            _view.CloseProjectCommand.Execute += Raise.Event<CommandHandler>();

            _model.Received().CloseProject();
        }

        //[Test]
        //public void SaveCommand_CallsSaveProject()
        //{
        //    _view.SaveCommand.Execute += Raise.Event<CommandHandler>();
        //    _model.Received().SaveProject();
        //}

        //[Test]
        //public void SaveAsCommand_CallsSaveProject()
        //{
        //    View.SaveAsCommand.Execute += Raise.Event<CommandHandler>();
        //    // This is NYI, change when we implement it
        //    Model.DidNotReceive().SaveProject();
        //}

        [Test]
        public void SaveResultsCommand_DisplaysDialogCorrectly()
        {
            // Return no file path so model is not called
            _view.DialogManager.GetFileSavePath(null, null, null, null).ReturnsForAnyArgs(NO_FILE_PATH);
            _model.WorkDirectory.Returns("WORKDIRECTORY");

            _view.SaveResultsCommand.Execute += Raise.Event<CommandHandler>();

            _view.DialogManager.Received().GetFileSavePath("Save Results in nunit3 format", "XML Files (*.xml)|*.xml|All Files (*.*)|*.*", "WORKDIRECTORY", "TestResult.xml");
        }

        [Test]
        public void SaveResultsCommand_FilePathSelected_SavesResults()
        {
            var savePath = Path.GetFullPath("/path/to/TestResult.xml");
            _view.DialogManager.GetFileSavePath(null, null, null, null).ReturnsForAnyArgs(savePath);

            _view.SaveResultsCommand.Execute += Raise.Event<CommandHandler>();

            _model.Received().SaveResults(savePath);
        }

        [Test]
        public void SaveResultsCommand_NoFilePathSelected_DoesNotSaveResults()
        {
            _view.DialogManager.GetFileSavePath(null, null, null, null).ReturnsForAnyArgs(NO_FILE_PATH);

            _view.SaveResultsCommand.Execute += Raise.Event<CommandHandler>();

            _model.DidNotReceiveWithAnyArgs().SaveResults(null);
        }

        [Test]
        public void ReloadTestsCommand_CallsReloadTests()
        {
            _view.ReloadTestsCommand.Execute += Raise.Event<CommandHandler>();
            _model.Received().ReloadTests();
        }

        public void SelectRuntimeCommand_PopsUpMenu()
        {
        }

        public void RecentProjectsMenu_PopsUpMenu()
        {
        }

        public void ExitCommand_CallsExit()
        {
        }

        [Test]
        public void ChangeFontCommand_DisplaysFontDialog()
        {
            Font currentFont = _settings.Gui.Font = new Font(FontFamily.GenericSansSerif, 12.0f);
            // Return same font to avoid setting the font
            _view.DialogManager.SelectFont(null).ReturnsForAnyArgs(currentFont);

            _view.ChangeFontCommand.Execute += Raise.Event<CommandHandler>();

            _view.DialogManager.Received().SelectFont(currentFont);
        }

        [Test]
        public void ChangeFontCommand_ChangesTheFont()
        {
            Font currentFont = _settings.Gui.Font = new Font(FontFamily.GenericSansSerif, 12.0f);
            Font newFont = new Font(FontFamily.GenericSerif, 16.0f);

            _view.DialogManager.SelectFont(null).ReturnsForAnyArgs(newFont);

            _view.ChangeFontCommand.Execute += Raise.Event<CommandHandler>();

            _view.Received().Font = newFont;
            Assert.That(_settings.Gui.Font, Is.EqualTo(newFont));
        }

        [Test]
        public void ApplyFontEvent_ChangesTheFont()
        {
            Font currentFont = _settings.Gui.Font = new Font(FontFamily.GenericSansSerif, 12.0f);
            Font newFont = new Font(FontFamily.GenericSerif, 16.0f);

            _view.DialogManager.ApplyFont += Raise.Event<ApplyFontHandler>(newFont);
            
            _view.Received().Font = newFont;
            Assert.That(_settings.Gui.Font, Is.EqualTo(newFont));
        }

        [Test]
        public void RunAllButton_RunsAllTests()
        {
            var loadedTests = new TestNode("<test-run id='ID' name='TOP' />");
            _model.LoadedTests.Returns(loadedTests);

            _view.RunAllButton.Execute += Raise.Event<CommandHandler>();

            _model.Received().RunTests(Arg.Is(loadedTests));
        }

        [Test]
        public void RunButton_RunsSelectedTests()
        {
            var testSelection = new TestSelection();
            _model.SelectedTests = testSelection;
            _view.RunSelectedButton.Execute += Raise.Event<CommandHandler>();
            _model.Received().RunTests(testSelection);
        }

        [Test]
        public void RerunButton_RerunsTests()
        {
            _view.RerunButton.Execute += Raise.Event<CommandHandler>();
            _model.Received().RepeatLastRun();
        }

        [Test]
        public void RunFailedButton_RunsFailedTests()
        {
            // TODO: Specify Results and test with specific argument
            _view.RunFailedButton.Execute += Raise.Event<CommandHandler>();
            _model.Received().RunTests(Arg.Any<TestSelection>());
        }

        //// TODO: No longer a setting, replace with change to VisualState
        //[Test]
        //public void DisplayFormatChange_ChangesModelSetting()
        //{
        //    _view.DisplayFormat.SelectedItem.Returns("TEST_LIST");
        //    _view.DisplayFormat.SelectionChanged += Raise.Event<CommandHandler>();

        //    // FakeSettings saves the setting so we can check if it was set
        //    var setting = (string)_model.Settings.GetSetting("Gui.TestTree.DisplayFormat");
        //    Assert.That(setting, Is.EqualTo("TEST_LIST"));
        //}

        [Test]
        public void GroupByChange_ChangesModelSetting()
        {
            _view.DisplayFormat.SelectedItem.Returns("TEST_LIST");
            _view.GroupBy.SelectedItem.Returns("OUTCOME");
            _view.GroupBy.SelectionChanged += Raise.Event<CommandHandler>();

            // FakeSettings saves the setting so we can check if it was set
            var setting = (string)_model.Settings.GetSetting("Gui.TestTree.TestList.GroupBy");
            Assert.That(setting, Is.EqualTo("OUTCOME"));
        }

        [Test]
        public void StopRunButton_StopsTests()
        {
            _view.StopRunButton.ClearReceivedCalls();
            _view.ForceStopButton.ClearReceivedCalls();
            _view.StopRunButton.Execute += Raise.Event<CommandHandler>();
            _model.Received().StopTestRun(false);
        }

        [Test]
        public void ForceStopButton_ForcesTestsToStop()
        {
            _view.ForceStopButton.Execute += Raise.Event<CommandHandler>();
            _model.Received().StopTestRun(true);
        }

        [Test]
        public void RunSelectedTestCommand_NoTestsSelected_IsDisabled()
        {
            // Arrange
            _model.HasTests.Returns(true);
            _model.SelectedTests = null;

            // Act + Assert
            Assert.That(_view.RunSelectedButton.Enabled, Is.False);
        }

        [Test]
        public void SelectedTestsChanged_NoTestSelected_CommandIsDisabled()
        {
            // Arrange
            _model.HasTests.Returns(true);
            _model.SelectedTests.Returns(new TestSelection());

            // Act
            _model.Events.SelectedTestsChanged += Raise.Event<TestSelectionEventHandler>(new TestSelectionEventArgs(null));

            // Assert
            Assert.That(_view.RunSelectedButton.Enabled, Is.False);
        }

        [Test]
        public void SelectedTestsChanged_TestSelected_CommandIsEnabled()
        {
            // Arrange
            _model.HasTests.Returns(true);
            _model.SelectedTests.Returns(new TestSelection(new[] { new TestNode("<test-case id='1' />") }));

            // Act
            _model.Events.SelectedTestsChanged += Raise.Event<TestSelectionEventHandler>(new TestSelectionEventArgs(null));

            // Assert
            Assert.That(_view.RunSelectedButton.Enabled, Is.True);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShowNamespaceChanged_ChangesModelSetting(bool showNamespace)
        {
            // Arrange
            _view.ShowNamespace.Checked .Returns(showNamespace);

            // Act
            _view.ShowNamespace.CheckedChanged += Raise.Event<CommandHandler>();

            // Assert
            Assert.That(_model.Settings.Gui.TestTree.ShowNamespace, Is.EqualTo(showNamespace));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShowFilterChanged_ChangesModelSetting(bool show)
        {
            // Arrange
            _view.ShowHideFilterButton.Checked.Returns(show);

            // Act
            _view.ShowHideFilterButton.CheckedChanged += Raise.Event<CommandHandler>();

            // Assert
            Assert.That(_model.Settings.Gui.TestTree.ShowFilter, Is.EqualTo(show));
        }

        [TestCase("NUNIT_TREE", true)]
        [TestCase("FIXTURE_List", false)]
        public void ShowFilterIsEnabled_ForDisplayFormat(string displayFormat, bool expectedIsEnabled)
        {
            // Arrange
            _model.HasTests.Returns(true);
            _view.DisplayFormat.SelectedItem = displayFormat;

            // Act
            _model.Events.SelectedTestsChanged += Raise.Event<TestSelectionEventHandler>(new TestSelectionEventArgs(null));

            // Assert
            Assert.That(_view.ShowHideFilterButton.Enabled, Is.EqualTo(expectedIsEnabled));
        }

        [TestCase("NUNIT_TREE", true)]
        [TestCase("FIXTURE_List", false)]
        public void ShowFilterIsVisible_ForDisplayFormat(string displayFormat, bool expectedIsVisible)
        {
            // Arrange
            _model.HasTests.Returns(true);
            _view.DisplayFormat.SelectedItem = displayFormat;

            // Act
            _model.Events.SelectedTestsChanged += Raise.Event<TestSelectionEventHandler>(new TestSelectionEventArgs(null));

            // Assert
            Assert.That(_view.ShowHideFilterButton.Visible, Is.EqualTo(expectedIsVisible));
        }
    }
}
