// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TestCentric.Gui.Model
{
    [TestFixture]
    internal class TestCentricTestFilterTests
    {
        private ITestModel _model;

        [SetUp]
        public void Setup()
        {
            _model = Substitute.For<ITestModel>();
        }

        [Test]
        public void Set_OutcomeFilter_FilterChangedEvent_IsInvoked()
        {
            // Arrange
            bool isCalled = false;
            var testNode = new TestNode($"<test-case id='1' />");
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { isCalled = true; });
            testFilter.OutcomeFilter = new List<string>();

            // Assert
            Assert.That(isCalled, Is.True);
        }

        [Test]
        public void Set_TextFilter_FilterChangedEvent_IsInvoked()
        {
            // Arrange
            bool isCalled = false;
            var testNode = new TestNode($"<test-case id='1' />");
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { isCalled = true; });
            testFilter.TextFilter = "";

            // Assert
            Assert.That(isCalled, Is.True);
        }

        [Test]
        public void Set_CategoryFilter_FilterChangedEvent_IsInvoked()
        {
            // Arrange
            bool isCalled = false;
            var testNode = new TestNode($"<test-case id='1' />");
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { isCalled = true; });
            testFilter.CategoryFilter = new List<string>();

            // Assert
            Assert.That(isCalled, Is.True);
        }

        [Test]
        public void AllCategories_NoCategoriesDefinedInModel_ReturnsDefaultCategory()
        {
            // Arrange
            var testNode = new TestNode($"<test-case id='1' />");
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            var allCategories = testFilter.AllCategories;

            // Assert
            Assert.That(allCategories.Count(), Is.EqualTo(1));
            Assert.That(allCategories, Contains.Item(TestCentricTestFilter.NoCategory));
        }

        [Test]
        public void AllCategories_CategoriesDefinedInModel_ReturnsModelAndDefaultCategory()
        {
            // Arrange
            var testNode = new TestNode($"<test-case id='1' />");
            _model.LoadedTests.Returns(testNode);
            _model.AvailableCategories.Returns(new List<string>() { "Feature_1" });

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            var allCategories = testFilter.AllCategories;

            // Assert
            Assert.That(allCategories.Count(), Is.EqualTo(2));
            Assert.That(allCategories, Contains.Item("Feature_1"));
            Assert.That(allCategories, Contains.Item(TestCentricTestFilter.NoCategory));
        }

        [Test]
        public void ClearFilter_AllFiltersAreReset()
        {
            // Arrange
            var testNode = new TestNode($"<test-case id='1' name='TestA' />");
            _model.LoadedTests.Returns(testNode);
            _model.AvailableCategories.Returns(new List<string>() { "Feature_1" });

            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            testFilter.TextFilter = "TestA";
            testFilter.CategoryFilter = new List<string>() { "Feature_1" };
            testFilter.OutcomeFilter = new List<string>() { "Passed" };

            // Act
            testFilter.ClearAllFilters();

            // Assert
            var allCategories = testFilter.AllCategories;
            Assert.That(allCategories.Count(), Is.EqualTo(2));
            Assert.That(allCategories, Contains.Item("Feature_1"));
            Assert.That(allCategories, Contains.Item(TestCentricTestFilter.NoCategory));

            var outcomeFilter = testFilter.OutcomeFilter;
            Assert.That(outcomeFilter.Count, Is.EqualTo(1));
            Assert.That(outcomeFilter, Contains.Item(TestCentricTestFilter.AllOutcome));

            Assert.That(testFilter.TextFilter, Is.Empty);
        }

        private static object[] FilterByOutcomeTestCases =
    {
                new object[] { new List<string>() { "Passed" },  new List<string>() { "3-1000", "3-1001", "3-1010", "3-1011", "3-1012", "3-1020", "3-1022" } },
                new object[] { new List<string>() { "Failed" }, new List<string>() { "3-1000", "3-1001", "3-1020", "3-1021" } },
                new object[] { new List<string>() { TestCentricTestFilter.NotRunOutcome }, new List<string>() { "3-1000", "3-1001", "3-1030", "3-1031", "3-1032" } },
                new object[] { new List<string>() { "Passed", TestCentricTestFilter.NotRunOutcome }, new List<string>() { "3-1000", "3-1001", "3-1010", "3-1011", "3-1012", "3-1020", "3-1022", "3-1030", "3-1031", "3-1032" } },
                new object[] { new List<string>() { "Passed", "Failed" }, new List<string>() { "3-1000", "3-1001", "3-1010", "3-1011", "3-1012", "3-1020", "3-1022", "3-1020", "3-1021" } },
                new object[] { new List<string>() { TestCentricTestFilter.NotRunOutcome, "Failed" }, new List<string>() { "3-1000", "3-1001", "3-1030", "3-1031", "3-1032", "3-1020", "3-1021" } },
                new object[] { new List<string>() { TestCentricTestFilter.AllOutcome }, new List<string>() { "3-1000", "3-1001", "3-1010", "3-1011", "3-1012", "3-1020", "3-1021", "3-1022", "3-1030", "3-1031", "3-1032" } },
            };

        [Test]
        [TestCaseSource(nameof(FilterByOutcomeTestCases))]
        public void FilterByOutcome_TestNodesAreVisible(IList<string> outcomeFilter, IList<string> expectedVisibleNodes)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "Failed",
                    CreateTestSuiteXml("3-1001", "NamespaceA", "Failed",
                        CreateTestFixtureXml("3-1010", "Fixture_1", "Passed",
                            CreateTestcaseXml("3-1011", "TestA", "Passed"),
                            CreateTestcaseXml("3-1012", "TestB", "Passed")) +
                        CreateTestFixtureXml("3-1020", "Fixture_2", "Failed",
                            CreateTestcaseXml("3-1021", "TestA", "Failed"),
                            CreateTestcaseXml("3-1022", "TestB", "Passed")) +
                        CreateTestFixtureXml("3-1030", "Fixture_3", "",
                            CreateTestcaseXml("3-1031", "TestA", ""),
                            CreateTestcaseXml("3-1032", "TestB", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            testFilter.OutcomeFilter = outcomeFilter;

            // Assert
            foreach (string testId in expectedVisibleNodes)
            {
                TestNode node = GetTestNode(testNode, testId);
                Assert.That(node.IsVisible, Is.True);
            }
        }

        private static object[] FilterByOutcomeNamespacesTestCases =
{
                new object[] { new List<string>() { "Passed" },  new List<string>() { "3-1000", "3-1000", "3-1110", "3-1111", "3-1112", "3-1200", "3-1210", "3-1212" } },
                new object[] { new List<string>() { "Failed" }, new List<string>() { "3-1000", "3-1200", "3-1210", "3-1211", "3-1400", "3-1411" } },
                new object[] { new List<string>() { TestCentricTestFilter.NotRunOutcome }, new List<string>() { "3-1000", "3-1300", "3-1310", "3-1311", "3-1400", "3-1410", "3-1412" } },
                new object[] { new List<string>() { "Passed", TestCentricTestFilter.NotRunOutcome }, new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1212", "3-1300", "3-1310", "3-1311", "3-1312", "3-1400", "3-1410", "3-1412" } },
                new object[] { new List<string>() { "Passed", "Failed" }, new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1210", "3-1211", "3-1212", "3-1400", "3-1411" } },
                new object[] { new List<string>() { TestCentricTestFilter.NotRunOutcome, "Failed" }, new List<string>() { "3-1000", "3-1200", "3-1210", "3-1211", "3-1300", "3-1310", "3-1311", "3-1312", "3-1400", "3-1410", "3-1411", "3-1412" } },
                new object[] { new List<string>() { TestCentricTestFilter.AllOutcome }, new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1210", "3-1211", "3-1212", "3-1300", "3-1310", "3-1311", "3-1312", "3-1400", "3-1410", "3-1411", "3-1412" } },
            };

        [Test]
        [TestCaseSource(nameof(FilterByOutcomeNamespacesTestCases))]
        public void FilterByOutcome_Namespaces_TestNodesAreVisible(IList<string> outcomeFilter, IList<string> expectedVisibleNodes)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "Failed",
                    CreateTestSuiteXml("3-1100", "NamespaceA", "Passed",
                        CreateTestFixtureXml("3-1110", "Fixture_1", "Passed",
                            CreateTestcaseXml("3-1111", "TestA", "Passed"),
                            CreateTestcaseXml("3-1112", "TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1200", "NamespaceB", "Failed",
                        CreateTestFixtureXml("3-1210", "Fixture_2", "Failed",
                            CreateTestcaseXml("3-1211", "TestA", "Failed"),
                            CreateTestcaseXml("3-1212", "TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1300", "NamespaceC", "",
                        CreateTestFixtureXml("3-1310", "Fixture_1", "",
                            CreateTestcaseXml("3-1311", "TestA", ""),
                            CreateTestcaseXml("3-1312", "TestB", ""))) +
                    CreateTestSuiteXml("3-1400", "NamespaceD", "Failed",
                        CreateTestFixtureXml("3-1410", "Fixture_2", "",
                            CreateTestcaseXml("3-1411", "TestC", "Failed"),
                            CreateTestcaseXml("3-1412", "TestD", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            testFilter.OutcomeFilter = outcomeFilter;

            // Assert
            foreach (string testId in expectedVisibleNodes)
            {
                TestNode node = GetTestNode(testNode, testId);
                Assert.That(node.IsVisible, Is.True);
            }
        }

        private static object[] FilterByTextTestCases =
            {
                new object[] { "NamespaceA",  new List<string>() { "3-1000", "3-1000", "3-1110", "3-1111", "3-1112" } },
                new object[] { "TestA", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1200", "3-1210", "3-1211", "3-1300", "3-1310", "3-1311" } },
                new object[] { "TestC", new List<string>() { "3-1000", "3-1300", "3-1320", "3-1321"} },
                new object[] { "_1", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1300", "3-1310", "3-1311", "3-1312", } },
                new object[] { "aryA", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1210", "3-1211", "3-1212", "3-1300", "3-1310", "3-1311", "3-1312", "3-1320", "3-1321", "3-1322" } },
                new object[] { "Namespace", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1210", "3-1211", "3-1212", "3-1300", "3-1310", "3-1311", "3-1312", "3-1320", "3-1321", "3-1322" } },
                new object[] { "Fixture", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1210", "3-1211", "3-1212", "3-1300", "3-1310", "3-1311", "3-1312", "3-1320", "3-1321", "3-1322" } },
                new object[] { "", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1210", "3-1211", "3-1212", "3-1300", "3-1310", "3-1311", "3-1312", "3-1320", "3-1321", "3-1322" } },
            };

        [Test]
        [TestCaseSource(nameof(FilterByTextTestCases))]
        public void FilterByText_TestNodesAreVisible(string textFilter, IList<string> expectedVisibleNodes)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "Failed",
                    CreateTestSuiteXml("3-1100", "LibraryA.NamespaceA", "Passed",
                        CreateTestFixtureXml("3-1110", "LibraryA.NamespaceA.Fixture_1", "Passed",
                            CreateTestcaseXml("3-1111", "LibraryA.NamespaceA.Fixture_1.TestA", "Passed"),
                            CreateTestcaseXml("3-1112", "LibraryA.NamespaceA.Fixture_1.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1200", "LibraryA.NamespaceB", "Failed",
                        CreateTestFixtureXml("3-1210", "LibraryA.NamespaceB.Fixture_2", "Failed",
                            CreateTestcaseXml("3-1211", "LibraryA.NamespaceB.Fixture_2.TestA", "Failed"),
                            CreateTestcaseXml("3-1212", "LibraryA.NamespaceB.Fixture_2.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1300", "LibraryA.NamespaceC", "",
                        CreateTestFixtureXml("3-1310", "LibraryA.NamespaceC.Fixture_1", "",
                            CreateTestcaseXml("3-1311", "LibraryA.NamespaceC.Fixture_1.TestA", ""),
                            CreateTestcaseXml("3-1312", "LibraryA.NamespaceC.Fixture_1.TestB", "")) +
                        CreateTestFixtureXml("3-1320", "LibraryA.NamespaceC.Fixture_2", "",
                            CreateTestcaseXml("3-1321", "LibraryA.NamespaceC.Fixture_2.TestC", "Failed"),
                            CreateTestcaseXml("3-1322", "LibraryA.NamespaceC.Fixture_2.TestD", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            testFilter.TextFilter = textFilter;

            // Assert
            foreach (string testId in expectedVisibleNodes)
            {
                TestNode node = GetTestNode(testNode, testId);
                Assert.That(node.IsVisible, Is.True);
            }
        }

        private static object[] FilterByCategoryTestCases =
        {
            new object[] { new[] { "Category_1" },  new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1300", "3-1310", "3-1311", "3-1312" } },
            new object[] { new[] { "Category_1", "Category_2" }, new List<string>() { "3-1100", "3-1000", "3-1110", "3-1111", "3-1112", "3-1200", "3-1211", "3-1300", "3-1311", "3-1312" } },
            new object[] { new[] { "Category_2" }, new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1211", "3-1300", "3-1312" } },
            new object[] { new[] { "Category_3" }, new List<string>() { "3-1000", "3-1300", "3-1320", "3-1321" } },
            new object[] { new[] { "Category_3", TestCentricTestFilter.NoCategory }, new List<string>() { "3-1000", "3-1200", "3-1210", "3-1212", "3-1300", "3-1320", "3-1321", "3-1322" } },
            new object[] { new[] { TestCentricTestFilter.NoCategory }, new List<string>() { "3-1000", "3-1200", "3-1210", "3-1212", "3-1300", "3-1320", "3-1322" } },
        };

        [Test]
        [TestCaseSource(nameof(FilterByCategoryTestCases))]
        public void FilterByCategory_TestNodesAreVisible(IList<string> categoryFilter, IList<string> expectedVisibleNodes)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "Failed",
                    CreateTestSuiteXml("3-1100", "LibraryA.NamespaceA", "Passed",
                        CreateTestFixtureXml("3-1110", "LibraryA.NamespaceA.Fixture_1", "Passed", new[] { "Category_1", "Category_2" },
                            CreateTestcaseXml("3-1111", "LibraryA.NamespaceA.Fixture_1.TestA", "Passed"),
                            CreateTestcaseXml("3-1112", "LibraryA.NamespaceA.Fixture_1.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1200", "LibraryA.NamespaceB", "Failed",
                        CreateTestFixtureXml("3-1210", "LibraryA.NamespaceB.Fixture_2", "Failed",
                            CreateTestcaseXml("3-1211", "LibraryA.NamespaceB.Fixture_2.TestA", "Failed", new[] { "Category_2" }),
                            CreateTestcaseXml("3-1212", "LibraryA.NamespaceB.Fixture_2.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1300", "LibraryA.NamespaceC", "",
                        CreateTestFixtureXml("3-1310", "LibraryA.NamespaceC.Fixture_1", "", new[] { "Category_1" },
                            CreateTestcaseXml("3-1311", "LibraryA.NamespaceC.Fixture_1.TestA", ""),
                            CreateTestcaseXml("3-1312", "LibraryA.NamespaceC.Fixture_1.TestB", "", new[] { "Category_2" })) +
                        CreateTestFixtureXml("3-1320", "LibraryA.NamespaceC.Fixture_2", "",
                            CreateTestcaseXml("3-1321", "LibraryA.NamespaceC.Fixture_2.TestC", "Failed", new[] { "Category_3" }),
                            CreateTestcaseXml("3-1322", "LibraryA.NamespaceC.Fixture_2.TestD", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            testFilter.CategoryFilter= categoryFilter;

            // Assert
            foreach (string testId in expectedVisibleNodes)
            {
                TestNode node = GetTestNode(testNode, testId);
                Assert.That(node.IsVisible, Is.True);
            }
        }

        private static object[] FilterByCategoryCategoryAndTextTestCases =
        {
            new object[] { new[] { "Category_1" }, new[] { "Passed" }, "", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112" } },
            new object[] { new[] { "Category_2" }, new[] { "Failed" }, "", new List<string>() { "3-1000", "3-1200", "3-1211" } },
            new object[] { new[] { "Category_2" }, new[] { TestCentricTestFilter.NotRunOutcome }, "", new List<string>() { "3-1000", "3-1300", "3-1312" } },
            new object[] { new[] { "Category_2" }, new[] { TestCentricTestFilter.AllOutcome }, "", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1111", "3-1112", "3-1200", "3-1211", "3-1300", "3-1312" } },
            new object[] { new[] { "Category_2" }, new[] { "Passed", "Failed" }, "TestB", new List<string>() { "3-1000", "3-1100", "3-1110", "3-1112" } },
            new object[] { new[] { "Category_1" }, new[] { TestCentricTestFilter.AllOutcome }, "NamespaceC", new List<string>() { "3-1000", "3-1300", "3-1311", "3-1312" } },
            new object[] { new[] { "Category_1", "Category_2"}, new[] { "Failed" }, "TestA", new List<string>() { "3-1000", "3-1200", "3-1210", "3-1211" } },
            new object[] { new[] { "Category_3" }, new[] { "Failed" }, "TestC", new List<string>() { "3-1000", "3-1300", "3-1320", "3-1321" } },
            new object[] { new[] { TestCentricTestFilter.NoCategory }, new[] { TestCentricTestFilter.NotRunOutcome }, "NamespaceC", new List<string>() { "3-1000", "3-1300", "3-1320", "3-1322" } },
        };

        [Test]
        [TestCaseSource(nameof(FilterByCategoryCategoryAndTextTestCases))]
        public void FilterByOutcomeCategoryAndText_TestNodesAreVisible(IList<string> categoryFilter, IList<string> outcomeFilter, string textFilter, IList<string> expectedVisibleNodes)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "Failed",
                    CreateTestSuiteXml("3-1100", "LibraryA.NamespaceA", "Passed",
                        CreateTestFixtureXml("3-1110", "LibraryA.NamespaceA.Fixture_1", "Passed", new[] { "Category_1", "Category_2" },
                            CreateTestcaseXml("3-1111", "LibraryA.NamespaceA.Fixture_1.TestA", "Passed"),
                            CreateTestcaseXml("3-1112", "LibraryA.NamespaceA.Fixture_1.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1200", "LibraryA.NamespaceB", "Failed",
                        CreateTestFixtureXml("3-1210", "LibraryA.NamespaceB.Fixture_2", "Failed",
                            CreateTestcaseXml("3-1211", "LibraryA.NamespaceB.Fixture_2.TestA", "Failed", new[] { "Category_2" }),
                            CreateTestcaseXml("3-1212", "LibraryA.NamespaceB.Fixture_2.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1300", "LibraryA.NamespaceC", "",
                        CreateTestFixtureXml("3-1310", "LibraryA.NamespaceC.Fixture_1", "", new[] { "Category_1" },
                            CreateTestcaseXml("3-1311", "LibraryA.NamespaceC.Fixture_1.TestA", ""),
                            CreateTestcaseXml("3-1312", "LibraryA.NamespaceC.Fixture_1.TestB", "", new[] { "Category_2" })) +
                        CreateTestFixtureXml("3-1320", "LibraryA.NamespaceC.Fixture_2", "",
                            CreateTestcaseXml("3-1321", "LibraryA.NamespaceC.Fixture_2.TestC", "Failed", new[] { "Category_3" }),
                            CreateTestcaseXml("3-1322", "LibraryA.NamespaceC.Fixture_2.TestD", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            testFilter.CategoryFilter = categoryFilter;
            testFilter.OutcomeFilter = outcomeFilter;
            testFilter.TextFilter = textFilter;

            // Assert
            foreach (string testId in expectedVisibleNodes)
            {
                TestNode node = GetTestNode(testNode, testId);
                Assert.That(node.IsVisible, Is.True);
            }
        }

        private static object[] FilterByCategoryCategoryAndTextAllInvisibleTestCases =
{
            new object[] { new[] { "Category_XY" }, new[] { TestCentricTestFilter.AllOutcome }, ""},
            new object[] { new[] { "Category_1" }, new[] { "Passed", "Failed" }, "NamespaceXY"},
            new object[] { new[] { "Category_3" }, new[] { TestCentricTestFilter.NotRunOutcome }, ""},
            new object[] { new[] { TestCentricTestFilter.NoCategory }, new[] { TestCentricTestFilter.NotRunOutcome }, "TestC"},
            new object[] { new[] { "Category_2" }, new[] { "Failed" }, "TestB"},
        };

        [Test]
        [TestCaseSource(nameof(FilterByCategoryCategoryAndTextAllInvisibleTestCases))]
        public void FilterByOutcomeCategoryAndText_AllNodesAreInvisible(IList<string> categoryFilter, IList<string> outcomeFilter, string textFilter)
        {
            // Arrange
            TestNode testNode = new TestNode(
                CreateTestSuiteXml("3-1000", "LibraryA", "Failed",
                    CreateTestSuiteXml("3-1100", "LibraryA.NamespaceA", "Passed",
                        CreateTestFixtureXml("3-1110", "LibraryA.NamespaceA.Fixture_1", "Passed", new[] { "Category_1", "Category_2" },
                            CreateTestcaseXml("3-1111", "LibraryA.NamespaceA.Fixture_1.TestA", "Passed"),
                            CreateTestcaseXml("3-1112", "LibraryA.NamespaceA.Fixture_1.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1200", "LibraryA.NamespaceB", "Failed",
                        CreateTestFixtureXml("3-1210", "LibraryA.NamespaceB.Fixture_2", "Failed",
                            CreateTestcaseXml("3-1211", "LibraryA.NamespaceB.Fixture_2.TestA", "Failed", new[] { "Category_2" }),
                            CreateTestcaseXml("3-1212", "LibraryA.NamespaceB.Fixture_2.TestB", "Passed"))) +
                    CreateTestSuiteXml("3-1300", "LibraryA.NamespaceC", "",
                        CreateTestFixtureXml("3-1310", "LibraryA.NamespaceC.Fixture_1", "", new[] { "Category_1" },
                            CreateTestcaseXml("3-1311", "LibraryA.NamespaceC.Fixture_1.TestA", ""),
                            CreateTestcaseXml("3-1312", "LibraryA.NamespaceC.Fixture_1.TestB", "", new[] { "Category_2" })) +
                        CreateTestFixtureXml("3-1320", "LibraryA.NamespaceC.Fixture_2", "",
                            CreateTestcaseXml("3-1321", "LibraryA.NamespaceC.Fixture_2.TestC", "Failed", new[] { "Category_3" }),
                            CreateTestcaseXml("3-1322", "LibraryA.NamespaceC.Fixture_2.TestD", "")))));
            _model.LoadedTests.Returns(testNode);

            // Act
            TestCentricTestFilter testFilter = new TestCentricTestFilter(_model, () => { });
            testFilter.Init();
            testFilter.CategoryFilter = categoryFilter;
            testFilter.OutcomeFilter = outcomeFilter;
            testFilter.TextFilter = textFilter;

            // Assert
            AssertTestNodeIsInvisible(testNode);
        }

        private void AssertTestNodeIsInvisible(TestNode testNode)
        {
            Assert.That(testNode.IsVisible, Is.False, $"TestNode {testNode.Id} is not invisible.");
            foreach (TestNode child in testNode.Children)
                AssertTestNodeIsInvisible(child);
        }

        private TestNode GetTestNode(TestNode testNode, string testId)
        {
            if (testNode.Id == testId) 
                return testNode;

            foreach (TestNode child in testNode.Children)
            {
                TestNode n = GetTestNode(child, testId);
                if (n != null)
                    return n;
            }

            return null;
        }

        private string CreateTestcaseXml(string testId, string testName, string outcome)
        {
            return CreateTestcaseXml(testId, testName, outcome, new List<string>());
        }

        private string CreateTestcaseXml(string testId, string testName, string outcome, IList<string> categories)
        {
            string str = $"<test-case id='{testId}' fullname='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            str += "</test-case> ";

            if (!string.IsNullOrEmpty(outcome))
                _model.GetResultForTest(testId).Returns(new ResultNode($"<test-case id='{testId}' result='{outcome}' />"));
            return str;
        }

        private string CreateTestFixtureXml(string testId, string testName, string outcome, params string[] testCases)
        {
            return CreateTestFixtureXml(testId, testName, outcome, new List<string>(), testCases);
        }

        private string CreateTestFixtureXml(string testId, string testName, string outcome, IEnumerable<string> categories, params string[] testCases)
        {
            string str = $"<test-suite type='TestFixture' id='{testId}'  fullname='{testName}'> ";

            str += "<properties> ";
            foreach (string category in categories)
                str += $"<property name='Category' value='{category}' /> ";
            str += "</properties> ";

            foreach (string testCase in testCases)
                str += testCase;

            str += "</test-suite>";

            if (!string.IsNullOrEmpty(outcome))
                _model.GetResultForTest(testId).Returns(new ResultNode($"<test-case id='{testId}' result='{outcome}' />"));

            return str;
        }

        private string CreateTestSuiteXml(string testId, string testName, string outcome, params string[] testSuites)
        {
            string str = $"<test-suite type='TestSuite' id='{testId}' fullname='{testName}'> ";
            foreach (string testSuite in testSuites)
                str += testSuite;

            str += "</test-suite>";

            if (!string.IsNullOrEmpty(outcome))
                _model.GetResultForTest(testId).Returns(new ResultNode($"<test-case id='{testId}' result='{outcome}' />"));

            return str;
        }
    }
}
