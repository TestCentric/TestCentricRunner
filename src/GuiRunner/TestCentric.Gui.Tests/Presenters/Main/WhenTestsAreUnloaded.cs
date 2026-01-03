// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.Main
{
    public class WhenTestsAreUnloaded : MainPresenterTestBase
    {
        [SetUp]
        public void SimulateTestUnload()
        {
            ClearAllReceivedCalls();

            _model.HasTests.Returns(false);
            _model.IsTestRunning.Returns(false);
            FireTestUnloadedEvent();
        }

        [TestCase("OpenProjectCommand", true)]
        [TestCase("SaveProjectCommand", false)]

        [TestCase("CloseProjectCommand", false)]
        [TestCase("AddTestFilesCommand", false)]
        [TestCase("ReloadTestsCommand", false)]
        [TestCase("RecentFilesMenu", true)]
        [TestCase("ExitCommand", true)]
        [TestCase("SaveResultsCommand", false)]
        [TestCase("TransformResultsCommand", false)]
        [TestCase("RunAllButton", false)]
        [TestCase("RunSelectedButton", false)]
        [TestCase("RerunButton", false)]
        [TestCase("RunFailedButton", false)]
        [TestCase("DisplayFormatButton", false)]
        [TestCase("RunParametersButton", false)]
        [TestCase("StopRunButton", false)]
        [TestCase("ForceStopButton", false)]
        [TestCase("TestRunSettingsCommand", false)]
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
