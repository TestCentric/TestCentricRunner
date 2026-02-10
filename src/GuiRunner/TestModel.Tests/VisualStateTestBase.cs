// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;

namespace TestCentric.Gui.Model
{
    public class VisualStateTestBase
    {
        // Flags for use with VTN
        protected const int EXP = 1;
        protected const int CHK = 2;
        protected const int SEL = 4;
        protected const int TOP = 8;

        protected static VisualTreeNode VTN(string name, int flags = 0, params VisualTreeNode[] childNodes)
            => VisualStateTestData.VTN(name, flags, childNodes);

        // Helper used to create a TreeNode for use in the tests
        // NOTE: Unlike the TreeNodes used in the production app, the Tag
        // is not set because VisualState doesn't make use of it.
        protected static TreeNode TN(string name, params TreeNode[] childNodes)
            => VisualStateTestData.TN(name, childNodes);
    }
}
