// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Drawing;

namespace TestCentric.Gui.Presenters
{
    using System.Collections.Generic;
    using System.Drawing.Drawing2D;
    using Model;
    using Views;

    public class TextOutputPresenter
    {
        static readonly Color OutputColor = Color.Black;

        ITextOutputView _view;
        ITestModel _model;

        private bool _displayBeforeOutput = true;
        private bool _displayBeforeTest = false;
        private bool _displayAfterTest = false;

        private string _currentLabel = null;
        private string _lastTestOutput = null;

        private List<Action> OutputActions = new List<Action>();

        private Action<TestNodeEventArgs> TestStartingAction { get; set; }
        private Action<TestResultEventArgs> TestFinishedAction { get; set; }
        private Action<TestResultEventArgs> SuiteFinishedAction { get; set; }
        private Action<TestOutputEventArgs> TestOutputAction { get; set; }

        bool _wantNewLine = false;

        #region Construction and Initialization

        public TextOutputPresenter(ITextOutputView view, ITestModel model)
        {
            _view = view;
            _model = model;

            Initialize();

            WireUpEvents();
        }

        private void Initialize(bool clearDisplay = false)
        {
            var labels = _model.Settings.Gui.TextOutput.Labels;

            _displayBeforeTest = labels == "ALL" || labels == "BEFORE" || labels == "BEFOREANDAFTER";
            _displayAfterTest = labels == "AFTER" || labels == "BEFOREANDAFTER";
            _displayBeforeOutput = _displayBeforeTest || _displayAfterTest || labels == "ON";

            _currentLabel = _lastTestOutput = null;
            _wantNewLine = false;

            if (clearDisplay)
                _view.Clear();
        }

        private void WireUpEvents()
        {
            TestStartingAction = (e) =>
            {
                if (_displayBeforeTest)
                    WriteLabelLine(e.Test.FullName, OutputColor);
            };

            TestFinishedAction = (e) =>
            {
                if (e.Result.Output != null)
                {
                    if (_displayBeforeOutput)
                        WriteLabelLine(e.Result.FullName, OutputColor);

                    WriteOutputLine(e.Result.FullName, e.Result.Output, OutputColor);
                }

                if (_displayAfterTest)
                    WriteLabelLineAfterTest(e.Result.FullName, e.Result.Outcome);
            };

            SuiteFinishedAction = (e) =>
            {
                if (e.Result.Output != null)
                {
                    if (_displayBeforeOutput)
                        WriteLabelLine(e.Result.FullName, OutputColor);

                    FlushNewLineIfNeeded();
                    WriteOutputLine(e.Result.FullName, e.Result.Output, OutputColor);
                }
            };

            TestOutputAction = (e) =>
            {
                if (_displayBeforeOutput && e.TestName != null)
                    WriteLabelLine(e.TestName, OutputColor);

                WriteOutputLine(e.TestName, e.Text,
                    e.Stream == "Error" ? Color.Red : Color.Black);
            };

            _model.Events.TestLoaded += ((TestNodeEventArgs e) =>
            {
                Initialize(true);
            });

            _model.Events.TestUnloaded += ((TestEventArgs e) =>
            {
                Initialize(true);
            });

            _model.Events.TestReloaded += (TestNodeEventArgs e) =>
            {
                Initialize(false);
            };

            _model.Events.RunStarting += (RunStartingEventArgs e) =>
            {
                Initialize(true);
            };

            _model.Events.TestStarting += (TestNodeEventArgs e) =>
            {
                OutputActions.Add(() => TestStartingAction(e));

                TestStartingAction(e);
            };

            _model.Events.TestFinished += (TestResultEventArgs e) =>
            {
                OutputActions.Add(() => TestFinishedAction(e));

                TestFinishedAction(e);
            };

            _model.Events.SuiteFinished += (TestResultEventArgs e) =>
            {
                OutputActions.Add(() => SuiteFinishedAction(e));

                SuiteFinishedAction(e);
            };

            _model.Events.TestOutput += (TestOutputEventArgs e) =>
            {
                OutputActions.Add(() => TestOutputAction(e));

                TestOutputAction(e);
            };

            //_model.Events.SelectedItemChanged += (e) =>
            //{
            //    var testNode = _model.ActiveTestItem as TestNode;
            //    _view.
            //}

            _view.Labels.SelectionChanged += () => ChangeLabelSetting(_view.Labels.SelectedItem);
        }

        private void ChangeLabelSetting(string labels)
        {
            _model.Settings.Gui.TextOutput.Labels = labels;
            Initialize(true);

            foreach (var action in OutputActions)
            {
                action();
            }
        }

        #endregion

        private void WriteLabelLine(string label, Color color)
        {
            if (label != _currentLabel)
            {
                FlushNewLineIfNeeded();
                _lastTestOutput = label;

                WriteLine($"=> {label}", color);

                _currentLabel = label;
            }
        }

        private void WriteOutputLine(string testName, string text, Color color)
        {
            if (_lastTestOutput != testName)
            {
                FlushNewLineIfNeeded();
                _lastTestOutput = testName;
            }

            _view.Write(text, color);

            // If the text we just wrote did not have a new line, flag that we should eventually emit one.
            if (!text.EndsWith("\n"))
            {
                _wantNewLine = true;
            }
        }

        private void WriteLabelLineAfterTest(string label, ResultState outcome)
        {
            FlushNewLineIfNeeded();
            _lastTestOutput = label;

            var status = outcome.Label;
            if (string.IsNullOrEmpty(status))
                status = outcome.Status.ToString();

            var labelColor =
                outcome.Status == TestStatus.Passed ? Color.Green :
                outcome.Status == TestStatus.Failed ? Color.Red :
                outcome.Status == TestStatus.Warning ? Color.DarkOrange : Color.Purple;

            WriteLine($"{status} => {label}", labelColor);

            _currentLabel = label;
        }

        public void WriteLine(string text)
        {
            _view.Write(text + Environment.NewLine);
        }

        public void WriteLine(string text, Color color)
        {
            _view.Write(text + Environment.NewLine, color);
        }

        private void FlushNewLineIfNeeded()
        {
            if (_wantNewLine)
            {
                _view.Write(Environment.NewLine);
                _wantNewLine = false;
            }
        }
    }
}
