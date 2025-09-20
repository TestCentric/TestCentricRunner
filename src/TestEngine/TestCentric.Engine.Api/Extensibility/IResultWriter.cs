// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.IO;
using System.Xml;

namespace TestCentric.Engine.Extensibility
{
    /// <summary>
    /// Common interface for objects that process and write out test results
    /// </summary>
    public interface IResultWriter
    {
        /// <summary>
        /// Checks if the output path is writable. If the output is not
        /// writable, this method should throw an exception.
        /// </summary>
        /// <param name="outputPath"></param>
        void CheckWritability(string outputPath);

        /// <summary>
        /// Writes result to the specified output path.
        /// </summary>
        /// <param name="resultNode">XmlNode for the result</param>
        /// <param name="outputPath">Path to which it should be written</param>
        void WriteResultFile(XmlNode resultNode, string outputPath);

        /// <summary>
        /// Writes result to a TextWriter.
        /// </summary>
        /// <param name="resultNode">XmlNode for the result</param>
        /// <param name="writer">TextWriter to which it should be written</param>
        void WriteResultFile(XmlNode resultNode, TextWriter writer);
    }
}
