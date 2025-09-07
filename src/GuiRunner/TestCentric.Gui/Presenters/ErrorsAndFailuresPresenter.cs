// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Model.Settings;
    using Views;

    public class ErrorsAndFailuresPresenter
    {
        private IErrorsAndFailuresView _view;
        private ITestModel _model;
        private UserSettings _settings;

        private ITestItem _selectedItem;
        private ResultNode _selectedResult;
        private ITestResultSubViewPresenter _testSubViewPresenter;

        public ErrorsAndFailuresPresenter(IErrorsAndFailuresView view, ITestModel model) :
            this(view, model, new TestResultSubViewPresenter(view.TestResultSubView, model))
        {
        }

        public ErrorsAndFailuresPresenter(IErrorsAndFailuresView view, ITestModel model, ITestResultSubViewPresenter testResultSubViewPresenter)
        {
            _view = view;
            _model = model;
            _settings = model.Settings;

            _view.SetFixedFont(_settings.Gui.FixedFont);
            _view.SplitterPosition = _settings.Gui.ErrorDisplay.SplitterPosition;
            _view.EnableToolTips = _settings.Gui.ErrorDisplay.ToolTipsEnabled;

            var orientation = _settings.Gui.ErrorDisplay.SourceCodeSplitterOrientation;
            _view.SourceCodeSplitOrientation = orientation;
            _view.SourceCodeSplitterDistance = orientation == Orientation.Vertical
                ? _settings.Gui.ErrorDisplay.SourceCodeVerticalSplitterPosition
                : _settings.Gui.ErrorDisplay.SourceCodeHorizontalSplitterPosition;

            _view.SourceCodeDisplay = _settings.Gui.ErrorDisplay.SourceCodeDisplay;

            _testSubViewPresenter = testResultSubViewPresenter;

            WireUpEvents();
        }

        private void WireUpEvents()
        {
            // Events that arise in the model

            _model.Events.TestLoaded += (e) => ClearDisplay();
            _model.Events.TestUnloaded += (e) => ClearDisplay();
            _model.Events.TestReloaded += (e) =>
            {
                if (_settings.Gui.ClearResultsOnReload)
                    ClearDisplay();
            };

            _model.Events.RunStarting += (e) => ClearDisplay();

            _model.Events.TestFinished += (TestResultEventArgs e) => OnNewResult(e.Result);
            _model.Events.SuiteFinished += (TestResultEventArgs e) => OnNewResult(e.Result);
            _model.Events.RunFinished += (TestResultEventArgs e) =>
            {
                if (_selectedItem is TestGroup)
                    InitializeTestResultSubView();
            };

            _model.Events.SelectedItemChanged += (TestItemEventArgs e) =>
            {
                _selectedItem = e.TestItem;
                var testNode = e.TestItem as TestNode;
                _selectedResult = testNode == null ? null : _model.TestResultManager.GetResultForTest(testNode.Id);

                _view.Header = e.TestItem.Name;
                UpdateDisplay();
            };

            _model.Settings.Changed += (object sender, SettingsEventArgs e) =>
            {
                if (e.SettingName == "TestCentric.Gui.FixedFont")
                    _view.SetFixedFont(_settings.Gui.FixedFont);
            };

            // Events that arise in the view

            _view.SplitterPositionChanged += (object sender, EventArgs e) =>
            {
                _model.Settings.Gui.ErrorDisplay.SplitterPosition = _view.SplitterPosition;
            };

            _view.SourceCodeSplitOrientationChanged += (object sender, EventArgs e) =>
            {
                _settings.Gui.ErrorDisplay.SourceCodeSplitterOrientation = _view.SourceCodeSplitOrientation;
            };

            _view.SourceCodeSplitterDistanceChanged += (object sender, EventArgs e) =>
            {
                if (_view.SourceCodeSplitOrientation == Orientation.Vertical)
                    _settings.Gui.ErrorDisplay.SourceCodeVerticalSplitterPosition = _view.SourceCodeSplitterDistance;
                else
                    _settings.Gui.ErrorDisplay.SourceCodeHorizontalSplitterPosition = _view.SourceCodeSplitterDistance;
            };

            _view.SourceCodeDisplayChanged += (object sender, EventArgs e) =>
            {
                _settings.Gui.ErrorDisplay.SourceCodeDisplay = _view.SourceCodeDisplay;
            };
        }

        private void OnNewResult(ResultNode resultNode)
        {
            if (resultNode.Name == _selectedItem?.Name)
            {
                _selectedResult = resultNode;
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            ClearDisplay();

            if (_selectedResult != null &&
                (_selectedResult.Status == TestStatus.Failed || _selectedResult.Status == TestStatus.Warning) &&
                _selectedResult.Site != FailureSite.Parent)
            {
                string status = _selectedResult.Outcome.Label;
                if (string.IsNullOrEmpty(status))
                    status = _selectedResult.Status.ToString();
                string testName = _selectedResult.FullName;

                if (_selectedResult.IsSuite && _selectedResult.Site == FailureSite.SetUp)
                    testName += " (TestFixtureSetUp)";

                if (_selectedResult.Assertions.Count > 0)
                    foreach (var assertion in _selectedResult.Assertions)
                        AddResult(testName, assertion);
                else if (_selectedResult.TestCount == 1)
                    // Add result to detail list only for single test cases: avoid displaying general message "One or more child tests had errors"
                    AddResult(_selectedResult);
            }

            InitializeTestResultSubView();
            InitializeTestOutputSubView();
        }

        private void ClearDisplay()
        {
            _view.Clear();
            _testSubViewPresenter.Clear();
        }

        private void AddResult(string testName, AssertionResult assertion)
        {
            string status = assertion.Status;
            if (string.IsNullOrEmpty(status))
                status = GetStatusDisplay(_selectedResult);
            _view.AddResult(status, testName, IndentMessage(assertion.Message), assertion.StackTrace);
        }

        private void AddResult(ResultNode resultNode)
        {
            _view.AddResult(GetStatusDisplay(resultNode), resultNode.FullName, IndentMessage(resultNode.Message), resultNode.StackTrace);
        }

        private string GetStatusDisplay(ResultNode resultNode)
        {
            string status = _selectedResult.Outcome.Label;
            if (string.IsNullOrEmpty(status))
                status = _selectedResult.Status.ToString();
            return status;
        }

        private string IndentMessage(string message)
        {
            return message == null ? null : message.StartsWith("  ") ? message : "  " + message;
        }

        private void InitializeTestResultSubView()
        {
            _testSubViewPresenter.Update(_selectedItem);
        }

        private void InitializeTestOutputSubView()
        {
            string testOutput = (_selectedResult != null) ? _selectedResult.Xml.SelectSingleNode("output")?.InnerText : "";
            _view.TestOutputSubView.Output = testOutput;
            _view.TestOutputSubView.SetVisibility(!string.IsNullOrEmpty(testOutput));
        }
    }
}
