// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace TestCentric.Gui
{
    /// <summary>
    /// GuiOptions encapsulates the option settings for TestCentric.
    /// It inherits from the Mono.Options OptionSet class and provides a 
    /// central location for defining and parsing options.
    /// </summary>
    public class GuiOptions
    {
        private bool _validated;
        private List<Option> _options = new List<Option>();
        private Dictionary<string, Option> _optionIndex = new Dictionary<string, Option>();
#if DEBUG
        private bool _isDebug = true;
#else
        private bool _isDebug = false;
#endif

        #region Constructor

        public GuiOptions(params string[] args)
        {
            DefineOptions();
            ParseArguments(args);
        }

        private void DefineOptions()
        {
            Add("config", "Project {CONFIG} to load (e.g.: Debug).", true,
                v => ActiveConfig = v);

            Add("noload", "Suppress loading of the most recent test file.", false,
                v => NoLoad = true);

            Add("run", "Automatically run the loaded tests.", false,
                v => RunAllTests = true);

            Add("unattended", "Unattended execution: perform requested actions, then exit.", false,
                v => Unattended = true);

            Add("full-gui", "Use the standard (full) GUI interface.", false,
                v => GuiLayout = "Full");

            Add("mini-gui", "Use the mini-GUI interface.", false,
                v => GuiLayout = "Mini");

            Add("x86", "Run tests in an X86 process on 64-bit systems.", false,
                v => RunAsX86 = true);

            Add("agents", "Specify maximum {NUMBER} of test assembly agents to run at one time. If not specified, there is no limit.", true,
                v =>
                {
                    if (int.TryParse(v, out int val))
                        MaxAgents = val;
                    else
                        ErrorMessages.Add($"An int value is required: {val}");
                });

            Add("work", "{PATH} of the directory to use for output files. If not specified, defaults to the current directory.", true,
                v =>
                {
                    if (CheckRequiredValue(v, "--work"))
                        WorkDirectory = v;
                });

            Add("trace", "Set internal trace {LEVEL}. Valid values are Off, Error, Warning, Info or Debug. Verbose is a synonym for Debug.", true,
                v =>
                {
                if (CheckRequiredValue(v, "--trace", "Off", "Error", "Warning", "Info", "Verbose", "Debug"))
                    InternalTraceLevel = v;
                });

            Add("param|p", "Followed by a {KEY-VALUE PAIR} separated by an equals sign. Test code can access the value by name. This option may be repeated.", true,
                v =>
                {
                    if (CheckRequiredValue(v, "--param"))
                    {
                        string name, val;
                        int eq = v.IndexOf('=');
                        if (eq > 0 && eq < v.Length - 1)
                        {
                            name = v.Substring(0, eq);
                            val = v.Substring(eq + 1);
                            TestParameters[name] = val;
                        }
                    }
                });

            Add("debug-agent", "Launch debugger in testcentric-agent when it starts.", false,
                v => DebugAgent = _isDebug, true);

            Add("simulate-unload-error", "Throw CannotUnloadAppDomainException when unloading AppDomain.", false,
                v => SimulateUnloadError = _isDebug, true);

            Add("simulate-unload-timeout", "Inject infinite loop when unloading AppDomain.", false,
                v => SimulateUnloadTimeout = _isDebug, true);

            Add("help|h", "Display the help message and exit.", false,
                v => ShowHelp = true);

            void Add(string pattern, string description, bool requiresValue, Action<string> action, bool debugOnly=false)
            {
                var option = new Option(pattern, description, requiresValue, action, debugOnly);
                _options.Add(option);
                foreach (string alias in option.Aliases)
                    _optionIndex.Add(alias, option);
            }
        }

        private void ParseArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];
                Option option;
                string val;
                if (TryParseOption(arg, out option, out val))
                {
                    if (option.RequiresValue)
                    {
                        if (val is null && i + 1 < args.Length)
                            val = args[++i];

                        if (val is null)
                            ValueRequiredError(arg);
                        else
                            option.Action(val);
                    }
                    else if (val is not null)
                        ValueNotAllowedError(arg);
                    else
                        option.Action(null);
                }
                else
                    InputFiles.Add(arg);
            }
        }

        private bool TryParseOption(string arg, out Option option, out string value)
        {
            option = null;
            value = null;
            if (!arg.StartsWith("-"))
                return false;

            string opt = arg.StartsWith("--")
                ? arg.Substring(2)
                : arg.Substring(1);
            string val = null;
            int delim = opt.IndexOfAny(['=', ':']);
            if (delim > 0)
            {
                val = opt.Substring(delim + 1);
                opt = opt.Substring(0, delim);
            }

            if (_optionIndex.TryGetValue(opt, out option))
            {
                value = val;
                return true;
            }
            else
            {
                InvalidArgumentError(arg);
                return false;
            }

        }

        private void ValueRequiredError(string arg) =>
            ErrorMessages.Add($"Option requires a value: {arg}");

        private void ValueNotAllowedError(string arg) =>
            ErrorMessages.Add($"Option does not take a value: {arg}");

        private void InvalidArgumentError(string arg) =>
            ErrorMessages.Add($"Invalid argument: {arg}");

        #endregion

        #region Properties

        // Action to Perform

        public bool ShowHelp { get; private set; }
        public bool NoLoad { get; private set; }
        public bool RunAllTests { get; private set; }
        public bool Unattended { get; private set; }

        // Select tests

        private List<string> inputFiles = new List<string>();
        public IList<string> InputFiles { get { return inputFiles; } }

        public string ActiveConfig { get; private set; }

        // How to Run Tests

        public string GuiLayout { get; private set; }
        public bool RunAsX86 { get; private set; }
        public int MaxAgents { get; private set; }
        public string InternalTraceLevel { get; private set; }
        public string WorkDirectory { get; private set; }
        public IDictionary<string, string> TestParameters { get; } = new Dictionary<string, string>();
        public bool DebugAgent { get; private set; }
        public bool SimulateUnloadError { get; private set; }
        public bool SimulateUnloadTimeout { get; private set; }

        // Error Processing

        private List<string> errorMessages = new List<string>();
        public IList<string> ErrorMessages { get { return errorMessages; } }

        #endregion

        #region Public Methods

        public bool Validate()
        {
            if (!_validated)
            {
                // Additional Checks here

                _validated = true;
            }

            return ErrorMessages.Count == 0;
        
        }

        public string GetHelpText()
        {
            var writer = new StringWriter();

            writer.WriteLine("TESTCENTRIC [inputfiles] [options]");
            writer.WriteLine();
            writer.WriteLine("Starts the TestCentric Runner, optionally loading and running a set of NUnit tests. Various options are provided, most of which are intended for use in running unattended.");
            writer.WriteLine();
            writer.WriteLine("Options only available in a Debug build are marked '(DEBUG)'.");
            writer.WriteLine();
            writer.WriteLine("InputFiles:");
            writer.WriteLine();
            writer.WriteLine("One or more assemblies or supported test projects. If no input files are given, the tests contained in the most recently used project or assembly are loaded, unless the --noload option is specified");
            writer.WriteLine();
            writer.WriteLine("Options:");
            writer.WriteLine();

            foreach (var option in _options)
                writer.WriteLine(option.HelpText);

            return writer.ToString();
        }

        #endregion

        #region Helper Methods

        private bool CheckRequiredValue(string val, string option, params string[] validValues)
        {
            if (string.IsNullOrEmpty(val))
                ErrorMessages.Add($"Missing required value for option '{option}'");

            if (validValues == null || validValues.Length == 0)
                return true;

            foreach (string valid in validValues)
                if (string.Compare(valid, val, StringComparison.OrdinalIgnoreCase) == 0)
                    return true;

            ErrorMessages.Add(string.Format("The value '{0}' is not valid for option '{1}'.", val, option));
            return false;
        }

        #endregion
    }

    internal class Option
    {
        public Option(string aliases, string description, bool requiresValue, Action<string> action, bool debugOnly = false)
        {
            Aliases = aliases.Split('|');
            Description = description;
            RequiresValue = requiresValue;
            Action = action;
            DebugOnly = debugOnly;
        }

        public string[] Aliases { get; }
        public bool RequiresValue { get; }
        public string Description { get; }
        public Action<string> Action { get; }
        public bool DebugOnly { get; }
        public string HelpText
        {
            get
            {
                string indent = new string(' ', 20);
                var sb = new StringBuilder();

                sb.Append($"{string.Join(", ", Aliases.Select(a => (a.Length == 1 ? "-" : "--") + a))}");
               
                if (RequiresValue)
                {
                    var match = Regex.Match(Description, "\\{(.*?)\\}");
                    var valueName = match.Success
                        ? match.Groups[1].Value : "VALUE";
                    sb.Append($"={valueName}");
                }
                if (DebugOnly)
                    sb.Append(" (DEBUG)");
                sb.AppendLine();

                sb.AppendLine($"{Description.Replace("{", "").Replace("}", "")}");

                return sb.ToString();
            }
        }
    }
}
