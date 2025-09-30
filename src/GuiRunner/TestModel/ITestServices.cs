// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using TestCentric.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Gui.Model
{
    /// <summary>
    /// ITestServices extends IServiceLocator in order to
    /// conveniently cache commonly used services.
    /// </summary>
    public interface ITestServices : IServiceLocator
    {
        IExtensionService ExtensionService { get; }
        IResultService ResultService { get; }
        // TODO: Figure out how to handle this
        //IProjectService ProjectService { get; }
        TestAgentService TestAgentService { get; }
    }
}
