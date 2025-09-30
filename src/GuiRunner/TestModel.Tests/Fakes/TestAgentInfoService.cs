// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using TestCentric.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Gui.Model.Fakes
{
    public class TestAgentInfoService : Service
    {
        public IList<NUnit.Engine.TestAgentInfo> GetAvailableAgents()
        {
            return new NUnit.Engine.TestAgentInfo[0];
        }

        public IList<NUnit.Engine.TestAgentInfo> GetAgentsForPackage(TestPackage package)
        {
            return new NUnit.Engine.TestAgentInfo[0];
        }
    }
}
