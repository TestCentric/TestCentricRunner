// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using System.Xml;
using TestCentric.Extensibility;

namespace TestCentric.Engine.Services.Fakes
{
    [Extension]
    [ExtensionProperty("Format", "custom")]
    public class FakeNUnitResultWriter : NUnit.Engine.Extensibility.IResultWriter
    {
        public string Output { get; private set; }
        public string OutputPath { get; private set; }

        public void CheckWritability(string outputPath)
        {
            OutputPath = outputPath;
            Output = $"Able to write to {outputPath}";
        }

        public void WriteResultFile(XmlNode resultNode, string outputPath)
        {
            Output = resultNode.OuterXml;
            OutputPath = outputPath;
        }

        public void WriteResultFile(XmlNode resultNode, TextWriter writer)
        {
            Output = resultNode.OuterXml;
            OutputPath = null;
            writer.Write(Output);
        }
    }
}
