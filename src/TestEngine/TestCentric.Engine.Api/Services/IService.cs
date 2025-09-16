// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Extensibility;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// Enumeration representing the status of a service
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>Service was never started or has been stopped</summary>
        Stopped,
        /// <summary>Started successfully</summary>
        Started,
        /// <summary>Service failed to start and is unavailable</summary>
        Error
    }

    /// <summary>
    /// The IService interface is implemented by all Services.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// The ServiceContext
        /// </summary>
        IServiceLocator ServiceContext { get; set; }

        /// <summary>
        /// Gets the ServiceStatus of this service
        /// </summary>
        ServiceStatus Status { get;  }

        /// <summary>
        /// Initialize the Service
        /// </summary>
        void StartService();

        /// <summary>
        /// Do any cleanup needed before terminating the service
        /// </summary>
        void StopService();
    }
}
