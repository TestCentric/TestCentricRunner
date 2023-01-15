// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric GUI contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Versioning;
using Microsoft.Win32;
using NUnit.Engine;

namespace TestCentric.Engine
{
    /// <summary>
    /// RuntimeFramework represents a particular version
    /// of a common language runtime implementation.
    /// </summary>
    [Serializable]
    public sealed class RuntimeFramework : IRuntimeFramework
    {
        private static RuntimeFramework _currentFramework;

        /// <summary>
        /// Construct from a Runtime and Version.
        /// </summary>
        /// <param name="runtime">A Runtime instance</param>
        /// <param name="version">The Version of the framework</param>
        public RuntimeFramework(Runtime runtime, Version version)
            : this(runtime, version, null)
        {
        }

        /// <summary>
        /// Construct from a Runtime, Version and profile.
        /// </summary>
        /// <param name="runtime">A Runtime instance.</param>
        /// <param name="version">The Version of the framework.</param>
        /// <param name="profile">A string representing the profile of the framework. Null if unspecified.</param>
        public RuntimeFramework(Runtime runtime, Version version, string profile)
        {
            Runtime = runtime;
            FrameworkVersion = version;

            Profile = profile;

            DisplayName = GetDefaultDisplayName(runtime, FrameworkVersion, profile);

            FrameworkName = new FrameworkName(runtime.FrameworkIdentifier, FrameworkVersion);
        }

        /// <summary>
        /// Static method to return a RuntimeFramework object
        /// for the framework that is currently in use.
        /// </summary>
        public static RuntimeFramework CurrentFramework
        {
            get
            {
                if (_currentFramework == null)
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

                return _currentFramework;
            }
        }

        /// <summary>
        /// The version of Mono in use or null if runtime type is not Mono.
        /// </summary>
        /// <value>The mono version.</value>
        public Version MonoVersion { get; set; }

        /// <summary>
        /// The install directory where the version of mono in
        /// use is located. Null if this is not a Mono runtime.
        /// </summary>
        public string MonoPrefix { get; set; }

        /// <summary>
        /// The path to the mono executable, based on the
        /// Mono prefix if available. Otherwise, uses "mono",
        /// to invoke a script of that name.
        /// </summary>
        public string MonoExePath
        {
            get
            {
                return MonoPrefix != null && Environment.OSVersion.Platform == PlatformID.Win32NT
                    ? Path.Combine(MonoPrefix, "bin/mono.exe")
                    : "mono";
            }
        }

        /// <summary>
        /// Gets the unique Id for this runtime, such as "net-4.5"
        /// </summary>
        public string Id
        {
            get
            {
                return Runtime.ToString().ToLower() + "-" + FrameworkVersion.ToString();
            }
        }

        public FrameworkName FrameworkName { get; private set; }

        /// <summary>
        /// The runtime framework
        /// </summary>
        public Runtime Runtime { get; private set; }

        /// <summary>
        /// The framework version for this runtime framework
        /// </summary>
        public Version FrameworkVersion { get; private set; }

        /// <summary>
        /// The Profile for this framework, where relevant.
        /// May be null and will have different sets of
        /// values for each Runtime.
        /// </summary>
        public string Profile { get; private set; }

        /// <summary>
        /// Returns the Display name for this framework
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Parses a string representing a RuntimeFramework.
        /// The string may be just a RuntimeType name or just
        /// a Version or a hyphenated RuntimeType-Version or
        /// a Version prefixed by 'v'.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static RuntimeFramework Parse(string s)
        {
            Guard.ArgumentNotNullOrEmpty(s, nameof(s));

            string[] parts = s.Split(new char[] { '-' });
            Guard.ArgumentValid(parts.Length == 2 && parts[0].Length > 0 && parts[1].Length > 0, "RuntimeFramework id not in correct format", nameof(s));

            var runtime = Runtime.Parse(parts[0]);
            var version = new Version(parts[1]);
            return new RuntimeFramework(runtime, version);
        }

        public static bool TryParse(string s, out RuntimeFramework runtimeFramework)
        {
            try
            {
                runtimeFramework = Parse(s);
                return true;
            }
            catch
            {
                runtimeFramework = null;
                return false;
            }
        }

        public static RuntimeFramework FromFrameworkName(string frameworkName)
        {
            return FromFrameworkName(new FrameworkName(frameworkName));
        }

        public static RuntimeFramework FromFrameworkName(FrameworkName frameworkName)
        {
            return new RuntimeFramework(Runtime.FromFrameworkIdentifier(frameworkName.Identifier), frameworkName.Version, frameworkName.Profile);
        }

        /// <summary>
        /// Overridden to return the short name of the framework
        /// </summary>
        /// <returns></returns>
        public override string ToString() => Id;
        
        /// <summary>
        /// Returns true if the current framework matches the
        /// one supplied as an argument. Both the Runtime
        /// and the Version must match.
        /// </summary>
        /// <param name="target">The RuntimeFramework to be matched.</param>
        /// <returns><c>true</c> on match, otherwise <c>false</c></returns>
        public bool Supports(RuntimeFramework target)
        {
            if (!Runtime.Matches(target.Runtime))
                return false;

            return Runtime.Supports(this.FrameworkVersion, target.FrameworkVersion);
        }

        public bool CanLoad(IRuntimeFramework requested)
        {
            return FrameworkVersion >= requested.FrameworkVersion;
        }

        private static string GetDefaultDisplayName(Runtime runtime, Version version, string profile)
        {
            string displayName = $"{runtime.DisplayName} {version}";

            if (!string.IsNullOrEmpty(profile) && profile != "Full")
                displayName += " - " + profile;

            return displayName;
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
    }
}
