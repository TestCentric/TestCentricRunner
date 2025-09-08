// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    public class OutcomeGrouping : GroupingBase
    {
        public OutcomeGrouping(INUnitTreeDisplayStrategy support, ITestModel model, ITestTreeView view) :
            base(support, model, view)
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
