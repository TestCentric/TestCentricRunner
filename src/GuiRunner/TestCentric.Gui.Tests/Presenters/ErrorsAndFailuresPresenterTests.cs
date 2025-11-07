// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;
using System.Windows.Forms;
using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using TestCentric.Gui.Model.Settings;
    using Views;

    public class ErrorsAndFailuresPresenterTests : PresenterTestBase<IErrorsAndFailuresView>
    {
        private static readonly TestNode FAKE_TEST_RUN = new TestNode("<test-suite id='1' testcasecount='1234' />");
        ITestResultSubViewPresenter _testResultPresenter;

        [SetUp]
        public void CreatePresenter()
        {
            _testResultPresenter = Substitute.For<ITestResultSubViewPresenter>();
            new ErrorsAndFailuresPresenter(_view, _model, _testResultPresenter);
        }

        [Test]
        public void WhenTestIsLoaded_DisplayIsCleared()
        {
            FireTestLoadedEvent(FAKE_TEST_RUN);

            _view.Received().Clear();
        }

        [Test]
        public void WhenTestIsReloaded_IfClearResultsIsFalse_DisplayIsNotCleared()
        {
            _settings.Gui.ClearResultsOnReload = false;

            FireTestReloadedEvent(FAKE_TEST_RUN);

            _view.DidNotReceive().Clear();
        }

        [Test]
        public void WhenTestIsReloaded_IfClearResultsIsTrue_DisplayIsCleared()
        {
            _settings.Gui.ClearResultsOnReload = true;

            FireTestReloadedEvent(FAKE_TEST_RUN);

            _view.Received().Clear();
        }

        [Test]
        public void WhenTestIsUnloaded_DisplayIsCleared()
        {
            FireTestUnloadedEvent();

            _view.Received().Clear();
        }

        [Test]
        public void WhenTestRunStarts_DisplayIsCleared()
        {
            FireRunStartingEvent(1234);

            _view.Received().Clear();
        }

        [TestCase("Passed", FailureSite.Test, false)]
        [TestCase("Failed", FailureSite.Test, true)]
        [TestCase("Failed", FailureSite.SetUp, true)]
        [TestCase("Failed", FailureSite.TearDown, true)]
        [TestCase("Failed", FailureSite.Parent, false)]
        [TestCase("Failed:Error", FailureSite.Test, true)]
        [TestCase("Failed:Invalid", FailureSite.Test, true)]
        [TestCase("Failed:Cancelled", FailureSite.Test, true)]
        [TestCase("Warning", FailureSite.Test, true)]
        [TestCase("Warning", FailureSite.SetUp, true)]
        [TestCase("Warning", FailureSite.TearDown, true)]
        [TestCase("Warning", FailureSite.Parent, false)]
        [TestCase("Skipped", FailureSite.Test, false)]
        [TestCase("Skipped:Ignored", FailureSite.Test, false)]
        [TestCase("Inconclusive", FailureSite.Test, false)]
        public void WhenTestCaseFinishes_FailuresAndErrorsAreDisplayed(string resultState, FailureSite site, bool shouldDisplay)
        {
            FireTestFinishedEvent("MyTest", resultState, site);

            VerifyDisplay(shouldDisplay);
        }

        [TestCase("Passed", FailureSite.Test, false)]
        [TestCase("Failed", FailureSite.Parent, false)]
        [TestCase("Failed", FailureSite.SetUp, true)]
        [TestCase("Failed", FailureSite.TearDown, true)]
        [TestCase("Failed", FailureSite.Child, true)]
        [TestCase("Failed", FailureSite.Test, true)]
        [TestCase("Failed:Error", FailureSite.Test, true)]
        [TestCase("Failed:Invalid", FailureSite.Test, true)]
        [TestCase("Failed:Cancelled", FailureSite.Test, true)]
        [TestCase("Warning", FailureSite.Test, true)]
        [TestCase("Warning", FailureSite.SetUp, true)]
        [TestCase("Warning", FailureSite.TearDown, true)]
        [TestCase("Warning", FailureSite.Parent, false)]
        [TestCase("Warning", FailureSite.Child, true)]
        [TestCase("Skipped", FailureSite.Test, false)]
        [TestCase("Skipped:Ignored", FailureSite.Test, false)]
        [TestCase("Inconclusive", FailureSite.Test, false)]
        public void WhenTestSuiteFinishes_FailuresAndErrorsAreDisplayed(string resultState, FailureSite site, bool shouldDisplay)
        {
            FireSuiteFinishedEvent("MyTest", resultState, site, 1);

            VerifyDisplay(shouldDisplay);
        }

        [TestCase(1, true)]
        [TestCase(2, false)]
        public void WhenTestSuiteFinishes_WithFailures_TestCount_FailuresAndErrorsAreDisplayed(int testCount, bool shouldDisplay)
        {
            FireSuiteFinishedEvent("MyTest", "Failed", FailureSite.Test, testCount);

            VerifyDisplay(shouldDisplay);
        }

        [Test]
        public void WhenPresenterIsCreated_FontIsSetToDefault()
        {
            var font = _settings.Gui.FixedFont;
            _view.Received().SetFixedFont(font);
        }

        [Test]
        public void WhenPresenterIsCreated_SplitterPositionIsSet()
        {
            int split = _settings.Gui.ErrorDisplay.SplitterPosition;
            _view.Received().SplitterPosition = split;
        }

        [Test]
        public void WhenPresenterIsCreated_SourceCodeSplitterDistanceIsSet()
        {
            var orientation = _settings.Gui.ErrorDisplay.SourceCodeSplitterOrientation;
            var distance = orientation == Orientation.Vertical
                ? _settings.Gui.ErrorDisplay.SourceCodeVerticalSplitterPosition
                : _settings.Gui.ErrorDisplay.SourceCodeHorizontalSplitterPosition;
            _view.Received().SourceCodeSplitterDistance = distance;
        }

        [Test]
        public void WhenPresenterIsCreated_SourceCodeSplitOrientationIsSet()
        {
            var orientation = _settings.Gui.ErrorDisplay.SourceCodeSplitterOrientation;
            _view.Received().SourceCodeSplitOrientation = orientation;
        }

        [Test]
        public void WhenPresenterIsCreated_SourceCodeDisplayIsSet()
        {
            bool enabled = _settings.Gui.ErrorDisplay.SourceCodeDisplay;
            _view.Received().SourceCodeDisplay = enabled;
        }

        [Test]
        public void WhenPresenterIsCreated_EnableToolTipsIsSet()
        {
            bool enabled = _settings.Gui.ErrorDisplay.ToolTipsEnabled;
            _view.Received().EnableToolTips = enabled;
        }

        [Test]
        public void WhenFixedFontSettingChanges_ViewIsUpdated()
        {
            _view.ClearReceivedCalls();
            var newFont = new Font(FontFamily.GenericMonospace, 12.0f);
            _settings.Gui.FixedFont.Returns(newFont);
            _settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.FixedFont"));

            _view.Received().SetFixedFont(newFont);
        }

        [Test]
        public void WhenUserChangesSplitterPosition_SettingIsUpdated()
        {
            _view.SplitterPosition = 1234;
            _view.SplitterPositionChanged += Raise.Event<System.EventHandler>(this, new System.EventArgs());

            Assert.That(_settings.Gui.ErrorDisplay.SplitterPosition, Is.EqualTo(1234));
        }

        [Test]
        public void WhenUserChangesSourceCodeSplitOrientation_SettingIsUpdated()
        {
            var orientation = _settings.Gui.ErrorDisplay.SourceCodeSplitterOrientation == Orientation.Vertical
                ? Orientation.Horizontal
                : Orientation.Vertical;

            _view.SourceCodeSplitOrientation = orientation;
            _view.SourceCodeSplitOrientationChanged += Raise.Event<System.EventHandler>(this, new System.EventArgs());

            Assert.That(_settings.Gui.ErrorDisplay.SourceCodeSplitterOrientation, Is.EqualTo(orientation));
        }

        [Test]
        public void WhenUserChangesSourceCodeHorizontalSplitterDistance_SettingIsUpdated()
        {
            _view.SourceCodeSplitOrientation.Returns(Orientation.Horizontal);
            _view.SourceCodeSplitterDistance.Returns(0.35f);
            _view.SourceCodeSplitterDistanceChanged += Raise.Event<System.EventHandler>(this, new System.EventArgs());

            Assert.That(_settings.Gui.ErrorDisplay.SourceCodeHorizontalSplitterPosition, Is.EqualTo(0.35f));
        }

        [Test]
        public void WhenUserChangesSourceCodeVerticalSplitterDistance_SettingIsUpdated()
        {
            _view.SourceCodeSplitOrientation.Returns(Orientation.Vertical);
            _view.SourceCodeSplitterDistance.Returns(0.5f);
            _view.SourceCodeSplitterDistanceChanged += Raise.Event<System.EventHandler>(this, new System.EventArgs());

            Assert.That(_settings.Gui.ErrorDisplay.SourceCodeVerticalSplitterPosition, Is.EqualTo(0.5f));
        }

        [Test]
        public void WhenUserChangesSourceCodeDisplay_SettingIsUpdated()
        {
            bool originalSetting = _settings.Gui.ErrorDisplay.SourceCodeDisplay;
            bool newSetting = !originalSetting;

            _view.SourceCodeDisplay.Returns(newSetting);
            _view.SourceCodeDisplayChanged += Raise.Event<System.EventHandler>(this, new System.EventArgs());

            Assert.That(_settings.Gui.ErrorDisplay.SourceCodeDisplay, Is.EqualTo(newSetting));
        }

        [Test]
        public void WhenSelectedItemChanged_ToTestItem_TestResultSubView_IsUpdated()
        {
            ITestItem testItem = Substitute.For<ITestItem>();

            _model.Events.SelectedItemChanged += Raise.Event<TestItemEventHandler>(new TestItemEventArgs(testItem));

            _testResultPresenter.Received().Update(testItem);
        }

        [Test]
        public void WhenSelectedItemChanged_ToTestNode_TestResultSubView_IsUpdated()
        {
            TestNode testItem = FAKE_TEST_RUN;
            var resultNode = new ResultNode($"<test-case id='1'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            _model.Events.SelectedItemChanged += Raise.Event<TestItemEventHandler>(new TestItemEventArgs(testItem));

            _testResultPresenter.Received().Update(testItem);
        }

        [Test]
        public void WhenTestCaseFinishes_ForSelectedNode_TestResultSubView_IsUpdated()
        {
            // Arrange
            TestNode testItem = new TestNode("<test-case id='1' name='TestA' />");
            var resultNode = new ResultNode($"<test-case id='1' name='TestA' />");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);
            _model.Events.SelectedItemChanged += Raise.Event<TestItemEventHandler>(new TestItemEventArgs(testItem));
            _testResultPresenter.ClearReceivedCalls();

            // Act
            FireTestFinishedEvent(resultNode);

            // Assert
            _testResultPresenter.Received().Update(testItem);
        }

        [Test]
        public void WhenTestCaseFinishes_ForDifferentSelectedNode_TestResultSubView_IsNotUpdated()
        {
            // Arrange
            TestNode testItem = new TestNode("<test-case id='1' name='TestA' />");
            var resultNode = new ResultNode($"<test-case id='1' name='TestB' />");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);
            _model.Events.SelectedItemChanged += Raise.Event<TestItemEventHandler>(new TestItemEventArgs(testItem));
            _testResultPresenter.ClearReceivedCalls();

            // Act
            FireTestFinishedEvent(resultNode);

            // Assert
            _testResultPresenter.DidNotReceiveWithAnyArgs().Update(null);
        }

        [Test]
        public void WhenTestCaseFinishes_TestContainsOutput_OutputView_IsVisible()
        {
            var resultNode = new ResultNode($"<test-case id='1'> <output>Hello world</output> </test-case>");
            FireTestFinishedEvent(resultNode);

            _view.TestOutputSubView.Received().SetVisibility(true);
        }

        [Test]
        public void WhenTestCaseFinishes_TestContainsNoOutput_OutputView_IsHidden()
        {
            var resultNode = new ResultNode($"<test-case id='1'>  </test-case>");
            FireTestFinishedEvent(resultNode);

            _view.TestOutputSubView.Received().SetVisibility(false);
        }

        [Test]
        public void WhenTestCaseFinishes_TestContainsOutput_OutputView_ShowsOutput()
        {
            var resultNode = new ResultNode($"<test-case id='1'> <output>Hello world</output> </test-case>");
            FireTestFinishedEvent(resultNode);

            _view.TestOutputSubView.Received().Output = "Hello world";
        }

        [Test]
        public void WhenTestRunFinishes_SelectedNodeIsGroup_TestResultSubView_IsUpdated()
        {
            // Arrange
            TestNode testItem = new TestNode("<test-case id='1' name='TestA' />");
            var resultNode = new ResultNode($"<test-case id='1' name='TestA' />");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            TestGroup testGroup = new TestGroup("TestGroup") { testItem };
            _model.Events.SelectedItemChanged += Raise.Event<TestItemEventHandler>(new TestItemEventArgs(testGroup));
            _testResultPresenter.ClearReceivedCalls();

            // Act
            FireRunFinishedEvent(resultNode);

            // Assert
            _testResultPresenter.Received().Update(testGroup);
        }

        [Test]
        public void WhenTestRunFinishes_SelectedNodeIsTestNode_TestResultSubView_IsNotUpdated()
        {
            // Arrange
            TestNode testItem = new TestNode("<test-case id='1' name='TestA' />");
            var resultNode = new ResultNode($"<test-case id='1' name='TestA' />");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);
            _model.Events.SelectedItemChanged += Raise.Event<TestItemEventHandler>(new TestItemEventArgs(testItem));
            _testResultPresenter.ClearReceivedCalls();

            // Act
            FireRunFinishedEvent(resultNode);

            // Assert
            _testResultPresenter.DidNotReceiveWithAnyArgs().Update(testItem);
        }



        private void VerifyDisplay(bool shouldDisplay)
        {
            // NOTE: We only verify that something was sent, not the content
            if (shouldDisplay)
                _view.Received().AddResult(Arg.Compat.Any<string>(), Arg.Compat.Any<string>(), Arg.Compat.Any<string>(), Arg.Compat.Any<string>());
            else
                _view.DidNotReceiveWithAnyArgs().AddResult(null, null, null, null);
        }
    }
}
