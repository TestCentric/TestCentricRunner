// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// Enumeration used to report AgentStatus
    /// </summary>
    public enum AgentStatus
    {
        /// <summary>Waiting for the agent to register</summary>
        Starting,

        /// <summary>The agent has registered and is available to perform work</summary>
        Available,

        /// <summary>The agent has terminated</summary>
        Terminated
    }
}
