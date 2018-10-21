﻿// ***********************************************************************
// Copyright (c) 2017 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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

#if NYI // Add after implementation of project or package saving
        [TestCase("NewProjectCommand", false)]
        [TestCase("OpenProjectCommand", false)]
        [TestCase("SaveCommand", false)]
        [TestCase("SaveAsCommand", false)
#endif

        [TestCase("RunButton", false)]
        [TestCase("StopButton", false)]
        [TestCase("OpenCommand", false)]
        [TestCase("CloseCommand", false)]
        [TestCase("AddTestFileCommand", false)]
        [TestCase("ReloadTestsCommand", false)]
        [TestCase("RuntimeMenu", false)]
        [TestCase("RecentFilesMenu", false)]
        [TestCase("ExitCommand", false)]
        [TestCase("RunAllCommand", false)]
        [TestCase("RunSelectedCommand", false)]
        [TestCase("RunFailedCommand", false)]
        [TestCase("StopRunCommand", false)]
        [TestCase("SaveResultsCommand", false)]
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
