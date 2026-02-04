// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;

namespace TestCentric.Gui.Model
{
    /// <summary>
    /// Argument used for all test events
    /// </summary>
    public class TestEventArgs : EventArgs
    {
    }

    public class TestNodeEventArgs : TestEventArgs
    {
        public TestNodeEventArgs(TestNode test)
        {
            if (test == null)
                throw new ArgumentNullException("test");

            Test = test;
        }

        public TestNode Test { get; }
    }

    public class RunStartingEventArgs : TestEventArgs
    {
        public RunStartingEventArgs(int testCount)
        {
            TestCount = testCount;
        }

        public int TestCount { get; set; }
    }

    public class TestResultEventArgs : TestEventArgs
    {
        public TestResultEventArgs(ResultNode result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            Result = result;
        }

        public ResultNode Result { get; set; }
    }

    public class TestItemEventArgs : TestEventArgs
    {
        public TestItemEventArgs(ITestItem testItem)
        {
            TestItem = testItem;
        }

        public ITestItem TestItem { get; private set; }
    }

    public class TestSelectionEventArgs : TestEventArgs
    {
        public TestSelectionEventArgs(TestSelection testSelection)
        {
            TestSelection = testSelection;
        }

        public TestSelection TestSelection { get; private set; }
    }

    public class TestOutputEventArgs : TestEventArgs
    {
        public TestOutputEventArgs(string testName, string stream, string text)
        {
            TestName = testName;
            Stream = stream;
            Text = text;
        }

        public string TestName { get; }
        public string Stream { get; }
        public string Text { get; }
    }

    public class UnhandledExceptionEventArgs : TestEventArgs
    {
        public UnhandledExceptionEventArgs(string message, string stackTrace)
        {
            Message = message;
            StackTrace = stackTrace;
        }

        public string Message;
        public string StackTrace;
    }

    public class TestFilesLoadingEventArgs : TestEventArgs
    {
        public TestFilesLoadingEventArgs(IList<string> testFilesLoading)
        {
            TestFilesLoading = testFilesLoading;
        }

        public IList<string> TestFilesLoading { get; }
    }

    public class TestLoadFailureEventArgs : TestEventArgs
    {
        public TestLoadFailureEventArgs(Exception ex)
        {
            Exception = ex;
        }

        public Exception Exception;
    }

    public class VisualStateEventArgs : TestEventArgs
    {
        public VisualState VisualState;
    }
}
