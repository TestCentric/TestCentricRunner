// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Views
{
    using System;
    using Elements;

    public interface ITestPropertiesView : IView
    {
        event EventHandler Resize;
        event CommandHandler DisplayHiddenPropertiesChanged;

        bool Visible { get; set; }
        int ClientHeight { get; }

        string Header { get; set; }
        string TestType { get; set; }
        string FullName { get; set; }
        string Description { get; set; }
        string Categories { get; set; }
        string TestCount { get; set; }
        string RunState { get; set; }
        string SkipReason { get; set; }
        bool DisplayHiddenProperties { get; }
        string Properties { get; set; }
        string PackageSettings { get; set; }

        TestPackageSubView TestPackageSubView { get; }
        TestPropertiesSubView TestPropertiesSubView { get; }

        TestGroupPropertiesSubView TestGroupPropertiesSubView { get; }

        TestPropertiesView.SubView[] SubViews { get; }
    }
}
