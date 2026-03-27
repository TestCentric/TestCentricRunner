// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters
{
    public class AssemblyGrouping : TestGrouping
    {
        public AssemblyGrouping(GroupDisplayStrategy display) : base(display) { }

        public override string ID => "ASSEMBLY";

        public override TestGroup[] SelectGroups(TestNode testNode)
        {
            for (TestNode parent = testNode.Parent; parent != null; parent = parent.Parent)
            {
                if (parent.IsAssembly)
                {
                    foreach (var group in Groups)
                        if (group.Name == parent.Name)
                            return [group];

                    var newGroup = new TestGroup(parent.Name);
                    Groups.Add(newGroup);
                    return [newGroup];
                }
            }

            throw new Exception("Assembly Node not found");
        }
    }
}
