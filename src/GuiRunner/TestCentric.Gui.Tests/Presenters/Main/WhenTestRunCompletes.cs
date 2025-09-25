// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.Main
{
    using System.Windows.Forms;
    using Model;

    public class WhenTestRunCompletes : MainPresenterTestBase
    {
        [SetUp]
        public void SimulateTestRunFinish()
        {
            ClearAllReceivedCalls();

            _model.HasTests.Returns(true);
            _model.HasResults.Returns(true);
            _model.ResultSummary.Returns(new ResultSummary() { FailureCount = 1 });
            _model.IsTestRunning.Returns(false);
            _model.SelectedTests.Returns(new TestSelection(new[] { new TestNode("<test-case id='1' />") }));
            _view.ResultTabs.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));

            var resultNode = new ResultNode("<test-run id='XXX' result='Failed' />");
            FireRunFinishedEvent(resultNode);
        }

        [TestCase("OpenProjectCommand", true)]
        [TestCase("SaveProjectCommand", true)]
        [TestCase("CloseProjectCommand", true)]
        [TestCase("AddTestFilesCommand", true)]
        [TestCase("ReloadTestsCommand", true)]
        [TestCase("RecentFilesMenu", true)]
        [TestCase("ExitCommand", true)]
        [TestCase("SaveResultsCommand", true)]
        [TestCase("TransformResultsCommand", true)]
        [TestCase("RunAllButton", true)]
        [TestCase("RunSelectedButton", true)]
        [TestCase("RerunButton", true)]
        [TestCase("RunFailedButton", true)]
        [TestCase("DisplayFormatButton", true)]
        [TestCase("RunParametersButton", true)]
        [TestCase("StopRunButton", false)]
        [TestCase("ForceStopButton", false)]
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

        [Test]
        public void TestResultTabIsDisplay()
        {
            _view.ResultTabs.Received().SelectedIndex = 1;
        }
    }
}
