// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;

namespace NUnit.Tests
{
    // The tests in this fixture run without delay, usually before we get a chance to stop the run
    public class FastFixture
    {
        [SetUp]
        public void SetUp()
        {
            TestContext.Progress.WriteLine($"SetUp executing");
        }

        [TearDown]
        public void TearDown()
        {
            TestContext.Progress.WriteLine($"TearDown executing");
        }

        [Test]
        public void TestMethod([Range(1, 5)] int i)
        {
            TestContext.Progress.WriteLine($"Test executing");
        }
    }
}
