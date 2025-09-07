// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using NUnit.Framework;

namespace TestCentric.Gui.Model
{
    [TestFixture]
    internal class TestFilterTests
    {
        [Test]
        public void MakeIdFilter_TestRunNode_ReturnsEmptyFilter()
        {
            // Arrange
            var testNode = new TestNode("<test-run type='TestRun' />");

            // Act
            TestFilter filter = TestFilter.MakeIdFilter(testNode);

            // Assert
            Assert.That(filter.IsEmpty, Is.True);
        }

        [Test]
        public void MakeIdFilter_Testcase_ReturnsIdFilter()
        {
            // Arrange
            var testNode = new TestNode("<test-case id='1' />");

            // Act
            TestFilter filter = TestFilter.MakeIdFilter(testNode);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><id>1</id></filter>"));
        }

        [Test]
        public void MakeIdFilter_TestFixture_ReturnsIdFilter()
        {
            // Arrange
            var testNode = new TestNode("<test-suite type='TestFixture' id='100' />");

            // Act
            TestFilter filter = TestFilter.MakeIdFilter(testNode);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><id>100</id></filter>"));
        }

        [Test]
        public void MakeIdFilter_TestSelection_SingleTestRunNode_ReturnsEmptyFilter()
        {
            // Arrange
            var testNode = new TestNode("<test-run type='TestRun' />");
            var testSelection = new TestSelection(new[] { testNode });

            // Act
            TestFilter filter = TestFilter.MakeIdFilter(testSelection);

            // Assert
            Assert.That(filter.IsEmpty, Is.True);
        }

        [Test]
        public void MakeIdFilter_TestSelection_SingleTestcase_ReturnsIdFilter()
        {
            // Arrange
            var testNode = new TestNode("<test-case id='1' />");
            var testSelection = new TestSelection(new[] { testNode });

            // Act
            TestFilter filter = TestFilter.MakeIdFilter(testSelection);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><id>1</id></filter>"));
        }

        [Test]
        public void MakeIdFilter_TestSelection_MultipleTests_ReturnsIdFilter()
        {
            // Arrange
            var testNode1 = new TestNode("<test-case id='1' />");
            var testNode2 = new TestNode("<test-suite type='TestFixture' id='100' />");

            var testSelection = new TestSelection(new[] { testNode1, testNode2 });

            // Act
            TestFilter filter = TestFilter.MakeIdFilter(testSelection);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><or><id>1</id><id>100</id></or></filter>"));
        }

        [Test]
        public void MakeCategoryFilter_SingleCategory_ReturnsCategoryFilter()
        {
            // Arrange
            // Act
            TestFilter filter = TestFilter.MakeCategoryFilter(new[] { "CategoryA" });

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><cat>CategoryA</cat></filter>"));
        }

        [Test]
        public void MakeCategoryFilter_MultipleCategory_ReturnsCategoryFilter()
        {
            // Arrange
            // Act
            TestFilter filter = TestFilter.MakeCategoryFilter(new[] { "CategoryA", "CategoryB" });

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><or><cat>CategoryA</cat><cat>CategoryB</cat></or></filter>"));
        }

        [Test]
        public void MakeAndFilter_OfOneEmptyFilter_ReturnsEmptyFilter()
        {
            // Arrange
            // Act
            TestFilter filter = TestFilter.MakeAndFilter(TestFilter.Empty);

            // Assert
            Assert.That(filter.IsEmpty, Is.True);
        }

        [Test]
        public void MakeAndFilter_OfTwoEmptyFilters_ReturnsEmptyFilter()
        {
            // Arrange
            // Act
            TestFilter filter = TestFilter.MakeAndFilter(TestFilter.Empty, TestFilter.Empty);

            // Assert
            Assert.That(filter.IsEmpty, Is.True);
        }

        [Test]
        public void MakeAndFilter_OfFilters_ReturnsAndFilter()
        {
            // Arrange
            var testNode = new TestNode("<test-case id='1' />");
            var filter1 = TestFilter.MakeIdFilter(testNode);

            // Act
            TestFilter filter = TestFilter.MakeAndFilter(TestFilter.Empty, filter1);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><id>1</id></filter>"));
        }

        [Test]
        public void MakeAndFilter_OfFilters2_ReturnsAndFilter()
        {
            // Arrange
            var testNode1 = new TestNode("<test-case id='1' />");
            var filter1 = TestFilter.MakeIdFilter(testNode1);

            var testNode2 = new TestNode("<test-case id='2' />");
            var filter2 = TestFilter.MakeIdFilter(testNode2);

            // Act
            TestFilter filter = TestFilter.MakeAndFilter(filter1, filter2);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><and><id>1</id><id>2</id></and></filter>"));
        }

        [Test]
        public void MakeOrFilter_OfOneEmptyFilter_ReturnsEmptyFilter()
        {
            // Arrange
            // Act
            TestFilter filter = TestFilter.MakeOrFilter(TestFilter.Empty);

            // Assert
            Assert.That(filter.IsEmpty, Is.True);
        }

        [Test]
        public void MakeOrFilter_OfTwoEmptyFilters_ReturnsEmptyFilter()
        {
            // Arrange
            // Act
            TestFilter filter = TestFilter.MakeOrFilter(TestFilter.Empty, TestFilter.Empty);

            // Assert
            Assert.That(filter.IsEmpty, Is.True);
        }

        [Test]
        public void MakeOrFilter_OfFilters_ReturnsOrFilter()
        {
            // Arrange
            var testNode = new TestNode("<test-case id='1' />");
            var filter1 = TestFilter.MakeIdFilter(testNode);

            // Act
            TestFilter filter = TestFilter.MakeOrFilter(TestFilter.Empty, filter1);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><id>1</id></filter>"));
        }

        [Test]
        public void MakeOrFilter_OfFilters2_ReturnsAndFilter()
        {
            // Arrange
            var testNode1 = new TestNode("<test-case id='1' />");
            var filter1 = TestFilter.MakeIdFilter(testNode1);

            var testNode2 = new TestNode("<test-case id='2' />");
            var filter2 = TestFilter.MakeIdFilter(testNode2);

            // Act
            TestFilter filter = TestFilter.MakeOrFilter(filter1, filter2);

            // Assert
            Assert.That(filter.XmlText, Is.EqualTo("<filter><or><id>1</id><id>2</id></or></filter>"));
        }
    }
}
