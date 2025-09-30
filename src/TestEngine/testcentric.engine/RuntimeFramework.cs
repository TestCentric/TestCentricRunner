// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Runtime.Versioning;

namespace TestCentric.Engine
{
    /// <summary>
    /// RuntimeFramework represents a particular version
    /// of a common language runtime implementation.
    /// </summary>
    [Serializable]
    public sealed class RuntimeFramework : NUnit.Engine.IRuntimeFramework
    {
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

        public static RuntimeFramework FromFrameworkName(string framework)
        {
            var frameworkName = new FrameworkName(framework);
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

        public bool CanLoad(NUnit.Engine.IRuntimeFramework requested)
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
    }
}
