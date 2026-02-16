// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NSubstitute;
using NSubstitute.Core.Arguments;
using NUnit.Framework;
using TestCentric.Gui.Model;

namespace TestCentric.Gui.Presenters.TestTree
{
    public class SaveVisualStateTests : TreeViewPresenterTestBase
    {
        [Test]
        public void WhenTestRunBegins()
        {
            TestNode testNode = new TestNode("<test-suite id='1'/>");
            _model.LoadedTests.Returns(testNode);
            var tv = new TreeView();
            _view.TreeView.Returns(tv);
            _view.Nodes.Returns(tv.Nodes);
            _view.InvokeIfRequired(Arg.Do<MethodInvoker>(x => x.Invoke()));

            FireTestLoadedEvent(testNode);
            FireRunStartingEvent(1234);

            _model.Received().SaveVisualState(Arg.Any<VisualState>());
        }
    }
}
