// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using NUnit.Engine;
using NUnit.Extensibility;
using TestCentric.Engine.Agents;
using TestCentric.Engine.Communication.Transports.Remoting;
using TestCentric.Engine.Communication.Transports.Tcp;
using TestCentric.Engine.Extensibility;
using TestCentric.Engine.Internal;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// The TestAgency class provides ITestAgents
    /// on request and tracks their status.
    /// </summary>
    class TestAgency : ITestAgentProvider, ITestAgency, IService
    {
        private static readonly Logger log = InternalTrace.GetLogger(typeof(TestAgency));

        private const int NORMAL_TIMEOUT = 30000;               // 30 seconds
        private const int DEBUG_TIMEOUT = NORMAL_TIMEOUT * 10;  // 5 minutes
        private const string AGENT_LAUNCHERS_PATH = "/TestCentric/Engine/AgentLaunchers";

        private readonly AgentStore _agentStore = new AgentStore();

        private ExtensionService _extensionService;

        // Transports used for various target runtimes
        private TestAgencyTcpTransport _tcpTransport; // .NET Standard 2.0

        internal virtual string TcpEndPoint => _tcpTransport.ServerUrl;

        public TestAgency() : this("TestAgency", 0) { }

        public TestAgency(string uri, int port )
        {
            _tcpTransport = new TestAgencyTcpTransport(this, port);
        }

        #region ITestAgentProvider Implementation

        /// <summary>
        /// Gets a list containing <see cref="TestAgentInfo"/> for all available agents.
        /// </summary>
        public IList<NUnit.Engine.TestAgentInfo> GetAvailableAgents()
        {
            var agents = new List<NUnit.Engine.TestAgentInfo>();

            foreach (var node in LauncherNodes)
                agents.Add(GetAgentInfo(node));

            return agents;
        }

        /// <summary>
        /// Gets a list containing <see cref="TestAgentInfo"/> for any available agents,
        /// which are able to handle the specified package.
        /// </summary>
        /// <param name="package">A TestPackage</param>
        /// <returns>
        /// A list of suitable agents for running the package or an empty
        /// list if no agent is available for the package.
        /// </returns>
        public IList<NUnit.Engine.TestAgentInfo> GetAgentsForPackage(TestPackage targetPackage)
        {
            Guard.ArgumentNotNull(targetPackage, nameof(targetPackage));

            // Initialize lists with ALL available agents
            var availableAgents = new List<ExtensionNode>(LauncherNodes);
            //var validAgentNames = new List<string>(availableAgents.Select(info => info.AgentName));

            // Look at each included assembly package
            foreach (var assemblyPackage in targetPackage.Select(p => p.IsAssemblyPackage))
            {
                // Remove agents that won't work for this assembly
                for (int index = availableAgents.Count - 1; index >= 0; index--)
                {
                    if (!CanCreateAgent(availableAgents[index], assemblyPackage))
                        availableAgents.RemoveAt(index);
                }

                //// Remove agents from final result if they don't work for this assembly
                //for (int index = validAgentNames.Count - 1; index >= 0; index--)
                //{
                //    var agentName = validAgentNames[index];
                //    if (!agentsForAssembly.Contains(agentName))
                //        validAgentNames.RemoveAt(index);
                //}
            }

            //// Finish up by deleting all unsuitable entries form the List of TestAgentInfo
            //for (int index = availableAgents.Count - 1; index >= 0; index--)
            //{
            //    var agentName = availableAgents[index].AgentName;
            //    if (!validAgentNames.Contains(agentName))
            //        availableAgents.RemoveAt(index);
            //}

            return new List<TestAgentInfo>(availableAgents.Select(x => GetAgentInfo(x)));
        }

        #endregion

        #region ITestAgentProvider Implementation

        /// <summary>
        /// Returns true if an agent can be found, which is suitable
        /// for running the provided test package.
        /// </summary>
        /// <param name="package">A TestPackage</param>
        public bool IsAgentAvailable(TestPackage package)
        {
            foreach (var node in LauncherNodes)
            {
                if (CanCreateAgent(node, package))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Return an agent, which best matches the criteria defined
        /// in a TestPackage.
        /// </summary>
        /// <param name="package">The test package to be run</param>
        /// <returns>An ITestAgent</returns>
        /// <exception cref="ArgumentException">If no agent is available.</exception>
        public ITestAgent GetAgent(TestPackage package)
        {
            // Target Runtime must be specified by this point
            string runtimeSetting = package.Settings.GetValueOrDefault(SettingDefinitions.TargetRuntimeFramework);
            Guard.OperationValid(runtimeSetting.Length > 0, "LaunchAgentProcess called with no runtime specified");

            var targetRuntime = RuntimeFramework.Parse(runtimeSetting);
            var agentId = Guid.NewGuid();
            string agencyUrl = TcpEndPoint;
            var agentProcess = CreateAgentProcess(agentId, agencyUrl, package);

            agentProcess.Exited += (sender, e) => OnAgentExit((Process)sender);

            agentProcess.Start();
            log.Debug("Launched Agent process {0} - see testcentric-agent_{0}.log", agentProcess.Id);
            log.Debug("Command line: \"{0}\" {1}", agentProcess.StartInfo.FileName, agentProcess.StartInfo.Arguments);

            _agentStore.AddAgent(agentId, agentProcess);

            log.Debug($"Waiting for agent {agentId:B} to register");

            const int pollTime = 200;

            // Increase the timeout to give time to attach a debugger
            bool debug = package.Settings.GetValueOrDefault(SettingDefinitions.DebugAgent) ||
                         package.Settings.GetValueOrDefault(SettingDefinitions.PauseBeforeRun);

            int waitTime = debug ? DEBUG_TIMEOUT : NORMAL_TIMEOUT;

            // Wait for agent registration based on the agent actually getting processor time to avoid falling over
            // under process starvation.
            while (waitTime > agentProcess.TotalProcessorTime.TotalMilliseconds && !agentProcess.HasExited)
            {
                Thread.Sleep(pollTime);

                if (_agentStore.IsAvailable(agentId, out var agent))
                {
                    log.Debug($"Returning new agent {agentId:B}");

                    switch (targetRuntime.Runtime.FrameworkIdentifier)
                    {
                        case FrameworkIdentifiers.NetFramework:
                            return new TestAgentRemotingProxy(agent, agentId);

                        case FrameworkIdentifiers.NetCoreApp:
                            return agent;

                        default:
                            throw new InvalidOperationException($"Invalid runtime: {targetRuntime.Runtime.FrameworkIdentifier}");
                    }
                }
            }

            return null;
        }

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
        public void ReleaseAgent(ITestAgent agent)
        {
            Process process;

            if (_agentStore.IsAgentActive(agent.Id, out process))
                try
                {
                    log.Debug("Stopping remote agent");
                    agent.Stop();
                }
                catch (SocketException se)
                {
                    int exitCode;

                    try
                    {
                        exitCode = process.ExitCode;
                    }
                    catch (NotSupportedException)
                    {
                        exitCode = -17;
                    }

                    if (exitCode == 0)
                    {
                        log.Warning("Agent connection was forcibly closed. Exit code was 0, so agent shutdown OK");
                    }
                    else
                    {
                        var stopError = $"Agent connection was forcibly closed. Exit code was {exitCode}. {Environment.NewLine}{ExceptionHelper.BuildMessageAndStackTrace(se)}";
                        log.Error(stopError);

                        throw;
                    }
                }
                catch (Exception e)
                {
                    var stopError = "Failed to stop the remote agent." + Environment.NewLine + ExceptionHelper.BuildMessageAndStackTrace(e);
                    log.Error(stopError);
                }
        }

        #endregion

        #region ITestAgency Implementation

        public void Register(ITestAgent agent)
        {
            log.Debug($"Registered agent {agent.Id:B}");
            _agentStore.Register(agent);
        }

        #endregion

        #region IService Implementation

        public IServiceLocator ServiceContext { get; set; }

        public ServiceStatus Status { get; private set; }

        // TODO: it would be better if we had a list of transports to start and stop!

        public void StopService()
        {
            try
            {
                _tcpTransport.Stop();
            }
            finally
            {
                Status = ServiceStatus.Stopped;
            }
        }

        public void StartService()
        {

            try
            {
            _extensionService = ServiceContext.GetService<ExtensionService>();

                _tcpTransport.Start();

                Status = ServiceStatus.Started;
            }
            catch
            {
                Status = ServiceStatus.Error;
                throw;
            }
        }

        #endregion

        private List<ExtensionNode> _launcherNodes;

        private List<ExtensionNode> LauncherNodes
        {
            get
            {
                if (_launcherNodes is null)
                {
                    _launcherNodes = new List<ExtensionNode>();

                    foreach (var node in _extensionService.GetExtensionNodes(AGENT_LAUNCHERS_PATH))
                    {
                        if (node.TypeName.StartsWith("NUnit."))
                        {
                            node.AddProperty("AgentName", GetAgentName(node));
                            node.AddProperty("AgentType", "LocalProcess");
                        }

                        _launcherNodes.Add(node);
                    }

                }

                return _launcherNodes;
            }
        }

        private Process CreateAgentProcess(Guid agentId, string agencyUrl, TestPackage package)
        {
            if (_extensionService is null)
                throw new InvalidOperationException("The field '_extensionService' must be non null when calling this method");

            // Check to see if a specific agent was selected
            string requestedAgent = package.Settings.GetValueOrDefault(SettingDefinitions.RequestedAgentName);
            bool specificAgentRequested = !string.IsNullOrEmpty(requestedAgent);

            foreach (var node in LauncherNodes)
            {
                if (specificAgentRequested && node.TypeName != requestedAgent)
                    continue;

                if (CanCreateAgent(node, package))
                {
                    var launcher = GetLauncherInstance(node);

                    var launcherName = launcher.GetType().Name;
                    log.Info($"Selected launcher {launcherName}");
                    package.Settings.Set(SettingDefinitions.SelectedAgentName.WithValue(launcherName));
                    return launcher.CreateAgent(agentId, agencyUrl, package);
                }
            }

            if (specificAgentRequested)
                throw new NUnitEngineException($"The requested launcher {requestedAgent} cannot load package {package.Name}");
            else
                throw new NUnitEngineException($"No agent available for TestPackage {package.Name}");
        }

        private bool CanCreateAgent(ExtensionNode node, TestPackage package)
        {
            // Newer implementations use a TargetFramework property to avoid
            // intantiating any agents, which will not be used.
            var runtimes = node.GetValues("TargetFramework");

            // If there is no property, we have to instantiate it to check.
            if (runtimes.Count() == 0)
            {
                var launcher = GetLauncherInstance(node);
                return launcher is not null && launcher.CanCreateAgent(package);
            }

            // The property is present, so no instantiation is needed.
            var agentTarget = new FrameworkName(runtimes.First());
            log.Debug($"Agent {node.TypeName} targets {agentTarget}");
            var packageTargetSetting = 
                package.Settings.GetValueOrDefault(SettingDefinitions.ImageTargetFrameworkName);

            if (!string.IsNullOrEmpty(packageTargetSetting))
            {
                var packageTarget = new FrameworkName(packageTargetSetting);
                return agentTarget.Identifier == packageTarget.Identifier
                    && agentTarget.Version.Major >= packageTarget.Version.Major;
            }

            var packageRuntimeVersion =
                package.Settings.GetValueOrDefault(SettingDefinitions.ImageRuntimeVersion);
            if (!string.IsNullOrEmpty(packageRuntimeVersion))
                return agentTarget.Identifier == FrameworkIdentifiers.NetFramework &&
                    new Version(packageRuntimeVersion).Major <= agentTarget.Version.Major;

            return false;
        }

        private IAgentLauncher GetLauncherInstance(ExtensionNode node)
        {
            var obj = node.ExtensionObject;
            if (obj is IAgentLauncher)
                return (IAgentLauncher)obj;
            if (obj is NUnit.Engine.Extensibility.IAgentLauncher)
                return new AgentLauncherWrapper(node, (NUnit.Engine.Extensibility.IAgentLauncher)obj);
            return null;
        }

        private TestAgentInfo GetAgentInfo(ExtensionNode node)
        {
            string agentName = node.GetValues("AgentName").FirstOrDefault() ?? node.TypeName;
            var agentType = TestAgentType.LocalProcess;
            string targetFramework = node.GetValues("TargetFramework").FirstOrDefault();

            return targetFramework is not null
                ? new TestAgentInfo(
                    agentName,
                    agentType,
                    new FrameworkName(targetFramework)) //RuntimeFramework.Parse(targetFramework).FrameworkName)
                : GetLauncherInstance(node).AgentInfo;
        }

        private string GetAgentName(IExtensionNode node) => node.TypeName;

        //private IAgentLauncher GetBestLauncher(TestPackage package)
        //{
        //    foreach (var launcher in _launchers.Where(l => l.CanCreateProcess(package)))
        //    {

        //    }
        //}

        internal bool IsAgentProcessActive(Guid agentId, out Process process)
        {
            return _agentStore.IsAgentActive(agentId, out process);
        }

        internal void OnAgentExit(Process process)
        {
            _agentStore.MarkProcessTerminated(process);

            string errorMsg;

            switch (process.ExitCode)
            {
                case AgentExitCodes.OK:
                case AgentExitCodes.CANCELLED_BY_USER:
                    return;
                case AgentExitCodes.PARENT_PROCESS_TERMINATED:
                    errorMsg = "Remote test agent believes agency process has exited.";
                    break;
                case AgentExitCodes.UNEXPECTED_EXCEPTION:
                    errorMsg = "Unhandled exception on remote test agent. " +
                               "To debug, try using --trace=debug to output logs.";
                    break;
                case AgentExitCodes.FAILED_TO_START_REMOTE_AGENT:
                    errorMsg = "Failed to start remote test agent.";
                    break;
                case AgentExitCodes.DEBUGGER_SECURITY_VIOLATION:
                    errorMsg = "Debugger could not be started on remote agent due to System.Security.Permissions.UIPermission not being set.";
                    break;
                case AgentExitCodes.DEBUGGER_NOT_IMPLEMENTED:
                    errorMsg = "Debugger could not be started on remote agent as not available on platform.";
                    break;
                case AgentExitCodes.UNABLE_TO_LOCATE_AGENCY:
                    errorMsg = "Remote test agent unable to locate agency process.";
                    break;
                default:
                    errorMsg = $"Remote test agent exited with non-zero exit code {process.ExitCode}";
                    break;
            }

            throw new EngineException(errorMsg);
        }
    }
}
