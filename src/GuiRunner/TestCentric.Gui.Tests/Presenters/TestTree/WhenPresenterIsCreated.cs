// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using NSubstitute;
using System.Collections;
using NSubstitute.Core.Arguments;

namespace TestCentric.Gui.Presenters.TestTree
{
    public class WhenPresenterIsCreated : TreeViewPresenterTestBase
    {
        [Test]
        public void ShowCheckBoxesIsSet()
        {
            bool showCheckBoxes = _settings.Gui.TestTree.ShowCheckBoxes;
            _view.ShowCheckBoxes.Received().Checked = showCheckBoxes;
        }

        [Test]
        public void ShowTestDurationIsSet()
        {
            bool showTestDuration = _settings.Gui.TestTree.ShowTestDuration;
            _view.ShowTestDuration.Received().Checked = showTestDuration;
        }

        [Test]
        public void SortingMode_IsUpdated_ToDefaultSorter()
        {
            _view.Received().Sort(Arg.Is<IComparer>(c => c.GetType().Name == "NameComparer"));
        }

        //[Test]
        //public void StrategyIsSet()
        //{
        //    string displayFormat = _settings.Gui.TestTree.DisplayFormat.ToUpperInvariant();

        //    switch (displayFormat)
        //    {
        //        case "NUNIT_TREE":
        //            Assert.That(_presenter.Strategy, Is.TypeOf<NUnitTreeDisplayStrategy>());
        //            break;
        //        case "TEST_LIST":
        //            Assert.That(_presenter.Strategy, Is.TypeOf<TestListDisplayStrategy>());
        //            break;
        //        default:
        //            Assert.Fail($"{displayFormat} is not a valid display format");
        //            break;
        //    }
        //}
    }
}
