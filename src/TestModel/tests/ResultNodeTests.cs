// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

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
        public void IsLatestRun_NewResultNode_IsTrue()
        {
            var resultNode = new ResultNode("<test-case id='1'/>");
            
            Assert.That(resultNode.IsLatestRun, Is.True);
        }
    }
}
