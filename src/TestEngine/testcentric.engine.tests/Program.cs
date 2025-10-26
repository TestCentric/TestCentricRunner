// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Reflection;
using NUnitLite;

namespace TestCentric.Engine
{
    class Program
    {
        static int Main(string[] args)
        {
            var list = new List<string>(args);
#if NETFRAMEWORK
            return new AutoRun().Execute(args);
#else
            return new TextRunner(typeof(Program).GetTypeInfo().Assembly).Execute(args);
#endif
        }
    }
}
