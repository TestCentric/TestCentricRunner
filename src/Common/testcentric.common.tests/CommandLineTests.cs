// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Reflection;
using NUnit.Framework;

namespace TestCentric.Gui.Tests
{
    [TestFixture]
    public class CommandLineTests
    {
        [TestCase]
        [TestCase("tests.dll")]
        [TestCase("one.dll", "two.dll", "three.dll")]
        public void InputFiles(params string[] files)
        {
            var options = new CommandLineOptions(files);

            Assert.That(options.InputFiles.Count, Is.EqualTo(files.Length));
            for (int i = 0; i < files.Length; i++)
                Assert.That(options.InputFiles[i], Is.EqualTo(files[i]));

            Assert.That(options.Validate());
        }

        [TestCase("NoLoad", false)]
        [TestCase("RunAllTests", false)]
        [TestCase("RunAsX86", false)]
        [TestCase("ShowHelp", false)]
        [TestCase("ProcessModel", null)]
        [TestCase("DomainUsage", null)]
        [TestCase("InternalTraceLevel", null)]
        [TestCase("DebugTests", false)]
        [TestCase("DebugAgent", false)]
        [TestCase("Unattended", false)]
        public void DefaultOptionValues(string propertyName, object val)
        {
            var property = GetPropertyInfo(propertyName);
            var options = new CommandLineOptions();

            Assert.That(property.GetValue(options, null), Is.EqualTo(val));
        }

        [TestCase("NoLoad", "--noload", true)]
        [TestCase("RunAllTests", "--run", true)]
        [TestCase("Unattended", "--unattended", true)]
        [TestCase("ProcessModel", "--process:InProcess")]
        [TestCase("ProcessModel", "--process:Separate")]
        [TestCase("ProcessModel", "--process:Multiple")]
        [TestCase("ProcessModel", "--inprocess", "InProcess")]
        [TestCase("DomainUsage", "--domain:None")]
        [TestCase("DomainUsage", "--domain:Single")]
        [TestCase("DomainUsage", "--domain:Multiple")]
        [TestCase("WorkDirectory", "--work:PathToWorkDirectory")]
        [TestCase("RunAsX86", "--x86", true)]
        [TestCase("MaxAgents", "--agents:8", 8)]
        [TestCase("InternalTraceLevel", "--trace:Off")]
        [TestCase("InternalTraceLevel", "--trace:Error")]
        [TestCase("InternalTraceLevel", "--trace:Warning")]
        [TestCase("InternalTraceLevel", "--trace:Info")]
        [TestCase("InternalTraceLevel", "--trace:Verbose")]
        [TestCase("InternalTraceLevel", "--trace:Debug")]
        [TestCase("ShowHelp", "--help", true)]
        [TestCase("ShowHelp", "-h", true)]
        [TestCase("DebugTests", "--debug", true)]
#if DEBUG
        [TestCase("DebugAgent", "--debug-agent", true)]
#endif
        public void ValidOptionsAreRecognized(string propertyName, string option, object expected = null)
        {
            var property = GetPropertyInfo(propertyName);
            var options = new CommandLineOptions(option);

            if (expected == null)
            {
                var index = option.IndexOf(':');
                expected = option.Substring(index + 1);
            }

            Assert.That(options.Validate(), Is.True, $"Should be valid: {option}");
            Assert.That(property.GetValue(options, null), Is.EqualTo(expected));
        }

        [TestCase("--process")]
        [TestCase("--agents")]
        [TestCase("--domain")]
        [TestCase("--trace")]
        public void InvalidOptionsAreDetectedByMonoOptions(string option)
        {
            // We would prefer to handle all errors ourselves so
            // this will eventually change. The test documents
            // the current behavior.
            Assert.Throws<Mono.Options.OptionException>(
                () => new CommandLineOptions(option));
        }

        [TestCase("--assembly:nunit.tests.dll")]
        [TestCase("--garbage")]
        [TestCase("--process:Unknown")]
        [TestCase("--agents:XYZ")]
        [TestCase("--trace:Something")]
        public void InvalidOptionsAreDetected(string option)
        {
            var options = new CommandLineOptions(option);
            Assert.That(options.Validate(), Is.False);
            Assert.That(options.ErrorMessages.Count, Is.EqualTo(1));
        }

        private static PropertyInfo GetPropertyInfo(string propertyName)
        {
            PropertyInfo property = typeof(CommandLineOptions).GetProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"The property '{propertyName}' is not defined");
            return property;
        }
    }
}

