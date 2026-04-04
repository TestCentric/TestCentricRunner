// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System.Windows.Forms;
using NUnit.Framework;

namespace TestCentric.Gui.Views
{
    [TestFixture]
    public class TestCentricMainViewTests : ControlTester
    {
        [OneTimeSetUp]
        public void CreateForm()
        {
            this.Control = new TestCentricMainView();
        }

        [OneTimeTearDown]
        public void CloseForm()
        {
            this.Control.Dispose();
        }

        [Test]
        public void DisplayFormatToolstripButtonExists()
        {
            var displayFormatButton = GetToolStripItem<ToolStripButton>("displayFormatButton");
            Assert.That(displayFormatButton, Is.Not.Null);
        }

        T GetToolStripItem<T>(string name) where T : ToolStripItem
        {
            ToolStrip toolStrip = GetToolStrip();
            foreach (ToolStripItem ctl in toolStrip.Items)
            {
                if (ctl.Name == name)
                    return ctl as T;
            }

            return null;
        }

        ToolStrip GetToolStrip()
        {
            foreach (Control ctl in _control.Controls)
            {
                if (ctl.GetType() == typeof(ToolStrip))
                    return ctl as ToolStrip;
            }

            return null;
        }
    }
}
