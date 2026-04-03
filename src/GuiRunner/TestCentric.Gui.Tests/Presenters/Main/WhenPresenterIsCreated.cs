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
        [TestCase("OpenProjectCommand", true)]
        [TestCase("SaveProjectCommand", false)]
        [TestCase("CloseProjectCommand", false)]
        [TestCase("EditProjectCommand", false)]
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
        [TestCase("TransformResultsCommand", false)]
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

        // NOTE: Tests below need to re-create the presenter, which was
        // created in MainPresenterTestBase, so that it will reflect different
        // values for user and package settings.

        [TestCase(true)]
        [TestCase(false)]
        public void FilterButton_IsInitializedFromSettings(bool filterIsVisible)
        {
            // 1. Arrange
            _settings.Gui.TestTree.ShowFilter = filterIsVisible;

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model);

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
            _presenter = new TestCentricPresenter(_view, _model);

            // 3. Assert
            (runSelectedTestsButton as IToolTip).Received().ToolTipText = expectedTooltip;
        }
    }
}
