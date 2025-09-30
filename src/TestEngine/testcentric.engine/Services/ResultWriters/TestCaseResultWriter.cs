// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using System.Xml;
using TestCentric.Engine.Extensibility;

namespace TestCentric.Engine.Services
{
    public class TestCaseResultWriter : NUnit.Engine.Extensibility.IResultWriter
    {
        public void CheckWritability(string outputPath)
        {
            using (new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            {
                // Opening is enough to check
            }
        }

        public void WriteResultFile(XmlNode resultNode, string outputPath)
        {
            using (var stream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                WriteResultFile(resultNode, writer);
            }
        }

        public void WriteResultFile(XmlNode resultNode, TextWriter writer)
        {
            foreach (XmlNode node in resultNode.SelectNodes("//test-case"))
                writer.WriteLine(node.Attributes["fullname"].Value);
        }
    }
}
