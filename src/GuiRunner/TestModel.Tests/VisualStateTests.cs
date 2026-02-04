// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Windows.Forms;
using NUnit.Framework;

namespace TestCentric.Gui.Model
{
    [TestFixtureSource(nameof(TestData))]
    public class VisualStateTests : VisualStateTestBase
    {
        private VisualStateTestData _data;
        private TreeView ExpandedTreeView;
        private VisualState ExpectedVisualState;

        public VisualStateTests(VisualStateTestData data)
        {
            _data = data;
            ExpandedTreeView = data.GetExpandedTreeView();
            ExpectedVisualState = data.GetExpectedVisualState();
        }

        [Test]
        public void CanCreateVisualStateFromTree()
        {
            var visualState = new VisualState().LoadFrom(ExpandedTreeView);

            Assert.Multiple(() =>
            {
                Assert.That(visualState.ShowCheckBoxes, Is.EqualTo(ExpandedTreeView.CheckBoxes));
                Assert.That(visualState.Nodes, Is.EqualTo(ExpectedVisualState.Nodes));
            });
        }

        [Test]
        public void CanApplyVisualStateToTree_Unchanged()
        {
            var treeView = _data.GetInitialTreeView();

            ExpectedVisualState.ApplyTo(treeView);

            VerifyTreeView(treeView, selectedNode: "Test2");
        }

        [Test]
        public void CanApplyVisualStateToTree_TestsAdded()
        {
            var treeView = _data.GetInitialTreeView();

            AddSomeTests(treeView);

            ExpectedVisualState.ApplyTo(treeView);

            VerifyTreeView(treeView, selectedNode: "Test2");
        }

        [Test]
        public void CanApplyVisualStateToTree_TestsAddedAndDeleted()
        {
            var treeView = _data.GetInitialTreeView();

            AddSomeTests(treeView);

            // Test4 and Test5 are deleted before FixtureA, which may not be displayed
            treeView.Remove("Test3", "Test4", "Test5", "FixtureA");

            ExpectedVisualState.ApplyTo(treeView);

            VerifyTreeView(treeView, selectedNode: "Test2");
        }

        private void AddSomeTests(TreeView treeView)
        {
            // Add NewFixture, SomeTest, NewTest, FixtureB, Test1
            if (_data.DisplayStrategy == "TEST_LIST") // Just add the test cases
            {
                treeView.Search("Test1").AddSibling(TN("SomeTest"));
                treeView.Search("Test1").AddSibling(TN("NewTest"));
                treeView.Search("Test4").AddSibling(TN("Test7"));
            }
            else // Add both fixtures and cases
            {
                treeView.Search("MyFixture").AddSibling(TN("New Fixture", TN("SomeTest")));
                treeView.Search("MyFixture").Nodes.Add(TN("NewTest"));
                treeView.Search("FixtureA").AddSibling(TN("FixtureB", TN("Test7")));
            }
        }

        [Test]
        public void CanApplyVisualStateToTree_SingleDeletion()
        {
            var treeView = _data.GetInitialTreeView();

            treeView.Remove("Test2");

            ExpectedVisualState.ApplyTo(treeView);

            VerifyTreeView(treeView, selectedNode: null);
        }

        [Test]
        public void CanApplyVisualStateToTree_MultipleDeletions()
        {
            var treeView = _data.GetInitialTreeView();

            // Test4 and Test5 are deleted before FixtureA, which may not be displayed
            treeView.Remove("Test2", "Test4", "Test5", "FixtureA");

            ExpectedVisualState.ApplyTo(treeView);

            VerifyTreeView(treeView, selectedNode: null);
        }

        [Test]
        // This test verifies that the visual state is saved and restored
        // in the form of the tree created by a test run, which may differ
        // from the initial form upon load.
        public void CanSaveAndReloadVisualStateAfterTestRun()
        {
            StringWriter writer = new StringWriter();
            var visualState = _data.GetSimulatedVisualStateAfterTestRun();
            visualState.Save(writer);

            string output = writer.GetStringBuilder().ToString();
            StringReader reader = new StringReader(output);

            VisualState restoredState = VisualState.LoadFrom(reader);

            Assert.Multiple(() =>
            {
                Assert.That(restoredState.ShowCheckBoxes, Is.EqualTo(visualState.ShowCheckBoxes));
                Assert.That(restoredState.ShowNamespace, Is.EqualTo(visualState.ShowNamespace));
                // TODO: Categories not yet supported
                //Assert.AreEqual(ExpectedVisualState.SelectedCategories, restoredState.SelectedCategories);
                //Assert.AreEqual(ExpectedVisualState.ExcludeCategories, restoredState.ExcludeCategories);
                Assert.That(restoredState.Nodes, Is.EqualTo(visualState.Nodes));
            });
        }

