// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;

namespace TestCentric.Gui.Model.Settings
{
    public interface IErrorDisplaySettings
    {
        int SplitterPosition { get; set; }

        bool WordWrapEnabled { get; set; }

        bool ToolTipsEnabled { get; set; }

        bool SourceCodeDisplay {  get; set; }

        Orientation SourceCodeSplitterOrientation { get; set; }

        float SourceCodeVerticalSplitterPosition { get; set; }

        float SourceCodeHorizontalSplitterPosition  { get; set ; }
    }
}
