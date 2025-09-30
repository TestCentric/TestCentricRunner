// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

namespace TestCentric.Engine.Api
{
    public class TestFilterTests
    {
        [Test]
        public void EmptyFilter()
        {
            var filter = NUnit.Engine.TestFilter.Empty;
            Assert.That(filter.Text, Is.EqualTo("<filter/>"));
        }

        [Test]
        public void FilterWithOneTest()
        {
            string text = "<filter><tests><test>My.Test.Name</test></tests></filter>";
            var filter = new NUnit.Engine.TestFilter(text);
            Assert.That(filter.Text, Is.EqualTo(text));
        }

        [Test]
        public void FilterWithThreeTests()
        {
            string text = "<filter><tests><test>My.First.Test</test><test>My.Second.Test</test><test>My.Third.Test</test></tests></filter>";
            var filter = new NUnit.Engine.TestFilter(text);
            Assert.That(filter.Text, Is.EqualTo(text));
        }
    }
}
