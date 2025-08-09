// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TestCentric.Gui
{
    using System.Diagnostics;
    using System.Linq;
    using Model;
    using Presenters;
    using Views;

    /// <summary>
    /// Class to manage application startup.
    /// </summary>
    public class AppEntry
    {
        static Logger log = InternalTrace.GetLogger(typeof(AppEntry));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static int Main(string[] args)
        {
            Application.EnableVisualStyles();
            var options = new CommandLineOptions(args);

            if (options.ShowHelp)
            {
                // TODO: We would need to have a custom message box
                // in order to use a fixed font and display the options
                // so that the values all line up.
                MessageDisplay.Info(GetHelpText(options));
                return 0;
            }

            if (!options.Validate())
            {
                var NL = Environment.NewLine;
                var sb = new StringBuilder($"Error(s) in command line:{NL}");
                foreach (string msg in options.ErrorMessages)
                    sb.Append($"  {msg}{NL}");
                sb.Append($"{NL}{GetHelpText(options)}");
                MessageDisplay.Error(sb.ToString());
                return 2;
            }

            log.Info("Instantiating TestModel");
            ITestModel model = null;
            try
            {
                model = TestModel.CreateTestModel(options);
            }
            catch(Exception ex)
            {
                MessageDisplay.Error(ex.Message);
                return 3;
            }

            if (model.Services.ExtensionService.GetExtensionNodes("/TestCentric/Engine/TypeExtensions/IAgentLauncher").Count() == 0)
            {
                if (!MessageDisplay.OkCancel(
                    "Either the GUI was installed without any agents or all the installed agents have been deleted.\r\n\r\n" +
                    "You must install at least one agent in order to be able to load or run tests.\r\n\r\n" +
                    "Install agents using the same source (i.e. nuget or choolatey) from which you installed the GUI itself.\r\n\r\n" +
                    "You should select agents which match the target platforms you are using for development.\r\n\r\n" +
                    "Click 'OK' to continue with extremely limited functionality, 'Cancel' to exit."))
                return 4;
            }

            log.Info("Constructing Form");
            TestCentricMainView view = new TestCentricMainView();

            log.Info("Constructing presenters");
            new ProgressBarPresenter(view.ProgressBarView, model);
            new StatusBarPresenter(view.StatusBarView, model);
            new TestPropertiesPresenter(view.TestPropertiesView, model);
            new ErrorsAndFailuresPresenter(view.ErrorsAndFailuresView, model);
            new TextOutputPresenter(view.TextOutputView, model);
            new TreeViewPresenter(view.TreeView, model, new TreeDisplayStrategyFactory());
            new TestCentricPresenter(view, model, options);

            try
            {
                log.Info("Starting Gui Application");
                Application.Run(view);
                log.Info("Application Exit");
            }
            catch (Exception ex)
            {
                log.Error("Gui Application threw an exception", ex);
                throw;
            }
            finally
            {
                log.Info("Exiting TestCentric Runner");
                InternalTrace.Close();
                model.Dispose();
            }

            return 0;
        }

        private static string GetHelpText(CommandLineOptions options)
        {
            StringWriter writer = new StringWriter();

            writer.WriteLine("TESTCENTRIC [inputfiles] [options]");
            writer.WriteLine();
            writer.WriteLine("Starts the TestCentric Runner, optionally loading and running a set of NUnit tests. You may specify any combination of assemblies and supported project files as arguments.");
            writer.WriteLine();
            writer.WriteLine("InputFiles:");
            writer.WriteLine("   One or more assemblies or test projects of a recognized type.");
            writer.WriteLine("   If no input files are given, the tests contained in the most");
            writer.WriteLine("   recently used project or assembly are loaded, unless the");
            writer.WriteLine("   --noload option is specified");
            writer.WriteLine();
            writer.WriteLine("Options:");
            options.WriteOptionDescriptions(writer);

            return writer.GetStringBuilder().ToString();
        }

        private static IMessageDisplay MessageDisplay
        {
            get { return new MessageBoxDisplay("TestCentric Runner for NUnit"); }
        }
    }
}
