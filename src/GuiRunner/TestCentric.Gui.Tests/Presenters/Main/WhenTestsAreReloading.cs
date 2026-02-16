// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.Main
{
    public class WhenTestsAreReloading : MainPresenterTestBase
    {
        [Test]
        public void TestCentricProjectIsSaved()
        {
            var project = Substitute.For<TestCentricProject>("MyProject", "test.dll");
            _model.TestCentricProject.Returns(project);

            FireTestsReloadingEvent();

            project.Received().SaveAs("test.dll.tcproj");
        }
    }
}
