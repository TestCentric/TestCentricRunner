// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    public interface ITextOutputSettings
    {
        string Labels { get; set; }

        bool WordWrapEnabled { get; set; }
    }
}
