// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Engine;
using TestCentric.Gui.Model.Filter;
using TestCentric.Gui.Model.Settings;

namespace TestCentric.Gui.Model
{
    public interface ITestModel : IDisposable
    {
        #region General Properties

        GuiOptions Options { get; }

        // Work Directory
        string WorkDirectory { get; }

        // Event Dispatcher
        ITestEvents Events { get; }

        IServiceLocator Services { get; }

        IUserSettings Settings { get; }

        ITreeConfiguration TreeConfiguration { get; }

        // Project Support
        bool NUnitProjectSupport { get; }
        bool VisualStudioSupport { get; }

        // List of available runtimes, based on the engine's list
        // but filtered to meet the GUI's requirements
        IList<NUnit.Engine.IRuntimeFramework> AvailableRuntimes { get; }

        IList<string> AvailableAgents { get; }

        // Result Format Support
        IEnumerable<string> ResultFormats { get; }

        #endregion

        #region Current State of the Model

        TestCentricProject TestCentricProject { get; }

        TestPackage TopLevelPackage { get; }

        bool IsProjectLoaded { get; }

        // TestNode hierarchy representing the discovered tests
        TestNode LoadedTests { get; }

        // See if tests are available
        bool HasTests { get; }

        IList<string> AvailableCategories { get; }

        // See if a test is running
        bool IsTestRunning { get; }

        // Manager maintaining all test results
        ITestResultManager TestResultManager { get; }

        // Summary of last test run
        ResultSummary ResultSummary { get; }

        // Is Resultsummary available?
        bool HasResults { get; }

        /// <summary>
        /// Gets or sets the active test item. This is the item
        /// for which details are displayed in the various views.
        /// </summary>
        ITestItem ActiveTestItem { get; set; }

        /// <summary>
        ///  Gets or sets the list of selected tests.
        /// </summary>
        TestSelection SelectedTests { get; set; }

        List<string> SelectedCategories { get; }

        bool ExcludeSelectedCategories { get; }

        TestFilter CategoryFilter { get; }

        /// <summary>
        /// Provides filter functionality: by outcome, by duration, by category...
        /// </summary>
        ITestCentricTestFilter TestCentricTestFilter { get; }

        /// <summary>
        /// Checks if the testNode is executed in the current test run
        /// </summary>
        bool IsInTestRun(TestNode testNode);

        TestSelection TestsInRun { get; }

        #endregion

        #region Methods

        // Create a new project containing the provided test files
        TestCentricProject CreateNewProject(string[] filenames);
        void CreateNewProject(GuiOptions options);

        /// <summary>
        /// Create a new empty project
        /// </summary>
        void CreateNewProject();

        bool TryLoadVisualState(out VisualState visualState);

        /// <summary>
        /// Add the test files to the current test project
        /// </summary>
        void AddTests(IEnumerable<string> fileNames);

        /// <summary>
        /// Remove a test package from the current test project
        /// </summary>
        void RemoveTestPackage(NUnit.Engine.TestPackage subPackage);


        void OpenExistingProject(string filename);

        void OpenMostRecentFile();

        void OpenExistingFile(string filename);

        void SaveProject(string filename = null);

        void CloseProject();

        #region Loading and Unloading Tests

        void LoadTests(IList<string> files);
        void UnloadTests();
        void ReloadTests();
        void ReloadPackage(NUnit.Engine.TestPackage package, string config);

        #endregion

        #region Running Tests

        void RunTests(TestNode testNode);
        void RunTests(TestSelection testSelection);
        void RepeatLastRun();
        void DebugTests(TestNode testNode);
        void DebugTests(TestSelection testSelection);

        void StopTestRun(bool force);

        #endregion

        // Save the results of the last run in the specified format
        void SaveResults(string fileName, string format="nunit3");

        // Use xslt file to transform the results of the last run into target file
        void TransformResults(string targetFile, string xsltFile);

        // Get a specific test given its id
        TestNode GetTestById(string id);

        // Get the TestPackage represented by a test,if available
        NUnit.Engine.TestPackage GetPackageForTest(string id);
        NUnit.Engine.PackageSettings GetPackageSettingsForTest(string id);

        // Get Agents available for a package
        IList<string> GetAgentsForPackage(NUnit.Engine.TestPackage package);

        // Clear the results for all tests
        void ClearResults();

        // Set the category filters for running and tree display
        void SelectCategories(IList<string> categories, bool exclude);

        #endregion
    }
}
