// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using TestCentric.Gui.Model;
using TestCentric.Gui.Views;

namespace TestCentric.Gui.Presenters.NUnitGrouping
{
    public class DurationGrouping : GroupingBase
    {
        public DurationGrouping(INUnitTreeDisplayStrategy support, ITestModel model, ITestTreeView view) :
            base(support, model, view)
        {
            SupportsRegrouping = true;
        }

        public override IList<string> GetGroupNames(TestNode testNode)
        {
            string outcome = GetDuration(testNode);
            return new List<string> { outcome };
        }

        private string GetDuration(TestNode child)
        {
            var result = Model.TestResultManager.GetResultForTest(child.Id);
            if (result == null)
                return "Not run";

            if (result.Duration < 0.1)
            {
                return "Fast";
            }
            else if (result.Duration < 0.5)
                return "Medium";

            return "Slow";
        }
    }
}
