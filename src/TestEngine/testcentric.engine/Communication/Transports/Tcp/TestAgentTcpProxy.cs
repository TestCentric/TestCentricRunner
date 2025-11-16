// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Engine.Communication.Messages;
using NUnit.Engine.Communication.Protocols;
using NUnit.Engine.Communication.Transports.Tcp;

namespace TestCentric.Engine.Communication.Transports.Tcp
{
    /// <summary>
    /// TestAgentTcpProxy wraps a RemoteTestAgent so that certain
    /// of its properties may be accessed directly.
    /// </summary>
    internal class TestAgentTcpProxy : ITestAgent, NUnit.Engine.ITestEngineRunner
    {
        private static readonly Logger log = InternalTrace.GetLogger(typeof(TestAgentTcpProxy));

        private Socket _socket;
        private BinarySerializationProtocol _wireProtocol = new BinarySerializationProtocol();
        private XmlSerializer _testPackageSerializer = new XmlSerializer(typeof(TestPackage));

        public TestAgentTcpProxy(Socket socket, Guid id)
        {
           _socket = socket;
            Id = id;
        }

        public Guid Id { get; }

        public NUnit.Engine.ITestEngineRunner CreateRunner(TestPackage package)
        {
            var writer = new StringWriter();
            var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings() { OmitXmlDeclaration = true });
            _testPackageSerializer.Serialize(xmlWriter, package);
            SendCommandMessage(MessageCode.CreateRunner, writer.ToString());

            // Agent also functions as the runner
            return this;
        }

        public bool Start()
        {
            // Not used for TCP since agent must already be started
            // in order to receive any messages at all.
            throw new NotImplementedException("Not used for TCP Transport");
        }

        public void Stop() => SendCommandMessage(MessageCode.StopAgent);

        public NUnit.Engine.TestEngineResult Load()
        {
            SendCommandMessage(MessageCode.LoadCommand);
            return new NUnit.Engine.TestEngineResult(GetCommandResult());
        }

        public void Unload() => SendCommandMessage(MessageCode.UnloadCommand);

        public NUnit.Engine.TestEngineResult Reload()
        {
            SendCommandMessage(MessageCode.ReloadCommand);
            return new NUnit.Engine.TestEngineResult(GetCommandResult());
        }

        public int CountTestCases(NUnit.Engine.TestFilter filter)
        {
            SendCommandMessage(MessageCode.CountCasesCommand, filter.Text);
            return int.Parse(GetCommandResult());
        }

        public NUnit.Engine.TestEngineResult Run(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            SendCommandMessage(MessageCode.RunCommand, filter.Text);

            return TestRunResult(listener);
        }

        public NUnit.Engine.AsyncTestEngineResult RunAsync(NUnit.Engine.ITestEventListener listener, NUnit.Engine.TestFilter filter)
        {
            SendCommandMessage(MessageCode.RunAsyncCommand, ((NUnit.Engine.TestFilter)filter).Text);

            return new NUnit.Engine.AsyncTestEngineResult();
        }

        public void RequestStop() => SendCommandMessage(MessageCode.RequestStopCommand);

        public void ForcedStop() => SendCommandMessage(MessageCode.ForcedStopCommand);

        public NUnit.Engine.TestEngineResult Explore(NUnit.Engine.TestFilter filter)
        {
            SendCommandMessage(MessageCode.ExploreCommand, filter.Text);
            return new NUnit.Engine.TestEngineResult(GetCommandResult());
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        private void SendCommandMessage(string command, string argument = null)
        {
            _socket.Send(_wireProtocol.Encode(new TestEngineMessage(command, argument)));
            log.Debug($"Sent {command} command");
        }

        private string GetCommandResult()
        {
            log.Debug("Waiting for command result");
            return new SocketReader(_socket, _wireProtocol).GetNextMessage().Data;
        }

        // Return the result of a test run as a TestEngineResult. ProgressMessages
        // preceding the final CommandReturnMessage are handled as well.
        private NUnit.Engine.TestEngineResult TestRunResult(NUnit.Engine.ITestEventListener listener)
        {
            var rdr = new SocketReader(_socket, _wireProtocol);
            while (true)
            {
                var message = rdr.GetNextMessage();

                switch(message.Code)
                {
                    case MessageCode.CommandResult:
                        return new NUnit.Engine.TestEngineResult(message.Data);
                    case MessageCode.ProgressReport:
                        listener.OnTestEvent(message.Data);
                        break;
                    default:
                        throw new InvalidOperationException($"Expected either a ProgressMessage or a CommandReturnMessage but received a {message.Code} message");
                }
            }
        }
    }
}
