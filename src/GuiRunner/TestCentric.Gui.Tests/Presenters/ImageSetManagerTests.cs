// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters
{
    using TestCentric.Gui.Model.Settings;

    public class ImageSetManagerTests
    {
        private ImageSetManager _manager;
        private ITestModel _model;
        private IMainView _mainView;

        [SetUp]
        public void CreateManager()
        {
            _model = Substitute.For<ITestModel>();
            _model.Settings.Gui.TestTree.AlternateImageSet.Returns("Classic");
            _mainView = Substitute.For<IMainView>();

            Assert.That(_mainView.TreeView, Is.Not.Null);
            Assert.That(_mainView.TestResultSubView, Is.Not.Null);
            Assert.That(_mainView.StatusBarView, Is.Not.Null);

            _manager = new ImageSetManager(_model, _mainView);
        }

        [TestCase("Circles")]
        [TestCase("Classic")]
        [TestCase("Visual Studio")]
        public void AllImageSetsAreFound(string name)
        {
            Assert.That(_manager.ImageSets.ContainsKey(name));
        }

        [Test]
        public void DefaultImageSetIsDefault()
        {
            Assert.That(_manager.CurrentImageSet.Name, Is.EqualTo("Classic"));
        }

        [Test]
        public void CanChangeCurrentImageSet()
        {
            OutcomeImageSet imgSet = _manager.LoadImageSet("Visual Studio");
            Assert.That(imgSet.Name, Is.EqualTo("Visual Studio"));
            Assert.That(_manager.CurrentImageSet, Is.SameAs(imgSet));
        }

        [Test]
        public void SettingCurrentImageSetToInvalidValueThrows()
        {
            Assert.That(() => _manager.LoadImageSet("NonExistent"), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void WhenManagerIsCreated_TreeViewImagesAreSet()
        {
            _mainView.TreeView.Received().OutcomeImages = Arg.Is<OutcomeImageSet>((set) => set.Name == "Classic");
        }

        [Test]
        public void WhenManagerIsCreated_TestResultImagesAreSet()
        {
            _mainView.TestResultSubView.Received().LoadImages(Arg.Is<OutcomeImageSet>((set) => set.Name == "Classic"));
        }

        [Test]
        public void WhenManagerIsCreated_StatusBarImagesAreSet()
        {
            _mainView.StatusBarView.Received().LoadImages(Arg.Is<OutcomeImageSet>((set) => set.Name == "Classic"));
        }

        [Test]
        public void WhenImageSetChanges_TreeView_ImageSetIsSet()
        {
            _mainView.TreeView.ClearReceivedCalls();
            string newImageSet = "Visual Studio";
            _model.Settings.Gui.TestTree.AlternateImageSet.Returns(newImageSet);

            _model.Settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.AlternateImageSet"));
            Assert.That(_manager.CurrentImageSet.Name, Is.EqualTo(newImageSet));

            _mainView.TreeView.Received().OutcomeImages = Arg.Is<OutcomeImageSet>((set) => set.Name == newImageSet);
        }

        [Test]
        public void WhenImageSetChanges_TestResult_ImageSetIsSet()
        {
            _mainView.TestResultSubView.ClearReceivedCalls();
            string newImageSet = "Visual Studio";
            _model.Settings.Gui.TestTree.AlternateImageSet.Returns(newImageSet);

            _model.Settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.AlternateImageSet"));
            Assert.That(_manager.CurrentImageSet.Name, Is.EqualTo(newImageSet));

            _mainView.TestResultSubView.Received().LoadImages(Arg.Is<OutcomeImageSet>((set) => set.Name == newImageSet));
        }

        [Test]
        public void WhenImageSetChanges_StatusBarImagesAreSet()
        {
            _mainView.StatusBarView.ClearReceivedCalls();
            string newImageSet = "Visual Studio";
            _model.Settings.Gui.TestTree.AlternateImageSet.Returns(newImageSet);

            _model.Settings.Changed += Raise.Event<SettingsEventHandler>(this, new SettingsEventArgs("TestCentric.Gui.TestTree.AlternateImageSet"));
            Assert.That(_manager.CurrentImageSet.Name, Is.EqualTo(newImageSet));

            _mainView.StatusBarView.Received().LoadImages(Arg.Is<OutcomeImageSet>((set) => set.Name == newImageSet));
        }
    }
}
