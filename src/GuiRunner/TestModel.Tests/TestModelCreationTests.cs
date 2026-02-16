// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using NUnit.Framework;
using TestCentric.Gui.Model.Fakes;
using NUnit.Engine;

using PackageSettings = NUnit.Engine.PackageSettings;
using SettingDefinitions = NUnit.Common.SettingDefinitions;

namespace TestCentric.Gui.Model
{
    public class TestModelCreationTests
    {
        [TestCase("dummy.dll")]
        [TestCase("dummy.dll", "--work=/Path/To/Directory")]
        [TestCase("dummy.dll", "--trace=Off")]
        [TestCase("dummy.dll", "--trace=Error", "--work=/Path/To/Directory")]
        [TestCase("dummy.dll", "--trace=Warning")]
        [TestCase("dummy.dll", "--trace=Info", "--work=/Path/To/Directory")]
        [TestCase("dummy.dll", "--trace=Debug")]
        [TestCase("dummy.dll", "--work=/Some/Directory", "--agents:32")]
        [TestCase("dummy.dll", "--agents:5")]
        [TestCase("dummy.dll", "--X86")]
        [TestCase("dummy.dll", "--param:X=5")]
        [TestCase("dummy.dll", "--param:X=5", "-p:Y=7")]
        public void CreateTestModel(params string[] args)
        {
            ITestEngine engine = new MockTestEngine();
            var options = new GuiOptions(args);
            var model = TestModel.CreateTestModel(engine, options);

            Assert.That(model, Is.Not.Null, "Unable to create TestModel");

            Assert.That(engine.WorkDirectory, Is.EqualTo(options.WorkDirectory));
            Assert.That(engine.InternalTraceLevel.ToString(), Is.EqualTo(options.InternalTraceLevel ?? "Off"));

            model.CreateNewProject("MyProject", options);
            var checker = new PackageSettingsChecker(model.TopLevelPackage.Settings);

            checker.CheckSetting(options.MaxAgents, SettingDefinitions.MaxAgents.Name);
            checker.CheckSetting(options.RunAsX86, SettingDefinitions.RunAsX86.Name);

            if (options.TestParameters.Count > 0)
            {
                string[] parms = new string[options.TestParameters.Count];
                int index = 0;
                foreach (string key in options.TestParameters.Keys)
                    parms[index++] = $"{key}={options.TestParameters[key]}";

                checker.CheckSetting(options.TestParameters, SettingDefinitions.TestParametersDictionary.Name);
            }
        }

        private class PackageSettingsChecker
        {
           PackageSettings _settings;

            public PackageSettingsChecker(PackageSettings settings)
            {
                _settings = settings;
            }

            public void CheckSetting(bool option, string key)
            {
                if (option || _settings.HasSetting(key))
                {
                    //Assert.That(_settings, Contains.Key(key));
                    Assert.That(_settings.GetSetting(key), Is.EqualTo(option));
                }
            }

            public void CheckSetting<T>(T option, string key)
            {
                if (option != null)
                {
                    Assert.That(_settings.HasSetting(key));
                    Assert.That(_settings.GetSetting(key), Is.EqualTo(option));
                }
                else
                    Assert.That(_settings.HasSetting(key), Is.False);
            }
        }
    }
}
