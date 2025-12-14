// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using NUnit.Engine;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// The ServiceContext is used by services, runners and
    /// external clients to locate the services they need through
    /// the IServiceLocator interface.
    /// </summary>
    public class ServiceContext : IServiceLocator
    {
        public ServiceContext()
        {
            ServiceManager = new ServiceManager();
        }

        public ServiceManager ServiceManager { get; private set; }

        public int ServiceCount { get { return ServiceManager.ServiceCount; } }

        public void Add(IService service)
        {
            ServiceManager.AddService(service);
            service.ServiceContext = this;
        }

        public T GetService<T>() where T : class
        {
            return ServiceManager.GetService(typeof(T)) as T;
        }

        public object GetService(Type serviceType)
        {
            return ServiceManager.GetService(serviceType);
        }

        public bool TryGetService<T>(out T service) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
