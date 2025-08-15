// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;

namespace TestCentric.Engine.Services
{
    /// <summary>
    /// ServiceManager handles access to all services - global
    /// facilities shared by all instances of TestEngine.
    /// </summary>
    public class ServiceManager : IDisposable
    {
        private List<IService> _services = new List<IService>();
        private Dictionary<Type, IService> _serviceIndex = new Dictionary<Type, IService>();

        static Logger log = InternalTrace.GetLogger(typeof(ServiceManager));

        public bool ServicesInitialized { get; private set; }

        public int ServiceCount { get { return _services.Count; } }

        public IService GetService( Type serviceType )
        {
            IService theService = null;

            if (_serviceIndex.ContainsKey(serviceType))
                theService = _serviceIndex[serviceType];
            else
                foreach( IService service in _services )
                {
                    if( serviceType.IsInstanceOfType( service ) )
                    {
                        _serviceIndex[serviceType] = service;
                        theService = service;
                        break;
                    }
                }

            if (theService != null)
            {
                log.Debug(string.Format("Request for service {0} will be satisfied by {1}", serviceType.Name, theService.GetType().Name));
                if (theService.Status == ServiceStatus.Stopped)
                    StartService(theService);
                return theService;
            }

            log.Error(string.Format("Requested service {0} was not found", serviceType.FullName));
            return null;
        }

        public void AddService(IService service)
        {
            _services.Add(service);
            log.Debug("Added " + service.GetType().Name);
        }

        public void StartServices()
        {
            //foreach( IService service in _services )
            //{
            //    if (service.Status == ServiceStatus.Stopped)
            //    {
            //        StartService(service);
            //    }
            //}

            this.ServicesInitialized = true;
        }

        private static void StartService(IService service)
        {
            string name = service.GetType().Name;
            log.Info("Initializing " + name);
            try
            {
                service.StartService();
                if (service.Status == ServiceStatus.Error)
                    throw new InvalidOperationException("Service failed to initialize " + name);
            }
            catch (Exception ex)
            {
                // TODO: Should we pass this exception through?
                log.Error("Failed to initialize " + name);
                log.Error(ex.ToString());
                throw;
            }
        }

        public void StopServices()
        {
            // Stop services in reverse of initialization order
            int index = _services.Count;
            while (--index >= 0)
            {
                IService service = _services[index];
                if (service.Status == ServiceStatus.Started)
                {
                    string name = service.GetType().Name;
                    log.Info("Stopping " + name);
                    try
                    {
                        service.StopService();
                    }
                    catch (Exception ex)
                    {
                        log.Error("Failure stopping service " + name);
                        log.Error(ex.ToString());
                    }
                }
            }
        }

        public void ClearServices()
        {
            log.Info("Clearing Service list");
            _services.Clear();
        }

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                StopServices();

                if (disposing)
                    foreach (IService service in _services)
                    {
                        IDisposable disposable = service as IDisposable;
                        if (disposable != null)
                            disposable.Dispose();
                    }

                _disposed = true;
            }
        }
    }
}
