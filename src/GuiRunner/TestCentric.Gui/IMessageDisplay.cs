// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui
{
    /// <summary>
    /// Interface implemented by objects, which know how to display a message
    /// </summary>
    public interface IMessageDisplay
    {
        void Error(string text, string caption = null);

        void Info(string text, string caption = null);

        bool YesNo(string text, string caption = null);

        bool OkCancel(string text, string caption = null);

        DialogResult YesNoCancel(string text, string caption = null);
    }
}
