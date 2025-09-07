// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Xml;

namespace TestCentric.Gui.Model
{
    public delegate bool TestNodePredicate(TestNode testNode);

    /// <summary>
    /// TestNode represents a single NUnit test in the test model.
    /// 
    /// It is used in three different ways:
    /// 
    /// 1. When tests are loaded, the XML contains all the
    /// info that defines a test.
    /// 
    /// 2. When a test is starting, only the Id, Name and
    /// FullName are available.
    /// 
    /// 3. When a test completes, information about the 
    /// result of running it is added to the full test
    /// information and the derived class ResultNode
    /// is created from it. 
    /// </summary>
    public class TestNode : ITestItem
    {
        #region Constructors

        public TestNode(XmlNode xmlNode)
        {
            IsVisible = true;
            Xml = xmlNode;

            // It's a quirk of the test engine that the test-run element does;
            // not have attributes for Name, FullName, Type, or RunState.
            if (Xml.Name == "test-run")
            {
                Xml.AddAttribute("name", "TestRun");
                Xml.AddAttribute("fullname", "TestRun");
                Xml.AddAttribute("type", "TestRun");
                Xml.AddAttribute("runstate", "Runnable");
            }
        }

        public TestNode(string xmlText) : this(XmlHelper.CreateXmlNode(xmlText)) { }

        #endregion

        #region ITestItem Implementation

        public string Name => Xml.GetAttribute("name");

        #endregion

        #region Additonal Public Properties

        public XmlNode Xml { get; }

        public bool IsSuite => Xml.Name == "test-suite" || Xml.Name == "test-run";
        public string Id => GetAttribute("id");
        public string FullName => GetAttribute("fullname") ?? Name;
        public string Type => IsSuite ? GetAttribute("type") : "TestCase";
        public bool IsAssembly => Type == "Assembly";
        public bool IsProject => Type == "Project";

        /// <summary>
        /// Controls if the TestNode should be visible or hidden in the TestTree
        /// </summary>
        public bool IsVisible { get; set; }
        public int TestCount => IsSuite ? GetAttribute("testcasecount", 0) : 1;
        public RunState RunState => GetRunState();

        public TestNode Parent { get; private set; }

        private List<TestNode> _children;
        public IList<TestNode> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new List<TestNode>();
                    foreach (XmlNode node in Xml.ChildNodes)
                        if (node.Name == "test-case" || node.Name == "test-suite")
                            _children.Add(new TestNode(node) { Parent = this });
                }

                return _children;
            }
        }

        #endregion

        #region Additional Public Methods

        public string GetAttribute(string name)
        {
            return Xml.GetAttribute(name);
        }

        public int GetAttribute(string name, int defaultValue)
        {
            return Xml.GetAttribute(name, defaultValue);
        }

        public string GetProperty(string name)
        {
            var propNode = Xml.SelectSingleNode("properties/property[@name='" + name + "']");

            return propNode?.GetAttribute("value");
        }

        public string[] GetProperties(string name)
        {
            var propList = Xml.SelectNodes("properties/property[@name='" + name + "']");
            if (propList == null || propList.Count == 0) return new string[0];

            var result = new List<string>();
            foreach (XmlNode propNode in propList)
                result.Add(propNode.GetAttribute("value"));

            return result.ToArray();
        }

        // Get a comma-separated list of all properties having the specified name
        public string GetPropertyList(string name)
        {
            return string.Join(",", GetProperties(name));
            //var propList = Xml.SelectNodes("properties/property[@name='" + name + "']");
            //if (propList == null || propList.Count == 0) return string.Empty;

            //StringBuilder result = new StringBuilder();

            //foreach (XmlNode propNode in propList)
            //{
            //    var val = propNode.GetAttribute("value");
            //    if (result.Length > 0)
            //        result.Append(',');
            //    result.Append(FormatPropertyValue(val));
            //}

            //return result.ToString();
        }

        public string[] GetAllProperties(bool displayHiddenProperties)
        {
            var items = new List<string>();

            foreach (XmlNode propNode in this.Xml.SelectNodes("properties/property"))
            {
                var name = propNode.GetAttribute("name");
                var val = propNode.GetAttribute("value");
                if (name != null && val != null)
                    if (displayHiddenProperties || !name.StartsWith("_"))
                        items.Add(name + " = " + FormatPropertyValue(val));
            }

            return items.ToArray();
        }

        public TestSelection Select(TestNodePredicate predicate)
        {
            var selection = new TestSelection();
            Accumulate(selection, this, predicate);
            return selection;
        }

        private void Accumulate(TestSelection selection, TestNode testNode, TestNodePredicate predicate)
        {
            if (predicate(testNode))
                selection.Add(testNode);
            else if (testNode.IsSuite)
                foreach (TestNode child in testNode.Children)
                    Accumulate(selection, child, predicate);
        }

        #endregion

        #region Helper Methods

        private static string FormatPropertyValue(string val)
        {
            return val == null
                ? "<null>"
                : val == string.Empty
                    ? "<empty>"
                    : val;
        }

        private RunState GetRunState()
        {
            switch (GetAttribute("runstate"))
            {
                case "Runnable":
                    return RunState.Runnable;
                case "NotRunnable":
                    return RunState.NotRunnable;
                case "Ignored":
                    return RunState.Ignored;
                case "Explicit":
                    return RunState.Explicit;
                case "Skipped":
                    return RunState.Skipped;
                default:
                    return RunState.Unknown;
            }
        }

        #endregion
    }
}
