// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.Main
{
    public class WhenTestRunBegins : MainPresenterTestBase
    {
        [SetUp]
        protected void SimulateTestRunStarting()
        {
            ClearAllReceivedCalls();

            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(true);
            FireRunStartingEvent(1234);
        }

        [TestCase("OpenProjectCommand", false)]
        [TestCase("SaveProjectCommand", false)]
        [TestCase("CloseProjectCommand", false)]
        [TestCase("AddTestFilesCommand", false)]
        [TestCase("ReloadTestsCommand", false)]
        [TestCase("RecentFilesMenu", false)]
        [TestCase("ExitCommand", true)]
        [TestCase("SaveResultsCommand", false)]
        [TestCase("TransformResultsCommand", false)]
        [TestCase("RunAllButton", false)]
        [TestCase("RunSelectedButton", false)]
        [TestCase("RerunButton", false)]
        [TestCase("RunFailedButton", false)]
        [TestCase("DisplayFormatButton", false)]
        [TestCase("RunParametersButton", false)]
        [TestCase("StopRunButton", true)]
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

        [Test]
        public void TestCentricProjectIsSaved()
        {
            var project = Substitute.For<TestCentricProject>("MyProject", "test.dll");
            _model.TestCentricProject.Returns(project);

            this.FireRunStartingEvent(123);
            project.Received().SaveAs("test.dll.tcproj");
        }
    }
}
