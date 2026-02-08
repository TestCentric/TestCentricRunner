// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;
using TestCentric.Gui.Dialogs;

namespace TestCentric.Gui
{
    /// <summary>
    /// MessageBoxDisplay provides a simple implementation of IMessageDisplay using
    /// a custom window, i.e. MessageDisplayDialog. Additionally, it provides static 
    /// methods for a displaying a Message for use when the main form has not yet
    /// been created.
    /// </summary>
    public class MessageDisplay : IMessageDisplay
    {
        private const string DEFAULT_CAPTION = "TestCentric";

        #region Static members for displaying messages outside of any View

        // Static instance of MessageDisplayForm, used by code that does not have access
        // to one of our Views or to any other Windows Form to serve as the owner of the
        // dialog. Messages using this instance are displayed centered on the screen.
        private static readonly IMessageDisplay DefaultDialog = new MessageDisplayForm(DEFAULT_CAPTION);

        public static void Error(string message, string caption = DEFAULT_CAPTION) =>
            DefaultDialog.Error(message, caption);

        public static void Info(string message, string caption = DEFAULT_CAPTION) =>
            DefaultDialog.Info(message, caption);

        public static bool YesNo(string message, string caption = DEFAULT_CAPTION) =>
            DefaultDialog.YesNo(message, caption);

        public static bool OkCancel(string message, string caption = DEFAULT_CAPTION) =>
            DefaultDialog.OkCancel(message, caption);

        public static DialogResult YesNoCancel(string message, string caption = DEFAULT_CAPTION) =>
            DefaultDialog.YesNoCancel(message, caption);

        #endregion

        #region Instance members for use when a View is available

        private readonly string _caption;
        private MessageDisplayForm _myDialog;

        public MessageDisplay(string caption = DEFAULT_CAPTION)
        {
            _caption = caption;
            _myDialog = new MessageDisplayForm(caption);
        }

        void IMessageDisplay.Error(string text, string caption) =>
            _myDialog.Error(text, caption ?? _caption);

        void IMessageDisplay.Info(string text, string caption) =>
            _myDialog.Info(text, caption ?? _caption);

        bool IMessageDisplay.YesNo(string text, string caption) =>
            _myDialog.YesNo(text, caption ?? _caption);

        bool IMessageDisplay.OkCancel(string text, string caption) =>
            _myDialog.OkCancel(text, caption ?? _caption);

        DialogResult IMessageDisplay.YesNoCancel(string text, string caption) =>
            _myDialog.YesNoCancel(text, caption);

        #endregion
    }
}
