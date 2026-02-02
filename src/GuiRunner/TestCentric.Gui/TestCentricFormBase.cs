// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Windows.Forms;

namespace TestCentric.Gui
{
    public class TestCentricFormBase : Form
    {
        private IMessageDisplay _messageDisplay;
        private string _caption;

        public TestCentricFormBase(string caption = null)
        {
            _caption = caption;
        }

        public IMessageDisplay MessageDisplay
        {
            get
            {
                if (_messageDisplay == null)
                    _messageDisplay = new MessageDisplay(_caption ?? Text);

                return _messageDisplay;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Text = _caption;
        }
    }
}
