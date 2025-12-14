// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Xml;
using NUnit.Extensibility;
using TestCentric.Engine.Extensibility;
using TestCentric.Engine.Services;

namespace TestCentric.Engine
{
    [Extension]
    public class DummyProjectLoaderExtension : IProjectLoader
    {
        public bool CanLoadFrom(string path)
        {
            throw new NotImplementedException();
        }

        public IProject LoadFrom(string path)
        {
            throw new NotImplementedException();
        }
    }

    [Extension]
    public class DummyResultWriterExtension : NUnit.Engine.Extensibility.IResultWriter
    {
        public void CheckWritability(string outputPath)
        {
            throw new NotImplementedException();
        }

        public void WriteResultFile(XmlNode resultNode, TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public void WriteResultFile(XmlNode resultNode, string outputPath)
        {
            throw new NotImplementedException();
        }
    }

    [Extension]
    public class DummyEventListenerExtension : NUnit.Engine.ITestEventListener
    {
        public void OnTestEvent(string report)
        {
            throw new NotImplementedException();
        }
    }

    [Extension]
    public class DummyServiceExtension : IService
    {
        public NUnit.Engine.IServiceLocator ServiceContext
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ServiceStatus Status
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public void StartService()
        {
            throw new NotImplementedException();
        }

        public void StopService()
        {
            throw new NotImplementedException();
        }
    }

    [Extension(Enabled=false)]
    public class DummyDisabledExtension : NUnit.Engine.ITestEventListener
    {
        public void OnTestEvent(string report)
        {
            throw new NotImplementedException();
        }
    }
}
