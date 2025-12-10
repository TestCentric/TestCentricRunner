// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using System.Xml;
using NUnit;
using NUnit.Framework;
using TestCentric.Engine.Services.Fakes;

namespace TestCentric.Engine.Services
{
    public class NUnitResultWriterWrapperTests
    {
        private FakeNUnitResultWriter _resultWriter;

        [SetUp]
        public void SetUp()
        {
            _resultWriter = new FakeNUnitResultWriter();
        }

        [Test]
        public void CheckWritability()
        {
            _resultWriter.CheckWritability(@"\Path\To\File");

            Assert.That(_resultWriter.OutputPath, Is.EqualTo(@"\Path\To\File"));
            Assert.That(_resultWriter.Output, Is.EqualTo(@"Able to write to \Path\To\File"));
        }

        [Test]
        public void WriteResultFileToPath()
        {
            XmlNode xmlNode = XmlHelper.CreateXmlNode("<msg>This is the output</msg>");
            _resultWriter.WriteResultFile(xmlNode, @"\Path\To\File");

            Assert.That(_resultWriter.OutputPath, Is.EqualTo(@"\Path\To\File"));
            Assert.That(_resultWriter.Output, Is.EqualTo("<msg>This is the output</msg>"));
        }

        [Test]
        public void ReadResultFileToTextWriter()
        {
            StringWriter writer = new StringWriter();
            XmlNode xmlNode = XmlHelper.CreateXmlNode("<msg>This is the output</msg>");
            _resultWriter.WriteResultFile(xmlNode, writer);

            Assert.That(_resultWriter.OutputPath, Is.Null);
            Assert.That(_resultWriter.Output, Is.EqualTo("<msg>This is the output</msg>"));
            Assert.That(writer.GetStringBuilder().ToString(), Is.EqualTo("<msg>This is the output</msg>"));
        }
    }
}
