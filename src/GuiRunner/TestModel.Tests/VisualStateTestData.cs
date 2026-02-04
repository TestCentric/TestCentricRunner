// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms; 

namespace TestCentric.Gui.Model
{
    public class VisualStateTestData
    {
        public string DisplayStrategy;
        public string Grouping;
        public VisualState InitialVisualState;

        public VisualStateTestData(string strategy)
        {
            DisplayStrategy = strategy;
        }

        public VisualStateTestData(string strategy, string grouping)
        {
            DisplayStrategy = strategy;
            Grouping = grouping;
        }

        public TreeView GetInitialTreeView()
        {
            switch (DisplayStrategy)
            {
                case "NUNIT_TREE":
                    return CreateTreeView(
                        true,
                        TN("Assembly1", TN("NUnit", TN("Tests",
                            TN("MyFixture", TN("Test1"), TN("Test2"), TN("Test3"))))),
                        TN("Assembly2", TN("UnitTests",
                            TN("FixtureA", TN("Test4"), TN("Test5")),
                            TN("FixtureB", TN("Test6")))));

                case "TEST_LIST":
                    switch (Grouping)
                    {
                        case "UNGROUPED":
                            return CreateTreeView(
                                true,
                                TN("Test1"), TN("Test2"), TN("Test3"), TN("Test4"), TN("Test5"), TN("Test6"));

                        case "ASSEMBLY":
                            return CreateTreeView(
                                true,
                                TN("Assembly1", TN("Test1"), TN("Test2"), TN("Test3")),
                                TN("Assembly2", TN("Test4"), TN("Test5"), TN("Test6")));

                        case "FIXTURE":
                            return CreateTreeView(
                                true,
                                TN("MyFixture", TN("Test1"), TN("Test2"), TN("Test3")),
                                TN("FixtureA", TN("Test4"), TN("Test5")),
                                TN("FixtureB", TN("Test6")));

                        case "CATEGORY":
                            return CreateTreeView(
                                true,
                                TN("None", TN("Test1"), TN("Test2"), TN("Test3"), TN("Test4"), TN("Test5"), TN("Test6")));

                        case "OUTCOME":
                        case "DURATION":
                            return CreateTreeView(
                                true,
                                TN("Not Run", TN("Test1"), TN("Test2"), TN("Test3"), TN("Test4"), TN("Test5"), TN("Test6")));

                        default:
                            throw new Exception($"Grouping {Grouping} is not recognized");
                    }

                default:
                    throw new Exception($"DisplayStrategy {DisplayStrategy} is not recognized");
            }
        }

        public TreeView GetExpandedTreeView()
        {
            var tv = GetInitialTreeView();

            tv.Expand("Assembly1", "None", "Not Run", "NUnit", "Tests", "MyFixture", "Assembly2", "Tests", "FixtureA");
            tv.Check("Test1", "Test3", "FixtureA");
            tv.SelectedNode = tv.Search("Test2");
            if (DisplayStrategy == "NUNIT_TREE" || Grouping == "ASSEMBLY")
                tv.TopNode = tv.Search("Assembly1");
            else if (Grouping == "FIXTURE")
                tv.TopNode = tv.Search("MyFixture");
            else if (Grouping == "CATEGORY")
                tv.TopNode = tv.Search("None");
            else if (Grouping == "OUTCOME" || Grouping == "DURATION")
                tv.TopNode = tv.Search("Not Run");

            return tv;
        }

        public VisualState GetExpectedVisualState()
        {
            switch (DisplayStrategy)
            {
                case "NUNIT_TREE":
                    return  CreateVisualState(
                        DisplayStrategy,
                        null,
                        true,
                        true,
                        VTN("Assembly1", EXP + TOP,
                            VTN("NUnit", EXP,
                                VTN("Tests", EXP,
                                    VTN("MyFixture", EXP,
                                        VTN("Test1", CHK),
                                        VTN("Test2", SEL),
                                        VTN("Test3", CHK))))),
                        VTN("Assembly2", EXP,
                            VTN("UnitTests", EXP,
                                VTN("FixtureA", EXP + CHK))));

                case "TEST_LIST":
                    switch (Grouping)
                    {
                        case "UNGROUPED":
                            return CreateVisualState(
                                DisplayStrategy,
                                true,
                                VTN("Test1", CHK),
                                VTN("Test2", SEL),
                                VTN("Test3", CHK));

                        case "ASSEMBLY":
                            return CreateVisualState(
                                DisplayStrategy,
                                true,
                                VTN("Assembly1", EXP + TOP,
                                    VTN("Test1", CHK),
                                    VTN("Test2", SEL),
                                    VTN("Test3", CHK)),
                                VTN("Assembly2", EXP));

                        case "FIXTURE":
                            return CreateVisualState(
                                DisplayStrategy,
                                true,
                                VTN("MyFixture", EXP + TOP,
                                    VTN("Test1", CHK),
                                    VTN("Test2", SEL),
                                    VTN("Test3", CHK)),
                                VTN("FixtureA", EXP + CHK));

                        case "CATEGORY":
                            return CreateVisualState(
                                DisplayStrategy,
                                true,
                                VTN("None", EXP + TOP,
                                    VTN("Test1", CHK),
                                    VTN("Test2", SEL),
                                    VTN("Test3", CHK)));

                        case "OUTCOME":
                        case "DURATION":
                            return CreateVisualState(
                                DisplayStrategy,
                                true,
                                VTN("Not Run", EXP + TOP,
                                    VTN("Test1", CHK),
                                    VTN("Test2", SEL),
                                    VTN("Test3", CHK)));

                        default:
                            throw new Exception($"ExpectedVisualState: Test list grouping {Grouping} is not recognized");
                    }
            }

            throw new Exception($"ExpectedVisualState: Unrecognized DisplayStrategy: {DisplayStrategy}");
        }

