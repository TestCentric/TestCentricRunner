// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using TestCentric.Engine;
using TestCentric.Engine.Services;

namespace TestCentric.Gui.Model.Fakes
{
    public class AvailableRuntimesService : IAvailableRuntimes
    {
        private List<NUnit.Engine.IRuntimeFramework> _availableRuntimes = new List<NUnit.Engine.IRuntimeFramework>();

        public void AddRuntimes(params NUnit.Engine.IRuntimeFramework[] runtimes)
        {
            _availableRuntimes.AddRange(runtimes);
        }

        public IList<NUnit.Engine.IRuntimeFramework> AvailableRuntimes
        {
            get { return _availableRuntimes; }
        }

        public IList<NUnit.Engine.IRuntimeFramework> AvailableX86Runtimes
        {
            get { return _availableRuntimes; }
        }
    }
}
