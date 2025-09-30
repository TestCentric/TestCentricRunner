// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using TestCentric.Engine.Extensibility;

namespace TestCentric.Engine.Services
{
    public class XmlTransformResultWriter : NUnit.Engine.Extensibility.IResultWriter
    {
        private string _xsltFile;
        private readonly XslCompiledTransform _transform = new XslCompiledTransform();

        public XmlTransformResultWriter(object[] args)
        {
            Guard.ArgumentNotNull(args, "args");
            _xsltFile = args[0] as string;

            Guard.ArgumentValid(
                !string.IsNullOrEmpty(_xsltFile),
                "Argument to XmlTransformWriter must be a non-empty string",
                "args");

            try
            {
                var settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;
                using (var xmlReader = XmlReader.Create(_xsltFile, settings))
                    _transform.Load(xmlReader);
            }
            catch (Exception ex)
            {
                throw new ArgumentException("Unable to load transform " + _xsltFile, ex.InnerException);
            }
        }

        /// <summary>
        /// Checks if the output is writable. If the output is not
        /// writable, this method should throw an exception.
        /// </summary>
        /// <param name="outputPath"></param>
        public void CheckWritability(string outputPath)
        {
            using ( new StreamWriter( outputPath, false ) )
            {
                // We don't need to check if the XSLT file exists,
                // that would have thrown in the constructor
            }
        }

        public void WriteResultFile(XmlNode result, TextWriter writer)
        {
            using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings {Indent = true, ConformanceLevel = ConformanceLevel.Auto}))
            {
                _transform.Transform(result, xmlWriter);
            }
        }

        public void WriteResultFile(XmlNode result, string outputPath)
        {
            using (var xmlWriter = XmlWriter.Create(outputPath, new XmlWriterSettings {Indent = true, ConformanceLevel = ConformanceLevel.Auto}))
            {
                _transform.Transform(result, xmlWriter);
            }
        }
    }
}
