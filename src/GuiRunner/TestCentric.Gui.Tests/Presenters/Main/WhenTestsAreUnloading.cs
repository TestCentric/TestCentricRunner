// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.Main
{
    public class WhenTestsAreUnloading : MainPresenterTestBase
    {
        [Test]
        public void TestCentricProjectIsSaved()
        {
            var project = Substitute.For<TestCentricProject>(new GuiOptions("test.dll"));
            _model.TestCentricProject.Returns(project);

            FireTestsUnloadingEvent();

            project.Received().SaveAs("test.dll.tcproj");
        }
    }
}
