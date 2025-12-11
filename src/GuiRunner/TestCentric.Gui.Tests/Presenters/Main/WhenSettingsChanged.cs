// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Elements;

namespace TestCentric.Gui.Presenters.Main
{
    using TestCentric.Gui.Model.Settings;

    internal class WhenSettingsChanged : MainPresenterTestBase
    {
        [TestCase("NUNIT_TREE")]
        [TestCase("TEST_LIST")]
        public void DisplayFormat_SettingChanged_MenuItemIsUpdated(string displayFormat)
        {
            // 1. Act
            _settings.Gui.TestTree.DisplayFormat.Returns(displayFormat);
            _settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.DisplayFormat"));

            // 2. Assert
            Assert.That(_view.DisplayFormat.SelectedItem, Is.EqualTo(displayFormat));
        }

        [TestCase("UNGROUPED")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        [TestCase("DURATION")]
        public void NUnitTreeGroupBy_SettingChanged_MenuItemIsUpdated(string groupBy)
        {
            // 1. Act
            _settings.Gui.TestTree.NUnitGroupBy.Returns(groupBy);
            _settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.NUnitGroupBy"));

            // 2. Assert
            Assert.That(_view.NUnitGroupBy.SelectedItem, Is.EqualTo(groupBy));
        }

        [TestCase("ASSEMBLY")]
        [TestCase("CATEGORY")]
        [TestCase("OUTCOME")]
        public void TestListGroupBy_SettingChanged_MenuItemIsUpdated(string groupBy)
        {
            // 1. Act
            _settings.Gui.TestTree.TestList.GroupBy.Returns(groupBy);
            _settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.TestList.GroupBy"));

            // 2. Assert
            Assert.That(_view.TestListGroupBy.SelectedItem, Is.EqualTo(groupBy));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ShowNamespace_SettingChanged_MenuItemIsUpdated(bool showNamespace)
        {
            // 1. Act
            _settings.Gui.TestTree.ShowNamespace.Returns(showNamespace);
            _settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.ShowNamespace"));

            // 2. Assert
            Assert.That(_view.ShowNamespace.Checked, Is.EqualTo(showNamespace));
        }

        [TestCase("NUNIT_TREE", true)]
        [TestCase("TEST_LIST", false)]
        public void DisplayFormat_SettingChanged_ShowHideFilterButton_IsUpdated(string displayFormat, bool expectedState)
        {
            // 1. Arrange
            _model.HasTests.Returns(true);

            // 2. Act
            _settings.Gui.TestTree.DisplayFormat.Returns(displayFormat);
            _settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.DisplayFormat"));

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
