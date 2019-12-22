// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NUnit.Engine.Runners.Fakes
{
    internal class EmptyDirectTestRunner : Engine.Runners.DirectTestRunner
    {
        public EmptyDirectTestRunner(IServiceLocator services, TestPackage package) : base(services, package)
        {
#if !NETCOREAPP1_1
            TestDomain = AppDomain.CurrentDomain;
#endif
        }

        public new void LoadPackage()
        {
            base.LoadPackage();
        }
    }
}
