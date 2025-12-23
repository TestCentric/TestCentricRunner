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
        private ITestModel _model;

        [SetUp]
        public void Setup()
        {
            _guiFilter = Substitute.For<ITestCentricTestFilter>();
            _loadedTests = TestFilterData.GetSampleTestProject();
            _model = Substitute.For<ITestModel>();
        }

        [Test]
        public void GetTestFilter_ForTestRun_ReturnsAllNodesInGroup()
        {
            // Arrange
            TreeNodeViewModel viewModel = CreateViewModel(_loadedTests);
            GroupingTestGroup group = new GroupingTestGroup(viewModel, "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id><id>3-2011</id><id>3-2012</id><id>3-4011</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForAssembly_ReturnsAllNodesInGroup()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-1000");
            TreeNodeViewModel viewModel = CreateViewModel(associatedTestNode);
            GroupingTestGroup group = new GroupingTestGroup(viewModel, "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id><id>3-2011</id><id>3-2012</id><id>3-4011</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForNamespace_ReturnsAllNodesInGroup()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-1001");
            TreeNodeViewModel viewModel = CreateViewModel(associatedTestNode);
            GroupingTestGroup group = new GroupingTestGroup(viewModel, "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-1011</id><id>3-1012</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForNamespaceWithExplicitFixture_ReturnsAllNodesInGroup()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-2001");
            TreeNodeViewModel viewModel = CreateViewModel(associatedTestNode);
            GroupingTestGroup group = new GroupingTestGroup(viewModel, "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<or><id>3-2011</id><id>3-2012</id><id>3-4011</id></or>"));
        }

        [Test]
        public void GetTestFilter_ForTestFixture_ReturnsAllNodesInGroup()
        {
            // Arrange
            var associatedTestNode = TestFilterData.GetTestById(_loadedTests, "3-1010");
            TreeNodeViewModel viewModel = CreateViewModel(associatedTestNode);
            GroupingTestGroup group = new GroupingTestGroup(viewModel, "Name");

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
            TreeNodeViewModel viewModel = CreateViewModel(associatedTestNode);
            GroupingTestGroup group = new GroupingTestGroup(viewModel, "Name");

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
            TreeNodeViewModel viewModel = CreateViewModel(associatedTestNode);
            GroupingTestGroup group = new GroupingTestGroup(viewModel, "Name");

            // Act
            var filter = group.GetTestFilter(_guiFilter);

            // Assert
            Assert.That(filter.InnerXml, Is.EqualTo("<id>3-4011</id>"));
        }

        private TreeNodeViewModel CreateViewModel(TestNode testNode)
        {
            TestNode node = TestFilterData.GetTestById(_loadedTests, testNode.Id);

            var viewModel = new TreeNodeViewModel(_model, node, node.Name);
            foreach (TestNode childNode in node.Children)
            {
                var childViewModel = CreateViewModel(childNode);
                viewModel.AddChild(childViewModel);
            }


            return viewModel;
        }
    }
}
