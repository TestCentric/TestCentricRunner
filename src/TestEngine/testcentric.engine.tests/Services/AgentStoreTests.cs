// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

#if !NETCOREAPP2_1
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Engine;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    public static partial class AgentStoreTests
    {
        private static readonly Process DummyProcess = new Process();
        private static readonly ITestAgent DummyAgent = new DummyTestAgent(Guid.NewGuid());

        [Test]
        public static void IdCannotBeReused()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            Assert.That(() => database.AddAgent(DummyAgent.Id, DummyProcess), Throws.ArgumentException.With.Property("ParamName").EqualTo("agentId"));

            database.Register(DummyAgent);
            Assert.That(() => database.AddAgent(DummyAgent.Id, DummyProcess), Throws.ArgumentException.With.Property("ParamName").EqualTo("agentId"));

            database.MarkProcessTerminated(DummyProcess);
            Assert.That(() => database.AddAgent(DummyAgent.Id, DummyProcess), Throws.ArgumentException.With.Property("ParamName").EqualTo("agentId"));
        }

        [Test]
        public static void AgentMustBeStartedBeforeRegistering()
        {
            var database = new AgentStore();

            Assert.That(() => database.Register(DummyAgent), Throws.ArgumentException.With.Property("ParamName").EqualTo("agent"));
        }

        [Test]
        public static void AgentMustNotRegisterTwice()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            database.Register(DummyAgent);
            Assert.That(() => database.Register(DummyAgent), Throws.ArgumentException.With.Property("ParamName").EqualTo("agent"));
        }

        [Test]
        public static void AgentMustNotRegisterAfterTerminating()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            database.MarkProcessTerminated(DummyProcess);
            Assert.That(() => database.Register(DummyAgent), Throws.ArgumentException.With.Property("ParamName").EqualTo("agent"));
        }

        [Test]
        public static void AgentMustBeStartedBeforeTerminating()
        {
            var database = new AgentStore();

            Assert.That(() => database.MarkProcessTerminated(DummyProcess),
                Throws.Exception.With.Message.EqualTo("Process terminated without registering an agent."));
        }

        [Test]
        public static void AgentIsNotReadyWhenNotStarted()
        {
            var database = new AgentStore();

            Assert.That(database.IsAvailable(DummyAgent.Id, out _), Is.False);
        }

        [Test]
        public static void AgentIsNotReadyWhenStarted()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            Assert.That(database.IsAvailable(DummyAgent.Id, out _), Is.False);
        }

        [Test]
        public static void AgentIsReadyWhenRegistered()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            database.Register(DummyAgent);
            Assert.That(database.IsAvailable(DummyAgent.Id, out var registeredAgent), Is.True);
            Assert.That(registeredAgent, Is.SameAs(DummyAgent));
        }

        [Test]
        public static void AgentIsNotReadyWhenTerminated()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            database.Register(DummyAgent);
            database.MarkProcessTerminated(DummyProcess);
            Assert.That(database.IsAvailable(DummyAgent.Id, out _), Is.False);
        }

        [Test]
        public static void AgentIsNotRunningWhenNotStarted()
        {
            var database = new AgentStore();

            Assert.That(database.IsAgentActive(DummyAgent.Id, out _), Is.False);
        }

        [Test]
        public static void AgentIsRunningWhenStarted()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            Assert.That(database.IsAgentActive(DummyAgent.Id, out var process), Is.True);
            Assert.That(process, Is.SameAs(DummyProcess));
        }

        [Test]
        public static void AgentIsRunningWhenRegistered()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            database.Register(DummyAgent);
            Assert.That(database.IsAgentActive(DummyAgent.Id, out var process), Is.True);
            Assert.That(process, Is.SameAs(DummyProcess));
        }

        [Test]
        public static void AgentIsNotRunningWhenTerminated()
        {
            var database = new AgentStore();

            database.AddAgent(DummyAgent.Id, DummyProcess);
            database.Register(DummyAgent);
            database.MarkProcessTerminated(DummyProcess);
            Assert.That(database.IsAgentActive(DummyAgent.Id, out _), Is.False);
        }

        [Test]
        public static void ConcurrentOperationsDoNotCorruptState()
        {
            var database = new AgentStore();

            RunActionConcurrently(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var id = Guid.NewGuid();
                    var process = new Process();

                    Assert.That(database.IsAgentActive(id, out _), Is.False);
                    Assert.That(database.IsAvailable(id, out _), Is.False);

                    database.AddAgent(id, process);
                    Assert.That(database.IsAgentActive(id, out _), Is.True);
                    Assert.That(database.IsAvailable(id, out _), Is.False);

                    database.Register(new DummyTestAgent(id));
                    Assert.That(database.IsAgentActive(id, out _), Is.True);
                    Assert.That(database.IsAvailable(id, out _), Is.True);

                    database.MarkProcessTerminated(process);
                    Assert.That(database.IsAgentActive(id, out _), Is.False);
                    Assert.That(database.IsAvailable(id, out _), Is.False);
                }
            }, threadCount: Environment.ProcessorCount);
        }

        private static void RunActionConcurrently(Action action, int threadCount)
        {
            var threads = new List<Thread>();
            var exceptions = new List<Exception>();

            for (var i = 0; i < threadCount; i++)
            {
                threads.Add(new Thread(() =>
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                            exceptions.Add(ex);
                    }
                }));
            }

            foreach (var thread in threads)
                thread.Start();

            foreach (var thread in threads)
                thread.Join();

            if (exceptions.Count != 0) throw exceptions[0];
        }
    }
}
#endif
