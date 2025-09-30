// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using TestCentric.Engine;

namespace TestCentric.Gui.Model.Fakes
{
    public class RuntimeFramework : NUnit.Engine.IRuntimeFramework
    {
        public RuntimeFramework(string id, Version version)
        {
            Id = id;
            FrameworkVersion = version.Build >= 0
                ? new Version(version.Major, version.Minor)
                : version;
            DisplayName = id;
        }

        public string Id { get; }

        public Version FrameworkVersion { get; }

        public string DisplayName { get; set; }

        public string Profile { get; set; }
    }
}
