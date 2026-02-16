// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace TestCentric.Gui.Views
{
    public class DialogManager : IDialogManager
    {
        #region IDialogManager Members

        public string[] SelectMultipleFiles(string title, string filter)
        {
            OpenFileDialog dlg = CreateOpenFileDialog(title, filter);

            dlg.Multiselect = true;
            return dlg.ShowDialog() == DialogResult.OK
                ? dlg.FileNames
                : new string[0];
        }

        public string GetFileOpenPath(string title, string filter)
        {
            OpenFileDialog dlg = CreateOpenFileDialog(title, filter);

            dlg.Multiselect = false;

            return dlg.ShowDialog() == DialogResult.OK
                ? dlg.FileName
                : null;
        }

        // TODO: Can we remove tcproj option?
        public string CreateOpenFileFilter(bool nunit = false, bool vs = false, bool tcproj = false)
        {
            StringBuilder sb = new StringBuilder();

            // If any project types are supported, build Projects & Assemblies entry
            if (nunit || vs || tcproj)
            {
                List<string> supportedSuffix = new List<string>();
                if (nunit)
                    supportedSuffix.Add("*.nunit");
                if (vs)
                    supportedSuffix.AddRange(new[] { "*.csproj", "*.fsproj", "*.vbproj", "*.vjsproj", "*.vcproj", "*.sln" });
                if (tcproj)
                    supportedSuffix.Add("*.tcproj");

                supportedSuffix.AddRange(new[] { "*.dll", "*.exe" });

                var description = string.Join(",", supportedSuffix);
                var filter = string.Join(";", supportedSuffix);

                string str = $"Projects & Assemblies ({description})|{filter}|";
                sb.Append(str);

                // Build entries for each individual project type
                if (nunit)
                    sb.Append("NUnit Projects (*.nunit)|*.nunit|");

                if (vs)
                    sb.Append("Visual Studio Projects (*.csproj,*.fsproj,*.vbproj,*.vjsproj,*.vcproj,*.sln)|*.csproj;*.fsproj;*.vbproj;*.vjsproj;*.vcproj;*.sln|");

                if (tcproj)
                    sb.Append("TestCentric Projects (*.tcproj)|*.tcproj|");
            }

            sb.Append("Assemblies (*.dll,*.exe)|*.dll;*.exe|");
            sb.Append("All Files (*.*)|*.*");

            return sb.ToString();
        }

        public string GetFileSavePath(string title, string filter, string initialDirectory, string suggestedName)
        {
            return GetFileSavePath(title, filter, initialDirectory, suggestedName, out int _);
        }

        public string GetFileSavePath(string title, string filter, string initialDirectory, string suggestedName, out int selectedFilterIndex)
        {
            SaveFileDialog dlg = CreateSaveFileDialog(title, filter, initialDirectory, suggestedName);

            var dialogResult = dlg.ShowDialog();
            selectedFilterIndex = dlg.FilterIndex;

            return dialogResult == DialogResult.OK ? dlg.FileName : null;
        }

        public string GetFolderPath(string message, string initialPath)
        {
            FolderBrowserDialog browser = new FolderBrowserDialog();
            browser.Description = message;
            browser.SelectedPath = initialPath;
            return browser.ShowDialog() == DialogResult.OK
                ? browser.SelectedPath
                : null;
        }

        public Font SelectFont(Font currentFont)
        {
            FontDialog dlg = new FontDialog();
            dlg.FontMustExist = true;
            dlg.Font = currentFont;
            dlg.MinSize = 6;
            dlg.MaxSize = 12;
            dlg.AllowVectorFonts = false;
            dlg.ScriptsOnly = true;
            dlg.ShowEffects = false;
            dlg.ShowApply = true;
            dlg.Apply += (s, e) => ApplyFont?.Invoke(currentFont = dlg.Font);

            return dlg.ShowDialog() == DialogResult.OK ? dlg.Font : currentFont;
        }

        public event ApplyFontHandler ApplyFont;

        #endregion

        #region Helper Methods

        private static OpenFileDialog CreateOpenFileDialog(string title, string filter)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = title;
            dlg.Filter = filter;
            //if (initialDirectory != null)
            //    dlg.InitialDirectory = initialDirectory;
            dlg.FilterIndex = 1;
            dlg.FileName = "";
            return dlg;
        }

        private static SaveFileDialog CreateSaveFileDialog(string title, string filter, string initialDirectory, string suggestedName)
        {
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = title;
            dlg.Filter = filter;
            dlg.FilterIndex = 1;
            dlg.InitialDirectory = initialDirectory;
            dlg.FileName = suggestedName;
            return dlg;
        }

        #endregion
    }
}
