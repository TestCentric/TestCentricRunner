// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

#if !NETCOREAPP // TODO: Can't instantiate engine under .NET CORE since it's not in the agent directory
using System.Collections.Generic;
using System.IO;
using NUnit.Engine;
using NUnit.Framework;

namespace TestCentric.Engine.Services.ResultWriters
{
    using Runner = NUnit.Framework.Api.NUnitTestAssemblyRunner;
    using Builder = NUnit.Framework.Api.DefaultTestAssemblyBuilder;
    using TestListener = NUnit.Framework.Internal.TestListener;
    using TestFilter = NUnit.Framework.Internal.TestFilter;

    /// <summary>
    /// This is the abstract base for all XML output tests,
    /// which need to work on a TestEngineResult. Creating a
    /// second level engine in the test domain causes
    /// problems, so this class uses internal framework
    /// classes to run the test and then transforms the XML
    /// result into a TestEngineResult for use by derived tests.
    /// </summary>
    public abstract class XmlOutputTest
    {
        private ITestEngine engine;

        protected NUnit.Engine.TestEngineResult EngineResult { get; private set; }

        protected const string AssemblyName = "mock-assembly.dll";
        protected string AssemblyPath { get; private set; }

        // Method used by derived classes to get the path to a file name
        protected string GetLocalPath(string fileName)
        {
            return Path.Combine(TestContext.CurrentContext.TestDirectory, fileName);
        }

        [OneTimeSetUp]
        public void InitializeTestEngineResult()
        {

            AssemblyPath = GetLocalPath(AssemblyName);

            // Create a fresh copy of the engine, since we can't use the
            // one that is running this test.
            engine = new TestEngine(); ;
            engine.InternalTraceLevel = InternalTraceLevel.Off;

            // Create a new DefaultAssemblyRunner, which is actually a framework class,
            // because we can't use the one that's currently running this test.
            var runner = new Runner(new Builder());
            var settings = new Dictionary<string, object>();

            // Make sure the runner loaded the mock assembly.
            Assert.That(
                runner.Load(AssemblyPath, settings).RunState.ToString(),
                Is.EqualTo("Runnable"),
                "Unable to load mock-assembly.dll");

            // Run the tests, saving the result as an XML string
            var xmlText = runner.Run(TestListener.NULL, TestFilter.Empty).ToXml(true).OuterXml;

            // Create a TestEngineResult from the string, just as the TestEngine does,
            // then add a test-run element to the result, wrapping the result so it
            // looks just like what the engine would return!
            this.EngineResult = new NUnit.Engine.TestEngineResult(xmlText).Aggregate("test-run", "ID", AssemblyName, AssemblyPath);
        }
    }
}
#endif
