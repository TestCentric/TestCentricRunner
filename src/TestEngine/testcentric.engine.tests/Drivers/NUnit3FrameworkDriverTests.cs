// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

#if !NETCOREAPP1_1 &&!NETCOREAPP2_1
using System;
using System.Collections.Generic;
using System.Xml;
using NUnit.Engine.Helpers;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Tests.Assemblies;

namespace NUnit.Engine.Drivers
{
    // Functional tests of the NUnitFrameworkDriver calling into the framework.
    public class NUnit3FrameworkDriverTests
    {
        private const string MOCK_ASSEMBLY = "mock-assembly.dll";
        private const string LOAD_MESSAGE = "Method called without calling Load first";

        private IDictionary<string, object> _settings = new Dictionary<string, object>();

        private NUnit3FrameworkDriver _driver;
        private string _mockAssemblyPath;

        [SetUp]
        public void CreateDriver()
        {
            var assemblyName = typeof(NUnit.Framework.TestAttribute).Assembly.GetName();
            _mockAssemblyPath = System.IO.Path.Combine(TestContext.CurrentContext.TestDirectory, MOCK_ASSEMBLY);
            _driver = new NUnit3FrameworkDriver(AppDomain.CurrentDomain, assemblyName);
        }

        //[Test]
        //public void ConstructController()
        //{
        //    Assert.That(_controller..Builder, Is.TypeOf<DefaultTestAssemblyBuilder>());
        //    Assert.That(_controller.Runner, Is.TypeOf<DefaultTestAssemblyRunner>());
        //    Assert.That(_controller.AssemblyPath, Is.EqualTo(MOCK_ASSEMBLY));
        //    Assert.That(_controller.Settings, Is.SameAs(_settings));
        //}

        public void ConstructController_MissingFile_ThrowsArgumentInvalid()
        {
            var assemblyName = typeof(NUnit.Framework.TestAttribute).Assembly.GetName();
            Assert.That(new NUnit3FrameworkDriver(AppDomain.CurrentDomain, assemblyName), Throws.ArgumentException);
        }

        [Test]
        public void Load_GoodFile_ReturnsRunnableSuite()
        {
            var result = XmlHelper.CreateXmlNode(_driver.Load(_mockAssemblyPath, _settings));

            Assert.That(result.Name, Is.EqualTo("test-suite"));
            Assert.That(result.GetAttribute("type"), Is.EqualTo("Assembly"));
            Assert.That(result.GetAttribute("runstate"), Is.EqualTo("Runnable"));
            Assert.That(result.GetAttribute("testcasecount"), Is.EqualTo(MockAssembly.Tests.ToString()));
            Assert.That(result.SelectNodes("test-suite").Count, Is.EqualTo(0), "Load result should not have child tests");
        }

        [Test]
        public void Explore_AfterLoad_ReturnsRunnableSuite()
        {
            _driver.Load(_mockAssemblyPath, _settings);
            var result = XmlHelper.CreateXmlNode(_driver.Explore(TestFilter.Empty.Text));

            Assert.That(result.Name, Is.EqualTo("test-suite"));
            Assert.That(result.GetAttribute("type"), Is.EqualTo("Assembly"));
            Assert.That(result.GetAttribute("runstate"), Is.EqualTo("Runnable"));
            Assert.That(result.GetAttribute("testcasecount"), Is.EqualTo(MockAssembly.Tests.ToString()));
            Assert.That(result.SelectNodes("test-suite").Count, Is.GreaterThan(0), "Explore result should have child tests");
        }

        [Test]
        public void ExploreTestsAction_WithoutLoad_ThrowsInvalidOperationException()
        {
            var ex = Assert.Catch(() => _driver.Explore(TestFilter.Empty.Text));
            if (ex is System.Reflection.TargetInvocationException)
                ex = ex.InnerException;
            Assert.That(ex, Is.TypeOf<InvalidOperationException>());
            Assert.That(ex.Message, Is.EqualTo(LOAD_MESSAGE));
        }

        [Test]
        public void CountTestsAction_AfterLoad_ReturnsCorrectCount()
        {
            _driver.Load(_mockAssemblyPath, _settings);
            Assert.That(_driver.CountTestCases(TestFilter.Empty.Text), Is.EqualTo(MockAssembly.Tests));
        }

        [Test]
        public void CountTestsAction_WithoutLoad_ThrowsInvalidOperationException()
        {
            var ex = Assert.Catch(() => _driver.CountTestCases(TestFilter.Empty.Text));
            if (ex is System.Reflection.TargetInvocationException)
                ex = ex.InnerException;
            Assert.That(ex, Is.TypeOf<InvalidOperationException>());
            Assert.That(ex.Message, Is.EqualTo(LOAD_MESSAGE));
        }

        [Test]
        public void RunTestsAction_AfterLoad_ReturnsRunnableSuite()
        {
            _driver.Load(_mockAssemblyPath, _settings);
            var result = XmlHelper.CreateXmlNode(_driver.Run(new NullListener(), TestFilter.Empty.Text));

            Assert.That(result.Name, Is.EqualTo("test-suite"));
            Assert.That(result.GetAttribute("type"), Is.EqualTo("Assembly"));
            Assert.That(result.GetAttribute("runstate"), Is.EqualTo("Runnable"));
            Assert.That(result.GetAttribute("testcasecount"), Is.EqualTo(MockAssembly.Tests.ToString()));
            Assert.That(result.GetAttribute("result"), Is.EqualTo("Failed"));
            Assert.That(result.GetAttribute("passed"), Is.EqualTo(MockAssembly.PassedInAttribute.ToString()));
            Assert.That(result.GetAttribute("failed"), Is.EqualTo(MockAssembly.Failed.ToString()));
            Assert.That(result.GetAttribute("skipped"), Is.EqualTo(MockAssembly.Skipped.ToString()));
            Assert.That(result.GetAttribute("inconclusive"), Is.EqualTo(MockAssembly.Inconclusive.ToString()));
            Assert.That(result.SelectNodes("test-suite").Count, Is.GreaterThan(0), "Explore result should have child tests");
        }

        [Test]
        public void RunTestsAction_WithoutLoad_ThrowsInvalidOperationException()
        {
            var ex = Assert.Catch(() => _driver.Run(new NullListener(), TestFilter.Empty.Text));
            Assert.That(ex, Is.TypeOf<InvalidOperationException>());
            Assert.That(ex.Message, Is.EqualTo(LOAD_MESSAGE));
        }

        [Test]
        public void RunTestsAction_WithInvalidFilterElement_ThrowsNUnitEngineException()
        {
            _driver.Load(_mockAssemblyPath, _settings);

            var invalidFilter = "<filter><invalidElement>foo</invalidElement></filter>";
            var ex = Assert.Catch(() => _driver.Run(new NullListener(), invalidFilter));
            Assert.That(ex, Is.TypeOf<NUnitEngineException>());
        }

        private static string GetSkipReason(XmlNode result)
        {
            var propNode = result.SelectSingleNode(string.Format("properties/property[@name='{0}']", PropertyNames.SkipReason));
            return propNode == null ? null : propNode.GetAttribute("value");
        }

        private class CallbackEventHandler : System.Web.UI.ICallbackEventHandler
        {
            private string _result;

            public string GetCallbackResult()
            {
                return _result;
            }

            public void RaiseCallbackEvent(string eventArgument)
            {
                _result = eventArgument;
            }
        }

        public class NullListener : ITestEventListener
        {
            public void OnTestEvent(string testEvent)
            {
                // No action
            }
        }
    }
}
#endif
