// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.Filter
{
    using System.Collections.Generic;
    using NSubstitute;
    using NUnit.Framework;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Model.Filter;

    [TestFixture]
    internal class TestSelectionTests
    {
        private ITestCentricTestFilter _guiFilter;
        private TestNode _loadedTests;

        [SetUp]
        public void Setup()
        {
            _guiFilter = Substitute.For<ITestCentricTestFilter>();
            _loadedTests = TestFilterData.GetSampleTestProject();
        }

        [Test]
        public void GetTestFilter_ForTestRun_ReturnsEmptyFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { _loadedTests };

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.IsEmpty, Is.EqualTo(true));
        }

        [Test]
        public void GetTestFilter_ForTestFixture_ReturnsIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-1010") };

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-1010</id>"));
        }

        [Test]
        public void GetTestFilter_ForExplicitTestFixture_ReturnsIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-3010") };

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-3010</id>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixtureWithExplicitTestcase_ReturnsIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-4010") };

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-4010</id>"));
        }

        [Test]
        public void GetTestFilter_ForTestRun_WithCategoryFilter_ReturnsIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { _loadedTests };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.AllCategories.Returns(new List<string>() { "CategoryA", "CategoryB" });
            _guiFilter.CategoryFilter.Returns(new List<string>() { "CategoryA" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><not><or><id>3-3010</id><id>3-4012</id></or></not><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixture_WithCategoryFilter_ReturnsIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-2010") };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.AllCategories.Returns(new List<string>() { "CategoryA", "CategoryB" });
            _guiFilter.CategoryFilter.Returns(new List<string>() { "CategoryA" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-2010</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForExplicitTestFixture_WithCategoryFilter_ReturnsIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-3010") };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.AllCategories.Returns(new List<string>() { "CategoryA", "CategoryB" });
            _guiFilter.CategoryFilter.Returns(new List<string>() { "CategoryA" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-3010</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixtureWithExplicitTestcase_WithCategoryFilter_ReturnsIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-4010") };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.AllCategories.Returns(new List<string>() { "CategoryA", "CategoryB" });
            _guiFilter.CategoryFilter.Returns(new List<string>() { "CategoryA" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-4010</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestRun_WithOutcomeFilter_ReturnsVisibleIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { _loadedTests };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.OutcomeFilter.Returns(new List<string>() { "Passed" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id><id>3-2011</id><id>3-2012</id><id>3-4011</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixture_WithOutcomeFilter_ReturnsVisibleIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-1010") };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.OutcomeFilter.Returns(new List<string>() { "Passed" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForExplicitTestFixture_WithOutcomeFilter_ReturnsVisibleIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-3010") };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.OutcomeFilter.Returns(new List<string>() { "Passed" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-3011</id><id>3-3012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixtureWithExplicitTestcases_WithOutcomeFilter_ReturnsVisibleIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-4010") };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.OutcomeFilter.Returns(new List<string>() { "Passed" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-4011</id>"));
        }

        [Test]
        public void GetTestFilter_ForTestAssembly_WithNoneCategoryFilter_ReturnsVisibleIdFilter()
        {
            // Arrange
            var testSelection = new TestSelection() { TestFilterData.GetTestById(_loadedTests, "3-1000") };
            _guiFilter.IsActive.Returns(true);
            _guiFilter.AllCategories.Returns(new List<string>() { "CategoryA", "CategoryB" });
            _guiFilter.CategoryFilter.Returns(new List<string>() { "None" });

            // Act
            var filter = testSelection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id><id>3-2011</id><id>3-2012</id><id>3-4011</id></or>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestAssembly_ReturnsIdFilterWithExplicit()
        {
            // Arrange
            var testNode = TestFilterData.GetTestById(_loadedTests, "3-1000");
            var selection = new TestSelection() { testNode };
            selection.AddExplicitChildTests();

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1000</id><id>3-3010</id><id>3-4012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestFixture_ReturnsIdFilterWithExplicit()
        {
            // Arrange
            var testNode = TestFilterData.GetTestById(_loadedTests, "3-1010");
            var selection = new TestSelection() { testNode };
            selection.AddExplicitChildTests();

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-1010</id>"));
        }

        [Test]
        public void GetTestFilter_ForExplicitTestFixture_ReturnsIdFilterWithExplicit()
        {
            // Arrange
            var testNode = TestFilterData.GetTestById(_loadedTests, "3-3010");
            var selection = new TestSelection() { testNode };
            selection.AddExplicitChildTests();

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-3010</id>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestFixtureWithExplicitTestcase_ReturnsIdFilterWithExplicit()
        {
            // Arrange
            var testNode = TestFilterData.GetTestById(_loadedTests, "3-4010");
            var selection = new TestSelection() { testNode };
            selection.AddExplicitChildTests();

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-4010</id><id>3-4012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestFixtures_ReturnsIdFilterWithExplicit()
        {
            // Arrange
            var testNode1 = TestFilterData.GetTestById(_loadedTests, "3-1010");
            var testNode2 = TestFilterData.GetTestById(_loadedTests, "3-3010");
            var testNode3 = TestFilterData.GetTestById(_loadedTests, "3-4010");

            var selection = new TestSelection() { testNode1, testNode2, testNode3 };
            selection.AddExplicitChildTests();

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1010</id><id>3-3010</id><id>3-4010</id><id>3-4012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestcases_ReturnsIdFilterWithExplicit()
        {
            // Arrange
            var testNode1 = TestFilterData.GetTestById(_loadedTests, "3-1012");
            var testNode2 = TestFilterData.GetTestById(_loadedTests, "3-3012");
            var testNode3 = TestFilterData.GetTestById(_loadedTests, "3-4012");

            var selection = new TestSelection() { testNode1, testNode2, testNode3 };
            selection.AddExplicitChildTests();

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1012</id><id>3-3012</id><id>3-4012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestAssembly_WithOutcomeFilter_ReturnsVisibleIdFilterWithExplicit()
        {
            // Arrange
            var testNode = TestFilterData.GetTestById(_loadedTests, "3-1000");
            var selection = new TestSelection() { testNode };
            selection.AddExplicitChildTests();
            _guiFilter.IsActive.Returns(true);
            _guiFilter.OutcomeFilter.Returns(new List<string>() { "Passed" });

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id><id>3-2011</id><id>3-2012</id><id>3-3011</id><id>3-3012</id><id>3-4011</id><id>3-4012</id><id>3-3011</id><id>3-3012</id><id>3-4012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestFixtures_WithOutcomeFilter_ReturnsVisibleIdFilterWithExplicit()
        {
            // Arrange
            var testNode1 = TestFilterData.GetTestById(_loadedTests, "3-1010");
            var testNode2 = TestFilterData.GetTestById(_loadedTests, "3-3010");
            var testNode3 = TestFilterData.GetTestById(_loadedTests, "3-4010");

            var selection = new TestSelection() { testNode1, testNode2, testNode3 };
            selection.AddExplicitChildTests();
            _guiFilter.IsActive.Returns(true);
            _guiFilter.OutcomeFilter.Returns(new List<string>() { "Passed" });

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id><id>3-3011</id><id>3-3012</id><id>3-4011</id><id>3-4012</id><id>3-4012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ExplicitChildrenIncluded_ForTestcases_WithOutcomeFilter_ReturnsVisibleIdFilterWithExplicit()
        {
            // Arrange
            var testNode1 = TestFilterData.GetTestById(_loadedTests, "3-1012");
            var testNode2 = TestFilterData.GetTestById(_loadedTests, "3-3012");
            var testNode3 = TestFilterData.GetTestById(_loadedTests, "3-4012");

            var selection = new TestSelection() { testNode1, testNode2, testNode3 };
            selection.AddExplicitChildTests();
            _guiFilter.IsActive.Returns(true);
            _guiFilter.OutcomeFilter.Returns(new List<string>() { "Passed" });

            // Act
            var filter = selection.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1012</id><id>3-3012</id><id>3-4012</id></or>"));
        }
    }
}
