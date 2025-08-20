﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Linq;
using NUnit.Framework;

namespace TestCentric.Gui.Model
{
    public class TestSelectionTests
    {
        private TestSelection _selection;

        [SetUp]
        public void CreateSelection()
        {
            _selection = new TestSelection();
            _selection.Add(MakeTestNode("1", "Tom", "Passed"));
            _selection.Add(MakeTestNode("2", "Dick", "Failed"));
            _selection.Add(MakeTestNode("3", "Harry", "Passed"));
        }

        [Test]
        public void RemoveTestNode()
        {
            _selection.RemoveId("2");
            Assert.That(_selection.Count, Is.EqualTo(2));
        }

        private TestNode MakeTestNode(string id, string name, string result)
        {
            return new TestNode(XmlHelper.CreateXmlNode(string.Format("<test-case id='{0}' name='{1}' result='{2}'/>", id, name, result)));
        }
    }
}
