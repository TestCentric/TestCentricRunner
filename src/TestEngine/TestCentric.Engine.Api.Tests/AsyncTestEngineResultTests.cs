// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Xml;
using NUnit.Framework;

namespace TestCentric.Engine
{
    public class AsyncTestEngineResultTests
    {
        private NUnit.Engine.AsyncTestEngineResult _asyncResult;

        [SetUp]
        public void SetUp()
        {
            _asyncResult = new NUnit.Engine.AsyncTestEngineResult();
        }

        [Test]
        public void Result_ThrowsIfNotSet()
        {
            XmlNode result = null;
            Assert.Throws<InvalidOperationException>(() => result = _asyncResult.EngineResult.Xml);
        }

        [Test]
        public void SetResult_ThrowsIfNull()
        {
            Assert.Throws<ArgumentNullException>(() => _asyncResult.SetResult(null));
        }

        [Test]
        public void SetResult_ThrowsIfSetTwice()
        {
            var result = new NUnit.Engine.TestEngineResult();
            _asyncResult.SetResult(result);
            Assert.Throws<InvalidOperationException>(() => _asyncResult.SetResult(result));
        }

        [Test]
        public void IsComplete_FalseIfNotComplete()
        {
            Assert.That(_asyncResult.IsComplete, Is.False);
        }

        [Test]
        public void IsComplete_TrueIfComplete()
        {
            var result = new NUnit.Engine.TestEngineResult();
            _asyncResult.SetResult(result);
            Assert.That(_asyncResult.IsComplete, Is.True);
        }

        [Test]
        public void Wait_ReturnsFalseTillTestCompletes()
        {
            var result = new NUnit.Engine.TestEngineResult("<test-assembly />");

            Assert.That(_asyncResult.Wait(0), Is.False, "Expected wait to be false because test hasn't completed yet");

            _asyncResult.SetResult(result);

            Assert.That(_asyncResult.Wait(0), Is.True, "Expected wait to be true because the test is complete");

            Assert.That(_asyncResult.EngineResult.Xml, Is.EqualTo(result.Xml));
        }

        [Test]
        public void Wait_AllowsMultipleWaits()
        {
            _asyncResult.SetResult(new NUnit.Engine.TestEngineResult());
            
            Assert.That(_asyncResult.Wait(0), Is.True, "Expected wait to be true because the test is complete");

            Assert.That(_asyncResult.Wait(0), Is.True, "Expected the second wait to be non blocking");
        }
    }
}
