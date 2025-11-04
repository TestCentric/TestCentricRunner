// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestCentric.Gui
{
    using Dialogs;
    using Model;
    using Model.Settings;

    /// <summary>
    /// SettingsPage is the base class for all pages used
    /// in a tabbed or tree-structured SettingsDialog.
    /// </summary>
    public class SettingsPage : UserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        private string _key;
        private string _title;

        private MessageBoxDisplay _messageDisplay;

        // Constructor used by the Windows.Forms Designer
        public SettingsPage()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitializeComponent call
        }

        // Constructor we use in creating page for a Tabbed
        // or TreeBased dialog.
        public SettingsPage(string key) : this()
        {
            _key = key;
            _title = key;
            int dot = key.LastIndexOf('.');
            if (dot >= 0) _title = key.Substring(dot + 1);
            _messageDisplay = new MessageBoxDisplay("NUnit Settings");
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Properties

        public string Key
        {
            get { return _key; }
        }

        public string Title
        {
            get { return _title; }
        }

        public bool SettingsLoaded
        {
            get { return Settings != null; }
        }

        public IMessageDisplay MessageDisplay
        {
            get { return _messageDisplay; }
        }

        protected ITestModel Model { get; private set; }
        protected IUserSettings Settings { get; private set; }

        protected IDictionary<string, object> PackageSettingChanges { get; private set; }

        #endregion

        #region Public Methods
        public virtual void LoadSettings()
        {
        }

        public virtual void ApplySettings()
        {
        }
        #endregion

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // SettingsPage
            // 
            Name = "SettingsPage";
            Size = new System.Drawing.Size(456, 336);

        }
        #endregion

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!DesignMode)
            {
                var dlg = ParentForm as SettingsDialogBase;

                if (dlg == null)
                    throw new InvalidOperationException("SettingsPage is only designed to be used in a Settings Dialog");
                if (dlg.Settings == null)
                    throw new InvalidOperationException("The Settings Dialog was not properly initialized");

                Model = dlg.Model;
                Settings = dlg.Settings;
                PackageSettingChanges = dlg.PackageSettingChanges;

                LoadSettings();
            }
        }
    }
}
