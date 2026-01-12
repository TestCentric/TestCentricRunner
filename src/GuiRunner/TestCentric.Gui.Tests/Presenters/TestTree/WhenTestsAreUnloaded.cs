// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters.TestTree
{
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Model.Settings;

    public class WhenTestsAreUnloaded : TreeViewPresenterTestBase
    {
        [SetUp]
        public void SimulateTestUnload()
        {
            _model.TreeConfiguration.DisplayFormat.Returns("NUNIT_TREE");
            _model.TreeConfiguration.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs(nameof(TreeConfiguration.DisplayFormat)));

            ClearAllReceivedCalls();

            _model.HasTests.Returns(false);
            _model.IsTestRunning.Returns(false);
            FireTestUnloadedEvent();
        }

        [Test]
        public void TestUnloaded_CategoryFilter_IsClosed()
        {
            // Act: unload tests
            FireTestUnloadedEvent();

            // Assert
            _view.CategoryFilter.Received().Close();
        }

        [Test]
        public void TestUnloaded_TestFilters_AreReset()
        {
            // Act: unload tests
            FireTestUnloadedEvent();

            // Assert
            _view.TextFilter.Received().Text = "";
            _view.OutcomeFilter.ReceivedWithAnyArgs().SelectedItems = null;
            _view.CategoryFilter.ReceivedWithAnyArgs().SelectedItems = null;
        }
    }
}
