// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters
{
    public class UngroupedGrouping : TestGrouping
    {
        public UngroupedGrouping(GroupDisplayStrategy displayStrategy) : base(displayStrategy)
        {
        }

        public override string ID => "UNGROUPED";

        public override TestGroup[] SelectGroups(TestNode testNode)
        {
            return [ Groups[0] ];
        }

        public override void LoadGroups(IEnumerable<TestNode> tests)
        {
            Groups.Clear();
            Groups.Add(new TestGroup("All Tests"));

            base.LoadGroups(tests);
        }
    }
}
