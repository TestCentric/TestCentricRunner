// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace TestCentric.Gui.Model
{
    /// <summary>
    /// Utility class for working with TestFilters.
    /// </summary>
    public class TestFilter
    {
        public TestFilter(string xmlText)
        {
            XmlText = xmlText;
            Xml = XmlHelper.CreateXmlNode(xmlText);
            InnerXml = Xml.InnerXml;
            IsEmpty = string.IsNullOrEmpty(InnerXml);
        }

        public static TestFilter Empty { get; } = new TestFilter("<filter/>");

        public string XmlText { get; }

        public XmlNode Xml { get; }

        public string InnerXml { get; }

        public bool IsEmpty { get; }

        public TestCentric.Engine.TestFilter AsNUnitFilter()
        {
            return IsEmpty
                ? TestCentric.Engine.TestFilter.Empty
                : new TestCentric.Engine.TestFilter(XmlText);
        }

        #region Static Methods to Create TestFilters

        public static TestFilter MakeIdFilter(TestNode test)
        {
            if (test.Type == "TestRun")
                return TestFilter.Empty;

            return new TestFilter($"<filter><id>{test.Id}</id></filter>");
        }

        public static TestFilter MakeIdFilter(IEnumerable<TestNode> testNodes)
        {
            if (testNodes.Count() == 1)
                return MakeIdFilter(testNodes.First());

            StringBuilder sb = new StringBuilder("<filter><or>");

            foreach (TestNode test in testNodes)
                sb.Append($"<id>{test.Id}</id>");

            sb.Append("</or></filter>");

            return new TestFilter(sb.ToString());
        }

        /// <summary>
        /// Creates a category filter matching all tests without any defined category 
        /// </summary>
        public static TestFilter MakeEmptyCategoryFilter()
        {
            return new TestFilter("<filter><not><cat re=\"1\">.+</cat></not></filter>");
        }

        public static TestFilter MakeCategoryFilter(IList<string> categories)
        {
            var sb = new StringBuilder("<filter>");

            if (categories.Count > 1)
                sb.Append("<or>");

            foreach (string category in categories)
                sb.Append($"<cat>{category}</cat>");

            if (categories.Count > 1)
                sb.Append("</or>");

            sb.Append("</filter>");

            return new TestFilter(sb.ToString());
        }

        /// <summary>
        /// Creates a test name filter using a regular expression to match 'text' at any location
        /// </summary>
        public static TestFilter MakeTestFilter(string text)
        {
            return new TestFilter($"<filter><test re=\"1\">(.)*{text}(.)*</test></filter>");
        }

        public static TestFilter MakeNotFilter(TestFilter filter)
        {
            return new TestFilter($"<filter><not>{filter.InnerXml}</not></filter>");
        }

        public static TestFilter MakeAndFilter(params TestFilter[] filters)
        {
            var nonEmptyFilters = filters.Where(f => !f.IsEmpty).ToList();
            if (nonEmptyFilters.Count == 0)
                return Empty;
            if (nonEmptyFilters.Count == 1)
                return nonEmptyFilters.First();

            StringBuilder sb = new StringBuilder("<filter><and>");
            foreach (var filter in nonEmptyFilters)
                sb.Append(filter.InnerXml);

            sb.Append("</and></filter>");

            return new TestFilter(sb.ToString());
        }

        public static TestFilter MakeOrFilter(params TestFilter[] filters)
        {
            var nonEmptyFilters = filters.Where(f => !f.IsEmpty).ToList();

            if (nonEmptyFilters.Count == 0)
                return Empty;
            if (nonEmptyFilters.Count == 1)
                return nonEmptyFilters.First();

            StringBuilder sb = new StringBuilder("<filter><or>");
            foreach (var filter in nonEmptyFilters)
                sb.Append(filter.InnerXml);

            sb.Append("</or></filter>");

            return new TestFilter(sb.ToString());
        }

        #endregion
    }
}
