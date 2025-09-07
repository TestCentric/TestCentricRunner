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

        [SetUp]
        public void Setup()
        {
            _guiFilter = Substitute.For<ITestCentricTestFilter>();
            _loadedTests = TestFilterData.GetSampleTestProject();
        }

        [Test]
        public void GetTestFilter_ForTestRun_ReturnsCategoryFilterWithoutExplicit()
        {
            // Arrange
            var group = new CategoryGroupingTestGroup(_loadedTests, "CategoryA", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-1011"),
                TestFilterData.GetTestById(_loadedTests, "3-1012"),
                TestFilterData.GetTestById(_loadedTests, "3-2011"),
                TestFilterData.GetTestById(_loadedTests, "3-2012"),
                TestFilterData.GetTestById(_loadedTests, "3-3011"),
                TestFilterData.GetTestById(_loadedTests, "3-3012"),
                TestFilterData.GetTestById(_loadedTests, "3-4011"),
                TestFilterData.GetTestById(_loadedTests, "3-4012"),
            };

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
            var group = new CategoryGroupingTestGroup(associatedNode, "CategoryA", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-1011"),
                TestFilterData.GetTestById(_loadedTests, "3-1012"),
                TestFilterData.GetTestById(_loadedTests, "3-2011"),
                TestFilterData.GetTestById(_loadedTests, "3-2012"),
                TestFilterData.GetTestById(_loadedTests, "3-3011"),
                TestFilterData.GetTestById(_loadedTests, "3-3012"),
                TestFilterData.GetTestById(_loadedTests, "3-4011"),
                TestFilterData.GetTestById(_loadedTests, "3-4012"),
            };

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
            var group = new CategoryGroupingTestGroup(associatedNode, "CategoryA", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-1011"),
                TestFilterData.GetTestById(_loadedTests, "3-1012"),
            };

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
            var group = new CategoryGroupingTestGroup(associatedNode, "CategoryA", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-3011"),
                TestFilterData.GetTestById(_loadedTests, "3-3012"),
            };

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
            var group = new CategoryGroupingTestGroup(associatedNode, "CategoryA", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-4011"),
                TestFilterData.GetTestById(_loadedTests, "3-4012"),
            };

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<and><id>3-4010</id><cat>CategoryA</cat></and>"));
        }

        [Test]
        public void GetTestFilter_ForTestRun_WithNoneCategory_ReturnsVisibleIdFilterWithoutExplicit()
        {
            // Arrange
            var group = new CategoryGroupingTestGroup(_loadedTests, "None", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-1011"),
                TestFilterData.GetTestById(_loadedTests, "3-1012"),
                TestFilterData.GetTestById(_loadedTests, "3-2011"),
                TestFilterData.GetTestById(_loadedTests, "3-2012"),
                TestFilterData.GetTestById(_loadedTests, "3-3011"),
                TestFilterData.GetTestById(_loadedTests, "3-3012"),
                TestFilterData.GetTestById(_loadedTests, "3-4011"),
                TestFilterData.GetTestById(_loadedTests, "3-4012"),
            };

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
            var group = new CategoryGroupingTestGroup(associatedNode, "None", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-1011"),
                TestFilterData.GetTestById(_loadedTests, "3-1012"),
            };

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
            var group = new CategoryGroupingTestGroup(associatedNode, "None", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-3011"),
                TestFilterData.GetTestById(_loadedTests, "3-3012"),
            };

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
            var group = new CategoryGroupingTestGroup(associatedNode, "None", "Name")
            {
                TestFilterData.GetTestById(_loadedTests, "3-4011"),
                TestFilterData.GetTestById(_loadedTests, "3-4012"),
            };

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-4011</id>"));
        }
    }
}
