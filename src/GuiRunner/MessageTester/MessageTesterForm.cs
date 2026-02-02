// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui
{
    public class MessageTesterForm : Form
    {
        public MessageTesterForm()
        {
            InitializeComponent();
        }

        private Label label2;
        private TextBox textBox1;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private Label label3;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private RadioButton radioButton3;
        private RadioButton radioButton4;
        private RadioButton radioButton5;
        private Label label4;
        private RadioButton radioButton6;
        private RadioButton radioButton7;
        private Label label5;
        private Panel panel1;
        private Button button6;
        private Label label1;

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button6 = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(629, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Use this form to test various parameter combinations for display by the MessageDi" +
    "splay facility of TestCentric.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(9, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(304, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "1. Enter message Text or select a standard message";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(31, 67);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(468, 141);
            this.textBox1.TabIndex = 2;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(518, 67);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(116, 31);
            this.button1.TabIndex = 3;
            this.button1.Text = "Single Short Line";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(518, 104);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(116, 31);
            this.button2.TabIndex = 4;
            this.button2.Text = "Single Long Line";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(518, 141);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(116, 31);
            this.button3.TabIndex = 5;
            this.button3.Text = "Multiple Lines";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(518, 178);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(116, 30);
            this.button4.TabIndex = 6;
            this.button4.Text = "Very Large Message";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(291, 288);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(116, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "Display Message";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(19, 224);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "2. Select API:";
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Checked = true;
            this.radioButton1.Location = new System.Drawing.Point(111, 222);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(47, 17);
            this.radioButton1.TabIndex = 9;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Error";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(181, 223);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(43, 17);
            this.radioButton2.TabIndex = 10;
            this.radioButton2.Text = "Info";
            this.radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(238, 222);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(57, 17);
            this.radioButton3.TabIndex = 11;
            this.radioButton3.Text = "YesNo";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton4
            // 
            this.radioButton4.AutoSize = true;
            this.radioButton4.Location = new System.Drawing.Point(309, 222);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(72, 17);
            this.radioButton4.TabIndex = 12;
            this.radioButton4.Text = "OkCancel";
            this.radioButton4.UseVisualStyleBackColor = true;
            // 
            // radioButton5
            // 
            this.radioButton5.AutoSize = true;
            this.radioButton5.Location = new System.Drawing.Point(396, 222);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(90, 17);
            this.radioButton5.TabIndex = 13;
            this.radioButton5.TabStop = true;
            this.radioButton5.Text = "YesNoCancel";
            this.radioButton5.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(19, 258);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(152, 13);
            this.label4.TabIndex = 14;
            this.label4.Text = "3. Center message form...";
            // 
            // radioButton6
            // 
            this.radioButton6.AutoSize = true;
            this.radioButton6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radioButton6.Location = new System.Drawing.Point(14, 5);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(76, 17);
            this.radioButton6.TabIndex = 15;
            this.radioButton6.Text = "On Screen";
            this.radioButton6.UseVisualStyleBackColor = true;
            // 
            // radioButton7
            // 
            this.radioButton7.AutoSize = true;
            this.radioButton7.Checked = true;
            this.radioButton7.Location = new System.Drawing.Point(113, 5);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(100, 17);
            this.radioButton7.TabIndex = 16;
            this.radioButton7.TabStop = true;
            this.radioButton7.Text = "On this Window";
            this.radioButton7.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(19, 295);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(150, 13);
            this.label5.TabIndex = 17;
            this.label5.Text = "4. Display the message...";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.radioButton7);
            this.panel1.Controls.Add(this.radioButton6);
            this.panel1.Location = new System.Drawing.Point(167, 253);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(240, 25);
            this.panel1.TabIndex = 18;
            // 
            // button6
            // 
            this.button6.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button6.Location = new System.Drawing.Point(518, 287);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(116, 23);
            this.button6.TabIndex = 19;
            this.button6.Text = "Exit";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // MessageTesterForm
            // 
            this.CancelButton = this.button6;
            this.ClientSize = new System.Drawing.Size(646, 325);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.radioButton5);
            this.Controls.Add(this.radioButton4);
            this.Controls.Add(this.radioButton3);
            this.Controls.Add(this.radioButton2);
            this.Controls.Add(this.radioButton1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "MessageTesterForm";
            this.Text = "TestCentric Message Tester";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "Very short message";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = $"Failed to enable logging. Might be due to missing access rights in folder {Environment.CurrentDirectory}. Please consider to start with admin rights.";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text =
                "Either the GUI was installed without any agents or the installed agents have been deleted. " +
                "Some agents may have encountered errors in loading.\r\n\r\n" +
                "You must install at least one agent in order to be able to load or run tests. " +
                "Install agents using the same source (i.e. nuget or chocolatey) from which you installed the GUI itself." +
                "You should select agents which match the target platforms you are using for development.\r\n\r\n" +
                "Click 'OK' to continue with extremely limited functionality, 'Cancel' to exit.";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = new GuiOptions().GetHelpText();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (radioButton6.Checked)
            {
                IMessageDisplay messageDisplayForm = new MessageDisplayForm("Message Tester")
                {
                    StartPosition = FormStartPosition.CenterScreen
                };
                if (radioButton1.Checked)
                    messageDisplayForm.Error(textBox1.Text);
                else if (radioButton2.Checked)
                    messageDisplayForm.Info(textBox1.Text);
                else if (radioButton3.Checked)
                    messageDisplayForm.YesNo(textBox1.Text);
                else if (radioButton4.Checked)
                    messageDisplayForm.OkCancel(textBox1.Text);
                else if (radioButton5.Checked)
                    messageDisplayForm.YesNoCancel(textBox1.Text);
            }
            else if (radioButton7.Checked)
            {
                IMessageDisplay messageDisplayForm = new MessageDisplayForm("Message Tester")
                {
                    StartPosition = FormStartPosition.CenterParent
                };
                if (radioButton1.Checked)
                    messageDisplayForm.Error(textBox1.Text);
                else if (radioButton2.Checked)
                    messageDisplayForm.Info(textBox1.Text);
                else if (radioButton3.Checked)
                    messageDisplayForm.YesNo(textBox1.Text);
                else if (radioButton4.Checked)
                    messageDisplayForm.OkCancel(textBox1.Text);
                else if (radioButton5.Checked)
                    messageDisplayForm.YesNoCancel(textBox1.Text);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
