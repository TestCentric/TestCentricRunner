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
        List<string> SupportedProjectExtensions { get; }
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

        // Is ResultSummary available?
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

        /// <summary>
        /// Create a new project containing the provided test files
        /// </summary>
        /// <param name="path">Path where the project will be stored</param>
        /// <param name="testFiles">Array of top level test files to be included</param>
        TestCentricProject CreateNewProject(string path, params string[] testFiles);

        /// <summary>
        /// Create a new project based on command-line parameters.
        /// Test files may be included in the options
        /// </summary>
        /// <param name="path">Path where the project will be stored</param>
        /// <param name="options">The command-line options</param>
        void CreateNewProject(string path, GuiOptions options);

        bool IsWrapperProjectPath(string path);

        /// <summary>
        /// Add the test files to the current test project
        /// </summary>
        /// <param name="testFiles">Additional top-level test files to be added</param>
        void AddTests(IEnumerable<string> testFiles);

        /// <summary>
        /// Remove a top-level TestPackage from the current test project
        /// </summary>
        /// <param name="subPackage">The top-level package to remove</param>
        void RemoveTestPackage(NUnit.Engine.TestPackage subPackage);

        /// <summary>
        /// Open an existing TestCentricProject file.
        /// </summary>
        /// <param name="filePath">Path to the project file</param>
        void OpenExistingProject(string filePath);

        /// <summary>
        /// Open the most recently opened file that still exists, which may be
        /// a TestCentricProject, an assembly or a supported project type.
        /// </summary>
        void OpenMostRecentFile();

        /// <summary>
        /// Open an existing file, which may be a TestCentricProject, an assembly
        /// or a supported project type such as an NUnit or Visual Studio project.
        /// </summary>
        /// <remarks>
        /// If the file is not a TestCentricProject, it will be wrapped in one
        /// with the '.tcproj' extension appended to the filePath. For example,
        /// 'mock-assembly.dll.tcproj'. This project is loaded if found or created
        /// automatically if not found.
        /// </remarks>
        /// <param name="filePath">Path to the file to be opened</param>
        void OpenExistingFile(string filePath);

        /// <summary>
        /// Open or create a wrapper project for a single test file. If an
        /// existing wrapper project is found, it is opened. Otherwise a
        /// new one is created.
        /// </summary>
        /// <param name="filePath">Path to the test file</param>
        void OpenOrCreateWrapperProject(string filePath);

        /// <summary>
        /// Save the currently open TestCentricProject to the specified path,
        /// if provided. If not provided, save to the path already stored in 
        /// the project, which must not be null.
        /// </summary>
        /// <remarks>
        /// This method is called by both the 'Save' and 'SaveAs' functions of the GUI.
        /// </remarks>
        /// <param name="filePath">The path where the project will be saved</param>
        void SaveProject(string filePath = null);

        /// <summary>
        /// Close the currently open TestCentricProject.
        /// </summary>
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

        // Use '.xslt' file to transform the results of the last run into target file
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
