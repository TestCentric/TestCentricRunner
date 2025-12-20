// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using System.Linq;
    using NSubstitute;
    using NUnit.Framework;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Presenters.Filter;

    [TestFixture]
    public class TreeViewModelNoGroupingTests
    {
        private ITestModel _model;

        [SetUp]
        public void Setup()
        {
            _model = Substitute.For<ITestModel>();
            _model.Settings.Gui.TestTree.ShowNamespace.Returns(true);
        }

        [Test]
        public void CreateTreeModel_RootViewModel_IsCreated()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode("<test-suite type='TestFixture' id='0' name='Fixture1'><test-case id='1' name='TestA' /> </test-suite>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
        }

        [Test]
        public void CreateTreeModel_ForComplexProject_IsCreated()
        {
            // 1. Arrange
            TestNode rootNode = TestFilterData.GetSampleTestProject();
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
        }

        [Test]
        public void CreateTreeModel_SingleFixture_CreatesCorrectHierarchy()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                        <test-case id='3' name='TestB' />
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
            TreeNodeViewModel fixtureViewModel = treeModel.RootViewModels[0];
            Assert.That(fixtureViewModel.Name, Is.EqualTo("Fixture1"));
            Assert.That(fixtureViewModel.Children.Count, Is.EqualTo(2));
            Assert.That(fixtureViewModel.Children[0].Name, Is.EqualTo("TestA"));
            Assert.That(fixtureViewModel.Children[1].Name, Is.EqualTo("TestB"));
        }

        [Test]
        public void CreateTreeModel_MultipleFixtures_CreatesMultipleRootNodes()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                    </test-suite>
                    <test-suite type='TestFixture' id='3' name='Fixture2'>
                        <test-case id='4' name='TestB' />
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(2));
            Assert.That(treeModel.RootViewModels[0].Name, Is.EqualTo("Fixture1"));
            Assert.That(treeModel.RootViewModels[1].Name, Is.EqualTo("Fixture2"));
        }

        [Test]
        public void CreateTreeModel_WithNamespaces_CreatesCorrectHierarchy()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestSuite' id='1' name='MyNamespace'>
                        <test-suite type='TestFixture' id='2' name='Fixture1'>
                            <test-case id='3' name='TestA' />
                        </test-suite>
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
            TreeNodeViewModel namespaceViewModel = treeModel.RootViewModels[0];
            Assert.That(namespaceViewModel.Name, Is.EqualTo("MyNamespace"));
            Assert.That(namespaceViewModel.Children.Count, Is.EqualTo(1));
            Assert.That(namespaceViewModel.Children[0].Name, Is.EqualTo("Fixture1"));
        }

        [Test]
        public void CreateTreeModel_WithNestedNamespaces_FoldsNamespaces()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestSuite' id='1' name='Namespace1'>
                        <test-suite type='TestSuite' id='2' name='Namespace2'>
                            <test-suite type='TestFixture' id='3' name='Fixture1'>
                                <test-case id='4' name='TestA' />
                            </test-suite>
                        </test-suite>
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
            TreeNodeViewModel foldedViewModel = treeModel.RootViewModels[0];
            Assert.That(foldedViewModel.Name, Is.EqualTo("Namespace1.Namespace2"));
            Assert.That(foldedViewModel.Children.Count, Is.EqualTo(1));
            Assert.That(foldedViewModel.Children[0].Name, Is.EqualTo("Fixture1"));
        }

        [Test]
        public void CreateTreeModel_ShowNamespaceFalse_OmitsNamespaces()
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowNamespace.Returns(false);
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='Assembly' id='1' name='Library.Test.dll'>
                        <test-suite type='TestSuite' id='2' name='MyNamespace'>
                            <test-suite type='TestFixture' id='3' name='Fixture1'>
                                <test-case id='4' name='TestA' />
                            </test-suite>
                        </test-suite>
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
            TreeNodeViewModel assemblyViewModel = treeModel.RootViewModels[0];
            Assert.That(assemblyViewModel.Name, Is.EqualTo("Library.Test.dll"));
            Assert.That(assemblyViewModel.Children.Count, Is.EqualTo(1));

            TreeNodeViewModel fixtureViewModel = assemblyViewModel.Children[0];
            Assert.That(fixtureViewModel.Name, Is.EqualTo("Fixture1"));
            Assert.That(fixtureViewModel.Children.Count, Is.EqualTo(1));

            TreeNodeViewModel testCaseViewModel = fixtureViewModel.Children[0];
            Assert.That(testCaseViewModel.Name, Is.EqualTo("TestA"));
            Assert.That(testCaseViewModel.Children.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateTreeModel_WithInvisibleNode_ExcludesInvisibleNode()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                        <test-case id='3' name='TestB' runstate='Ignored'/>
                    </test-suite>
                </test-run>");
            
            // Mark TestB as invisible
            rootNode.Children[0].Children[1].IsVisible = false;
            
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
            TreeNodeViewModel fixtureViewModel = treeModel.RootViewModels[0];
            Assert.That(fixtureViewModel.Children.Count, Is.EqualTo(1));
            Assert.That(fixtureViewModel.Children[0].Name, Is.EqualTo("TestA"));
        }

        [Test]
        public void CreateTreeModel_EmptyTestRun_CreatesNoRootViewModels()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode("<test-run id='0'></test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(0));
        }

        [Test]
        public void CreateTreeModel_ComplexHierarchy_CreatesCorrectStructure()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestSuite' id='1' name='NamespaceA'>
                        <test-suite type='TestFixture' id='2' name='Fixture1'>
                            <test-case id='3' name='Test1' />
                            <test-case id='4' name='Test2' />
                        </test-suite>
                        <test-suite type='TestFixture' id='5' name='Fixture2'>
                            <test-case id='6' name='Test3' />
                        </test-suite>
                    </test-suite>
                    <test-suite type='TestSuite' id='7' name='NamespaceB'>
                        <test-suite type='TestFixture' id='8' name='Fixture3'>
                            <test-case id='9' name='Test4' />
                        </test-suite>
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(2));
            
            // Check NamespaceA
            TreeNodeViewModel namespaceA = treeModel.RootViewModels[0];
            Assert.That(namespaceA.Name, Is.EqualTo("NamespaceA"));
            Assert.That(namespaceA.Children.Count, Is.EqualTo(2));
            Assert.That(namespaceA.Children[0].Name, Is.EqualTo("Fixture1"));
            Assert.That(namespaceA.Children[0].Children.Count, Is.EqualTo(2));
            Assert.That(namespaceA.Children[1].Name, Is.EqualTo("Fixture2"));
            Assert.That(namespaceA.Children[1].Children.Count, Is.EqualTo(1));
            
            // Check NamespaceB
            TreeNodeViewModel namespaceB = treeModel.RootViewModels[1];
            Assert.That(namespaceB.Name, Is.EqualTo("NamespaceB"));
            Assert.That(namespaceB.Children.Count, Is.EqualTo(1));
            Assert.That(namespaceB.Children[0].Name, Is.EqualTo("Fixture3"));
        }

        [Test]
        public void CreateTreeModel_ParentChildRelationships_AreSetCorrectly()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            TreeNodeViewModel fixtureViewModel = treeModel.RootViewModels[0];
            TreeNodeViewModel testViewModel = fixtureViewModel.Children[0];
            
            Assert.That(fixtureViewModel.Parent, Is.Null);
            Assert.That(testViewModel.Parent, Is.SameAs(fixtureViewModel));
        }

        [Test]
        public void CreateTreeModel_WithSampleProject_CreatesCorrectTestCount()
        {
            // 1. Arrange
            TestNode rootNode = TestFilterData.GetSampleTestProject();
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert
            Assert.That(treeModel.RootViewModels.Count, Is.EqualTo(1));
            var allTestCases = TreeNodeViewModel.GetTestCases(treeModel.RootViewModels[0]);
            Assert.That(allTestCases.Count, Is.EqualTo(8)); // Sample project has 8 test cases
        }

        [Test]
        public void CreateTreeModel_TestRunMapping_IsCreated()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                    </test-suite>
                </test-run>");
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);
            _model.LoadedTests.Returns(rootNode);

            // 2. Act
            treeModel.CreateTreeModel(rootNode);

            // 3. Assert - The test-run node should be mapped to the root view model
            TreeNodeViewModel rootViewModel = treeModel.RootViewModels[0];
            Assert.That(rootViewModel, Is.Not.Null);
        }

        [Test]
        public void OnTestFinished_UpdatesViewModelResult()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                    </test-suite>
                </test-run>");
            
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);
            treeModel.CreateTreeModel(rootNode);

            TestNode testNode = rootNode.Children[0].Children[0];
            ResultNode resultNode = new ResultNode("<test-case id='2' result='Passed'/>");
            _model.GetTestById("2").Returns(testNode);
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode);

            // 2. Act
            var updatedViewModels = treeModel.OnTestFinished(resultNode);

            // 3. Assert
            Assert.That(updatedViewModels, Is.Not.Empty);
            Assert.That(updatedViewModels[0].ResultState.Status, Is.EqualTo(TestStatus.Passed));
        }

        [Test]
        public void OnTestRunStarting_SetsInTestRunFlag()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                    </test-suite>
                </test-run>");
            
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);
            treeModel.CreateTreeModel(rootNode);

            TestNode testNode = rootNode.Children[0].Children[0];
            var testSelection = new TestSelection(new[] { testNode });
            _model.TestsInRun.Returns(testSelection);

            // 2. Act
            treeModel.OnTestRunStarting();

            // 3. Assert
            var viewModel = treeModel.RootViewModels[0].Children[0];
            Assert.That(viewModel.IsInTestRun, Is.True);
        }

        [Test]
        public void OnTestRunFinished_ClearsInTestRunFlag()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                    </test-suite>
                </test-run>");
            
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);
            treeModel.CreateTreeModel(rootNode);

            TestNode testNode = rootNode.Children[0].Children[0];
            var testSelection = new TestSelection(new[] { testNode });
            _model.TestsInRun.Returns(testSelection);
            
            treeModel.OnTestRunStarting();

            // 2. Act
            treeModel.OnTestRunFinished();

            // 3. Assert
            var viewModel = treeModel.RootViewModels[0].Children[0];
            Assert.That(viewModel.IsInTestRun, Is.False);
        }

        [Test]
        public void GetAllViewModelsInTestRun_ReturnsCorrectViewModels()
        {
            // 1. Arrange
            TestNode rootNode = new TestNode(@"
                <test-run id='0'>
                    <test-suite type='TestFixture' id='1' name='Fixture1'>
                        <test-case id='2' name='TestA' />
                        <test-case id='3' name='TestB' />
                    </test-suite>
                </test-run>");
            
            TreeViewModelNoGrouping treeModel = new TreeViewModelNoGrouping(_model);
            treeModel.CreateTreeModel(rootNode);

            TestNode testNode1 = rootNode.Children[0].Children[0];
            var testSelection = new TestSelection(new[] { testNode1 });
            _model.TestsInRun.Returns(testSelection);
            
            treeModel.OnTestRunStarting();

            // 2. Act
            var viewModelsInRun = treeModel.GetAllViewModelsInTestRun();

            // 3. Assert
            Assert.That(viewModelsInRun.Count, Is.GreaterThan(0));
            Assert.That(viewModelsInRun.All(vm => vm.IsInTestRun), Is.True);
        }
    }
}
