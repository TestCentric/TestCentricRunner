// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;
using FakeUserSettings = TestCentric.Gui.Fakes.UserSettings;

namespace TestCentric.Gui.Presenters
{
    [TestFixture]
    public class TestResultSubViewPresenterTests
    {
        [Test]
        public void Constructor_CallsLoadImages()
        {
            // 1. Arrange
            ITestResultSubView view = Substitute.For<ITestResultSubView>();
            ITestModel model = Substitute.For<ITestModel>();
            var settings = new FakeUserSettings();
            model.Settings.Returns(settings);

            // 2. Act
            TestResultSubViewPresenter presenter = new TestResultSubViewPresenter(view, model);

            // 3. Assert
            view.Received().LoadImages(model.Settings.Gui.TestTree.AlternateImageSet);
        }

        [Test]
        public void Clear_ClearsView()
        {
            // 1. Arrange
            ITestResultSubView view = Substitute.For<ITestResultSubView>();
            ITestModel model = Substitute.For<ITestModel>();
            var settings = new FakeUserSettings();
            model.Settings.Returns(settings);

            // 2. Act
            TestResultSubViewPresenter presenter = new TestResultSubViewPresenter(view, model);
            presenter.Clear();

            // 3. Assert
            view.Received().Clear();
        }

        [Test]
        public void Update_WithTestCase_DetailSectionNotVisible()
        {
            // 1. Arrange
            ITestResultSubView view = Substitute.For<ITestResultSubView>();
            ITestModel model = Substitute.For<ITestModel>();
            var settings = new FakeUserSettings();
            model.Settings.Returns(settings);

            TestNode testNode = new TestNode("<test-case id='1' />");
            ResultNode resultNode = new ResultNode($"<test-case id='1' result='Passed' />");
            model.GetResultForTest("1").Returns(resultNode);

            // 2. Act
            TestResultSubViewPresenter presenter = new TestResultSubViewPresenter(view, model);
            presenter.Update(testNode);

            // 3. Assert
            view.Received().UpdateCaption(Arg.Any<TestResultCounts>(), resultNode);
            view.Received().UpdateDetailSectionVisibility(false);
            view.Received().ShrinkToCaption();
        }

        [Test]
        public void Update_WithTestSuite_DetailSectionIsVisible()
        {
            // 1. Arrange
            ITestResultSubView view = Substitute.For<ITestResultSubView>();
            ITestModel model = Substitute.For<ITestModel>();
            var settings = new FakeUserSettings();
            model.Settings.Returns(settings);

            TestNode testNode = new TestNode("<test-suite id='1' />");

            // 2. Act
            TestResultSubViewPresenter presenter = new TestResultSubViewPresenter(view, model);
            presenter.Update(testNode);

            // 3. Assert
            view.Received().UpdateCaption(Arg.Any<TestResultCounts>(), null);
            view.Received().UpdateDetailSectionVisibility(true);
            view.DidNotReceive().ShrinkToCaption();

            view.Received().UpdateDetailSection(Arg.Any<TestResultCounts>());
        }

        [Test]
        public void SettingsChanged_AlternateImageSet_LoadImagesIsCalled()
        {
            // 1. Arrange
            ITestResultSubView view = Substitute.For<ITestResultSubView>();
            ITestModel model = Substitute.For<ITestModel>();
            var settings = new FakeUserSettings();
            model.Settings.Returns(settings);

            // 2. Act
            TestResultSubViewPresenter presenter = new TestResultSubViewPresenter(view, model);
            settings.Gui.TestTree.AlternateImageSet = "Classic";

            // 3. Assert
            view.Received().LoadImages("Classic");
        }
    }
}
