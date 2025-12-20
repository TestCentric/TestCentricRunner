// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Gui.Presenters.NUnitGrouping;

namespace TestCentric.Gui.Presenters.Filter
{
    using NSubstitute;
    using NUnit.Framework;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Model.Filter;

    [TestFixture]
    internal class CategoryGroupingTestGroupTests
    {
        private ITestCentricTestFilter _guiFilter;
        private TestNode _loadedTests;
        private ITestModel _model;

        [SetUp]
        public void Setup()
        {
            _guiFilter = Substitute.For<ITestCentricTestFilter>();
            _loadedTests = TestFilterData.GetSampleTestProject();
            _model = Substitute.For<ITestModel>();
        }

        [Test]
        public void GetTestFilter_ForTestRun_ReturnsCategoryFilterWithoutExplicit()
        {
            // Arrange
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, _loadedTests, "Name");
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "CategoryA", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><not><or><id>3-3010</id><id>3-4012</id></or></not><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestAssembly_ReturnsCategoryFilter()
        {
            // Arrange
            TestNode associatedNode = TestFilterData.GetTestById(_loadedTests, "3-1000");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, associatedNode, "Name");
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "CategoryA", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-1000</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixture_ReturnsCategoryFilter()
        {
            // Arrange
            TestNode associatedNode = TestFilterData.GetTestById(_loadedTests, "3-1010");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, associatedNode, "Name");
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "CategoryA", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-1010</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForExplicitTestFixture_ReturnsCategoryFilter()
        {
            // Arrange
            TestNode associatedNode = TestFilterData.GetTestById(_loadedTests, "3-3010");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, associatedNode, "Name");
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "CategoryA", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-3010</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixtureWithExplicitTestCase_ReturnsCategoryFilter()
        {
            // Arrange
            TestNode associatedNode = TestFilterData.GetTestById(_loadedTests, "3-4010");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, associatedNode, "Name");
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "CategoryA", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-4010</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestRun_WithNoneCategory_ReturnsVisibleIdFilterWithoutExplicit()
        {
            // Arrange
            TreeNodeViewModel viewModel = CreateViewModel(_loadedTests);
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "None", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id><id>3-2011</id><id>3-2012</id><id>3-4011</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixture_WithNoneCategory_ReturnsVisibleIdFilter()
        {
            // Arrange
            TestNode associatedNode = TestFilterData.GetTestById(_loadedTests, "3-1010");
            TreeNodeViewModel viewModel = CreateViewModel(associatedNode);
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "None", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForExplicitTestFixture_WithNoneCategory_ReturnsVisibleIdFilter()
        {
            // Arrange
            TestNode associatedNode = TestFilterData.GetTestById(_loadedTests, "3-3010");
            TreeNodeViewModel viewModel = CreateViewModel(associatedNode);
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "None", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-3011</id><id>3-3012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixtureWithExplicitTestCase_WithNoneCategory_ReturnsVisibleIdFilter()
        {
            // Arrange
            TestNode associatedNode = TestFilterData.GetTestById(_loadedTests, "3-4010");
            TreeNodeViewModel viewModel = CreateViewModel(associatedNode);
            CategoryGroupingTestGroup group = new CategoryGroupingTestGroup(viewModel, "None", "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-4011</id>"));
        }

        private TreeNodeViewModel CreateViewModel(TestNode testNode)
        {
            TestNode node = TestFilterData.GetTestById(_loadedTests, testNode.Id);

            var viewModel = new TreeNodeViewModel(_model, node, node.Name);
            foreach (TestNode childNode in testNode.Children)
            {
                var childViewModel = CreateViewModel(childNode);
                viewModel.AddChild(childViewModel);
            }


            return viewModel;
        }
    }
}
