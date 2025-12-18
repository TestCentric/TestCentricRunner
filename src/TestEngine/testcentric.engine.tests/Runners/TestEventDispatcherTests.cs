// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using TestCentric.Engine.Services;
using TestCentric.Tests.Fakes;

namespace TestCentric.Engine.Runners
{
    public class TestEventDispatcherTests
    {
        private TestEventDispatcher _dispatcher;

        private const string REPORT1 = "<msg>First Message</msg>";
        private const string REPORT2 = "<msg>Second Message</msg>";
        private const string REPORT3 = "<msg>Third Message</msg>";

        private static readonly string[] EXPECTED_OUTPUT = new[] { REPORT1, REPORT2, REPORT3 };

        [SetUp]
        public void SetUp() 
        {
            _dispatcher = new TestEventDispatcher();
        }

        [Test]
        public void TestCentricListener()
        {
            var listener = new FakeTestEventListener();
            _dispatcher.Listeners.Add(listener);

            DispatchEvents();

            Assert.That(listener.Output, Is.EqualTo(EXPECTED_OUTPUT));
        }

        [Test]
        public void NUnitListener()
        {
            var listener = new FakeNUnitTestEventListener();
            _dispatcher.Listeners.Add(listener);

            DispatchEvents();

            Assert.That(listener.Output, Is.EqualTo(EXPECTED_OUTPUT));
        }

        [Test]
        public void MultipleListeners()
        {
            var listener1 = new FakeTestEventListener();
            var listener2 = new FakeNUnitTestEventListener();
            var listener3 = new FakeTestEventListener();

            _dispatcher.Listeners.Add(listener1);
            _dispatcher.Listeners.Add(listener2);
            _dispatcher.Listeners.Add(listener3);

            DispatchEvents();

            Assert.That(listener1.Output, Is.EqualTo(EXPECTED_OUTPUT), "Listener1");
            Assert.That(listener2.Output, Is.EqualTo(EXPECTED_OUTPUT), "Listener2");
            Assert.That(listener3.Output, Is.EqualTo(EXPECTED_OUTPUT), "Listener3");
        }

        private void DispatchEvents()
        {
            _dispatcher.OnTestEvent(REPORT1);
            _dispatcher.OnTestEvent(REPORT2);
            _dispatcher.OnTestEvent(REPORT3);
        }
    }
}
