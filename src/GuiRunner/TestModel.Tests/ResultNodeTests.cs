// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using NUnit.Framework;

namespace TestCentric.Gui.Model
{
    [TestFixture]
    public class ResultNodeTests
    {
        [Test]
        [SetCulture("fr-FR")]
        public void CreateResultNode_CurrentCultureWithDifferentDelimiter_DoesNotThrowException()
        {
            TestDelegate newResultNode = () =>
            {
                var resultNode = new ResultNode("<test-case duration='0.000'/>");
            };
            Assert.DoesNotThrow(newResultNode);
        }

        [Test]
        public void CreateResultNode_FromOldResult()
        {
            // Act
            var resultNode = new ResultNode("<test-case id='1' fullname='Assembly.Folder1.TestB' result='Passed'/>");

            // Arrange
            ResultNode newResultNode = ResultNode.Create(resultNode.Xml, "100");

            // Assert
            Assert.That(newResultNode.Id, Is.EqualTo("100"));
            Assert.That(newResultNode.FullName, Is.EqualTo(resultNode.FullName));
            Assert.That(newResultNode.Status, Is.EqualTo(resultNode.Status));
            Assert.That(newResultNode.IsLatestRun, Is.EqualTo(false));
        }

        private static object[] ReplaceResultStateTestCases =
        {
                new object[] { ResultState.Success, TestStatus.Passed },
                new object[] { ResultState.Failure, TestStatus.Failed },
                new object[] { ResultState.Error, TestStatus.Failed },
                new object[] { ResultState.Skipped, TestStatus.Skipped },
                new object[] { ResultState.Ignored, TestStatus.Skipped },
                new object[] { ResultState.Inconclusive, TestStatus.Inconclusive },
                new object[] { ResultState.Explicit, TestStatus.Skipped },
        };

        [Test]
        [TestCaseSource(nameof(ReplaceResultStateTestCases))]
        public void CreateResultNode_FromOldResult_ReplaceResultState(ResultState resultState, TestStatus expectedTestStatus)
        {
            // Act
            var resultNode = new ResultNode("<test-case id='1' fullname='Assembly.Folder1.TestB' result='Passed'/>");

            // Arrange
            ResultNode newResultNode = ResultNode.Create(resultNode.Xml, resultState);

            // Assert
            Assert.That(newResultNode.Id, Is.EqualTo("1"));
            Assert.That(newResultNode.FullName, Is.EqualTo(resultNode.FullName));
            Assert.That(newResultNode.Status, Is.EqualTo(expectedTestStatus));
            Assert.That(newResultNode.IsLatestRun, Is.EqualTo(false));
        }

        [TestCaseSource(nameof(ReplaceResultStateTestCases))]
        public void CreateResultNode_FromOldResult_ReplaceResultState2(ResultState resultState, TestStatus expectedTestStatus)
        {
            // Act
            var resultNode = new ResultNode("<test-case id='1' fullname='Assembly.Folder1.TestB' result='Skipped' label='Ignored' />");

            // Arrange
            ResultNode newResultNode = ResultNode.Create(resultNode.Xml, resultState);

            // Assert
            Assert.That(newResultNode.Id, Is.EqualTo("1"));
            Assert.That(newResultNode.FullName, Is.EqualTo(resultNode.FullName));
            Assert.That(newResultNode.Status, Is.EqualTo(expectedTestStatus));
            Assert.That(newResultNode.IsLatestRun, Is.EqualTo(false));
        }

        [Test]
        public void IsLatestRun_NewResultNode_IsTrue()
        {
            var resultNode = new ResultNode("<test-case id='1'/>");
            
            Assert.That(resultNode.IsLatestRun, Is.True);
        }
    }
}