        [Test]
        // This test verifies that when the visual state created after a test
        // run is applied, the normal tree shape prior to a run is restored.
        // E.g.: groups like "Passed", "Failed", etc. all become "Not Run".
        public void CanApplyVisualState_AfterTestRun()
        {
            var treeView = _data.GetInitialTreeView();
            var visualState = _data.GetSimulatedVisualStateAfterTestRun();

            visualState.ApplyTo(treeView);

            VerifyTreeView(treeView, selectedNode: "Test2");
        }

        #region Helper method to handle common asserts

        private void VerifyTreeView(TreeView treeView, string selectedNode = null)
        {
            var expectedTopNode = treeView.Nodes[0].Text;

            Assert.Multiple(() =>
            {
                Assert.That(treeView.CheckBoxes, Is.True, "CheckBoxes");
                Assert.That(treeView.SelectedNode?.Text, Is.EqualTo(selectedNode), "SelectedNode");
                Assert.That(treeView.TopNode?.Text, Is.EqualTo(expectedTopNode), "TopNode");

                var fixtureA = treeView.Search("FixtureA");

                // NOTE: The following code contains lots of duplication by design.
                // It's kept this way to make it easier to see the assertions for
                // each strategy and grouping all together in one place.

                switch (_data.DisplayStrategy)
                {
                    case "NUNIT_TREE":
                        Assert.That(treeView.Search("Assembly1").IsExpanded, "Assembly1 not expanded");
                        Assert.That(treeView.Search("Assembly2").IsExpanded, "Assembly2 not expanded");
                        Assert.That(treeView.Search("NUnit").IsExpanded, "NUnit namespace not expanded");
                        Assert.That(treeView.Search("Tests").IsExpanded, "NUnit/Tests namespace not expanded");
                        Assert.That(treeView.Search("UnitTests").IsExpanded, "UnitTests namespace not expanded");
                        Assert.That(treeView.Search("MyFixture").IsExpanded, "MyFixture not expanded");
                        if (fixtureA != null) // In case it was deleted
                        {
                            Assert.That(fixtureA.IsExpanded, "MyFixture not expanded");
                            Assert.That(fixtureA.Checked, "FixtureA not checked");
                        }
                        break;

                    case "TEST_LIST":
                        switch (_data.Grouping)
                        {
                            case "Assembly":
                                Assert.That(treeView.Search("Assembly1").IsExpanded, "Assembly1 not expanded");
                                Assert.That(treeView.Search("Assembly2").IsExpanded, "Assembly2 not expanded");
                                break;
                            case "FIXTURE":
                                Assert.That(treeView.Search("MyFixture").IsExpanded, "MyFixture not expanded");
                                if (fixtureA != null) // In case it was deleted
                                {
                                    Assert.That(fixtureA.IsExpanded, "MyFixture not expanded");
                                    Assert.That(fixtureA.Checked, "FixtureA not checked");
                                }
                                break;
                            case "CATEGORY":
                                Assert.That(treeView.Search("None").IsExpanded, "Category 'None' not expanded");
                                break;
                            case "OUTCOME":
                            case "DURATION":
                                Assert.That(treeView.Search("Not Run").IsExpanded, "'Not Run' not expanded");
                                break;
                        }
                        break;

                }

                Assert.That(treeView.Search("Test1").Checked, "Test1 not checked");
                var test3 = treeView.Search("Test3");
                if (test3 != null) // Removed in some scenarios
                    Assert.That(test3.Checked, "Test3 not checked");
            });
        }

        #endregion

        #region Test Data

        private static VisualStateTestData[] TestData = new VisualStateTestData[]
        {
            new VisualStateTestData("NUNIT_TREE"),
            new VisualStateTestData("TEST_LIST", "ASSEMBLY"),
            new VisualStateTestData("TEST_LIST", "FIXTURE"),
            new VisualStateTestData("TEST_LIST", "CATEGORY"),
            new VisualStateTestData("TEST_LIST", "OUTCOME"),
            new VisualStateTestData("TEST_LIST", "DURATION")
        };

        #endregion
    }
}
