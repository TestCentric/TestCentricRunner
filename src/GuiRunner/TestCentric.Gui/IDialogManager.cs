// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Drawing;

namespace TestCentric.Gui.Views
{
    public delegate void ApplyFontHandler(Font font);

    public interface IDialogManager
    {
        string[] SelectMultipleFiles(string title, string filter);

        string GetFileOpenPath(string title, string filter);

        string CreateOpenFileFilter(bool nunit = false, bool vs = false, bool tcproj = false);

        string GetFileSavePath(string title, string filter, string initialDirectory, string suggestedName);

        string GetFileSavePath(string title, string filter, string initialDirectory, string suggestedName, out int selectedFilterIndex);

        string GetFolderPath(string message, string initialPath);

        Font SelectFont(Font currentFont);

        event ApplyFontHandler ApplyFont;
    }
}
