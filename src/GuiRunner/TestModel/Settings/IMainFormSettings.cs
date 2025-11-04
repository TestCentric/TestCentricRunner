// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Drawing;

namespace TestCentric.Gui.Model.Settings
{
    public interface IMainFormSettings
    {
        Point Location { get; set; }

        Size Size { get; set; }

        bool Maximized { get; set; }

        int SplitPosition { get; set; }

        bool ShowStatusBar { get; set; }
    }
}
