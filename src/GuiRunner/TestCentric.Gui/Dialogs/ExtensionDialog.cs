// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using TestCentric.Engine.Services;
using TestCentric.Extensibility;

namespace TestCentric.Gui.Dialogs
{
    /// <summary>
    /// Summary description for ExtensionDialog.
    /// </summary>
    public class ExtensionDialog : System.Windows.Forms.Form
    {
        private IList<TestCentric.Extensibility.IExtensionPoint> _extensionPoints;
        private IList<TestCentric.Extensibility.IExtensionNode> _extensionPointExtensions;
        private IList<TestCentric.Extensibility.IExtensionNode> _allExtensions;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView extensionListView2;
        private System.Windows.Forms.ColumnHeader extensionNameColumn;
        private System.Windows.Forms.ColumnHeader extensionStatusColumn;
        private ColumnHeader extensionEnabledColumn;
        private TextBox extensionDescription2;
        private Label descriptionLabel;
        private Label label3;
        private TextBox extensionProperties2;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private TestCentric.Gui.Controls.ExpandingLabel assemblyPath2;
        private Label label4;
        private Label assemblyVersion2;
        private Label label6;
        private Label label5;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Label label8;
        private Controls.ExpandingLabel assemblyPath1;
        private Label label9;
        private TextBox extensionProperties1;
        private Label label10;
        private TextBox extensionDescription1;
        private Label label11;
        private TextBox extensionPointDescriptionTextBox;
        private ListBox extensionPointsListBox;
        private Label label1;
        private Label label7;
        private Label label2;
        private ListView extensionListView1;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Label assemblyVersion1;
        private Label label13;
        private Label label12;
        private TextBox extensionPointPath;
        private IExtensionService _extensionService;

