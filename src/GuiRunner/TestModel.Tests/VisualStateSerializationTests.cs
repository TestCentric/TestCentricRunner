// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using System.Xml;
using System.Collections;

namespace TestCentric.Gui.Model
{
    public class VisualStateSerializationTests : VisualStateTestBase
    {
        // XML provided as part of issue #945. Note that
        // V1 Gui does not serialize DisplayStrategy because
        // it doesn't support multiple strategies.
        const string V1_XML =
            "<VisualState ShowCheckBoxes=\"false\">\r\n" +
                //"<TopNode>0-1002</TopNode>\r\n" +
                //"<SelectedNode>0-1001</SelectedNode>\r\n" +
                //"<SelectedCategories/>\r\n" +
                //"<ExcludeCategories>false</ExcludeCategories>\r\n" +
                "<Nodes>\r\n" +
                    "<Node Id=\"0-1002\" Expanded=\"true\"/>\r\n" +
                    "<Node Id=\"0-1003\" Expanded=\"true\"/>\r\n" +
                    "<Node Id=\"0-1000\" Expanded=\"true\"/>\r\n" +
                "</Nodes>\r\n" +
            "</VisualState>";

        [Test]
        public void CanDeserializeVersion1VisualState()
        {
            var reader = new StringReader(V1_XML);

            var vs = VisualState.LoadFrom(reader);

            Assert.That(vs.DisplayStrategy, Is.EqualTo("NUNIT_TREE"));
            Assert.That(vs.ShowCheckBoxes, Is.EqualTo(false));
            Assert.That(vs.ShowNamespaces, Is.EqualTo(true));
        }

        [TestCaseSource(typeof(VisualStateSerializationData))]
        public void SerializeVisualState(VisualState vs)
        {
            // Serialize the VisualState
            var writer = new StringWriter();
            Assert.That(() => vs.Save(writer), Throws.Nothing);

            //TestContext.Progress.WriteLine($"Serialized Output...\r\n{writer.ToString()}");

            // Get result as XML document
            var doc = new XmlDocument();
            doc.LoadXml(writer.ToString());
            var docElement = doc.DocumentElement;
            var firstChild = docElement.FirstChild;
            var topNodes = firstChild.SelectNodes("Node");

            Assert.Multiple(() =>
            {
                Assert.That(docElement.Name, Is.EqualTo("VisualState"));
                Assert.That(docElement.GetAttribute("DisplayStrategy"), Is.EqualTo(vs.DisplayStrategy));
                Assert.That(docElement.GetAttribute("ShowCheckBoxes"), Is.EqualTo(vs.ShowCheckBoxes ? "True" : ""));
                Assert.That(docElement.GetAttribute("ShowNamespaces"), Is.EqualTo(!vs.ShowNamespaces ? "False" : ""));
                Assert.That(firstChild.Name, Is.EqualTo("Nodes"));
                Assert.That(topNodes.Count, Is.EqualTo(vs.Nodes.Count));
                for (int i = 0; i < topNodes.Count; i++)
                    CheckVisualTreeNode(topNodes[i], vs.Nodes[i]);
            });

            void CheckVisualTreeNode(XmlNode xmlNode, VisualTreeNode vsNode)
            {
                Assert.That(xmlNode.GetAttribute("Name"), Is.EqualTo(vsNode.Name), "Name");
                Assert.That(xmlNode.GetAttribute("Expanded"), Is.Null.Or.EqualTo("True"), "Expanded");
                Assert.That(xmlNode.GetAttribute("Checked"), Is.Null.Or.EqualTo("True"), "Checked");
                Assert.That(xmlNode.GetAttribute("Selected"), Is.Null.Or.EqualTo("True"), "Selected");
                Assert.That(xmlNode.GetAttribute("IsTopNode"), Is.Null.Or.EqualTo("True"), "IsTopNode");

                int expectedCount = vsNode.Nodes.Count;
                if (expectedCount > 0)
                {
                    Assert.That(xmlNode.FirstChild.Name, Is.EqualTo("Nodes"));
                    var childNodes = xmlNode.FirstChild.SelectNodes("Node");
                    Assert.That(childNodes.Count, Is.EqualTo(expectedCount));

                    for (int i = 0; i < expectedCount; i++)
                        CheckVisualTreeNode(childNodes[i], vsNode.Nodes[i]);
                }
            }
        }

