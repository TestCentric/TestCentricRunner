// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Engine.Extensibility;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// IResultWriterService provides result writers for a specified
    /// well-known format.
    /// </summary>
    public interface IResultService
    {
        /// <summary>
        /// Gets an array of the available formats
        /// </summary>
        string[] Formats { get; }

        /// <summary>
        /// Gets a ResultWriter for a given format and set of arguments.
        /// </summary>
        /// <param name="format">The name of the format to be used</param>
        /// <param name="args">A set of arguments to be used in constructing the writer or null if non arguments are needed</param>
        /// <returns>An IResultWriter</returns>
        NUnit.Engine.Extensibility.IResultWriter GetResultWriter(string format, object[] args);
    }
}
