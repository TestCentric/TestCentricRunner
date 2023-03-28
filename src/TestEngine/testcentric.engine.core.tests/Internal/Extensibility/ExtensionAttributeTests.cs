// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Engine.Extensibility;
using NUnit.Framework;

namespace TestCentric.Engine.Extensibility
{
    public class ExtensionAttributeTests
    {
        [Test]
        public void IsEnabledByDefault()
        {
            Assert.That(new ExtensionAttribute().Enabled, Is.True);
        }

        [Test]
        public void MayBeExplicitlyDisabled()
        {
            var attr = new ExtensionAttribute() { Enabled = false };
            Assert.That(attr.Enabled, Is.False);
        }

        [Test]
        public void MayBeExplicitlyEnabled()
        {
            var attr = new ExtensionAttribute() { Enabled = true };
            Assert.That(attr.Enabled, Is.True);
        }
    }
}

