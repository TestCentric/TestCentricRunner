// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Engine;

namespace TestCentric.Engine.Services.Fakes
{
    public class FakeRuntimeService : FakeService, IRuntimeFrameworkService
    {
        bool IRuntimeFrameworkService.IsAvailable(string framework)
        {
            return AvailableRuntimes.Contains(framework);
        }

        string IRuntimeFrameworkService.SelectRuntimeFramework(TestPackage package)
        {
            return SelectedRuntime;
        }

        public List<string> AvailableRuntimes { get; set; } = new List<string>(new [] { "NONE" });

        public string SelectedRuntime { get; set; } = "NONE";

        public IRuntimeFramework CurrentFramework => throw new NotImplementedException();
    }
}
