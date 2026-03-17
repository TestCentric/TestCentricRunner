// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters
{
    using System.Collections.Generic;
    using System.Windows.Forms;
    using NSubstitute;
    using NUnit.Framework;
    using NUnit.Framework.Interfaces;
    using NUnit.Framework.Internal;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Views;

    [TestFixture]
    internal class CategoryGroupingTests
    {
        [Test]
        public void Constructor_Groups_IsEmptyList()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            // 2. Act
            CategoryGrouping grouping = new CategoryGrouping(strategy, true);

            // 3. Assert
            Assert.That(grouping.Groups.Count, Is.EqualTo(0));
        }

        [Test]
        [TestCase(true, "CATEGORY")]
        [TestCase(false, "CATEGORY")]
        public void ID_Is_Category(bool includeAncestors, string expectedID)
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            // 2. Act
            CategoryGrouping grouping = new CategoryGrouping(strategy, includeAncestors);

            // 3. Assert
            Assert.That(grouping.ID, Is.EqualTo(expectedID));
        }

        [Test]
        public void Load_EmptyTestNodeList_NonCategory_IsCreated()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);
            IEnumerable<TestNode> tests = new List<TestNode>();

            // 2. Act
            CategoryGrouping grouping = new CategoryGrouping(strategy, false);
            grouping.LoadGroups(tests);

            // 3. Assert
            Assert.That(grouping.Groups.Count, Is.EqualTo(1));
        }

        [Test]
        public void Load_TestNodeWithoutCategory_IsAddedToNonCategory()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);
            
            var testNode = new TestNode($"<test-case id='1' />");
            var tests = new List<TestNode> { testNode };

            // 2. Act
            CategoryGrouping grouping = new CategoryGrouping(strategy, false);
            grouping.LoadGroups(tests);

            // 3. Assert
            Assert.That(grouping.Groups.Count, Is.EqualTo(1));
            Assert.That(grouping.Groups[0].Items, Contains.Item(testNode));
        }

        [Test]
        public void Load_TestNodeWithCategory_CategoryNodeIsCreated()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            var testNode = new TestNode($"<test-case id='1'> <properties> <property name='Category' value='Feature_1' /> </properties> </test-case>");
            var tests = new List<TestNode> { testNode };

            // 2. Act
            CategoryGrouping grouping = new CategoryGrouping(strategy, false);
            grouping.LoadGroups(tests);

            // 3. Assert
            Assert.That(grouping.Groups.Count, Is.EqualTo(2));
            var categoryGroup = grouping.Groups[1];
            Assert.That(categoryGroup.Name, Is.EqualTo("Feature_1"));
            Assert.That(categoryGroup.Items, Contains.Item(testNode));
        }

        [Test]
        public void Load_TestNodes_AllCategoryNodesAreCreated()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);

            var testNode1 = new TestNode($"<test-case id='1'> <properties> <property name='Category' value='Feature_1' /> </properties> </test-case>");
            var testNode2 = new TestNode($"<test-case id='2'> <properties> <property name='Category' value='Feature_1' /> </properties> </test-case>");
            var testNode3 = new TestNode($"<test-case id='3'> <properties> <property name='Category' value='Feature_2' /> </properties> </test-case>");
            var testNode4 = new TestNode($"<test-case id='4'> <properties> <property name='Category' value='Feature_2' /> </properties> </test-case>");
            var testNode5 = new TestNode($"<test-case id='5'> </test-case>");
            var tests = new List<TestNode> { testNode1, testNode2, testNode3, testNode4, testNode5 };

            // 2. Act
            CategoryGrouping grouping = new CategoryGrouping(strategy, false);
            grouping.LoadGroups(tests);

            // 3. Assert
            Assert.That(grouping.Groups.Count, Is.EqualTo(3));

            var categoryGroup = grouping.Groups[1];
            Assert.That(categoryGroup.Name, Is.EqualTo("Feature_1"));
            Assert.That(categoryGroup.Items, Contains.Item(testNode1));
            Assert.That(categoryGroup.Items, Contains.Item(testNode2));

            categoryGroup = grouping.Groups[2];
            Assert.That(categoryGroup.Name, Is.EqualTo("Feature_2"));
            Assert.That(categoryGroup.Items, Contains.Item(testNode3));
            Assert.That(categoryGroup.Items, Contains.Item(testNode4));

            categoryGroup = grouping.Groups[0];
            Assert.That(categoryGroup.Name, Is.EqualTo("None"));
            Assert.That(categoryGroup.Items, Contains.Item(testNode5));
        }

        [Test]
        public void OnTestFinished_Calls_ApplyResultToGroup()
        {
            // 1. Arrange
            ITestTreeView treeView = Substitute.For<ITestTreeView>();
            ITestModel model = Substitute.For<ITestModel>();
            GroupDisplayStrategy strategy = Substitute.For<GroupDisplayStrategy>(treeView, model);
            ResultNode result = new ResultNode("<test-case id='1'/>");

            // 2. Act
            CategoryGrouping grouping = new CategoryGrouping(strategy, false);
            grouping.OnTestFinished(result);

            // 3. Assert
            strategy.Received().ApplyResultToGroup(result);
        }
    }
}
