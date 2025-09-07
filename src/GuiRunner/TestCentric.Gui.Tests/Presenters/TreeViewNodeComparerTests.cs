// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections;
using System.Windows.Forms;
using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters
{
    [TestFixture]
    public class TreeViewNodeComparerTests
    {
        [TestCase(TreeViewNodeComparer.Name, TreeViewNodeComparer.Ascending, "NameComparer")]
        [TestCase(TreeViewNodeComparer.Name, TreeViewNodeComparer.Descending, "NameComparer")]
        [TestCase(TreeViewNodeComparer.Duration, TreeViewNodeComparer.Ascending, "DurationComparer")]
        [TestCase(TreeViewNodeComparer.Duration, TreeViewNodeComparer.Descending, "DurationComparer")]
        public void GetComparer(string sortMode, string sortDirection, string expectedComparerName)
        {
            // Arrange
            ITestModel model = Substitute.For<ITestModel>();

            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, sortMode, sortDirection);

            // Assert
            string classname = comparer.GetType().Name;
            Assert.That(classname, Is.EqualTo(expectedComparerName));
        }

        [TestCase("a", "b", -1)]
        [TestCase("aba", "aaa", 1)]
        [TestCase("abc", "abc", 0)]
        [TestCase("", "", 0)]
        public void Namecompare_Ascending_ReturnsExpectedResult(string text1, string text2, int expectedCompareResult)
        {
            // Arrange
            ITestModel model = Substitute.For<ITestModel>();
            TreeNode treeNode1 = new TreeNode(text1);
            TreeNode treeNode2 = new TreeNode(text2);


            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Name, TreeViewNodeComparer.Ascending);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }

        [TestCase("a", "b", 1)]
        [TestCase("aba", "aaa", -1)]
        [TestCase("abc", "abc", 0)]
        [TestCase("", "", 0)]
        public void Namecompare_Descending_ReturnsExpectedResult(string text1, string text2, int expectedCompareResult)
        {
            // Arrange
            ITestModel model = Substitute.For<ITestModel>();
            TreeNode treeNode1 = new TreeNode(text1);
            TreeNode treeNode2 = new TreeNode(text2);


            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Name, TreeViewNodeComparer.Descending);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }

        [TestCase("2", "4", -1)]
        [TestCase("0.5", "0.8", -1)]
        [TestCase("0.8", "1.2", -1)]
        [TestCase("0.8", "0.5", 1)]
        [TestCase("0.8", "0.8", 0)]
        public void Durationcompare_Ascending_ReturnsExpectedResult(string duration1, string duration2, int expectedCompareResult)
        {
            // Arrange
            TestNode testNode1 = new TestNode($"<test-start id='1'/>");
            TestNode testNode2 = new TestNode($"<test-start id='2'/>");
            var resultNode1 = new ResultNode($"<test-case id='1' duration='{duration1}'/>");
            var resultNode2 = new ResultNode($"<test-case id='2' duration='{duration2}'/>");
            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            model.TestResultManager.GetResultForTest("2").Returns(resultNode2);

            TreeNode treeNode1 = new TreeNode() { Tag = testNode1 };
            TreeNode treeNode2 = new TreeNode() { Tag = testNode2 };

            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Duration, TreeViewNodeComparer.Ascending);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }

        [TestCase("2", "4", 1)]
        [TestCase("0.5", "0.8", 1)]
        [TestCase("0.8", "1.2", 1)]
        [TestCase("0.8", "0.5", -1)]
        [TestCase("0.8", "0.8", 0)]
        public void Durationcompare_Descending_ReturnsExpectedResult(string duration1, string duration2, int expectedCompareResult)
        {
            // Arrange
            TestNode testNode1 = new TestNode($"<test-start id='1'/>");
            TestNode testNode2 = new TestNode($"<test-start id='2'/>");
            var resultNode1 = new ResultNode($"<test-case id='1' duration='{duration1}'/>");
            var resultNode2 = new ResultNode($"<test-case id='2' duration='{duration2}'/>");
            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            model.TestResultManager.GetResultForTest("2").Returns(resultNode2);

            TreeNode treeNode1 = new TreeNode() { Tag = testNode1 };
            TreeNode treeNode2 = new TreeNode() { Tag = testNode2 };

            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Duration, TreeViewNodeComparer.Descending);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }

        [TestCase("TestA", "TestB", TreeViewNodeComparer.Ascending, -1)]
        [TestCase("TestA", "TestB", TreeViewNodeComparer.Descending, 1)]
        [TestCase("TestA", "TestA", TreeViewNodeComparer.Descending, 0)]
        public void Durationcompare_NoTestResultAvailable_ReturnsExpectedResult(string text1, string text2, string sortDirection, int expectedCompareResult)
        {
            // Arrange
            TestNode testNode1 = new TestNode($"<test-start id='1'/>");
            TestNode testNode2 = new TestNode($"<test-start id='2'/>");
            ITestModel model = Substitute.For<ITestModel>();
            model.TestResultManager.GetResultForTest("1").Returns((ResultNode)null);
            model.TestResultManager.GetResultForTest("2").Returns((ResultNode)null);

            TreeNode treeNode1 = new TreeNode(text1) { Tag = testNode1 };
            TreeNode treeNode2 = new TreeNode(text2) { Tag = testNode2 };

            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Duration, sortDirection);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }

        [TestCase(2, 4, -1)]
        [TestCase(0.5, 0.8, -1)]
        [TestCase(0.8, 1.2, -1)]
        [TestCase(0.8, 0.5, 1)]
        [TestCase(0.8, 0.8, 0)]
        public void Durationcompare_Ascending_Groups_ReturnsExpectedResult(double duration1, double duration2, int expectedCompareResult)
        {
            // Arrange
            TestGroup testGroup1 = new TestGroup("Group_1") { Duration = duration1 };
            TestGroup testGroup2 = new TestGroup("Group_2") { Duration = duration2 };
            ITestModel model = Substitute.For<ITestModel>();

            TreeNode treeNode1 = new TreeNode() { Tag = testGroup1 };
            TreeNode treeNode2 = new TreeNode() { Tag = testGroup2 };

            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Duration, TreeViewNodeComparer.Ascending);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }

        [TestCase(2, 4, 1)]
        [TestCase(0.5, 0.8, 1)]
        [TestCase(0.8, 1.2, 1)]
        [TestCase(0.8, 0.5, -1)]
        [TestCase(0.8, 0.8, 0)]
        public void Durationcompare_Descending_Groups_ReturnsExpectedResult(double duration1, double duration2, int expectedCompareResult)
        {
            // Arrange
            TestGroup testGroup1 = new TestGroup("Group_1") { Duration = duration1 };
            TestGroup testGroup2 = new TestGroup("Group_2") { Duration = duration2 };
            ITestModel model = Substitute.For<ITestModel>();

            TreeNode treeNode1 = new TreeNode() { Tag = testGroup1 };
            TreeNode treeNode2 = new TreeNode() { Tag = testGroup2 };

            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Duration, TreeViewNodeComparer.Descending);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }

        [TestCase("TestA", "TestB", TreeViewNodeComparer.Ascending, -1)]
        [TestCase("TestA", "TestB", TreeViewNodeComparer.Descending, 1)]
        [TestCase("TestA", "TestA", TreeViewNodeComparer.Descending, 0)]
        public void Durationcompare_NoTestResultAvailable_Groups_ReturnsExpectedResult(string groupName1, string groupName2, string sortDirection, int expectedCompareResult)
        {
            // Arrange
            TestGroup testGroup1 = new TestGroup(groupName1);
            TestGroup testGroup2 = new TestGroup(groupName2);
            ITestModel model = Substitute.For<ITestModel>();

            TreeNode treeNode1 = new TreeNode(groupName1) { Tag = testGroup1 };
            TreeNode treeNode2 = new TreeNode(groupName2) { Tag = testGroup2 };

            // Act
            IComparer comparer = TreeViewNodeComparer.GetComparer(model, TreeViewNodeComparer.Duration, sortDirection);
            int result = comparer.Compare(treeNode1, treeNode2);

            // Assert
            Assert.That(result, Is.EqualTo(expectedCompareResult));
        }
    }
}
