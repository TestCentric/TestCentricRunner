// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Common;
using NUnit.Framework;
using TestCentric.Gui.Model.Fakes;
using TestCentric.Engine;

namespace TestCentric.Gui.Model
{
    public class CreateNewProjectTests
    {
        private TestModel _model;

        [SetUp]
        public void CreateModel()
        {
            _model = new TestModel(new MockTestEngine());
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists("temp.tcproj"))
                File.Delete("temp.tcproj");
        }

        [TestCase("my.test.assembly.dll")]
        [TestCase("one.dll", "two.dll", "three.dll")]
        [TestCase("tests.nunit")]
        public void PackageContainsOneSubPackagePerTestFile(params string[] testFiles)
        {
            _model.CreateNewProject(testFiles);

            Assert.That(_model.TestCentricProject.TestFiles, Is.EqualTo(testFiles));
        }

        // TODO: Remove? Use and test fluent methods?
        [TestCase(nameof(SettingDefinitions.RequestedRuntimeFramework), "net-2.0")]
        [TestCase(nameof(SettingDefinitions.MaxAgents), 8)]
        [TestCase(nameof(SettingDefinitions.ShadowCopyFiles), false)]
        public void PackageReflectsPackageSettings(string key, object value)
        {
            var package = _model.CreateNewProject(new[] { "my.dll" });
            package.AddSetting(key, value);

            Assert.That(package.Settings.HasSetting(key));
            Assert.That(package.Settings.GetSetting(key), Is.EqualTo(value));
        }

        // TODO: Remove? Use and test fluent methods?
        [Test]
        public void PackageReflectsTestParameters()
        {
            var testParms = new Dictionary<string, string>
            {
                { "parm1", "value1" },
                { "parm2", "value2" }
            };
            var package = _model.CreateNewProject(new[] { "my.dll" });
            package.AddSetting("TestParametersDictionary", testParms);

            Assert.That(package.Settings.HasSetting("TestParametersDictionary"));
            var parms = package.Settings.GetSetting("TestParametersDictionary") as IDictionary<string, string>;
            Assert.That(parms, Is.Not.Null);

            Assert.That(parms, Contains.Key("parm1"));
            Assert.That(parms, Contains.Key("parm2"));
            Assert.That(parms["parm1"], Is.EqualTo("value1"));
            Assert.That(parms["parm2"], Is.EqualTo("value2"));
        }

        [Test]
        public void DisplayShadowCopySettings()
        {
            Console.WriteLine($"Current AppDomain has ShadowCopyFiles = {AppDomain.CurrentDomain.SetupInformation.ShadowCopyFiles}");
        }

        [Test]
        public void DisplayTestParameters()
        {
            Console.WriteLine("Test Parameters for this run:");
            foreach (string name in TestContext.Parameters.Names)
                Console.WriteLine($"{name}={TestContext.Parameters[name]}");
        }

        [TestCase("my.dll")]
        [TestCase("my.sln")]
        [TestCase("my.dll,my.sln")]
        [TestCase("my.sln,my.dll")]
        [TestCase("my.sln,another.sln")]
        public void PackageForSolutionFileHasSkipNonTestAssemblies(string files)
        {
            _model.CreateNewProject(files.Split(','));
            string skipKey = SettingDefinitions.SkipNonTestAssemblies.Name;

            foreach (var subpackage in _model.TestCentricProject.SubPackages)
            {
                if (subpackage.Name.EndsWith(".sln"))
                {
                    Assert.That(subpackage.Settings.HasSetting(skipKey));
                    Assert.That(subpackage.Settings.GetSetting(skipKey), Is.True);
                }
                else
                    Assert.That(subpackage.Settings.HasSetting(skipKey), Is.False);
            }
        }


        [Test]
        public void NewProject_IsNotDirty()
        {
            var project = new TestCentricProject(_model, new[] { "dummy.dll" });
            Assert.That(project.IsDirty, Is.False);
        }

        public void NewProjectIsNotDirtyAfterSaving()
        {
            var project = new TestCentricProject(_model, new[] { "dummy.dll" });
            project.SaveAs("temp.tcproj");
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void LoadedProjectIsNotDirty()
        {
            var project = new TestCentricProject(_model);
            project.SaveAs("temp.tcproj");
            project.Load("temp.tcproj");
            Assert.That(project.IsDirty, Is.False);
        }

        [Test]
        public void AddingSubProjectMakesProjectDirty()
        {
            var project = _model.CreateNewProject(new[] { "dummy.dll" });
            project.SaveAs("temp.tcproj");
            project.AddSubPackage("another.dll");
            Assert.That(project.IsDirty);
        }

        [Test]
        public void AddingSettingMakesProjectDirty()
        {
            var project = _model.CreateNewProject(new[] { "dummy.dll" });
            project.SaveAs("temp.tcproj");
            project.AddSetting("NewSetting", "VALUE");
            Assert.That(project.IsDirty);
        }

        [Test]
        public void RemoveSubPackage_MakesProjectDirty()
        {
            var project = _model.CreateNewProject(new[] { "dummy.dll", "dummy2.dll" });
            project.SaveAs("temp.tcproj");

            var subPackage = project.SubPackages[0];
            project.RemoveSubPackage(subPackage);
            Assert.That(project.IsDirty);
        }

        [Test]
        public void RemoveSubPackage_PackagesIsDecreased()
        {
            var project = _model.CreateNewProject(new[] { "dummy.dll", "dummy2.dll" });
            project.SaveAs("temp.tcproj");

            var subPackage = project.SubPackages[0];
            project.RemoveSubPackage(subPackage);
            Assert.That(project.SubPackages.Count, Is.EqualTo(1));
        }
    }
}
