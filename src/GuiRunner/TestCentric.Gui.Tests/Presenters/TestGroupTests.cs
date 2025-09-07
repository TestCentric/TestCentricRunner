// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Xml;
using NUnit.Framework;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using NSubstitute;
    using TestCentric.Gui.Model.Filter;
    using TestCentric.Gui.Views;

    public class TestGroupTests
    {
        private TestGroup _group;

        [SetUp]
        public void CreateTestGroup()
        {
            _group = new TestGroup("NAME", 7);
            _group.Add(MakeTestNode("1", "Tom", "Runnable"));
            _group.Add(MakeTestNode("2", "Dick", "Explicit"));
            _group.Add(MakeTestNode("3", "Harry", "Runnable"));
        }

        [Test]
        public void GroupName()
        {
            Assert.That(_group.Name, Is.EqualTo("NAME"));
        }

        [Test]
        public void ImageIndex()
        {
            Assert.That(_group.ImageIndex, Is.EqualTo(7));
        }

        [Test]
        public void TestTestFilter()
        {
            ITestCentricTestFilter guiFilter = Substitute.For<ITestCentricTestFilter>();
            var filter = XmlHelper.CreateXmlNode(_group.GetTestFilter(guiFilter).XmlText);
            Assert.That(filter.Name, Is.EqualTo("filter"));
            Assert.That(filter.ChildNodes.Count, Is.EqualTo(1));

            filter = filter.ChildNodes[0];
            Assert.That(filter.Name, Is.EqualTo("or"));
            Assert.That(filter.ChildNodes.Count, Is.EqualTo(2));

            var ids = new List<string>();
            foreach (XmlNode child in filter.ChildNodes)
                ids.Add(child.InnerText);
            Assert.That(ids, Is.EqualTo(new string[] { "1", "3" }));
        }

        [TestCase(TestTreeView.InitIndex, "Passed", TestTreeView.SuccessIndex)]
        [TestCase(TestTreeView.InitIndex, "Failed", TestTreeView.FailureIndex)]
        [TestCase(TestTreeView.SkippedIndex, "Warning", TestTreeView.WarningIndex)]
        [TestCase(TestTreeView.SuccessIndex, "Failed", TestTreeView.FailureIndex)]
        [TestCase(TestTreeView.SuccessIndex, "Warning", TestTreeView.WarningIndex)]
        [TestCase(TestTreeView.WarningIndex, "Failed", TestTreeView.FailureIndex)]
        [TestCase(TestTreeView.InconclusiveIndex, "Passed", TestTreeView.SuccessIndex)]
        [TestCase(TestTreeView.IgnoredIndex, "Passed", TestTreeView.IgnoredIndex)]
        [TestCase(TestTreeView.FailureIndex, "Passed", TestTreeView.FailureIndex)]
        public void AddTestWithResult(int imageIndex, string outcome, int expectedImageIndex)
        {
            // Arrange
            var group = new TestGroup("TestGroup", imageIndex);
            var testNode = MakeTestNode("1", "Tom", "Runnable");
            var resultNode = new ResultNode($"<test-case id='1' result='{outcome}'/>");

            // Act
            group.Add(testNode, resultNode);

            // Assert
            Assert.That(group.ImageIndex, Is.EqualTo(expectedImageIndex));

        }

        private TestNode MakeTestNode(string id, string name, string runState)
        {
            return new TestNode(string.Format("<test-case id='{0}' name='{1}' runstate='{2}'/>", id, name, runState));
        }
    }
}
