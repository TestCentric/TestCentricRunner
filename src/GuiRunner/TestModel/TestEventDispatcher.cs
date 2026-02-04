// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Xml;

namespace TestCentric.Gui.Model
{
    public class TestEventDispatcher : ITestEvents, NUnit.Engine.ITestEventListener
    {
        private static Logger log = InternalTrace.GetLogger("TestEventDispatcher");

        private TestModel _model;

        private Dictionary<string, ProjectInfo> _projectLookup;

        private class ProjectInfo
        {
            public TestNode ProjectNode;
            public bool WasStarted;
            public int AssembliesToRun;

            public ProjectInfo(TestNode projectNode)
            {
                ProjectNode = projectNode;
                WasStarted = false;
                AssembliesToRun = 0;
            }
        }

        public TestEventDispatcher(TestModel model)
        {
            _model = model;
        }

        #region Public Methods to Fire Events

        //public void FireTestCentricProjectLoading()
        //{
        //    TestCentricProjectLoading?.Invoke(new TestEventArgs());
        //}

        //public void FireTestCentricProjecReloading()
        //{
        //    TestCentricProjectReloading?.Invoke(new TestEventArgs());
        //}

        //public void FireTestCentricProjectUnloading()
        //{
        //    TestCentricProjectUnloading?.Invoke(new TestEventArgs());
        //}

        public void FireTestCentricProjectLoaded()
        {
            TestCentricProjectLoaded?.Invoke(new TestEventArgs());
        }

        public VisualState RequestVisualState()
        {
            VisualStateEventArgs args = new VisualStateEventArgs();
            VisualStateRequest?.Invoke(args);
            return args.VisualState;
        }

        public void FireTestCentricProjectUnloaded()
        {
            TestCentricProjectUnloaded?.Invoke(new TestEventArgs());
        }

        public void FireTestsLoading(IList<string> files)
        {
            TestsLoading?.Invoke(new TestFilesLoadingEventArgs(files));
        }

        public void FireTestsReloading()
        {
            TestsReloading?.Invoke(new TestEventArgs());
        }

        public void FireTestsUnloading()
        {
            TestsUnloading?.Invoke(new TestEventArgs());
        }

        public void FireTestChanged()
        {
            TestChanged?.Invoke(new TestEventArgs());
        }

        public void FireTestLoaded(TestNode testNode)
        {
            TestLoaded?.Invoke(new TestNodeEventArgs(testNode));
        }

        public void FireTestUnloaded()
        {
            TestUnloaded?.Invoke(new TestEventArgs());
        }

        public void FireTestReloaded(TestNode testNode)
        {
            TestReloaded?.Invoke(new TestNodeEventArgs(testNode));
        }

        public void FireTestLoadFailure(Exception ex)
        {
            TestLoadFailure?.Invoke(new TestLoadFailureEventArgs(ex));
        }

        public void FireActiveItemChanged(ITestItem testItem)
        {
            SelectedItemChanged?.Invoke(new TestItemEventArgs(testItem));
        }

        public void FireSelectedTestsChanged(TestSelection testSelection)
        {
            SelectedTestsChanged?.Invoke(new TestSelectionEventArgs(testSelection));
        }

        public void FireCategorySelectionChanged()
        {
            CategorySelectionChanged?.Invoke(new TestEventArgs());
        }

        public void FireTestFilterChanged()
        {
            TestFilterChanged?.Invoke(new TestEventArgs());
        }

        #endregion

        #region ITestEvents Implementation

        // TestCentricProject loading events
        public event TestEventHandler TestCentricProjectLoaded;
        public event TestEventHandler TestCentricProjectUnloaded;
        public event VisualStateEventHandler VisualStateRequest;

        // Test loading events
        public event TestFilesLoadingEventHandler TestsLoading;
        public event TestEventHandler TestsReloading;
        public event TestEventHandler TestsUnloading;
        public event TestEventHandler TestChanged;

        public event TestNodeEventHandler TestLoaded;
        public event TestNodeEventHandler TestReloaded;
        public event TestEventHandler TestUnloaded;

        public event TestLoadFailureEventHandler TestLoadFailure;

        // Test running events
        public event RunStartingEventHandler RunStarting;
        public event TestResultEventHandler RunFinished;

        public event TestNodeEventHandler SuiteStarting;
        public event TestResultEventHandler SuiteFinished;

        public event TestNodeEventHandler TestStarting;
        public event TestResultEventHandler TestFinished;

        public event TestOutputEventHandler TestOutput;

        public event UnhandledExceptionEventHandler UnhandledException;

        // Test Selection Event
        public event TestItemEventHandler SelectedItemChanged;
        public event TestSelectionEventHandler SelectedTestsChanged;

