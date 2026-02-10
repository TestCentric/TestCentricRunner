// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Linq;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using Model;
    using Views;

    /// <summary>
    /// TestListDisplayStrategy is used to display lists
    /// of test cases grouped in various ways.
    /// </summary>
    public class TestListDisplayStrategy : GroupDisplayStrategy
    {
        #region Construction and Initialization

        public TestListDisplayStrategy(ITestTreeView view, ITestModel model) : base(view, model)
        {
            SetDefaultTestGrouping();
            _view.CollapseToFixturesCommand.Enabled = false;
        }

        #endregion

        #region Public Members

        public override string StrategyID => "TEST_LIST";

        public override string Description
        {
            get { return "Tests By " + DefaultGroupSetting; }
        }

        public override void OnTestLoaded(TestNode testNode, VisualState visualState)
        {
            ClearTree();

            string groupBy = DefaultGroupSetting;
            if (_grouping == null || _grouping.ID != groupBy)
            {
                _grouping = CreateTestGrouping(groupBy);
            }

            switch (groupBy)
            {
                default:
                case "UNGROUPED":
                    foreach (TestNode testCase in GetTestCases(testNode))
                        _view.Add(MakeTreeNode(testCase, false));
                    break;

                case "ASSEMBLY":
                    foreach (TestNode assembly in testNode
                        .Select((node) => node.IsSuite && node.Type == "Assembly"))
                    {
                        TreeNode treeNode = MakeTreeNode(assembly, false);

                        foreach (TestNode test in GetTestCases(assembly))
                            treeNode.Nodes.Add(MakeTreeNode(test, true));

                        _view.Add(treeNode);
                        treeNode.ExpandAll();
                    }
                    break;

                case "FIXTURE":
                    foreach (TestNode fixture in testNode
                        .Select((node) => node.IsSuite && node.Type == "TestFixture"))
                    {
                        TreeNode treeNode = MakeTreeNode(fixture, false);

                        foreach (TestNode test in GetTestCases(fixture))
                            treeNode.Nodes.Add(MakeTreeNode(test, true));

                        _view.Add(treeNode);
                        treeNode.ExpandAll();
                    }
                    break;

                case "CATEGORY":
                case "OUTCOME":
                case "DURATION":
                    _grouping.Load(GetTestCases(testNode));

                    UpdateDisplay();

                    break;
            }

            visualState?.ApplyTo(_view.TreeView);
            ApplyResultsToTree();
        }

        public override VisualState CreateVisualState()
        {
            VisualState visualState = null;

            _view.InvokeIfRequired(() =>
            {
                visualState = new VisualState("TEST_LIST", _grouping?.ID).LoadFrom(_view.TreeView);
            });

            return visualState;
        }

        #endregion

        #region Protected Members

        protected override string DefaultGroupSetting
        {
            get { return TreeConfiguration.TestListGroupBy; }
        }

        #endregion

        #region Private Members

        private TestSelection GetTestCases(TestNode testNode)
        {
            return new TestSelection(testNode
                .Select(n => !n.IsSuite)
                .OrderBy(s => s.Name));
        }

        #endregion
    }
}
