﻿// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using System.Xml;
using TestCentric.Engine.Extensibility;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// Wrapper class for listeners based on the NUnit API
    /// </summary>
    public class NUnitTestEventListenerWrapper : ITestEventListener
    {
        private NUnit.Engine.ITestEventListener _listener;

        public NUnitTestEventListenerWrapper(NUnit.Engine.ITestEventListener listener)
        {
            _listener = listener;
        }

        public void OnTestEvent(string report)
        {
            _listener.OnTestEvent(report);
        }
    }

    /// <summary>
    /// Wrapper class for result writers based on the NUnit API
    /// </summary>
    public class NUnitResultWriterWrapper : IResultWriter
    {
        private NUnit.Engine.Extensibility.IResultWriter _writer;

        public NUnitResultWriterWrapper(NUnit.Engine.Extensibility.IResultWriter writer)
        {
            _writer = writer;
        }

        public void CheckWritability(string outputPath)
        {
            _writer.CheckWritability(outputPath);
        }

        public void WriteResultFile(XmlNode resultNode, string outputPath)
        {
            _writer.WriteResultFile(resultNode, outputPath);
        }

        public void WriteResultFile(XmlNode resultNode, TextWriter writer)
        {
            _writer.WriteResultFile(resultNode, writer);
        }
    }
}
