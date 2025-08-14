// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui
{
    public interface ILongRunningOperationDisplay
    {
        void Display(string text);
        void Hide();
    }
}
