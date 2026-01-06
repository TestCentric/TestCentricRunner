// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Common;
using NUnit.Engine;

namespace TestCentric.Gui.Model
{
    using System.Linq;

    public class TestCentricProject : NUnit.Engine.TestPackage
    {

        public static bool IsProjectFile(string path) => Path.GetExtension(path).ToLower() == ".tcproj";

        public string FileName => Path.GetFileName(ProjectPath);
        public string ProjectPath { get; private set; }

        public IList<String> TestFiles { get; private set; }

        public bool IsDirty { get; private set; }

        public TestCentricProject(ITestModel model)
        {
            TestFiles = new List<String>();
        }

        public TestCentricProject(ITestModel model, string filename)
            : this(model, new string[] { filename }) { }

        public TestCentricProject(ITestModel model, IList<string> filenames)
            :base(filenames)
        {
            TestFiles = new List<string>(filenames);

            // Turn on shadow copy in new TestCentric project by default
            SetSubPackageSetting(SettingDefinitions.ShadowCopyFiles.WithValue(true));

            var options = model.Options;
            if (options != null) // Happens when we test
            {
                SetSubPackageSetting(SettingDefinitions.InternalTraceLevel.WithValue(options.InternalTraceLevel ?? "Off"));
                if (options.WorkDirectory != null)
                    SetSubPackageSetting(SettingDefinitions.WorkDirectory.WithValue(options.WorkDirectory));
                if (options.MaxAgents >= 0)
                    SetTopLevelSetting(SettingDefinitions.MaxAgents.WithValue(options.MaxAgents));
                if (options.RunAsX86)
                    SetTopLevelSetting(SettingDefinitions.RunAsX86.WithValue(true));
                if (options.DebugAgent)
                    SetSubPackageSetting(SettingDefinitions.DebugAgent.WithValue(true));
                if (options.TestParameters.Count > 0)
                    SetTopLevelSetting(SettingDefinitions.TestParametersDictionary.WithValue(options.TestParameters));
            }

            foreach (var subpackage in SubPackages)
                switch(Path.GetExtension(subpackage.Name))
                {
                    case ".sln":
                        subpackage.AddSetting(SettingDefinitions.SkipNonTestAssemblies.WithValue(true));
                        break;
                    case ".tcproj":
                        throw new InvalidOperationException("A TestCentric project may not contain another TestCentric project.");
                }

            IsDirty = false;
        }

        public void Load(string path)
        {
            ProjectPath = path;

            try
            {
                string fileContent = File.ReadAllText(ProjectPath);
                TestPackage newPackage = PackageHelper.FromXml(fileContent);

                // Apply top level settings from loaded package
                foreach (PackageSetting packageSetting in newPackage.Settings)
                    Settings.Set(packageSetting);

                foreach (var subPackage in newPackage.SubPackages)
                {
                    AddSubPackage(subPackage.FullName);
                    foreach (var setting in subPackage.Settings)
                        SubPackages.Last().Settings.Set(setting);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to load TestProject from {path}", ex);
            }


            IsDirty = false;
        }

        public void SaveAs(string projectPath)
        {
            ProjectPath = projectPath;
            Save();
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(ProjectPath))
                writer.Write(this.ToXml());

            IsDirty = false;
        }

        public new void AddSubPackage(string fullName)
        {
            base.AddSubPackage(fullName);
            TestFiles.Add(fullName);
            IsDirty = true;
        }
        public new void AddSubPackage(NUnit.Engine.TestPackage subPackage)
        {
            base.AddSubPackage(subPackage);
            IsDirty = true;
        }

        public void RemoveSubPackage(NUnit.Engine.TestPackage subPackage)
        {
            if (subPackage != null)
            {
                SubPackages.Remove(subPackage);
                TestFiles.Remove(subPackage.FullName);
                IsDirty = true;
            }
        }

        public void SetSubPackageSetting(PackageSetting setting)
        {
            RemoveSetting(setting.Name);
            AddSetting(setting);
            IsDirty = true;
        }

        public void AddSetting(string key, object value)
        {
            Settings.Set(key, value);
            IsDirty = true;
        }

        public void RemoveSetting(string key)
        {
            Settings.Remove(key);
            foreach (var subPackage in SubPackages)
                subPackage.Settings.Remove(key);

            IsDirty = true;
        }

        public void RemoveSetting(NUnit.Engine.SettingDefinition setting) => RemoveSetting(setting.Name);

        public void SetTopLevelSetting(PackageSetting setting)
        {
            Settings.Set(setting);
            IsDirty = true;
        }
    }
}
