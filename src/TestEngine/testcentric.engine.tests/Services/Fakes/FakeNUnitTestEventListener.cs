// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using NUnit.Extensibility;

namespace TestCentric.Tests.Fakes
{
    [Extension]
    public class FakeNUnitTestEventListener : NUnit.Engine.ITestEventListener
    {
        public List<string> Output { get; } = new List<string>();

        public void OnTestEvent(string report)
        {
            Output.Add(report);
        }
    }
}
