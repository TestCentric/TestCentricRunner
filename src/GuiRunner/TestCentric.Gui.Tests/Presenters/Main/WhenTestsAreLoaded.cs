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

    public class WhenTestsAreLoaded : MainPresenterTestBase
    {
        [SetUp]
        public void SimulateTestLoad()
        {
            ClearAllReceivedCalls();

            _model.HasTests.Returns(true);
            _model.IsTestRunning.Returns(false);

            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            _model.SelectedTests.Returns(new TestSelection(new[] { testNode }));
            _view.ResultTabs.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));

            var project = new TestCentricProject(new GuiOptions("dummy.dll"));
            _model.TestCentricProject.Returns(project);
            FireTestLoadedEvent(testNode);
        }

        [TestCase("OpenProjectCommand", true)]
        [TestCase("SaveProjectCommand", true)]

        [TestCase("CloseProjectCommand", true)]
        [TestCase("AddTestFilesCommand", true)]
        [TestCase("ReloadTestsCommand", true)]
        [TestCase("RecentFilesMenu", true)]
        [TestCase("ExitCommand", true)]
        [TestCase("RunAllButton", true)]
        [TestCase("RunSelectedButton", true)]
        [TestCase("RerunButton", false)]
        [TestCase("RunFailedButton", false)]
        [TestCase("DisplayFormatButton", true)]
        [TestCase("RunParametersButton", true)]
        [TestCase("StopRunButton", false)]
        [TestCase("ForceStopButton", false)]
        [TestCase("SaveResultsCommand", false)]
        [TestCase("TransformResultsCommand", false)]
        [TestCase("TestRunSettingsCommand", true)]
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
        public void TestPropertiesTabIsDisplay()
        {
            _view.ResultTabs.Received().SelectedIndex = 0;
        }

#if NYI
        [Test]
        public void View_Receives_TestAssembliesLoaded()
        {
            View.Received().OnTestAssembliesLoaded();
        }
#endif
    }
}
