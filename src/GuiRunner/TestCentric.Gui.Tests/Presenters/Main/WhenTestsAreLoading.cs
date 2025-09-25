// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.Main
{
    public class WhenTestsAreLoading : MainPresenterTestBase
    {
        [SetUp]
        public void SimulateTestsLoading()
        {
            ClearAllReceivedCalls();

            FireTestsLoadingEvent(new[] { "test.dll" });
        }

        [TestCase("OpenProjectCommand", false)]
        [TestCase("SaveProjectCommand", false)]

        [TestCase("CloseProjectCommand", false)]
        [TestCase("AddTestFilesCommand", false)]
        [TestCase("ReloadTestsCommand", false)]
        [TestCase("RecentFilesMenu", false)]
        [TestCase("ExitCommand", false)]
        [TestCase("RunAllButton", false)]
        [TestCase("RunSelectedButton", false)]
        [TestCase("RerunButton", false)]
        [TestCase("RunFailedButton", false)]
        [TestCase("StopRunButton", false)]
        [TestCase("DisplayFormatButton", false)]
        [TestCase("RunParametersButton", false)]
        [TestCase("ForceStopButton", false)]
        [TestCase("SaveResultsCommand", false)]
        [TestCase("TransformResultsCommand", false)]
        public void CheckCommandEnabled(string propName, bool enabled)
        {
            ViewElement(propName).Received().Enabled = enabled;
        }

#if NYI
		[Test]
        public void View_Receives_FileNameOfSingleAssembly()
        {
            var arguments = new TestFilesLoadingEventArgs(new[]
                {
                    "C:\\git\\projects\\pull-request\\SomeAssembly.AcceptanceTests.dll"
                });
            Model.Events.TestsLoading += Raise.Event<TestFilesLoadingEventHandler>(arguments);

            View.Received().OnTestAssembliesLoading("Loading Assembly: SomeAssembly.AcceptanceTests.dll");
        }

        [Test]
        public void View_Receives_CountOfMultipleAssemblies()
        {
            var arguments = new TestFilesLoadingEventArgs(new[]
                {
                    "C:\\git\\projects\\pull-request\\SomeAssembly.Tests.dll",
                    "C:\\git\\projects\\pull-request\\SomeAssembly.IntegrationTests.dll",
                    "C:\\git\\projects\\pull-request\\SomeAssembly.AcceptanceTests.dll"
                });
            Model.Events.TestsLoading += Raise.Event<TestFilesLoadingEventHandler>(arguments);

            View.Received().OnTestAssembliesLoading("Loading 3 Assemblies...");
        }
#endif
    }
}
