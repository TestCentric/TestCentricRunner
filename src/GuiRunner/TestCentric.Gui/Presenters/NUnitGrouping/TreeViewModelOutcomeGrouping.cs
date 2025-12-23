// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    public class TreeViewModelOutcomeGrouping : TreeViewModelGroupingBase
    {
        public TreeViewModelOutcomeGrouping(ITestModel model) : base(model)
        {
            SupportsRegrouping = true;
        }

        public override IList<string> GetGroupNames(TestNode testNode)
        {
            string outcome = GetOutcome(testNode);
            return new List<string> { outcome };
        }

        private string GetOutcome(TestNode child)
        {
            var result = Model.TestResultManager.GetResultForTest(child.Id);
            if (result == null)
                return "Not run";

            if (result.Outcome.Equals(ResultState.Ignored))
                return "Ignored";

            return result.Outcome.ToString();
        }
    }
}
