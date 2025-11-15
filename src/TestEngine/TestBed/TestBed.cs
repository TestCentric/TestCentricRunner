// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using TestCentric.Engine.Services;

namespace TestCentric.Engine.TestBed
{
    /// <summary>
    /// A simple test runner used for testing packages. The
    /// arguments are a list of tests to run, specified as
    /// a relative path to the test assembly from the location
    /// of the runner. No options are supported - all the
    /// tests in each assembly are run.
    /// </summary>
    class TestBed
    {
        static readonly ITestEngine TestEngine = new TestEngine();

        static void Main(string[] args)
        {
            var attr = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>();

            Console.WriteLine("TestCentric Engine Test Bed");
            Console.WriteLine();

            Console.WriteLine("Test Environment");
            Console.WriteLine($"  Engine Version: {attr.InformationalVersion}");
            Console.WriteLine();

            CommandLineOptions options = null;

            try
            {
                options = new CommandLineOptions(args);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Environment.Exit(1);
            }

            if (options.ListExtensions)
            {
                ListExtensions();
                return;
            }

            Console.WriteLine("Test Files");
            foreach (var file in options.Files)
                    Console.WriteLine("  " + file);
            Console.WriteLine();

            var package = new TestPackage(options.Files);

            if (options.Trace != null)
            {
                package.AddSetting("InternalTraceLevel", options.Trace);
                TestEngine.InternalTraceLevel = (InternalTraceLevel)Enum.Parse(typeof(InternalTraceLevel), options.Trace);
            }

            TestEngine.Initialize();

            if (options.DebugAgent)
                package.AddSetting("DebugAgent", true);

            string resultFile = "TestResult.xml";

            if (!string.IsNullOrEmpty(options.WorkDirectory))
            {
                package.AddSetting("WorkDirectory", options.WorkDirectory);
                resultFile = Path.Combine(options.WorkDirectory, resultFile);
            }

            if (!string.IsNullOrEmpty(options.RequestedRuntime))
                package.AddSetting(NUnit.Common.SettingDefinitions.RequestedRuntimeFramework.WithValue(options.RequestedRuntime));

            TestEngine.Services.GetService<IExtensionService>().InstallExtensions();

            var runner = TestEngine.GetRunner(package);

            var eventHandler = new TestEventHandler();
            eventHandler.RunStarted += () => OnRunStarted(runner, options);

            // TODO: Handle case with no files passed in arguments
            XmlNode resultNode = runner.Run(eventHandler, NUnit.Engine.TestFilter.Empty);

            ResultReporter.ReportResults(resultNode);

            SaveTestResults(resultNode, resultFile); 

            Environment.Exit(0);
        }

        private static void OnRunStarted(ITestRunner runner, CommandLineOptions options)
        {
            // NOTE: It takes some finite amount of time for the run to actually
            // start. Once it does, we set up delays for stop and cancel.
            if (options.StopTimeout > 0)
                Task.Delay(options.StopTimeout).ContinueWith(t =>
                {
                    if (runner.IsTestRunning)
                    {
                        runner.StopRun(false);
                        Console.WriteLine("Stopping...");
                    }
                    else
                        Console.WriteLine("Unable to stop run - not yet started.");
                });

            if (options.CancelTimeout > 0)
                Task.Delay(options.CancelTimeout).ContinueWith(t =>
                {
                    if (runner.IsTestRunning)
                    {
                        runner.StopRun(true);
                        Console.WriteLine("Cancelling...");
                    }
                    else
                        Console.WriteLine("Unable to cancel run - not yet started.");
                });
        }

        static void SaveTestResults(XmlNode resultNode, string resultFile)
        {
            using (var stream = new FileStream(resultFile, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                var settings = new XmlWriterSettings
                {
                    Indent = true
                };

                using (XmlWriter xmlWriter = XmlWriter.Create(writer, settings))
                {
                    xmlWriter.WriteStartDocument(false);
                    resultNode.WriteTo(xmlWriter);
                }

                Console.WriteLine();
                Console.WriteLine($"Saved results to {resultFile}");
            }
        }

        private static void ListExtensions()
        {
            var extensionService = TestEngine.Services.GetService<IExtensionService>();

            Console.WriteLine("Extension Points and Installed Extensions");
            Console.WriteLine();

            foreach (var ep in extensionService.ExtensionPoints)
            {
                Console.WriteLine($"  Extension Point: {ep.Path}");
                int extensionCount = 0;
                foreach (var extension in ep.Extensions)
                {
                    Console.WriteLine($"    Extension: {extension.TypeName}");
                    Console.WriteLine($"     Assembly: {extension.AssemblyPath}");
                    ++extensionCount;
                }
            }
        }
    }
}
