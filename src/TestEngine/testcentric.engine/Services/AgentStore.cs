// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit;
using NUnit.Engine;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// Defines the agent tracking operations that must be done atomically.
    /// </summary>
    internal sealed class AgentStore
    {
        private static readonly Logger log = InternalTrace.GetLogger(typeof(AgentStore));

        private readonly object LOCK = new object();

        private readonly Dictionary<Guid, AgentRecord> _agentIndex = new Dictionary<Guid, AgentRecord>();
        private readonly Dictionary<Process, AgentRecord> _processIndex = new Dictionary<Process, AgentRecord>();

        public void AddAgent(Guid agentId, Process process)
        {
            Guard.ArgumentNotNull(process, nameof(process));

            lock (LOCK)
            {
                if (_agentIndex.ContainsKey(agentId))
                {
                    throw new ArgumentException($"An agent has already been started with the ID '{agentId}'.", nameof(agentId));
                }

                _agentIndex[agentId] = _processIndex[process] = new AgentRecord(agentId, process);
            }
        }

        public void Register(ITestAgent agent)
        {
            lock (LOCK)
            {
                log.Debug($"Registering agent {agent.Id:B}");

                if (!_agentIndex.TryGetValue(agent.Id, out var record)
                    || record.Status != AgentStatus.Starting)
                {
                    string status = record?.Status.ToString() ?? "unknown";
                    throw new ArgumentException($"Agent {agent.Id} must have a status of {AgentStatus.Starting} in order to register, but the status was {status}.", nameof(agent));
                }

                record.Agent = agent;
                record.Status = AgentStatus.Available;
            }
        }

        public bool IsAvailable(Guid agentId, out ITestAgent agent)
        {
            lock (LOCK)
            {
                if (_agentIndex.TryGetValue(agentId, out var record)
                    && record.Status == AgentStatus.Available)
                {
                    agent = record.Agent;
                    return true;
                }

                agent = null;
                return false;
            }
        }

        public bool IsAgentActive(Guid agentId)
        {
            Process process;
            return IsAgentActive(agentId, out process);
        }

        public bool IsAgentActive(Guid agentId, out Process process)
        {
            lock (LOCK)
            {
                if (_agentIndex.TryGetValue(agentId, out var record)
                    && record.Status != AgentStatus.Terminated)
                {
                    process = record.Process;
                    return process != null;
                }

                process = null;
                return false;
            }
        }

        public void MarkProcessTerminated(Process process)
        {
            lock (LOCK)
            {
                if (!_processIndex.TryGetValue(process, out var record))
                    throw new NUnitEngineException("Process terminated without registering an agent.");

                if (record.Status == AgentStatus.Terminated)
                    throw new NUnitEngineException("Attempt to mark process as terminated, which is already terminated.");

                record.Status = AgentStatus.Terminated;
                try
                {
                    record.ExitCode = process.ExitCode;
                }
                catch(Exception ex)
                {
                    log.Error(ex.Message);
                    record.ExitCode = 0;
                }
            }
        }

        #region Nested AgentRecord Class

        private class AgentRecord
        {
            public AgentRecord(Guid agentId, Process process)
            {
                Guard.ArgumentNotNull(process, nameof(process));

                AgentId = agentId;
                Agent = null;
                Process = process;
                Status = AgentStatus.Starting;                
            }

            // AgentId is a property because it is needed before agent registers
            // and after it terminates, i.e. while Agent itself is null.
            public Guid AgentId { get; }
            public Process Process { get; }
            public AgentStatus Status { get; set; }
            public ITestAgent Agent { get; set; }
            // ExitCode is set when process terminates
            public int ExitCode { get; set; }
        }

        #endregion
    }
}
