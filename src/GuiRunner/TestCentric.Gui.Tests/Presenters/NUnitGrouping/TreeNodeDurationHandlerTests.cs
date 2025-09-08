// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NSubstitute;
using NUnit.Framework;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    [TestFixture]
    internal class TreeNodeDurationHandlerTests
    {
        [Test]
        public void ClearGroupDurations_AllDurationAreReset()
        {
            TestGroup testGroup1 = new TestGroup("Group_1") { Duration = 0.5 };
            TestGroup testGroup2 = new TestGroup("Group_2") { Duration = 0.5 };
            TestGroup testGroup3 = new TestGroup("Group_3") { Duration = 0.5 };

            // Act
            TreeNodeDurationHandler.ClearGroupDurations(new[] { testGroup1, testGroup2, testGroup3 });

            // Assert

            Assert.That(testGroup1.Duration.HasValue, Is.False);
            Assert.That(testGroup2.Duration.HasValue, Is.False);
            Assert.That(testGroup3.Duration.HasValue, Is.False);
        }

        [Test]
        public void SetGroupDurations_AllDurationsAreSet()
        {
            ITestModel model = Substitute.For<ITestModel>();
            TestNode testNode1 = new TestNode($"<test-start id='1'/>");
            TestNode testNode2 = new TestNode($"<test-start id='2'/>");
            TestNode testNode3 = new TestNode($"<test-start id='3'/>");
            var resultNode1 = new ResultNode($"<test-case id='1' duration='1'/>");
            var resultNode2 = new ResultNode($"<test-case id='2' duration='2'/>");
            model.TestResultManager.GetResultForTest("1").Returns(resultNode1);
            model.TestResultManager.GetResultForTest("2").Returns(resultNode2);

            TestGroup testGroup1 = new TestGroup("Group_1");
            TestGroup testGroup2 = new TestGroup("Group_2");
            TestGroup testGroup3 = new TestGroup("Group_3");

            testGroup1.Add(testNode1);
            testGroup2.Add(testNode2);
            testGroup3.Add(testNode1);
            testGroup3.Add(testNode2);
            testGroup3.Add(testNode3);

            // Act
            TreeNodeDurationHandler.SetGroupDurations(model, new[] { testGroup1, testGroup2, testGroup3 });

            // Assert

            Assert.That(testGroup1.Duration.Value, Is.EqualTo(1));
            Assert.That(testGroup2.Duration.Value, Is.EqualTo(2));
            Assert.That(testGroup3.Duration.Value, Is.EqualTo(3));
        }
    }
}
