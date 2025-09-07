// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.Filter
{
    using NSubstitute;
    using NUnit.Framework;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Model.Filter;
    using TestCentric.Gui.Presenters.NUnitGrouping;

    [TestFixture]
    internal class GroupingTestGroupTests
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
        public void GetTestFilter_ForTestRun_ReturnsAllNodesInGroup()
        {
            // Arrange
            var associatedTestNode = _loadedTests;
            var group = new GroupingTestGroup(associatedTestNode, "Name")
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
        public void GetTestFilter_ForAssembly_ReturnsAllNodesInGroup()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-1001");
            var group = new GroupingTestGroup(associatedTestNode, "Name")
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
        public void GetTestFilter_ForTestFixture_ReturnsAllNodesInGroup()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-1010");
            var group = new GroupingTestGroup(associatedTestNode, "Name")
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
        public void GetTestFilter_ForExplicitTestFixture_ReturnsNonExplicitTests()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-3010");
            var group = new GroupingTestGroup(associatedTestNode, "Name")
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
        public void GetTestFilter_ForFixtureWithExplicitTestCases_ReturnsNonExplicitTests()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-4010");
            var group = new GroupingTestGroup(associatedTestNode, "Name")
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
