// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NUnit.Engine;

namespace TestCentric.Gui.Model.Fakes
{
    public class ServiceLocator : IServiceLocator
    {
        private Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void AddService<T>(T service)
        {
            _services.Add(typeof(T), service);
        }

        public object GetService(Type serviceType)
        {
            return _services.ContainsKey(serviceType)
                ? _services[serviceType]
                : null;
        }

        public T GetService<T>() where T : class
        {
            return (T)GetService(typeof(T));
        }

        public bool TryGetService<T>(out T service) where T : class
        {
            throw new NotImplementedException();
        }
    }
}
