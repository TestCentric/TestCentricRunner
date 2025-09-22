// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.Main
{
    using Model;

    public class WhenTestsAreReloaded : MainPresenterTestBase
    {
        [SetUp]
        public void SimulateTestReload()
        {
            ClearAllReceivedCalls();

            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(false);

            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.SelectedTests.Returns(new TestSelection(new[] { testNode }));

            FireTestReloadedEvent(testNode);
        }

        [TestCase("OpenProjectCommand", true)]
        [TestCase("SaveProjectCommand", true)]

        [TestCase("CloseProjectCommand", true)]
        [TestCase("AddTestFilesCommand", true)]
        [TestCase("ReloadTestsCommand", true)]
        [TestCase("RecentFilesMenu", true)]
        [TestCase("ExitCommand", true)]
        [TestCase("SaveResultsCommand", false)]
        [TestCase("TransformResultsCommand", false)]
        [TestCase("RunAllButton", true)]
        [TestCase("RunSelectedButton", true)]
        [TestCase("RerunButton", false)]
        [TestCase("RunFailedButton", false)]
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
        public void CheckElementVisibility(string propName, bool visible)
        {
            ViewElement(propName).Received().Visible = visible;
        }
    }
}
