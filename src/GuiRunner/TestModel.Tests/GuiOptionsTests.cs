// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

namespace TestCentric.Gui.Tests
{
    [TestFixture]
    public class GuiOptionsTests
    {
        [TestCase]
        [TestCase("tests.dll")]
        [TestCase("one.dll", "two.dll", "three.dll")]
        public void InputFiles(params string[] files)
        {
            var options = new GuiOptions(files);

            Assert.That(options.InputFiles.Count, Is.EqualTo(files.Length));
            for (int i = 0; i < files.Length; i++)
                Assert.That(options.InputFiles[i], Is.EqualTo(files[i]));

            Assert.That(options.Validate(), Is.True);
        }

        [TestCase("NoLoad", false)]
        [TestCase("RunAllTests", false)]
        [TestCase("RunAsX86", false)]
        [TestCase("ShowHelp", false)]
        [TestCase("InternalTraceLevel", null)]
        [TestCase("DebugAgent", false)]
        [TestCase("Unattended", false)]
        public void DefaultOptionValues(string propertyName, object val)
        {
            var property = GetPropertyInfo(propertyName);
            var options = new GuiOptions();

            Assert.That(property.GetValue(options, null), Is.EqualTo(val));
        }

        [TestCase("NoLoad", "--noload", true)]
        [TestCase("RunAllTests", "--run", true)]
        [TestCase("Unattended", "--unattended", true)]
#if !DISABLE_MINI_GUI
        [TestCase("GuiLayout", "--full-gui", "Full")]
        [TestCase("GuiLayout", "--mini-gui", "Mini")]
#endif
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
#if DEBUG
        [TestCase("DebugAgent", "--debug-agent", true)]
#endif
        public void ValidOptionsAreRecognized(string propertyName, string option, object expected = null)
        {
            var property = GetPropertyInfo(propertyName);
            var options = new GuiOptions(option);

            if (expected == null)
            {
                var index = option.IndexOf(':');
                expected = option.Substring(index + 1);
            }

            Assert.That(options.Validate(), Is.True, $"Should be valid: {option}");
            Assert.That(property.GetValue(options, null), Is.EqualTo(expected));
        }

        [TestCase("--param:X=5")]
        [TestCase("-p:X=5")]
        public void SingleTestParameter(string option)
        {
            var options = new GuiOptions(option);
            Assert.That(options.ErrorMessages, Is.Empty);
            Assert.That(options.TestParameters, Is.EqualTo(new Dictionary<string, string>() { { "X", "5" } }));
            Assert.That(options.TestParameters["X"], Is.EqualTo("5"));
        }

        [Test]
        public void MultipleTestParameters()
        {
            var options = new GuiOptions("--param:X=5", "-p:Y=7");
            Assert.That(options.ErrorMessages, Is.Empty);
            Assert.That(options.TestParameters, Is.EqualTo(new Dictionary<string, string>() { { "X", "5" }, { "Y", "7" } }));
            Assert.That(options.TestParameters["X"], Is.EqualTo("5"));
            Assert.That(options.TestParameters["Y"], Is.EqualTo("7"));
        }

        [TestCase("--agents")]
        [TestCase("--trace")]
        [TestCase("--param")]
        [TestCase("--assembly:nunit.tests.dll")]
        [TestCase("--garbage")]
        [TestCase("--agents:XYZ")]
        [TestCase("--trace:Something")]
        public void InvalidOptionsAreDetected(string option)
        {
            var options = new GuiOptions(option);
            Assert.That(options.Validate(), Is.False);
            Assert.That(options.ErrorMessages.Count, Is.EqualTo(1));
        }

        private static PropertyInfo GetPropertyInfo(string propertyName)
        {
            PropertyInfo property = typeof(GuiOptions).GetProperty(propertyName);
            Assert.That(property, Is.Not.Null, $"The property '{propertyName}' is not defined");
            return property;
        }
    }
}

