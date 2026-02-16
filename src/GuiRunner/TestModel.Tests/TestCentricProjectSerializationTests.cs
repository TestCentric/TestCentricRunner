// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Xml;
using NUnit.Engine;
using NUnit.Framework;
using Org.XmlUnit.Constraints;

namespace TestCentric.Gui.Model
{
    public class TestCentricProjectSerializationTests
    {
		private const string CR = "\r";
		private const string LF = "\n";

        private static readonly string TEMP_PATH = Path.GetFullPath(Path.GetTempFileName());

		[TearDown] public void CleanUp()
		{
			if (File.Exists(TEMP_PATH))
				File.Delete(TEMP_PATH);
		}

		[Test]
		public void SaveAndReloadTestProject()
		{
			var project = new TestCentricProject("MyProject", "test1.dll", "test2.dll");
			var package = project.TopLevelPackage;
			var subPackages = package.SubPackages;
			Assert.That(subPackages.Count, Is.EqualTo(2));

            // Add settings intended for top level and both subpackages.
            // All these settings are non-standard, defined by the user.
            package.AddSetting("foo", "bar");
			package.AddSetting("num", 42);
			package.AddSetting("critical", true);

			// Add a setting to the first subpackage only
			subPackages[0].AddSetting("cpu", "x86");

			// Save the project
			project.SaveAs(TEMP_PATH);

            // Multi-line for ease of editing only, CR & LF removed
            string expectedXml = $"""
                <?xml version = "1.0" encoding="utf-8"?>
                <TestCentricProject>
                <TestPackage id="{package.ID}">
                <Settings foo="bar" num="42" critical="True" />
                <TestPackage id="{subPackages[0].ID}" fullname="{Path.GetFullPath("test1.dll")}">
                <Settings foo="bar" num="42" critical="True" cpu="x86" /></TestPackage>
                <TestPackage id="{subPackages[1].ID}" fullname="{Path.GetFullPath("test2.dll")}">
                <Settings foo="bar" num="42" critical="True" /></TestPackage></TestPackage>
                </TestCentricProject>
                """.Replace(CR, string.Empty).Replace(LF, string.Empty);

            // Load the saved file as a document and check content directly
            var doc = new XmlDocument();
			doc.Load(TEMP_PATH);
			Assert.That(doc.OuterXml, CompareConstraint.IsIdenticalTo(expectedXml));

            // Load the saved file into a new project and check that
            var newProject = TestCentricProject.LoadFrom(TEMP_PATH);
            Assert.That(newProject.ProjectPath, Is.EqualTo(TEMP_PATH));
            Assert.That(newProject.TopLevelPackage, Is.Not.Null);

            subPackages = newProject.TopLevelPackage.SubPackages;
            Assert.That(subPackages.Count, Is.EqualTo(2));
            Assert.That(subPackages[0].Settings.GetSetting("foo"), Is.EqualTo("bar"));
            Assert.That(subPackages[0].Settings.GetSetting("num"), Is.EqualTo(42));
            Assert.That(subPackages[0].Settings.GetSetting("critical"), Is.EqualTo(true));
            Assert.That(subPackages[0].Settings.GetSetting("cpu"), Is.EqualTo("x86"));
            Assert.That(subPackages[1].Settings.GetSetting("foo"), Is.EqualTo("bar"));
            Assert.That(subPackages[1].Settings.GetSetting("num"), Is.EqualTo(42));
            Assert.That(subPackages[1].Settings.GetSetting("critical"), Is.EqualTo(true));
        }

        [Test]
		public void SaveAndReloadEmptyTestProject()
		{
			var project = new TestCentricProject("MyProject");
			project.SaveAs(TEMP_PATH);

            string expectedXml = $"""
                <?xml version="1.0" encoding="utf-8"?>
                <TestCentricProject>
                <TestPackage id="{project.TopLevelPackage.ID}" />
                </TestCentricProject>
                """.Replace(CR, string.Empty).Replace(LF, string.Empty);

            // Load the saved file as a document and check content directly
            var doc = new XmlDocument();
			doc.Load(TEMP_PATH);
            Assert.That(doc.OuterXml, CompareConstraint.IsIdenticalTo(expectedXml));

            // Load the saved file into a new project and check that
            var newProject = TestCentricProject.LoadFrom(TEMP_PATH);
            Assert.That(newProject.ProjectPath, Is.EqualTo(TEMP_PATH));
            Assert.That(newProject.TopLevelPackage.ID, Is.EqualTo(project.TopLevelPackage.ID));
            Assert.That(newProject.TopLevelPackage.HasSubPackages, Is.False);
        }
    }
}
