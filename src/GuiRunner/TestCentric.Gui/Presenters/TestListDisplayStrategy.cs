// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Linq;
using System.Windows.Forms;

namespace TestCentric.Gui.Presenters
{
    using System.Collections.Generic;
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

        #region Properties

        public override string StrategyID => "TEST_LIST";

        public override string Description
        {
            get { return "Tests By " + DefaultGroupSetting; }
        }

        protected override string DefaultGroupSetting
        {
            get { return TreeConfiguration.TestListGroupBy; }
        }

        #endregion

        #region Methods

        public override void OnTestLoaded(TestNode rootNode, VisualState visualState)
        {
            ClearTree();

            var testGroups = GroupTestCases(rootNode);

            foreach (var group in testGroups)
                if (group.Items.Count() > 0)
                    _view.Nodes.Add(BuildTreeForGroup(group));

            _view.TreeView.ExpandAll();

            visualState?.ApplyTo(_view.TreeView);
            ApplyResultsToTree();

            _model.SaveProject();
        }

        private List<TestGroup> GroupTestCases(TestNode testNode)
        {
            string groupBy = DefaultGroupSetting;
            if (_grouping == null || _grouping.ID != groupBy)
                _grouping = CreateTestGrouping(groupBy);

            _grouping.LoadGroups(GetTestCases(testNode));

            return _grouping.Groups;
        }

        private TreeNode BuildTreeForGroup(TestGroup group)
        {
            bool showAssemblies = TreeConfiguration.TestListShowAssemblies;
            bool showFixtures = TreeConfiguration.TestListShowFixtures;

            if (showAssemblies || showFixtures)
            {
                var groupNode = MakeTreeNode(group, false);

                foreach (TestNode testNode in group)
                {
                    string assemblyName = null;
                    string fixtureName = null;

                    for (TestNode parent = testNode.Parent; parent != null; parent = parent.Parent)
                    {
                        if (showAssemblies && parent.IsAssembly)
                            assemblyName = parent.Name;
                        if (showFixtures && parent.IsFixture)
                            fixtureName = parent.Name;
                    }

                    TreeNode searchNode = groupNode;

                    if (showAssemblies && assemblyName != null)
                    {
                        if (groupNode.Nodes.ContainsKey(assemblyName))
                            searchNode = searchNode.Nodes[assemblyName];
                        else
                        {
                            searchNode = new TreeNode(assemblyName) { Name = assemblyName };
                            groupNode.Nodes.Add(searchNode);
                        }
                    }

                    if (showFixtures && fixtureName != null)
                    {
                        if (searchNode.Nodes.ContainsKey(fixtureName))
                            searchNode = searchNode.Nodes[fixtureName];
                        else
                        {
                            var fixtureNode = new TreeNode(fixtureName) { Name = fixtureName };
                            searchNode.Nodes.Add(fixtureNode);
                            searchNode = fixtureNode;
                        }
                    }

                    searchNode.Nodes.Add(MakeTreeNode(testNode, false));
                }

                return group.TreeNode = groupNode;
            }
            else
                return group.TreeNode = MakeTreeNode(group, true);
        }

        public override VisualState CreateVisualState()
        {
            VisualState visualState = null;

            _view.InvokeIfRequired(() =>
            {
                visualState = new VisualState("TEST_LIST", _grouping?.ID)
                {
                    ShowAssemblies = TreeConfiguration.TestListShowAssemblies,
                    ShowFixtures = TreeConfiguration.TestListShowFixtures
                }.LoadFrom(_view.TreeView);
            });

            return visualState;
        }

        /// <summary>
        /// Check if a tree node type should be shown or omitted.
        /// </summary>
        protected override bool ShowTreeNodeType(TestNode testNode)
        {
            if (testNode.IsAssembly)
                return TreeConfiguration.TestListShowAssemblies;

            if (testNode.IsFixture)
                return TreeConfiguration.TestListShowFixtures;

            return !testNode.IsSuite;
        }

        private TestSelection GetTestCases(TestNode testNode)
        {
            return new TestSelection(testNode
                .Select(n => !n.IsSuite)
                .OrderBy(s => s.Name));
        }

        #endregion
    }
}
