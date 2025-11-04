// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

namespace TestCentric.Gui.Fakes
{
    using NSubstitute;
    using TestCentric.Gui.Model.Settings;

    public class UserSettings
    {
        /// <summary>
        /// Create a IUserSetting substitute which is initialized with some default values
        /// </summary>
        public static IUserSettings Create()
        {
            IUserSettings userSettings = Substitute.For<IUserSettings>();
            userSettings.Gui.TestTree.AlternateImageSet.Returns("Classic");
            userSettings.Gui.TestTree.DisplayFormat.Returns("NUNIT_TREE");
            userSettings.Gui.TestTree.ShowNamespace.Returns(true);
            return userSettings;
        }
    }
}