        public ExtensionDialog(IExtensionService extensionService)
        {
            _extensionService = extensionService;

            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExtensionDialog));
            this.extensionListView2 = new System.Windows.Forms.ListView();
            this.extensionNameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.extensionStatusColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.extensionEnabledColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.assemblyVersion2 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.extensionProperties2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.extensionDescription2 = new System.Windows.Forms.TextBox();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.assemblyVersion1 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.extensionListView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.assemblyPath1 = new TestCentric.Gui.Controls.ExpandingLabel();
            this.label9 = new System.Windows.Forms.Label();
            this.extensionProperties1 = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.extensionDescription1 = new System.Windows.Forms.TextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.extensionPointDescriptionTextBox = new System.Windows.Forms.TextBox();
            this.assemblyPath2 = new TestCentric.Gui.Controls.ExpandingLabel();
            this.extensionPointsListBox = new System.Windows.Forms.ListBox();
            this.label12 = new System.Windows.Forms.Label();
            this.extensionPointPath = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // extensionListView2
            // 
            this.extensionListView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionListView2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionListView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.extensionNameColumn,
            this.extensionStatusColumn,
            this.extensionEnabledColumn});
            this.extensionListView2.FullRowSelect = true;
            this.extensionListView2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.extensionListView2.HideSelection = false;
            this.extensionListView2.Location = new System.Drawing.Point(9, 199);
            this.extensionListView2.MultiSelect = false;
            this.extensionListView2.Name = "extensionListView2";
            this.extensionListView2.ShowItemToolTips = true;
            this.extensionListView2.Size = new System.Drawing.Size(443, 99);
            this.extensionListView2.TabIndex = 0;
            this.extensionListView2.UseCompatibleStateImageBehavior = false;
            this.extensionListView2.View = System.Windows.Forms.View.Details;
            this.extensionListView2.SelectedIndexChanged += new System.EventHandler(this.Tab2_SelectedExtensionChanged);
            // 
            // extensionNameColumn
            // 
            this.extensionNameColumn.Text = "Type Name";
            this.extensionNameColumn.Width = 317;
            // 
            // extensionStatusColumn
            // 
            this.extensionStatusColumn.Text = "Status";
            this.extensionStatusColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.extensionStatusColumn.Width = 69;
            // 
            // extensionEnabledColumn
            // 
            this.extensionEnabledColumn.Text = "Enabled";
            this.extensionEnabledColumn.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.Location = new System.Drawing.Point(192, 584);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "OK";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // assemblyVersion2
            // 
            this.assemblyVersion2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.assemblyVersion2.BackColor = System.Drawing.SystemColors.Window;
            this.assemblyVersion2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assemblyVersion2.Location = new System.Drawing.Point(58, 504);
            this.assemblyVersion2.Name = "assemblyVersion2";
            this.assemblyVersion2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.assemblyVersion2.Size = new System.Drawing.Size(119, 21);
            this.assemblyVersion2.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(7, 504);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Version:";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(7, 473);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(37, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Path:";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(6, 450);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(118, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Extension Assembly";
            // 
            // extensionProperties2
            // 
            this.extensionProperties2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionProperties2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionProperties2.Location = new System.Drawing.Point(10, 394);
            this.extensionProperties2.Multiline = true;
            this.extensionProperties2.Name = "extensionProperties2";
            this.extensionProperties2.Size = new System.Drawing.Size(442, 41);
            this.extensionProperties2.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(7, 378);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Extension Properties";
            // 
            // extensionDescription2
            // 
            this.extensionDescription2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionDescription2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionDescription2.Location = new System.Drawing.Point(10, 333);
            this.extensionDescription2.Multiline = true;
            this.extensionDescription2.Name = "extensionDescription2";
            this.extensionDescription2.Size = new System.Drawing.Size(442, 31);
            this.extensionDescription2.TabIndex = 4;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionLabel.Location = new System.Drawing.Point(7, 314);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(0, 16);
            this.descriptionLabel.TabIndex = 3;
            this.descriptionLabel.Text = "Description:";
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(1, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(466, 566);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.extensionPointPath);
            this.tabPage1.Controls.Add(this.label12);
            this.tabPage1.Controls.Add(this.assemblyVersion1);
            this.tabPage1.Controls.Add(this.label13);
            this.tabPage1.Controls.Add(this.label8);
            this.tabPage1.Controls.Add(this.extensionListView1);
            this.tabPage1.Controls.Add(this.assemblyPath1);
            this.tabPage1.Controls.Add(this.label9);
            this.tabPage1.Controls.Add(this.extensionProperties1);
            this.tabPage1.Controls.Add(this.label11);
            this.tabPage1.Controls.Add(this.label10);
            this.tabPage1.Controls.Add(this.extensionDescription1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(458, 540);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Extensions";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // assemblyVersion1
            // 
            this.assemblyVersion1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.assemblyVersion1.BackColor = System.Drawing.SystemColors.Window;
            this.assemblyVersion1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assemblyVersion1.Location = new System.Drawing.Point(61, 405);
            this.assemblyVersion1.Name = "assemblyVersion1";
            this.assemblyVersion1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.assemblyVersion1.Size = new System.Drawing.Size(119, 21);
            this.assemblyVersion1.TabIndex = 14;
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(10, 405);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(53, 13);
            this.label13.TabIndex = 13;
            this.label13.Text = "Version:";
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(10, 375);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(37, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Path:";
            // 
            // extensionListView1
            // 
            this.extensionListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionListView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionListView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.extensionListView1.FullRowSelect = true;
            this.extensionListView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.extensionListView1.HideSelection = false;
            this.extensionListView1.Location = new System.Drawing.Point(3, 6);
            this.extensionListView1.MultiSelect = false;
            this.extensionListView1.Name = "extensionListView1";
            this.extensionListView1.ShowItemToolTips = true;
            this.extensionListView1.Size = new System.Drawing.Size(443, 182);
            this.extensionListView1.TabIndex = 11;
            this.extensionListView1.UseCompatibleStateImageBehavior = false;
            this.extensionListView1.View = System.Windows.Forms.View.Details;
            this.extensionListView1.SelectedIndexChanged += new System.EventHandler(this.Tab1_SelectedExtensionChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Type Name";
            this.columnHeader1.Width = 317;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Status";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.columnHeader2.Width = 69;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Enabled";
            this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // assemblyPath1
            // 
            this.assemblyPath1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assemblyPath1.BackColor = System.Drawing.SystemColors.Window;
            this.assemblyPath1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assemblyPath1.Location = new System.Drawing.Point(62, 374);
            this.assemblyPath1.Name = "assemblyPath1";
            this.assemblyPath1.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.assemblyPath1.Size = new System.Drawing.Size(384, 21);
            this.assemblyPath1.TabIndex = 9;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(4, 340);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(118, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Extension Assembly";
            // 
            // extensionProperties1
            // 
            this.extensionProperties1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionProperties1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionProperties1.Location = new System.Drawing.Point(3, 279);
            this.extensionProperties1.Multiline = true;
            this.extensionProperties1.Name = "extensionProperties1";
            this.extensionProperties1.Size = new System.Drawing.Size(443, 41);
            this.extensionProperties1.TabIndex = 6;
            // 
            // label11
            // 
            this.label11.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(0, 199);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(222, 16);
            this.label11.TabIndex = 3;
            this.label11.Text = "Description:";
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(4, 263);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(64, 13);
            this.label10.TabIndex = 5;
            this.label10.Text = "Properties";
            // 
            // extensionDescription1
            // 
            this.extensionDescription1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionDescription1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionDescription1.Location = new System.Drawing.Point(3, 218);
            this.extensionDescription1.Multiline = true;
            this.extensionDescription1.Name = "extensionDescription1";
            this.extensionDescription1.Size = new System.Drawing.Size(443, 31);
            this.extensionDescription1.TabIndex = 4;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.assemblyVersion2);
            this.tabPage2.Controls.Add(this.label1);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.extensionPointDescriptionTextBox);
            this.tabPage2.Controls.Add(this.label5);
            this.tabPage2.Controls.Add(this.assemblyPath2);
            this.tabPage2.Controls.Add(this.extensionPointsListBox);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.extensionListView2);
            this.tabPage2.Controls.Add(this.extensionProperties2);
            this.tabPage2.Controls.Add(this.descriptionLabel);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.extensionDescription2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(458, 540);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Extension Points";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(6, 308);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(130, 13);
            this.label7.TabIndex = 14;
            this.label7.Text = "Extension Description";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 183);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(191, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Extensions at this ExensionPoint";
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 113);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(211, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "ExtensionPoint Description:";
            // 
            // extensionPointDescriptionTextBox
            // 
            this.extensionPointDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionPointDescriptionTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionPointDescriptionTextBox.Location = new System.Drawing.Point(7, 132);
            this.extensionPointDescriptionTextBox.Multiline = true;
            this.extensionPointDescriptionTextBox.Name = "extensionPointDescriptionTextBox";
            this.extensionPointDescriptionTextBox.Size = new System.Drawing.Size(445, 31);
            this.extensionPointDescriptionTextBox.TabIndex = 1;
            // 
            // assemblyPath2
            // 
            this.assemblyPath2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.assemblyPath2.BackColor = System.Drawing.SystemColors.Window;
            this.assemblyPath2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.assemblyPath2.Location = new System.Drawing.Point(59, 472);
            this.assemblyPath2.Name = "assemblyPath2";
            this.assemblyPath2.Padding = new System.Windows.Forms.Padding(0, 2, 0, 2);
            this.assemblyPath2.Size = new System.Drawing.Size(393, 21);
            this.assemblyPath2.TabIndex = 9;
            // 
            // extensionPointsListBox
            // 
            this.extensionPointsListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.extensionPointsListBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.extensionPointsListBox.FormattingEnabled = true;
            this.extensionPointsListBox.Location = new System.Drawing.Point(6, 6);
            this.extensionPointsListBox.Name = "extensionPointsListBox";
            this.extensionPointsListBox.Size = new System.Drawing.Size(446, 93);
            this.extensionPointsListBox.TabIndex = 6;
            this.extensionPointsListBox.SelectedIndexChanged += new System.EventHandler(this.Tab2_SelectedExtensionPointChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(4, 442);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(121, 13);
            this.label12.TabIndex = 15;
            this.label12.Text = "ExtensionPoint Path";
            // 
            // extensionPointPath
            // 
            this.extensionPointPath.Location = new System.Drawing.Point(3, 462);
            this.extensionPointPath.Name = "extensionPointPath";
            this.extensionPointPath.Size = new System.Drawing.Size(443, 20);
            this.extensionPointPath.TabIndex = 16;
            // 
            // ExtensionDialog
            // 
            this.ClientSize = new System.Drawing.Size(464, 613);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(480, 569);
            this.Name = "ExtensionDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Engine Extensions";
            this.Load += new System.EventHandler(this.ExtensionDialog_Load);
            this.SizeChanged += new System.EventHandler(this.ExtensionDialog_ResizeEnd);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion

        private void ExtensionDialog_Load(object sender, System.EventArgs e)
        {
            if (!DesignMode)
            {
                Tab1_InitialDisplay();
                Tab2_InitialDisplay();
            }
        }

        private void ExtensionDialog_ResizeEnd(object sender, EventArgs e) => AdjustListViewLayout();

        private void button1_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void Tab1_InitialDisplay()
        {
            _allExtensions = new List<IExtensionNode>(_extensionService.Extensions);
            foreach (var extension in _allExtensions)
            {
                ListViewItem item = new ListViewItem(
                    new string[] {
                            extension.TypeName,
                            extension.Status.ToString(),
                            extension.Enabled ? "Yes" : "No" });

                extensionListView1.Items.Add(item);
            }

            if (extensionListView1.Items.Count > 0)
                extensionListView1.Items[0].Selected = true;
        }

        private void Tab1_SelectedExtensionChanged(object sender, EventArgs e)
        {
            if (!DesignMode && extensionListView1.SelectedIndices.Count > 0)
            {
                int index = extensionListView1.SelectedIndices[0];
                var extension = _allExtensions[index];

                extensionDescription1.Text = extension.Description ?? "==None Provided==";

                extensionProperties1.Clear();
                foreach (string prop in extension.PropertyNames)
                {
                    var sb = new StringBuilder($"{prop} :");
                    foreach (string val in extension.GetValues(prop))
                        sb.Append(" " + val);

                    extensionProperties1.AppendText(sb.ToString() + Environment.NewLine);
                }

                assemblyPath1.Text = extension.AssemblyPath;
                assemblyVersion1.Text = extension.AssemblyVersion.ToString();

                extensionPointPath.Text = extension.Path ?? "--No Path Found==";

                //AdjustListViewLayout();
            }
        }

        private void Tab2_InitialDisplay()
        {
            _extensionPoints = new List<IExtensionPoint>(_extensionService.ExtensionPoints);

            foreach (var ep in _extensionPoints)
                extensionPointsListBox.Items.Add(ep.Path);

            if (extensionPointsListBox.Items.Count > 0)
                extensionPointsListBox.SelectedIndex = 0;
        }

        private void Tab2_SelectedExtensionPointChanged(object sender, EventArgs e)
        {
            var index = extensionPointsListBox.SelectedIndex;
            if (index >= 0)
            {
                var ep = _extensionPoints[index];
                _extensionPointExtensions = new List<IExtensionNode>(ep.Extensions);
                extensionPointDescriptionTextBox.Text = ep.Description ?? "==None Provided==";

                extensionListView2.Items.Clear();
                foreach (var extension in ep.Extensions)
                {
                    ListViewItem item = new ListViewItem(
                        new string[] {
                            extension.TypeName,
                            extension.Status.ToString(),
                            extension.Enabled ? "Yes" : "No" });

                    //if (extension.Status == ExtensionStatus.Error)
                    //    item.ToolTipText = BuildExceptionMessage(extension.Exception);

                    extensionListView2.Items.Add(item);
                }

                if (extensionListView2.Items.Count > 0)
                {
                    extensionListView2.Items[0].Selected = true;
                }
                else
                {
                    extensionDescription2.Text = "";
                    extensionProperties2.Text = "";
                    assemblyPath2.Text = "";
                    assemblyVersion2.Text = "";
                    AdjustListViewLayout();
                }
            }
        }

        private void Tab2_SelectedExtensionChanged(object sender, System.EventArgs e)
        {
            if (!DesignMode && extensionListView2.SelectedIndices.Count > 0)
            {
                int index = extensionListView2.SelectedIndices[0];
                var extension = _extensionPointExtensions[index];

                extensionDescription2.Text = extension.Description ?? "==None Provided==";

                extensionProperties2.Clear();
                foreach (string prop in extension.PropertyNames)
                {
                    var sb = new StringBuilder($"{prop} :");
                    foreach (string val in extension.GetValues(prop))
                        sb.Append(" " + val);

                    extensionProperties2.AppendText(sb.ToString() + Environment.NewLine);
                }

                assemblyPath2.Text = extension.AssemblyPath;
                assemblyVersion2.Text = extension.AssemblyVersion.ToString();

                //AdjustListViewLayout();
            }
        }

        private static string BuildExceptionMessage(Exception exception)
        {
            var sb = new StringBuilder($"{exception.GetType().Name}: {exception.Message}");

            var inner = exception.InnerException;
            while (inner != null)
            {
                sb.AppendLine($"--> {inner.Message}");
                inner = inner.InnerException;
            }

            return sb.ToString();
        }

        private void AdjustListViewLayout()
        {
            int width = extensionListView2.ClientSize.Width;
            for (int i = 1; i < extensionListView2.Columns.Count; i++)
                width -= extensionListView2.Columns[i].Width;
            extensionListView2.Columns[0].Width = width;

            extensionListView2.Refresh();
        }
    }
}
