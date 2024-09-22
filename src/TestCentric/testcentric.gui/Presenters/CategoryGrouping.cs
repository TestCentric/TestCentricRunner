// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Xml;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Views;

    /// <summary>
    /// CategoryGrouping groups tests by category. A single
    /// test may fall into more than one group. The contents
    /// of the groups are stable once loaded but the
    /// icon changes according to the test results.
    /// </summary>
    public class CategoryGrouping : TestGrouping
    {
        private bool _includeAncestors;

        public CategoryGrouping(GroupDisplayStrategy display, bool includeAncestors) : base(display)
        {
            _includeAncestors = includeAncestors;
        }

        #region Overrides

        public override string ID => _includeAncestors ? "CATEGORY_EXTENDED" : "CATEGORY";

        public override void Load(IEnumerable<TestNode> tests)
        {
            Groups.Clear();
            Groups.Add(new TestGroup("None"));
            // Additional groups are added dynamically.

            base.Load(tests);

            if (_displayStrategy.HasResults)
                foreach (var group in Groups)
                    group.ImageIndex = _displayStrategy.CalcImageIndexForGroup(group);
        }

        public override void OnTestFinished(ResultNode result)
        {
            var imageIndex = DisplayStrategy.CalcImageIndex(result.Outcome);
            if (imageIndex >= TestTreeView.SuccessIndex)
            {
                var treeNodes = _displayStrategy.GetTreeNodesForTest(result);
                foreach (var treeNode in treeNodes)
                {
                    var parentNode = treeNode.Parent;
                    TestGroup group = treeNode.Parent?.Tag as TestGroup;
                    if (group == null)
                    {
                        continue;
                    }

                    // Category nodes are updated either as soon as a test result for all children is present or at the end of the test run (method OnTestRunFinished)
                    if (TestResultExistsForAllChildren(group))
                    {
                        int groupImageIndex = _displayStrategy.CalcImageIndexForGroup(group);
                        parentNode.SelectedImageIndex = parentNode.ImageIndex = group.ImageIndex = imageIndex;
                    }
                }
            }
        }

        public override TestGroup[] SelectGroups(TestNode testNode)
        {
            List<TestGroup> groups = new List<TestGroup>();
            string xpathExpression = "properties/property[@name='Category']";
            if (_includeAncestors)
                xpathExpression = "ancestor-or-self::*/" + xpathExpression;

            foreach (XmlNode node in testNode.Xml.SelectNodes(xpathExpression))
            {
                var groupName = node.Attributes["value"].Value;
                var group = Groups.Find((g) => g.Name == groupName);//GetGroup(groupName);
                if (group == null)
                {
                    group = new TestGroup(groupName);
                    Groups.Add(group);
                }

                groups.Add(group);
            }

            if (groups.Count == 0)
                groups.Add(Groups[0]);

            return groups.ToArray();
        }

        #endregion

        #region Helper Methods

        private void AddGroup(TestGroup group)
        {
            Groups.Add(group);

            Groups.Sort((x, y) =>
            {
                bool xNone = x.Name == "None";
                bool yNone = y.Name == "None";

                if (xNone && yNone) return 0;

                if (xNone) return 1;

                if (yNone) return -1;

                return x.Name.CompareTo(y.Name);
            });
        }

        private bool TestResultExistsForAllChildren(TestGroup group)
        {
            foreach (TestNode testItem in group)
            {
                ResultNode result = _displayStrategy.GetResultForTest(testItem);
                if (result == null)
                {
                    return false;
                }

                var imageIndexInGroup = DisplayStrategy.CalcImageIndex(result.Outcome);
                if (imageIndexInGroup < TestTreeView.SuccessIndex)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
