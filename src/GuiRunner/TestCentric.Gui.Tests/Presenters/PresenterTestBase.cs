// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using NSubstitute;
using NUnit.Framework;

using FakeUserSettings = TestCentric.Gui.Fakes.UserSettings;

namespace TestCentric.Gui.Presenters
{
    using Elements;
    using Model;
    using TestCentric.Gui.Model.Settings;

    /// <summary>
    /// Base class for presenter tests, providing utiity methods
    /// </summary>
    public class PresenterTestBase<TView> where TView : class
    {
        protected TView _view;
        protected ITestModel _model;
        protected IUserSettings _settings;

        [SetUp]
        public void Initialize()
        {
            _view = Substitute.For<TView>();
            _model = Substitute.For<ITestModel>();
            _settings = FakeUserSettings.Create();
            _model.Settings.Returns(_settings);
            //_model.TestProject.Returns(new TestCentricProject(_model, "Dummy.dll"));
        }

        #region Helper Methods

        protected void ClearAllReceivedCalls()
        {
            _view.ClearReceivedCalls();

            foreach (PropertyInfo prop in _view.GetType().GetProperties())
            {
                if (typeof(IViewElement).IsAssignableFrom(prop.PropertyType))
                    prop.GetValue(_view).ClearReceivedCalls();
            }
        }

        protected void FireProjectLoadedEvent()
        {
            _model.Events.TestCentricProjectLoaded += Raise.Event<TestEventHandler>(new TestEventArgs());
        }

        protected void FireProjectUnloadedEvent()
        {
            _model.Events.TestCentricProjectUnloaded += Raise.Event<TestEventHandler>(new TestEventArgs());
        }

        protected void FireTestsLoadingEvent(IList<string> testFiles)
        {
            _model.Events.TestsLoading += Raise.Event<TestFilesLoadingEventHandler>(new TestFilesLoadingEventArgs(testFiles));
        }

        protected void FireTestLoadedEvent(TestNode testNode)
        {
            _model.Events.TestLoaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(testNode));
        }

        protected void FireTestsReloadingEvent()
        {
            _model.Events.TestsReloading += Raise.Event<TestEventHandler>(new TestEventArgs());
        }

        protected void FireTestReloadedEvent(TestNode testNode)
        {
            _model.Events.TestReloaded += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(testNode));
        }

        protected void FireTestUnloadedEvent()
        {
            _model.Events.TestUnloaded += Raise.Event<TestEventHandler>(new TestEventArgs());
        }

        protected void FireTestsUnloadingEvent()
        {
            _model.Events.TestsUnloading += Raise.Event<TestEventHandler>(new TestEventArgs());
        }

        protected void FireTestAssemblyChangedEvent()
        {
            _model.Events.TestChanged += Raise.Event<TestEventHandler>(new TestEventArgs());
        }

        protected void FireRunStartingEvent(int testCount)
        {
            _model.Events.RunStarting += Raise.Event<RunStartingEventHandler>(new RunStartingEventArgs(testCount));
        }

        protected void FireRunFinishedEvent(ResultNode result)
        {
            _model.Events.RunFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(result));
        }

        protected void FireTestStartingEvent(string testName)
        {
            var start = new TestNode($"<test-start fullname='{testName}'/>");
            _model.Events.TestStarting += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(start));
        }

        protected void FireSuiteStartingEvent(string testName)
        {
            var start = new TestNode($"<suite-start fullname='{testName}'/>");
            _model.Events.SuiteStarting += Raise.Event<TestNodeEventHandler>(new TestNodeEventArgs(start));
        }

        protected void FireTestFinishedEvent(string testName, string result, string output = null)
        {
            FireTestFinishedEvent(CreateResultNode("test-case", testName, result, output));

        }

        protected void FireTestFinishedEvent(string testName, string result, FailureSite site)
        {
            FireTestFinishedEvent(CreateResultNode("test-case", testName, result, site));
        }

        protected void FireTestFinishedEvent(ResultNode result)
        {
            _model.Events.TestFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(result));
        }

        protected void FireSuiteFinishedEvent(string testName, string result, string output = null)
        {
            FireSuiteFinishedEvent(CreateResultNode("test-suite", testName, result, output));
        }

        protected void FireSuiteFinishedEvent(string testName, string result, FailureSite site, int testCount = 0)
        {
            FireSuiteFinishedEvent(CreateResultNode("test-suite", testName, result, site, testCount));
        }

        protected void FireSuiteFinishedEvent(ResultNode result)
        {
            _model.Events.SuiteFinished += Raise.Event<TestResultEventHandler>(new TestResultEventArgs(result));
        }

        private static ResultNode CreateResultNode(string element, string testName, string result, string output = null)
        {
            return new ResultNode(CreateResultXml(element, testName, result, output));
        }

        private static ResultNode CreateResultNode(string element, string testName, string result, FailureSite site, int testCount = 0)
        {
            XmlNode xmlNode = CreateResultXml(element, testName, result, site);
            if (testCount != 0)
                xmlNode.AddAttribute("testcasecount", testCount.ToString());

            return new ResultNode(xmlNode);
        }

        private static XmlNode CreateResultXml(string element, string testName, string result, FailureSite site)
        {
            var xmlNode = CreateResultXml(element, testName, result);
            xmlNode.AddAttribute("site", site.ToString());
            return xmlNode;
        }

        private static XmlNode CreateResultXml(string element, string testName, string result, string output = null)
        {
            int colon = result.IndexOf(':');
            string xml;

            if (colon > 0)
            {
                string status = result.Substring(0, colon);
                string label = result.Substring(colon + 1);
                xml = $"<{element} fullname='{testName}' result='{status}' label='{label}'/>";
            }
            else
            {
                xml = $"<{element} fullname='{testName}' result='{result}'/>";
            }

            var xmlNode = XmlHelper.CreateXmlNode(xml);
            if (output != null)
                xmlNode.AddElementWithCDataSection("output", output);

            return xmlNode;
        }

        protected void FireTestOutputEvent(string testName, string stream, string text)
        {
            _model.Events.TestOutput += Raise.Event<TestOutputEventHandler>(new TestOutputEventArgs(testName, stream, text));
        }

        #endregion
    }
}
