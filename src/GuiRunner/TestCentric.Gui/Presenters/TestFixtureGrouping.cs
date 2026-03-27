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
    public class TestFixtureGrouping : TestGrouping
    {
        public TestFixtureGrouping(GroupDisplayStrategy display) : base(display) { }

        public override string ID => "FIXTURE";

        public override TestGroup[] SelectGroups(TestNode testNode)
        {
            for (TestNode parent = testNode.Parent; parent != null; parent = parent.Parent)
            {
                if (parent.IsSuite && parent.Type == "TestFixture")
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
