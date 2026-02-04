// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model
{
    /// <summary>
    /// Delegates for all events related to the model
    /// </summary>
    public delegate void TestEventHandler(TestEventArgs args);
    public delegate void RunStartingEventHandler(RunStartingEventArgs args);
    public delegate void TestNodeEventHandler(TestNodeEventArgs args);
    public delegate void TestResultEventHandler(TestResultEventArgs args);
    public delegate void TestItemEventHandler(TestItemEventArgs args);
    public delegate void TestSelectionEventHandler(TestSelectionEventArgs args);
    public delegate void TestOutputEventHandler(TestOutputEventArgs args);
    public delegate void UnhandledExceptionEventHandler(UnhandledExceptionEventArgs args);
    public delegate void TestFilesLoadingEventHandler(TestFilesLoadingEventArgs args);
    public delegate void TestLoadFailureEventHandler(TestLoadFailureEventArgs args);
    public delegate void VisualStateEventHandler(VisualStateEventArgs args);

    /// <summary>
    /// ITestEvents provides events for all actions in the model, both those
    /// caused by an engine action like a test completion and those originated
    /// in the model itself like starting a test run.
    /// </summary>
    public interface ITestEvents
    {
        // Events related to loading and unloading TestCentric projects
        event TestEventHandler TestCentricProjectLoaded;
        event TestEventHandler TestCentricProjectUnloaded;
        event VisualStateEventHandler VisualStateRequest;

        // Events related to loading and unloading tests.
        event TestFilesLoadingEventHandler TestsLoading;
        event TestEventHandler TestsReloading;
        event TestEventHandler TestsUnloading;
        event TestEventHandler TestChanged;

        event TestNodeEventHandler TestLoaded;
        event TestNodeEventHandler TestReloaded;
        event TestEventHandler TestUnloaded;

        event TestLoadFailureEventHandler TestLoadFailure;

        // Events related to running tests
        event RunStartingEventHandler RunStarting;
        event TestNodeEventHandler SuiteStarting;
        event TestNodeEventHandler TestStarting;

        event TestResultEventHandler RunFinished;
        event TestResultEventHandler SuiteFinished;
        event TestResultEventHandler TestFinished;

        event TestOutputEventHandler TestOutput;
        event UnhandledExceptionEventHandler UnhandledException;

        // Event used to broadcast a change in the selected
        // item, so that all presenters may be notified.
        event TestItemEventHandler SelectedItemChanged;
        event TestSelectionEventHandler SelectedTestsChanged;

        event TestEventHandler CategorySelectionChanged;
        event TestEventHandler TestFilterChanged;
    }
}