        public event TestEventHandler CategorySelectionChanged;
        public event TestEventHandler TestFilterChanged;

        #endregion

        #region ITestEventListener Implementation

        public void OnTestEvent(string report)
        {
            XmlNode xmlNode = XmlHelper.CreateXmlNode(report);
            log.Debug($"Received event {xmlNode.Name}");

            switch (xmlNode.Name)
            {
                case "start-test":
                    InvokeHandler(TestStarting, new TestNodeEventArgs(new TestNode(xmlNode)));
                    break;

                case "start-suite":
                    var testNode = new TestNode(xmlNode);

                    CheckForProjectStart(testNode);

                    InvokeHandler(SuiteStarting, new TestNodeEventArgs(testNode));
                    break;

                case "start-run":
                    InitializeProjectDictionary();
                    InvokeHandler(RunStarting, new RunStartingEventArgs(xmlNode.GetAttribute("count", -1)));
                    break;

                case "test-case":
                    ResultNode resultNode = new ResultNode(xmlNode);
                    resultNode = _model.TestResultManager.AddResult(resultNode);
                    InvokeHandler(TestFinished, new TestResultEventArgs(resultNode));
                    break;

                case "test-suite":
                    resultNode = new ResultNode(xmlNode);
                    resultNode = _model.TestResultManager.AddResult(resultNode);

                    CheckForProjectFinish(resultNode);

                    InvokeHandler(SuiteFinished, new TestResultEventArgs(resultNode));
                    break;

                case "test-run":
                    resultNode = new ResultNode(xmlNode);
                    resultNode = _model.TestResultManager.AddResult(resultNode);
                    _model.ResultSummary = ResultSummaryCreator.FromResultNode(resultNode);
                    InvokeHandler(RunFinished, new TestResultEventArgs(resultNode));
                    break;

                case "test-output":
                    string testName = xmlNode.GetAttribute("testname");
                    string stream = xmlNode.GetAttribute("stream");
                    string text = xmlNode.InnerText;
                    InvokeHandler(TestOutput, new TestOutputEventArgs(testName, stream, text));
                    break;

                case "unhandled-exception":
                    string message = xmlNode.GetAttribute("message");
                    string stackTrace = xmlNode.GetAttribute("stacktrace");

                    InvokeHandler(UnhandledException, new UnhandledExceptionEventArgs(message, stackTrace));
                    break;
            }
        }

        #endregion

        #region Helper Methods

        private void InvokeHandler(MulticastDelegate handlerList, EventArgs e)
        {
            if (handlerList == null)
                return;

            object[] args = new object[] { e };
            foreach (Delegate handler in handlerList.GetInvocationList())
            {
                object target = handler.Target;
                System.Windows.Forms.Control control
                    = target as System.Windows.Forms.Control;

                if (control != null && control.InvokeRequired)
                        control.Invoke(handler, args);
                    else
                        handler.Method.Invoke(target, args);
            }
        }

        private void InitializeProjectDictionary()
        {
            // Initialize Dictionary to look up projects to which assemblies belong
            _projectLookup = new Dictionary<string, ProjectInfo>();

            foreach (var projectNode in _model.LoadedTests.Select(tn => tn.Type == "Project"))
            {
                var name = projectNode.GetAttribute("name");

                // TODO: Figure out why we are seeing duplicate assembly ids
                try
                {
                    var projectInfo = new ProjectInfo(projectNode);

                    foreach (var child in projectNode.Select((tn) => tn.Type == "Assembly"))
                    {
                        projectInfo.AssembliesToRun++;
                        _projectLookup.Add(child.GetAttribute("id"), projectInfo);
                    }
                }
                catch(Exception ex)
                {
                    log.Error("Unexpected exception", ex);
                }
            }
        }

        private void CheckForProjectStart(TestNode testNode)
        {
            if (_projectLookup.ContainsKey(testNode.Id))
            {
                var projectInfo = _projectLookup[testNode.Id];

                lock (projectInfo)
                {
                    if (!projectInfo.WasStarted)
                    {
                        InvokeHandler(SuiteStarting, new TestNodeEventArgs(projectInfo.ProjectNode));
                        projectInfo.WasStarted = true;
                    }
                }
            }
        }

        private void CheckForProjectFinish(ResultNode resultNode)
        {
            if (_projectLookup.ContainsKey(resultNode.Id))
            {
                var projectInfo = _projectLookup[resultNode.Id];

                lock (projectInfo)
                {
                    if (--projectInfo.AssembliesToRun == 0)
                    {
                        var projectResult = new ResultNode(projectInfo.ProjectNode.Xml);
                        InvokeHandler(SuiteFinished, new TestResultEventArgs(projectResult));
                    }
                }
            }
        }

        #endregion
    }
}
