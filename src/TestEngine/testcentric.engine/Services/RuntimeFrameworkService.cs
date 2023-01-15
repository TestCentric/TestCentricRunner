// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using NUnit.Engine;
using TestCentric.Engine.Internal;
using TestCentric.Engine.Services.RuntimeLocators;

namespace TestCentric.Engine.Services
{
    public class RuntimeFrameworkService : Service, IRuntimeFrameworkService, IAvailableRuntimes
    {
        private static readonly string DEFAULT_WINDOWS_MONO_DIR =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Mono");

        private static readonly Version AnyVersion = new Version(0, 0);

        static Logger log = InternalTrace.GetLogger(typeof(RuntimeFrameworkService));

        private List<RuntimeFramework> _availableRuntimes = new List<RuntimeFramework>();

        /// <summary>
        /// Gets a RuntimeFramework instance representing the runtime under
        /// which the code is currently running.
        /// </summary>
        private RuntimeFramework _currentFramework;
        public IRuntimeFramework CurrentFramework => _currentFramework;

        #region Service Overrides

        /// <summary>
        /// Start the service, initializing available runtimes.
        /// </summary>
        public override void StartService()
        {
            SetCurrentFramework();
            FindAvailableRuntimes();
            base.StartService();
        }

        private void SetCurrentFramework()
        {
            Type monoRuntimeType = Type.GetType("Mono.Runtime", false);
            bool isMono = monoRuntimeType != null;

            Runtime runtime = isMono
                ? Runtime.Mono
                : Runtime.Net;

            int major = Environment.Version.Major;
            int minor = Environment.Version.Minor;

            if (isMono)
            {
                switch (major)
                {
                    case 1:
                        minor = 0;
                        break;
                    case 2:
                        major = 3;
                        minor = 5;
                        break;
                }
            }
            else /* It's windows */
            if (major == 2)
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework");
                if (key != null)
                {
                    string installRoot = key.GetValue("InstallRoot") as string;
                    if (installRoot != null)
                    {
                        if (Directory.Exists(Path.Combine(installRoot, "v3.5")))
                        {
                            major = 3;
                            minor = 5;
                        }
                        else if (Directory.Exists(Path.Combine(installRoot, "v3.0")))
                        {
                            major = 3;
                            minor = 0;
                        }
                    }
                }
            }
            else if (major == 4 && Type.GetType("System.Reflection.AssemblyMetadataAttribute") != null)
            {
                minor = 5;
            }

            _currentFramework = new RuntimeFramework(runtime, new Version(major, minor));

            if (isMono)
            {
                _currentFramework.MonoPrefix = GetMonoPrefixFromAssembly(monoRuntimeType.Assembly);

                MethodInfo getDisplayNameMethod = monoRuntimeType.GetMethod(
                    "GetDisplayName", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.ExactBinding);
                if (getDisplayNameMethod != null)
                {
                    string displayName = (string)getDisplayNameMethod.Invoke(null, new object[0]);
                    Version monoVersion = new Version(0, 0);

                    int space = displayName.IndexOf(' ');
                    if (space >= 3) // Minimum length of a version
                    {
                        displayName = displayName.Substring(0, space);
                        monoVersion = new Version(displayName);
                    }

                    _currentFramework.DisplayName = "Mono " + displayName;
                    _currentFramework.MonoVersion = monoVersion;
                }
            }
        }

        private static string GetMonoPrefixFromAssembly(Assembly assembly)
        {
            string prefix = assembly.Location;

            // In all normal mono installations, there will be sufficient
            // levels to complete the four iterations. But just in case
            // files have been copied to some non-standard place, we check.
            for (int i = 0; i < 4; i++)
            {
                string dir = Path.GetDirectoryName(prefix);
                if (string.IsNullOrEmpty(dir)) break;

                prefix = dir;
            }

            return prefix;
        }

        #endregion

        #region IAvailableRuntimes Implementation

        /// <summary>
        /// Gets a list of available runtimes.
        /// </summary>
        public IList<IRuntimeFramework> AvailableRuntimes
        {
            get { return _availableRuntimes.ToArray(); }
        }

        #endregion

        #region IRuntimeFrameworkService Implementation

        /// <summary>
        /// Returns true if the runtime framework represented by
        /// the string passed as an argument is available.
        /// </summary>
        /// <param name="name">A string representing a framework, like 'net-4.0'</param>
        /// <returns>True if the framework is available, false if unavailable or nonexistent</returns>
        public bool IsAvailable(string name)
        {
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            RuntimeFramework requestedFramework;
            if (!RuntimeFramework.TryParse(name, out requestedFramework))
                throw new NUnitEngineException("Invalid or unknown framework requested: " + name);

            return IsAvailable(requestedFramework);
        }

