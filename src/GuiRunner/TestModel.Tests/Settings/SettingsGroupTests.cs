// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using NSubstitute;

namespace TestCentric.Gui.Model.Settings
{
    [TestFixture]
    public class SettingsGroupTests
    {
        private const string PREFIX = "Testing.";

        private SettingsGroup _settings;
        private ISettings _settingsService;

        [SetUp]
        public void BeforeEachTest()
        {
            _settingsService = Substitute.For<ISettings>();
            _settings = new SettingsGroup(_settingsService, PREFIX);
        }

        [Test]
        public void PrefixIsSetCorrectly()
        {
            Assert.That(_settings.GroupPrefix, Is.EqualTo(PREFIX));
        }
    }
}