        [TestCaseSource(typeof(VisualStateSerializationData))]
        public void RoundTrip(VisualState original)
        {
            // Serialize the VisualState
            var writer = new StringWriter();
            Assert.That(() => original.Save(writer), Throws.Nothing);

            //TestContext.Progress.WriteLine($"Serialized Output...\r\n{writer.ToString()}");

            // Deserialize using the same data
            var reader = new StringReader(writer.ToString());
            var restored = VisualState.LoadFrom(reader);

            Assert.Multiple(() =>
            {
                Assert.That(restored.DisplayStrategy, Is.EqualTo(original.DisplayStrategy));
                Assert.That(restored.GroupBy, Is.EqualTo(original.GroupBy));
                Assert.That(restored.ShowCheckBoxes, Is.EqualTo(original.ShowCheckBoxes), "ShowCheckBoxes");
                Assert.That(restored.Nodes.Count, Is.EqualTo(original.Nodes.Count), "VisualState.Nodes.Count");
                for (int i = 0; i < original.Nodes.Count; i++)
                    CheckVisualTreeNode(original.Nodes[i], restored.Nodes[i]);
            });

            void CheckVisualTreeNode(VisualTreeNode originalNode, VisualTreeNode restoredNode)
            {
                Assert.That(restoredNode.Name, Is.EqualTo(originalNode.Name));
                Assert.That(restoredNode.Expanded, Is.EqualTo(originalNode.Expanded));
                Assert.That(restoredNode.Checked, Is.EqualTo(originalNode.Checked));
                Assert.That(restoredNode.Selected, Is.EqualTo(originalNode.Selected));
                Assert.That(restoredNode.IsTopNode, Is.EqualTo(originalNode.IsTopNode));
                Assert.That(restoredNode.Nodes.Count, Is.EqualTo(originalNode.Nodes.Count), "VisualTreeNode.Nodes.Count");
                for (int i = 0; i < originalNode.Nodes.Count; i++)
                    CheckVisualTreeNode(originalNode.Nodes[i], restoredNode.Nodes[i]);
            }
        }

        public class VisualStateSerializationData : VisualStateTestBase, IEnumerable<TestCaseData>
        {
            public IEnumerator<TestCaseData> GetEnumerator()
            {
                // NUnitTreeDisplayStrategy
                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE"))
                    .SetName("NUnitTree_Empty)");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        checkBoxes: true))
                    .SetName("NUnitTree_EmptyWithOneAttribute)");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        null,
                        checkBoxes: true,
                        showNamespace: true))
                    .SetName("NUnitTree_Empty_ShowNamespaceIsTrue)");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        null,
                        checkBoxes: true,
                        showNamespace: false))
                    .SetName("NUnitTree_Empty_ShowNamespaceIsFalse)");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        checkBoxes: false,
                        VTN("Assembly1", 0),
                        VTN("Assembly2", 0)))
                    .SetName("NUnitTree_TwoEmptyAssemblies");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        checkBoxes: false,
                        VTN("Assembly1", 0,
                            VTN("FixtureA", 0),
                            VTN("FixtureB", 0)),
                        VTN("Assembly2", 0)))
                    .SetName("NUnitTree_TwoAssembliesWithEmptyFixtures");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
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
                                VTN("FixtureA", EXP + CHK,
                                    VTN("Test1", CHK),
                                    VTN("Test2", CHK))))))
                    .SetName("NUnitTree_ComplexVisualState");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        false,
                        VTN("test.dll", EXP + TOP,
                            VTN("test1", EXP,
                                VTN("test2", EXP,
                                    VTN("test3", EXP,
                                        VTN("test4", EXP,
                                            VTN("test5", EXP,
                                                VTN("test6", EXP,
                                                    VTN("test7", EXP,
                                                        VTN("test8", EXP,
                                                            VTN("test9", EXP))))))))))))
                    .SetName("NUnitTree_DeeplyNested");

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        false,
                        VTN("test.dll", EXP + TOP,
                            VTN("name1", EXP,
                                VTN("name2", EXP,
                                    VTN("name3_1", EXP,
                                        VTN("name3_1", EXP)),
                                    VTN("name3_2", EXP,
                                        VTN("name3_2", EXP)),
                                    VTN("name3_3", EXP,
                                        VTN("name3_3", EXP)))))))
                    .SetName("NUnitTree_Issue946");

                // TestListDisplayStrategy, Group by Assembly

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "NUNIT_TREE",
                        true,
                        VTN("Assembly1", EXP + TOP,
                            VTN("Test1", CHK),
                            VTN("Test2", SEL),
                            VTN("Test3", CHK)),
                        VTN("Assembly2", EXP,
                            VTN("Test1", CHK),
                            VTN("Test2", CHK))))
                    .SetName("TestListByAssembly_ComplexVisualState");

                // TestListDisplayStrategy, Group by Category

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "TEST_LIST", "CATEGORY",
                        true,
                        VTN("Category1", EXP + TOP,
                            VTN("Test1", CHK),
                            VTN("Test2", SEL),
                            VTN("Test3", CHK)),
                        VTN("Category2", EXP,
                            VTN("Test1", CHK),
                            VTN("Test2", CHK))))
                    .SetName("TestListByCategory_ComplexVisualState");

                // TestListDisplayStrategy, Group by Fixture

                yield return new TestCaseData(
                    VisualStateTestData.CreateVisualState(
                        "TEST_LIST", "FIXTURE",
                        true,
                        VTN("MyFixture", EXP + TOP,
                            VTN("Test1", CHK),
                            VTN("Test2", SEL),
                            VTN("Test3", CHK)),
                        VTN("FixtureA", EXP,
                            VTN("Test1", CHK),
                            VTN("Test2", CHK))))
                    .SetName("TestListByFxiture_ComplexVisualState");
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
