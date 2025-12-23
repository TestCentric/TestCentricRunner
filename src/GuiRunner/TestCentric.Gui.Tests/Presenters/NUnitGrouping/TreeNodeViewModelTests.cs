// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    using NSubstitute;
    using NUnit.Framework;
    using TestCentric.Gui.Model;
    using TestCentric.Gui.Presenters.Filter;

    [TestFixture]
    internal class TreeNodeViewModelTests
    {
        private ITestModel _model;

        [SetUp]
        public void Setup()
        {
            _model = Substitute.For<ITestModel>();
        }

        [Test]
        public void Name()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, testNode, "TestA");

            // 2. Act
            string name = viewModel.Name;

            // 3. Assert
            Assert.That(name, Is.EqualTo("TestA"));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DisplayName_ForTestCase(bool showTestDuration)
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowTestDuration.Returns(showTestDuration);
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");

            // 2. Act
            string name = viewModel.DisplayName;

            // 3. Assert
            Assert.That(name, Is.EqualTo("TestA"));
        }

        [Test]
        public void DisplayName_ForFixture_WithoutTestcases()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-suite type='TestFixture' id='0' name='Fixture1'></test-suite>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            string name = viewModel.DisplayName;

            // 3. Assert
            Assert.That(name, Is.EqualTo("Fixture1 (0)"));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void DisplayName_ForFixture_WithTestcases(bool showTestDuration)
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowTestDuration.Returns(showTestDuration);
            TestNode testNode = new TestNode("<test-suite type='TestFixture' id='0' name='Fixture1'><test-case id='1' name='TestA' /> </test-suite>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            string name = viewModel.DisplayName;

            // 3. Assert
            Assert.That(name, Is.EqualTo("Fixture1 (1)"));
        }

        [Test]
        public void DisplayName_ForSampleProject_ContainsTestCount()
        {
            // 1. Arrange
            TestNode testNode = TestFilterData.GetSampleTestProject();
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "3-1001");

            // 2. Act
            string name = viewModel.DisplayName;

            // 3. Assert
            Assert.That(name, Is.EqualTo("NamespaceA (2)"));
        }

        [Test]
        public void DisplayName_ForSampleProject2_ContainsTestCount()
        {
            // 1. Arrange
            TestNode testNode = TestFilterData.GetSampleTestProject();
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "3-2001");

            // 2. Act
            string name = viewModel.DisplayName;

            // 3. Assert
            Assert.That(name, Is.EqualTo("NamespaceB (6)"));
        }

        [Test]
        public void DisplayName_ShowTestDurationIsEnabled_ForTestCase_ContainsDuration()
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowTestDuration.Returns(true);
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");
            ResultNode resultNode = new ResultNode("<test-case id='1' duration='1.0'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            // 2. Act
            string name = viewModel.DisplayName;

            // 3. Assert
            Assert.That(name, Is.EqualTo("TestA [1.000s]"));
        }

        [Test]
        public void DisplayName_ShowTestDurationIsEnabled_ForTestFixture_ContainsDuration()
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowTestDuration.Returns(true);
            TestNode testNode = TestFilterData.GetSampleTestProject();
            ResultNode resultNode0 = new ResultNode("<test-case id='3-1010' duration='1.0'/>");
            ResultNode resultNode1 = new ResultNode("<test-case id='3-1011' duration='1.0'/>");
            ResultNode resultNode2 = new ResultNode("<test-case id='3-1012' duration='2.0'/>");
            _model.TestResultManager.GetResultForTest("3-1010").Returns(resultNode0);
            _model.TestResultManager.GetResultForTest("3-1011").Returns(resultNode1);
            _model.TestResultManager.GetResultForTest("3-1012").Returns(resultNode2);

            TreeNodeViewModel viewModel = CreateViewModel(testNode, "3-1010");

            // 2. Act
            string name = viewModel.DisplayName;

            // 3. Assert
            Assert.That(name, Is.EqualTo("Fixture_1 (2) [3.000s]"));
        }

        [Test]
        public void SetInTestRun_AllChildrenAndAllParent_AreInRun()
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowTestDuration.Returns(true);
            TestNode testNode = TestFilterData.GetSampleTestProject();
            TreeNodeViewModel rootViewModel = CreateViewModel(testNode, "3-1000");

            TreeNodeViewModel viewModel = rootViewModel.Children[0].Children[0];

            // 2. Act
            viewModel.SetInTestRun(true);

            // 3. Assert
            Assert.That(viewModel.IsInTestRun, Is.True);
            Assert.That(viewModel.Children[0].IsInTestRun, Is.True);
            Assert.That(viewModel.Children[1].IsInTestRun, Is.True);

            Assert.That(viewModel.Parent.IsInTestRun, Is.True);
            Assert.That(viewModel.Parent.Parent.IsInTestRun, Is.True);

            Assert.That(viewModel.Parent.Parent.Children[1].IsInTestRun, Is.False);    // check one sibling node
        }

        [Test]
        public void AddChild_UpdatesResultState()
        {
            // 1. Arrange
            ResultNode resultNode1 = new ResultNode("<test-case id='1' result='Passed'/>");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' result='Failed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode2);

            TestNode testNode = new TestNode("<test-suite type='TestFixture' id='0' name='Fixture1'/>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            TestNode testCaseNode1 = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel childNode1 = CreateViewModel(testCaseNode1, "1");

            TestNode testCaseNode2 = new TestNode("<test-case id='2' name='TestB' />");
            TreeNodeViewModel childNode2 = CreateViewModel(testCaseNode2, "2");

            // 2. Act
            string name = viewModel.DisplayName;
            Assert.That(name, Is.EqualTo("Fixture1 (0)"));

            viewModel.AddChild(childNode1);
            name = viewModel.DisplayName;
            Assert.That(name, Is.EqualTo("Fixture1 (1)"));
            Assert.That(viewModel.ResultState.Status, Is.EqualTo(TestStatus.Passed));

            viewModel.AddChild(childNode2);
            name = viewModel.DisplayName;
            Assert.That(name, Is.EqualTo("Fixture1 (2)"));
            Assert.That(viewModel.ResultState.Status, Is.EqualTo(TestStatus.Failed));
        }

        [Test]
        public void RemoveChild_UpdatesTestCount()
        {
            // 1. Arrange
            ResultNode resultNode1 = new ResultNode("<test-case id='1' result='Passed'/>");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' result='Failed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode2);

            TestNode testNode = new TestNode("<test-suite type='TestFixture' id='0' name='Fixture1'/>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            TestNode testCaseNode1 = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel childNode1 = CreateViewModel(testCaseNode1, "1");

            TestNode testCaseNode2 = new TestNode("<test-case id='2' name='TestB' />");
            TreeNodeViewModel childNode2 = CreateViewModel(testCaseNode2, "2");

            viewModel.AddChild(childNode1);
            viewModel.AddChild(childNode2);

            // 2. Act
            viewModel.RemoveChild(childNode2);

            // 3. Assert
            string name = viewModel.DisplayName;
            Assert.That(name, Is.EqualTo("Fixture1 (1)"));
            Assert.That(viewModel.ResultState.Status, Is.EqualTo(TestStatus.Passed));
            Assert.That(viewModel.Children.Count, Is.EqualTo(1));
        }

        [Test]
        public void RemoveChild_UpdatesResultState()
        {
            // 1. Arrange
            ResultNode resultNode1 = new ResultNode("<test-case id='1' result='Passed'/>");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' result='Failed'/>");
            ResultNode resultNode3 = new ResultNode("<test-case id='3' result='Passed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode2);
            _model.TestResultManager.GetResultForTest("3").Returns(resultNode3);

            TestNode testNode = new TestNode("<test-suite type='TestFixture' id='0' name='Fixture1'/>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            TestNode testCaseNode1 = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel childNode1 = CreateViewModel(testCaseNode1, "1");

            TestNode testCaseNode2 = new TestNode("<test-case id='2' name='TestB' />");
            TreeNodeViewModel childNode2 = CreateViewModel(testCaseNode2, "2");

            TestNode testCaseNode3 = new TestNode("<test-case id='3' name='TestC' />");
            TreeNodeViewModel childNode3 = CreateViewModel(testCaseNode3, "3");

            viewModel.AddChild(childNode1);
            viewModel.AddChild(childNode2);
            viewModel.AddChild(childNode3);
            Assert.That(viewModel.ResultState.Status, Is.EqualTo(TestStatus.Failed));

            // 2. Act - Remove the failed child
            viewModel.RemoveChild(childNode2);

            // 3. Assert - Now all children pass
            Assert.That(viewModel.ResultState.Status, Is.EqualTo(TestStatus.Passed));
        }

        [Test]
        public void OnTestRunStarted_ResetsIsResultFromLastRun()
        {
            // 1. Arrange
            ResultNode resultNode = new ResultNode("<test-case id='1' result='Passed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");

            // Access ResultState to populate IsResultFromLastRun
            var resultState = viewModel.ResultState;
            Assert.That(viewModel.IsResultFromLastRun, Is.True);

            viewModel.SetInTestRun(true);

            // 2. Act
            viewModel.OnTestRunStarted();

            // 3. Assert
            Assert.That(viewModel.IsResultFromLastRun, Is.False);
        }

        [Test]
        public void OnTestRunStarted_ResetsTestDuration_WhenInTestRun()
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowTestDuration.Returns(true);
            ResultNode resultNode = new ResultNode("<test-case id='1' result='Passed' duration='2.5'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");

            viewModel.SetInTestRun(true);

            // Access to populate duration
            var resultState = viewModel.DisplayName;
            Assert.That(viewModel.TestDuration, Is.Not.Null);

            // 2. Act
            viewModel.OnTestRunStarted();

            // 3. Assert
            Assert.That(viewModel.TestDuration, Is.Null);
        }

        [Test]
        public void OnTestFinished_ResetsResultState()
        {
            // 1. Arrange
            ResultNode resultNode = new ResultNode("<test-case id='1' result='Passed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");

            // Access ResultState to populate it
            var resultState = viewModel.ResultState;
            Assert.That(resultState.Status, Is.EqualTo(TestStatus.Passed));

            // Update the result
            ResultNode newResultNode = new ResultNode("<test-case id='1' result='Failed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(newResultNode);

            // 2. Act
            viewModel.OnTestFinished(newResultNode);

            // 3. Assert - ResultState should be recalculated on next access
            Assert.That(viewModel.ResultState.Status, Is.EqualTo(TestStatus.Failed));
        }

        [Test]
        public void GetTestCases_ForSingleTestCase_ReturnsSelf()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");

            // 2. Act
            var testCases = TreeNodeViewModel.GetTestCases(viewModel);

            // 3. Assert
            Assert.That(testCases.Count, Is.EqualTo(1));
            Assert.That(testCases[0], Is.EqualTo(viewModel));
        }

        [Test]
        public void GetTestCases_ForTestFixture_ReturnsAllTestCases()
        {
            // 1. Arrange
            TestNode testNode = TestFilterData.GetSampleTestProject();
            TreeNodeViewModel rootViewModel = CreateViewModel(testNode, "3-1000");
            TreeNodeViewModel fixture = rootViewModel.Children[0].Children[0];

            // 2. Act
            var testCases = TreeNodeViewModel.GetTestCases(fixture);

            // 3. Assert
            Assert.That(testCases.Count, Is.EqualTo(2));
        }

        [Test]
        public void GetTestCases_ForNamespace_ReturnsAllTestCases()
        {
            // 1. Arrange
            TestNode testNode = TestFilterData.GetSampleTestProject();
            TreeNodeViewModel rootViewModel = CreateViewModel(testNode, "3-1000");
            TreeNodeViewModel namespaceNode = rootViewModel.Children[1];

            // 2. Act
            var testCases = TreeNodeViewModel.GetTestCases(namespaceNode);

            // 3. Assert
            Assert.That(testCases.Count, Is.EqualTo(6));
        }

        [Test]
        public void GetNonExplicitTests_ForTestFixture_ReturnsNonExplicitTests()
        {
            // 1. Arrange
            TestNode testNode = new TestNode(@"
                <test-suite type='TestFixture' id='0' name='Fixture1'>
                    <test-case id='1' name='TestA' runstate='Runnable'/>
                    <test-case id='2' name='TestB' runstate='Explicit'/>
                    <test-case id='3' name='TestC' runstate='Runnable'/>
                </test-suite>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            var nonExplicitTests = viewModel.GetNonExplicitTests();

            // 3. Assert
            Assert.That(nonExplicitTests.Count, Is.EqualTo(2));
            Assert.That(nonExplicitTests[0].Id, Is.EqualTo("1"));
            Assert.That(nonExplicitTests[1].Id, Is.EqualTo("3"));
        }

        [Test]
        public void GetNonExplicitTests_WhenFixtureIsExplicit_ReturnsAllTests()
        {
            // 1. Arrange
            TestNode testNode = new TestNode(@"
                <test-suite type='TestFixture' id='0' name='Fixture1' runstate='Explicit'>
                    <test-case id='1' name='TestA' runstate='Runnable'/>
                    <test-case id='2' name='TestB' runstate='Runnable'/>
                </test-suite>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            var nonExplicitTests = viewModel.GetNonExplicitTests();

            // 3. Assert
            Assert.That(nonExplicitTests.Count, Is.EqualTo(2));
        }

        [Test]
        public void RunState_ReturnsTestNodeRunState()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' runstate='Explicit'/>");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, testNode, "TestA");

            // 2. Act
            var runState = viewModel.RunState;

            // 3. Assert
            Assert.That(runState, Is.EqualTo(RunState.Explicit));
        }

        [Test]
        public void AssociatedTestNode_ReturnsOriginalTestNode()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, testNode, "TestA");

            // 2. Act
            var associatedNode = viewModel.AssociatedTestNode;

            // 3. Assert
            Assert.That(associatedNode, Is.SameAs(testNode));
        }

        [Test]
        public void Parent_IsSetCorrectly_WhenAddingChild()
        {
            // 1. Arrange
            TestNode parentNode = new TestNode("<test-suite type='TestFixture' id='0' name='Fixture1'/>");
            TreeNodeViewModel parentViewModel = CreateViewModel(parentNode, "0");

            TestNode childNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel childViewModel = CreateViewModel(childNode, "1");

            // 2. Act
            parentViewModel.AddChild(childViewModel);

            // 3. Assert
            Assert.That(childViewModel.Parent, Is.SameAs(parentViewModel));
        }

        [Test]
        public void TestItem_ReturnsAssociatedTestNode_WhenCallbackIsNull()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, testNode, "TestA");

            // 2. Act
            var testItem = viewModel.TestItem;

            // 3. Assert
            Assert.That(testItem, Is.SameAs(testNode));
        }

        [Test]
        public void TestItem_UsesCallback_WhenCallbackIsSet()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = new TreeNodeViewModel(_model, testNode, "TestA");
            
            ITestItem customTestItem = Substitute.For<ITestItem>();
            viewModel.CreateTestItemCallback = (vm) => customTestItem;

            // 2. Act
            var testItem = viewModel.TestItem;

            // 3. Assert
            Assert.That(testItem, Is.SameAs(customTestItem));
        }

        [Test]
        public void SetInTestRun_False_ClearsAllDescendants()
        {
            // 1. Arrange
            TestNode testNode = TestFilterData.GetSampleTestProject();
            TreeNodeViewModel rootViewModel = CreateViewModel(testNode, "3-1000");
            
            rootViewModel.SetInTestRun(true);
            Assert.That(rootViewModel.IsInTestRun, Is.True);

            // 2. Act
            rootViewModel.SetInTestRun(false);

            // 3. Assert
            Assert.That(rootViewModel.IsInTestRun, Is.False);
            Assert.That(rootViewModel.Children[0].IsInTestRun, Is.False);
            Assert.That(rootViewModel.Children[0].Children[0].IsInTestRun, Is.False);
        }

        [Test]
        public void SetInTestRun_DoesNotSetExplicitChildren()
        {
            // 1. Arrange
            TestNode testNode = new TestNode(@"
                <test-suite type='TestFixture' id='0' name='Fixture1' runstate='Runnable'>
                    <test-case id='1' name='TestA' runstate='Runnable'/>
                    <test-case id='2' name='TestB' runstate='Explicit'/>
                </test-suite>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            viewModel.SetInTestRun(true);

            // 3. Assert
            Assert.That(viewModel.IsInTestRun, Is.True);
            Assert.That(viewModel.Children[0].IsInTestRun, Is.True);
            Assert.That(viewModel.Children[1].IsInTestRun, Is.False); // Explicit test not set
        }

        [Test]
        public void ResultState_WithNoResult_ReturnsNull()
        {
            // 1. Arrange
            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");
            _model.TestResultManager.GetResultForTest("1").Returns((ResultNode)null);

            // 2. Act
            var resultState = viewModel.ResultState;

            // 3. Assert
            Assert.That(resultState, Is.Null);
        }

        [Test]
        public void ResultState_ForSuite_AggregatesChildResults()
        {
            // 1. Arrange
            ResultNode resultNode1 = new ResultNode("<test-case id='1' result='Passed'/>");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' result='Passed'/>");
            ResultNode resultNode3 = new ResultNode("<test-case id='3' result='Failed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode2);
            _model.TestResultManager.GetResultForTest("3").Returns(resultNode3);

            TestNode testNode = new TestNode(@"
                <test-suite type='TestFixture' id='0' name='Fixture1'>
                    <test-case id='1' name='TestA'/>
                    <test-case id='2' name='TestB'/>
                    <test-case id='3' name='TestC'/>
                </test-suite>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            var resultState = viewModel.ResultState;

            // 3. Assert - Failed takes precedence
            Assert.That(resultState.Status, Is.EqualTo(TestStatus.Failed));
        }

        [Test]
        public void TestDuration_ForSuite_SumsChildDurations()
        {
            // 1. Arrange
            _model.Settings.Gui.TestTree.ShowTestDuration.Returns(true);
            ResultNode resultNode1 = new ResultNode("<test-case id='1' result='Passed' duration='1.5'/>");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' result='Passed' duration='2.5'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode2);

            TestNode testNode = new TestNode(@"
                <test-suite type='TestFixture' id='0' name='Fixture1'>
                    <test-case id='1' name='TestA'/>
                    <test-case id='2' name='TestB'/>
                </test-suite>");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            var _ = viewModel.DisplayName; // Trigger calculation
            var duration = viewModel.TestDuration;

            // 3. Assert
            Assert.That(duration, Is.EqualTo(4.0));
        }

        [Test]
        public void TestDuration_ForSuite_SumsChildDurationsAndSkipExlicitTests()
        {
            // 1. Arrange
            ResultNode resultNode1 = new ResultNode("<test-case id='1' result='Passed' duration='1.5'/>");
            ResultNode resultNode2 = new ResultNode("<test-case id='2' result='Skipped' label='Explicit' />");
            ResultNode resultNode3 = new ResultNode("<test-case id='3' result='Failed' duration='1.5'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            _model.TestResultManager.GetResultForTest("2").Returns(resultNode2);
            _model.TestResultManager.GetResultForTest("3").Returns(resultNode3);

            TestNode testNode = new TestNode(@"
                <test-suite type='TestFixture' id='0' name='Fixture1'>
                    <test-case id='1' name='TestA'/>
                    <test-case id='2' name='TestB' runstate='Explicit'/>
                    <test-case id='3' name='TestC'/>
                </test-suite>");

            TreeNodeViewModel viewModel = CreateViewModel(testNode, "0");

            // 2. Act
            var _ = viewModel.DisplayName; // Trigger calculation
            var duration = viewModel.TestDuration;

            // 3. Assert - Failed takes precedence
            Assert.That(duration, Is.EqualTo(3.0));

            var explicitTestCase = viewModel.Children[1];
            Assert.That(explicitTestCase.TestDuration, Is.EqualTo(null));
        }

        [Test]
        public void IsResultFromLastRun_IsFalse_AfterOnTestRunStarted()
        {
            // 1. Arrange
            ResultNode resultNode = new ResultNode("<test-case id='1' result='Passed'/>");
            _model.TestResultManager.GetResultForTest("1").Returns(resultNode);

            TestNode testNode = new TestNode("<test-case id='1' name='TestA' />");
            TreeNodeViewModel viewModel = CreateViewModel(testNode, "1");

            var _ = viewModel.ResultState;
            viewModel.SetInTestRun(true);

            // 2. Act
            viewModel.OnTestRunStarted();

            // 3. Assert
            Assert.That(viewModel.IsResultFromLastRun, Is.False);
        }

        private TreeNodeViewModel CreateViewModel(TestNode rootNode, string testId)
        {
            TestNode node = TestFilterData.GetTestById(rootNode, testId);

            var viewModel = new TreeNodeViewModel(_model, node, node.Name);
            foreach (TestNode childNode in node.Children)
            {
                var childViewModel = CreateViewModel(rootNode, childNode.Id);
                viewModel.AddChild(childViewModel);
            }


            return viewModel;
        }
    }
}
