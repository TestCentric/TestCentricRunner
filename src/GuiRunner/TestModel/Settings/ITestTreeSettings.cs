// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Model.Settings
{
    public interface ITestTreeSettings
    {
        int InitialTreeDisplay { get; set; }

        string AlternateImageSet { get; set; }

        bool ShowCheckBoxes { get; set; }

        bool ShowTestDuration { get; set; }

        string DisplayFormat { get; set; }

        bool ShowNamespace { get; set; }

        string NUnitGroupBy { get; set; }

        bool ShowFilter { get; set; }

        IFixtureListSettings FixtureList { get; }

        ITestListSettings TestList { get; }
    }
}