        /// <summary>
        /// Selects a target runtime framework for a TestPackage based on
        /// the settings in the package and the assemblies themselves.
        /// The package RuntimeFramework setting may be updated as a result
        /// and a string representing the selected runtime is returned.
        /// </summary>
        /// <param name="package">A TestPackage</param>
        /// <returns>A string representing the selected RuntimeFramework</returns>
        public string SelectRuntimeFramework(TestPackage package)
        {
            var targetFramework = SelectRuntimeFrameworkInner(package);

            return targetFramework.ToString();
        }

        #endregion

        #region Other Public Methods

        public bool IsAvailable(RuntimeFramework requestedFramework)
        {
            foreach (var framework in _availableRuntimes)
                if (FrameworksMatch(requestedFramework, framework))
                    return true;

            return false;
        }

        #endregion

        #region Helper Methods

        private static bool FrameworksMatch(RuntimeFramework f1, RuntimeFramework f2)
        {
            var rt1 = f1.Runtime;
            var rt2 = f2.Runtime;

            if (!rt1.Matches(rt2))
                return false;

            var v1 = f1.FrameworkVersion;
            var v2 = f2.FrameworkVersion;

            if (v1 == AnyVersion || v2 == AnyVersion)
                return true;

            return v1.Major == v2.Major &&
                   v1.Minor == v2.Minor &&
                   (v1.Build < 0 || v2.Build < 0 || v1.Build == v2.Build) &&
                   (v1.Revision < 0 || v2.Revision < 0 || v1.Revision == v2.Revision) &&
                   f1.FrameworkVersion.Major == f2.FrameworkVersion.Major &&
                   f1.FrameworkVersion.Minor == f2.FrameworkVersion.Minor;
        }

        private RuntimeFramework SelectRuntimeFrameworkInner(TestPackage package)
        {
            foreach (var subPackage in package.SubPackages)
            {
                SelectRuntimeFrameworkInner(subPackage);
            }

            // Examine the provided settings
            log.Debug("Current framework is " + CurrentFramework);

            string requestedFrameworkSetting = package.GetSetting(EnginePackageSettings.RequestedRuntimeFramework, "");

            if (requestedFrameworkSetting.Length > 0)
            {
                RuntimeFramework requestedFramework;
                if (!RuntimeFramework.TryParse(requestedFrameworkSetting, out requestedFramework))
                    throw new NUnitEngineException("Invalid or unknown framework requested: " + requestedFrameworkSetting);

                log.Debug($"Requested framework for {package.Name} is {requestedFramework}");

                if (!IsAvailable(requestedFramework))
                    throw new NUnitEngineException("Requested framework is not available: " + requestedFrameworkSetting);

                package.Settings[EnginePackageSettings.TargetRuntimeFramework] = requestedFrameworkSetting;
                return requestedFramework;
            }

            log.Debug($"No specific framework requested for {package.Name}");

            string imageTargetFrameworkNameSetting = package.GetSetting(EnginePackageSettings.ImageTargetFrameworkName, "");

            RuntimeFramework targetFramework;

            // HACK: handling the TargetFrameworkName does not currently work outside of windows
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && imageTargetFrameworkNameSetting.Length > 0)
            {
                targetFramework = RuntimeFramework.FromFrameworkName(imageTargetFrameworkNameSetting);
            }
            else
            {
                var runtimeVersion = package.GetSetting(EnginePackageSettings.ImageRuntimeVersion, CurrentFramework.FrameworkVersion);
                var targetVersion = new Version(runtimeVersion.Major, runtimeVersion.Minor);
                targetFramework = new RuntimeFramework(_currentFramework.Runtime, targetVersion);
            }

            if (!IsAvailable(targetFramework))
            {
                log.Debug("Preferred target framework {0} is not available.", targetFramework);
                if (_currentFramework.Supports(targetFramework))
                {
                    targetFramework = _currentFramework;
                    log.Debug($"Using {_currentFramework}");
                }
                else
                {
                    throw new NotImplementedException($"The GUI does not yet support {targetFramework.FrameworkName.FullName} tests.");
                }
            }

            package.Settings[EnginePackageSettings.TargetRuntimeFramework] = targetFramework.ToString();

            log.Debug($"Test will use {targetFramework} for {package.Name}");
            return targetFramework;
        }

        private void FindAvailableRuntimes()
        {
            _availableRuntimes = new List<RuntimeFramework>();

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                _availableRuntimes.AddRange(NetFxRuntimeLocator.FindRuntimes());

            //FindDefaultMonoFramework();
            _availableRuntimes.AddRange(NetCoreRuntimeLocator.FindRuntimes());
        }

#endregion
    }
}
