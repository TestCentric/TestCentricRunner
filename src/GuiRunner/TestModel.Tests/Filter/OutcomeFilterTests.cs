// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using NSubstitute;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace TestCentric.Gui.Model.Filter
{
    [TestFixture]
    internal class OutcomeFilterTests
    {
        [Test]
        public void Create_TestFilter_ConditionIsEmpty()
        {
            // 1. Arrange + Act
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);

            // 2. Assert
            Assert.That(filter.Condition, Is.Empty);
        }

        [TestCase(new[] { "Passed" }, "Passed")]
        [TestCase(new[] { "Failed", "Passed" }, "Passed")]
        [TestCase(new[] { "Warning" }, "Warning")]
        [TestCase(new[] { "Warning" }, "Skipped")]
        [TestCase(new[] { "Warning" }, "Inconclusive")]
        [TestCase(new string[0] , "Passed")]
        [TestCase(new[] { OutcomeFilter.AllOutcome }, "Passed")]
        public void IsMatching_TestOutcomeMatchesOutcomeFilter_ReturnsTrue(IList<string> outcomeFilter, string testOutcome)
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);
            filter.Condition = outcomeFilter;

            TestNode testNode = new TestNode($"<test-case id='1' />");
            var resultNode = new ResultNode($"<test-case id='1' result='{testOutcome}' />");
            testModel.TestResultManager.GetResultForTest(testNode.Id).Returns(resultNode);

            // 2. Act
            bool isMatch = filter.IsMatching(testNode);

            // 3. Assert
            Assert.That(isMatch, Is.True);
        }

        [TestCase(new[] { "Passed" }, "Failed")]
        [TestCase(new[] { "Failed", "Passed" }, "Skipped")]
        [TestCase(new[] { OutcomeFilter.NotRunOutcome }, "Passed")]
        public void IsMatching_TestOutcomeNotMatchesOutcomeFilter_ReturnsFalse(IList<string> outcomeFilter, string testOutcome)
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);
            filter.Condition = outcomeFilter;

            TestNode testNode = new TestNode($"<test-case id='1' />");
            var resultNode = new ResultNode($"<test-case id='1' result='{testOutcome}' />");
            testModel.TestResultManager.GetResultForTest(testNode.Id).Returns(resultNode);

            // 2. Act
            bool isMatch = filter.IsMatching(testNode);

            // 3. Assert
            Assert.That(isMatch, Is.False);
        }

        [TestCase(new[] { OutcomeFilter.NotRunOutcome }, "Passed")]
        public void IsMatching_NoTestOutcomeMatchesOutcomeFilter_ReturnsTrue(IList<string> outcomeFilter, string testOutcome)
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);
            filter.Condition = outcomeFilter;

            TestNode testNode = new TestNode($"<test-case id='1' />");
            testModel.TestResultManager.GetResultForTest(testNode.Id).Returns((ResultNode)null);

            // 2. Act
            bool isMatch = filter.IsMatching(testNode);

            // 3. Assert
            Assert.That(isMatch, Is.True);
        }

        [Test]
        public void Init_ConditionIsEmpty()
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);
            filter.Condition = new[] { "Failed" };

            // 2. Act
            filter.Init();

            // 3. Assert
            Assert.That(filter.Condition, Is.Empty);
        }

        [Test]
        public void Reset_ConditionIsEmpty()
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);
            filter.Condition = new[] { "Failed" };

            // 2. Act
            filter.Reset();

            // 3. Assert
            Assert.That(filter.Condition, Is.Empty);
        }

        [Test]
        public void IsActive_Condition_IsSet_ReturnsTrue()
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);

            // 2. Act
            filter.Condition = new[] { "Passed" };

            // 3. Assert
            Assert.That(filter.IsActive, Is.EqualTo(true));
        }

        [Test]
        public void IsActive_FilterIsReset_ReturnsFalse()
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);
            filter.Condition = new[] { "Failed" };

            // 2. Act
            filter.Reset();

            // 3. Assert
            Assert.That(filter.IsActive, Is.False);
        }

        [Test]
        public void IsActive_FilterIsInit_ReturnsFalse()
        {
            // 1. Arrange
            ITestModel testModel = Substitute.For<ITestModel>();
            OutcomeFilter filter = new OutcomeFilter(testModel);
            filter.Condition = new[] { "Failed" };

            // 2. Act
            filter.Init();

            // 3. Assert
            Assert.That(filter.IsActive, Is.False);
        }
    }
}
