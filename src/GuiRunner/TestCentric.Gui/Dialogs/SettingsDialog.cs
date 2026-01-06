// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Dialogs
{
    using Model;
    using Presenters;
    using SettingsPages;

    /// <summary>
    /// Static class used to display the tree-based SettingsDialog.
    /// </summary>
    public static class SettingsDialog
    {
        public static void Display(TestCentricPresenter presenter, ITestModel model)
        {
            TreeBasedSettingsDialog.Display(presenter, model,
            new GuiSettingsPage("General"),
            new TreeSettingsPage("Tree Display", presenter.ImageSetManager),
            new AssemblyReloadSettingsPage("Automatic Reload"));
        }

        public static void DisplayTestRunSettings(TestCentricPresenter presenter, ITestModel model)
        {
            TreeBasedSettingsDialog.Display(presenter, model, new AdvancedLoaderSettingsPage("Assembly Load Settings"));
        }
    }
}