        public VisualState GetSimulatedVisualStateAfterTestRun()
        { 
            switch(DisplayStrategy)
            {
                case "NUNIT_TREE":
                    return GetExpectedVisualState();

                case "TEST_LIST":
                    switch (Grouping)
                    {
                        case "UNGROUPED":
                        case "ASSEMBLY":
                        case "FIXTURE":
                        case "CATEGORY":
                            return GetExpectedVisualState();
                        case "OUTCOME":
                            return CreateVisualState(
                                DisplayStrategy,
                                true,
                                VTN("Passed", EXP + TOP,
                                    VTN("Test1", CHK),
                                    VTN("Test2", SEL),
                                    VTN("Test3", CHK)));
                        case "DURATION":
                            return CreateVisualState(
                                DisplayStrategy,
                                true,
                                VTN("Not Run", EXP + TOP,
                                    VTN("Test1", CHK),
                                    VTN("Test2", SEL),
                                    VTN("Test3", CHK)));
                    }

                    throw new Exception($"SimulateVisualStateAfterTestRun: Fixture list grouping {Grouping} is not recognized");
            }

            throw new Exception($"SimulateVisualStateAfterTestRun: Unrecognized DisplayStrategy: {DisplayStrategy}");
        }

        // Helper used to create a TreeNode for use in the tests
        // NOTE: Unlike the TreeNodes used in the production app, the Tag
        // is not set because VisualState doesn't make use of it.
        public static TreeNode TN(string name, params TreeNode[] childNodes)
        {
            var treeNode = new TreeNode(name);
            if (childNodes.Length > 0)
                treeNode.Nodes.AddRange(childNodes);
            return treeNode;
        }

        const int EXP = 1;
        const int CHK = 2;
        const int SEL = 4;
        const int TOP = 8;

        // Helper used to create a VisualTreeNode
        public static VisualTreeNode VTN(string name, int flags, params VisualTreeNode[] childNodes)
        {
            bool expand = (flags & EXP) == EXP;
            bool check = (flags & CHK) == CHK;
            bool select = (flags & SEL) == SEL;
            bool top = (flags & TOP) == TOP;

            var visualNode = new VisualTreeNode() { Name = name, Expanded = expand, Checked = check, Selected = select, IsTopNode = top };

            if (childNodes.Length > 0)
                visualNode.Nodes.AddRange(childNodes);

            return visualNode;
        }

        private static TreeView CreateTreeView(bool checkBoxes, params TreeNode[] treeNodes)
        {
            var tv = new TreeView() { CheckBoxes = checkBoxes };
            tv.Nodes.AddRange(treeNodes);
            return tv;
        }

        public static VisualState CreateVisualState(string strategy, bool checkBoxes = false, params VisualTreeNode[] visualTreeNodes)
        {
            return CreateVisualState(strategy, null, checkBoxes, visualTreeNodes);
        }

        public static VisualState CreateVisualState(string strategy, string grouping, bool checkBoxes = false, params VisualTreeNode[] visualTreeNodes)
        {
            return CreateVisualState(strategy, grouping, checkBoxes, false, visualTreeNodes);
        }

        public static VisualState CreateVisualState(string strategy, string grouping, bool checkBoxes = false, bool showNamespace = false, params VisualTreeNode[] visualTreeNodes)
        {
            VisualState visualState;

            switch (strategy)
            {
                case "NUNIT_TREE":
                    visualState = new VisualState(strategy) { ShowCheckBoxes = checkBoxes };
                    break;
                case "TEST_LIST":
                    visualState = new VisualState(strategy, grouping) { ShowCheckBoxes = checkBoxes };
                    break;
                default:
                    throw new Exception($"Unrecognized DisplayStrategy: {strategy}");
            }

            foreach (var visualTreeNode in visualTreeNodes)
                visualState.Nodes.Add(visualTreeNode);

            return visualState;
        }

        // Override ToString so tests deplay clearly
        public override string ToString()
        {
            return DisplayStrategy == "NUNIT_TREE"
                ? "Strategy=NUNIT_TREE"
                : $"Strategy={DisplayStrategy} Grouping={Grouping}";
        }
    }
}
