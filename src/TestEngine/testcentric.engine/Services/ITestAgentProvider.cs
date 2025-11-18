// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Engine;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// An object implementing ITestAgentProvider is able to provide
    /// test agents, which satisfy the criteria specified in a TestPackage.
    /// </summary>
    public interface ITestAgentProvider
    {
        /// <summary>
        /// Gets a list containing <see cref="TestAgentInfo"/> for all available agents.
        /// </summary>
        IList<TestAgentInfo> GetAvailableAgents();

        /// <summary>
        /// Gets a list containing <see cref="TestAgentInfo"/> for any available agents,
        /// which are able to handle the specified package.
        /// </summary>
        /// <param name="package">A Testpackage</param>
        /// <returns>
        /// A list of suitable agents for running the package or an empty
        /// list if no agent is available for the package.
        /// </returns>
        IList<TestAgentInfo> GetAgentsForPackage(NUnit.Engine.TestPackage package);

        /// <summary>
        /// Returns true if an agent can be found, which is suitable
        /// for running the provided test package.
        /// </summary>
        /// <param name="package">A TestPackage</param>
        bool IsAgentAvailable(TestPackage package);

        /// <summary>
        /// Return an agent, which best matches the criteria defined
        /// in a TestPackage.
        /// </summary>
        /// <param name="package">The test package to be run</param>
        /// <returns>An ITestAgent</returns>
        /// <exception cref="ArgumentException">If no agent is available.</exception>
        ITestAgent GetAgent(TestPackage package);

        /// <summary>
        /// Releases the test agent back to the supplier, which provided it.
        /// </summary>
        /// <param name="agent">An agent previously provided by a call to GetAgent.</param>
        /// <exception cref="InvalidOperationException">
        /// If agent was never provided by the factory or was previously released.
        /// </exception>
        /// <remarks>
        /// Disposing an agent also releases it. However, this should not
        /// normally be done by the client, but by the source that created
        /// the agent in the first place.
        /// </remarks>
        void ReleaseAgent(ITestAgent agent);
    }
}
