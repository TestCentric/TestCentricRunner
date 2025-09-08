// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections;
using System.Windows.Forms;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters
{
    /// <summary>
    /// This class is responsible for the sorting functionality of tree nodes
    /// by providing different IComparer implementations.
    /// </summary>
    public class TreeViewNodeComparer
    {
        public const string Name = "Name";
        public const string Duration = "Duration";
        public const string Ascending = "Ascending";
        public const string Descending = "Descending";


        /// <summary>
        /// Get a IComparer implementation used to sort the tree nodes
        /// The sortModes are used by the context menu items in their Tag property
        /// </summary>
        public static IComparer GetComparer(ITestModel model, string sortMode, string sortDirection)
        {
            bool ascending = sortDirection == Ascending;

            if (sortMode == Duration)
                return new DurationComparer(model, ascending);

            return new NameComparer(ascending);
        }

        /// <summary>
        /// The NameComparer uses the Name of the TestNodes (either Namespace or class or method name)
        /// It's invoked by Windows Forms for all nodes of one hierarchy level to provide a proper ordering
        /// </summary>
        private class NameComparer : IComparer
        {
            private bool _ascending;

            internal NameComparer(bool ascending)
            {
                _ascending = ascending;
            }

            public int Compare(object x, object y)
            {
                TreeNode node1 = x as TreeNode;
                TreeNode node2 = y as TreeNode;

                if (!_ascending)
                    Swap(ref node1, ref node2);

                if (node1 != null && node2 != null)
                        return node1.Text.CompareTo(node2.Text);

                return 1;
            }
        }

        /// <summary>
        /// The DurationComparer uses the Duration of the TestResults
        /// It no test results are available yet, it uses the Name.
        /// </summary>
        private class DurationComparer : IComparer
        {
            private ITestModel _model;
            private bool _ascending;

            internal DurationComparer(ITestModel model, bool ascending)
            {
                _model = model;
                _ascending = ascending;
            }

            public int Compare(object x, object y)
            {
                TreeNode node1 = x as TreeNode;
                TreeNode node2 = y as TreeNode;

                if (!_ascending)
                    Swap(ref node1, ref node2);

                TestGroup testGroup1 = node1?.Tag as TestGroup;
                TestGroup testGroup2 = node2?.Tag as TestGroup;
                if (testGroup1 != null && testGroup2 != null)
                {
                    if (testGroup1.Duration.HasValue && testGroup2.Duration.HasValue)
                        return testGroup1.Duration.Value.CompareTo(testGroup2.Duration.Value);

                    if (testGroup1.Duration.HasValue && !testGroup2.Duration.HasValue)
                        return 1;

                    if (!testGroup1.Duration.HasValue && testGroup2.Duration.HasValue)
                        return -1;

                    return node1.Text.CompareTo(node2.Text);
                }

                TestNode testNode1 = node1?.Tag as TestNode;
                TestNode testNode2 = node2?.Tag as TestNode;

                if (testNode1 == null || testNode2 == null)
                    return 1;

                ResultNode resultNode1 = _model.TestResultManager.GetResultForTest(testNode1.Id);
                ResultNode resultNode2 = _model.TestResultManager.GetResultForTest(testNode2.Id);

                if (resultNode1 != null && resultNode2 != null)
                        return resultNode1.Duration.CompareTo(resultNode2.Duration);

                return node1.Text.CompareTo(node2.Text);
            }
        }

        internal static void Swap<T>(ref T x, ref T y)
        {
            T tmp = x;
            x = y;
            y = tmp;
        }
    }
}
