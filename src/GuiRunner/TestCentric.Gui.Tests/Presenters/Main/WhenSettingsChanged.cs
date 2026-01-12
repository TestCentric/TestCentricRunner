// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Elements;

namespace TestCentric.Gui.Presenters.Main
{
    using System.Runtime.InteropServices;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Model.Settings;

    internal class WhenSettingsChanged : MainPresenterTestBase
    {
        [TestCase("NUNIT_TREE")]
        [TestCase("TEST_LIST")]
        public void DisplayFormat_TreeConfigurationChanged_MenuItemIsUpdated(string displayFormat)
        {
            // 1. Arrange
            _model.TreeConfiguration.DisplayFormat.Returns(displayFormat);

            // 2. Act
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(null, new SettingsEventArgs(nameof(TreeConfiguration.DisplayFormat)));

            // 3. Assert
            Assert.That(_view.DisplayFormat.SelectedItem, Is.EqualTo(displayFormat));
        }

        [TestCase("UNGROUPED")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        [TestCase("DURATION")]
        public void NUnitTreeGroupBy_TreeConfigurationChanged_MenuItemIsUpdated(string groupBy)
        {
            // 1. Arrange
            _model.TreeConfiguration.DisplayFormat.Returns("NUNIT_TREE");
            _model.TreeConfiguration.NUnitGroupBy.Returns(groupBy);

            // 2. Act
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(null, new SettingsEventArgs(nameof(TreeConfiguration.NUnitGroupBy)));

            // 3. Assert
            Assert.That(_view.NUnitGroupBy.SelectedItem, Is.EqualTo(groupBy));
        }

        [TestCase("ASSEMBLY")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        public void TestListGroupBy_TreeConfigurationChanged_MenuItemIsUpdated(string groupBy)
        {
            // 1. Arrange
            _model.TreeConfiguration.DisplayFormat.Returns("TEST_LIST");
            _model.TreeConfiguration.TestListGroupBy.Returns(groupBy);

            // 2. Act
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(null, new SettingsEventArgs(nameof(TreeConfiguration.TestListGroupBy)));

            // 3. Assert
            Assert.That(_view.TestListGroupBy.SelectedItem, Is.EqualTo(groupBy));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShowNamespace_TreeConfigurationChanged_MenuItemIsUpdated(bool showNamespace)
        {
            // 1. Arrange
            _model.TreeConfiguration.ShowNamespaces.Returns(showNamespace);

            // 2. Act
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(null, new SettingsEventArgs(nameof(TreeConfiguration.ShowNamespaces)));

            // 2. Assert
            Assert.That(_view.ShowNamespace.Checked, Is.EqualTo(showNamespace));
        }

        [TestCase("NUNIT_TREE", true)]
        [TestCase("TEST_LIST", false)]
        public void DisplayFormat_TreeConfigurationChanged_ShowHideFilterButton_IsUpdated(string displayFormat, bool expectedState)
        {
            // 1. Arrange
            _model.HasTests.Returns(true);
            _model.TreeConfiguration.DisplayFormat.Returns(displayFormat);

            // 2. Act
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(null, new SettingsEventArgs(nameof(TreeConfiguration.DisplayFormat)));


            // 3. Assert
            Assert.That(_view.ShowHideFilterButton.Visible, Is.EqualTo(expectedState));
            Assert.That(_view.ShowHideFilterButton.Enabled, Is.EqualTo(expectedState));
        }

        [TestCase(true, "Run Checked Tests")]
        [TestCase(false, "Run Selected Tests")]
        public void ShowCheckBoxes_Changed_Tooltip_IsUpdated(bool checkBoxVisible, string expectedTooltip)
        {
            // 1. Arrange
            ICommand runSelectedTestsButton = Substitute.For<ICommand, IToolTip>();
            _view.RunSelectedButton.Returns(runSelectedTestsButton);
            _view.TreeView.ShowCheckBoxes.Checked.Returns(checkBoxVisible);

            // 2. Act
            _presenter = new TestCentricPresenter(_view, _model, new GuiOptions());
            _view.TreeView.ShowCheckBoxes.CheckedChanged += Raise.Event<CommandHandler>();

            // 3. Assert
            (runSelectedTestsButton as IToolTip).Received().ToolTipText = expectedTooltip;
        }
    }
}
