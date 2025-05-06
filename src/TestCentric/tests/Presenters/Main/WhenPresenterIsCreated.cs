// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Elements;

namespace TestCentric.Gui.Presenters.Main
{
    public class WhenPresenterIsCreated : MainPresenterTestBase
    {
#if NYI // Add after implementation of project or package saving
        [TestCase("NewProjectCommand", true)]
        [TestCase("OpenProjectCommand", true)]
		[TestCase("SaveCommand", false)]
        [TestCase("SaveAsCommand", false)]
#endif

        [TestCase("NewProjectCommand", true)]
        [TestCase("OpenProjectCommand", true)]
        [TestCase("SaveProjectCommand", false)]
        [TestCase("CloseProjectCommand", false)]
        [TestCase("AddTestFilesCommand", false)]
        [TestCase("ReloadTestsCommand", false)]
        [TestCase("RecentFilesMenu", true)]
        [TestCase("ExitCommand", true)]
        [TestCase("RunAllButton", false)]
        [TestCase("RunFailedButton", false)]
        [TestCase("RerunButton", false)]
        [TestCase("RunSelectedButton", false)]
        [TestCase("DisplayFormatButton", false)]
        [TestCase("RunParametersButton", false)]
        [TestCase("StopRunButton", false)]
        [TestCase("ForceStopButton", false)]
        [TestCase("SaveResultsCommand", false)]
        public void CheckCommandEnabled(string propName, bool enabled)
        {
            ViewElement(propName).Received().Enabled = enabled;
        }

        [TestCase("StopRunButton", true)]
        [TestCase("ForceStopButton", false)]
        public void CheckCommandVisible(string propName, bool visible)
        {
            ViewElement(propName).Received().Visible = visible;
        }

        [TestCase("NUNIT_TREE")]
        [TestCase("FIXTURE_LIST")]
        [TestCase("TEST_LIST")]
        public void CheckMenu_DisplayFormat_SelectedItem_IsInitialzedFromSettings(string displayFormat)
        {
            // 1. Arrange
            _settings.Gui.TestTree.DisplayFormat = displayFormat;

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model, new CommandLineOptions());

            // 3. Assert
            _view.DisplayFormat.Received().SelectedItem = displayFormat;
        }

        [TestCase("UNGROUPED")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        [TestCase("DURATION")]
        public void CheckMenu_NUnitTreeGroupBy_SelectedItem_IsInitializedFromSettings(string groupBy)
        {
            // 1. Arrange
            _settings.Gui.TestTree.DisplayFormat = "NUNIT_TREE";
            _settings.Gui.TestTree.NUnitGroupBy = groupBy;

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model, new CommandLineOptions());

            // 3. Assert
            _view.NUnitGroupBy.Received().SelectedItem = groupBy;
        }

        [TestCase("ASSEMBLY")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        public void CheckMenu_FixtureListGroupBy_SelectedItem_FixtureList_IsInitialzedFromSettings(string groupBy)
        {
            // 1. Arrange
            _settings.Gui.TestTree.DisplayFormat = "FIXTURE_LIST";
            _settings.Gui.TestTree.FixtureList.GroupBy = groupBy;

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model, new CommandLineOptions());

            // 3. Assert
            _view.FixtureListGroupBy.Received().SelectedItem = groupBy;
        }

        [TestCase("ASSEMBLY")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        public void CheckMenu_TestListGroupBy_SelectedItem_TestList_IsInitialzedFromSettings(string groupBy)
        {
            // 1. Arrange
            _settings.Gui.TestTree.DisplayFormat = "TEST_LIST";
            _settings.Gui.TestTree.TestList.GroupBy = groupBy;

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model, new CommandLineOptions());

            // 3. Assert
            _view.TestListGroupBy.Received().SelectedItem = groupBy;
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FilterButton_IsInitialzedFromSettings(bool filterIsVisible)
        {
            // 1. Arrange
            _settings.Gui.TestTree.ShowFilter = filterIsVisible;

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model, new CommandLineOptions());

            // 3. Assert
            _view.ShowHideFilterButton.Received().Checked = filterIsVisible;
        }

        [TestCase(true, "Run Checked Tests")]
        [TestCase(false, "Run Selected Tests")]
        public void FilterRunSelectedTestButton_Tooltip_IsUpdated(bool checkBoxVisible, string expectedTooltip)
        {
            // 1. Arrange
            ICommand runSelectedTestsButton = Substitute.For<ICommand, IToolTip>();
            _view.RunSelectedButton.Returns(runSelectedTestsButton);
            _view.TreeView.ShowCheckBoxes.Checked.Returns(checkBoxVisible);

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model, new CommandLineOptions());

            // 3. Assert
            (runSelectedTestsButton as IToolTip).Received().ToolTipText = expectedTooltip;
        }
    }
}
