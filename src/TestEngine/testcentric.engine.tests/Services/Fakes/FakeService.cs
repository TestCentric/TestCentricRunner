// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Engine.Services.Fakes
{
    public class FakeService : IService
    {
        public NUnit.Engine.IServiceLocator ServiceContext { get; set; }

        private ServiceStatus _status;
        public ServiceStatus Status
        {
            get { return _status; }
        }

        public virtual void StartService()
        {
            _status = FailToStart
                ? ServiceStatus.Error
                : ServiceStatus.Started;
        }

        public virtual void StopService()
        {
            _status = ServiceStatus.Stopped;
        }

        // Set to true to cause the service to give
        // an error result when started
        public bool FailToStart { get; set; }

        // Set to true to cause the service to give
        // an error result when stopped
        public bool FailedToStop { get; set; }
    }
}
