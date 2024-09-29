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
    /// FixtureListDisplayStrategy is used to display lists
    /// of fixtures grouped in various ways.
    /// </summary>
    public class FixtureListDisplayStrategy : GroupDisplayStrategy
    {
        #region Construction and Initialization

        public FixtureListDisplayStrategy(ITestTreeView view, ITestModel model) : base(view, model)
        {
            SetDefaultTestGrouping();
            _view.CollapseToFixturesCommand.Enabled = true;
        }

        #endregion

        #region Public Members

        public override string StrategyID => "FIXTURE_LIST";

        public override string Description
        {
            get { return "Fixtures By " + DefaultGroupSetting; }
        }

        protected override VisualState CreateVisualState() => new VisualState("FIXTURE_LIST", _grouping?.ID).LoadFrom(_view.TreeView);

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
                case "ASSEMBLY":
                    foreach (TestNode assembly in testNode
                        .Select((node) => node.IsSuite && node.Type == "Assembly"))
                    {
                        TreeNode treeNode = MakeTreeNode(assembly, false);

                        foreach (TestNode fixture in GetTestFixtures(assembly))
                            treeNode.Nodes.Add(MakeTreeNode(fixture, true));

                        _view.Add(treeNode);
                        CollapseToFixtures(treeNode);
                    }
                    break;

                case "CATEGORY":
                case "OUTCOME":
                case "DURATION":
                    _grouping.Load(GetTestFixtures(testNode));

                    UpdateDisplay();

                    break;
            }

            visualState?.ApplyTo(_view.TreeView);
        }

        #endregion

        #region Protected Members

        protected override string DefaultGroupSetting
        {
            get { return _settings.Gui.TestTree.FixtureList.GroupBy; }
            set { _settings.Gui.TestTree.FixtureList.GroupBy = value; }
        }

        #endregion

        #region Private Members

        private TestSelection GetTestFixtures(TestNode testNode)
        {
            return new TestSelection(testNode
                    .Select((node) => node.Type == "TestFixture")
                    .OrderBy(s => s.Name));
        }

        #endregion
    }
}
