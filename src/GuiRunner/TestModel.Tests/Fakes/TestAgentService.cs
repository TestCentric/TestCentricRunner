// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

#if false
using System.Collections.Generic;
using NUnit.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Gui.Model.Fakes
{
    public class TestAgentService : Service, ITestAgentProvider
    {
        private List<NUnit.Engine.IRuntimeFramework> _availableRuntimes = new List<NUnit.Engine.IRuntimeFramework>();

        public void AddRuntimes(params NUnit.Engine.IRuntimeFramework[] runtimes)
        {
            _availableRuntimes.AddRange(runtimes);
        }

        public IList<NUnit.Engine.IRuntimeFramework> AvailableRuntimes => _availableRuntimes;

        public IList<NUnit.Engine.TestAgentInfo> GetAvailableAgents()
        {
            return new NUnit.Engine.TestAgentInfo[0];
        }

        public IList<NUnit.Engine.TestAgentInfo> GetAgentsForPackage(NUnit.Engine.TestPackage package)
        {
            return new NUnit.Engine.TestAgentInfo[0];
        }

        public bool IsAgentAvailable(NUnit.Engine.TestPackage package)
        {
            throw new System.NotImplementedException();
        }

        public ITestAgent GetAgent(NUnit.Engine.TestPackage package)
        {
            throw new System.NotImplementedException();
        }

        public void ReleaseAgent(ITestAgent agent)
        {
            throw new System.NotImplementedException();
        }
    }
}
#endif
