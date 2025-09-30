// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using TestCentric.Engine.Services;
using TestCentric.Engine.Extensibility;

namespace TestCentric.Gui.Model.Fakes
{
    public class ResultService : IResultService
    {
        public string[] Formats
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public NUnit.Engine.Extensibility.IResultWriter GetResultWriter(string format, object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
