// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using NUnit.Common;
using NUnit.Engine;

namespace TestCentric.Gui.Model
{
    public class TestCentricProject : NUnit.Engine.TestPackage
    {
        private ITestModel _model;

        public static bool IsProjectFile(string path) => Path.GetExtension(path).ToLower() == ".tcproj";

        public string FileName => Path.GetFileName(ProjectPath);
        public string ProjectPath { get; private set; }

        public IList<String> TestFiles { get; private set; }

        public bool IsDirty { get; private set; }

        public TestCentricProject(ITestModel model)
        {
            _model = model;
            TestFiles = new List<String>();
        }

        public TestCentricProject(ITestModel model, string filename)
            : this(model, new string[] { filename }) { }

        public TestCentricProject(ITestModel model, IList<string> filenames)
            :base(filenames)
        {
            _model = model;
            TestFiles = new List<string>(filenames);

            var engineSettings = _model.Settings.Engine;
            var options = model.Options;

            if (engineSettings.Agents > 0)
                AddSetting(SettingDefinitions.MaxAgents.WithValue(engineSettings.Agents));
            if (engineSettings.SetPrincipalPolicy)
                AddSetting(SettingDefinitions.PrincipalPolicy.WithValue(engineSettings.PrincipalPolicy));
            AddSetting(SettingDefinitions.ShadowCopyFiles.WithValue(engineSettings.ShadowCopyFiles));

            if (options != null) // Happens when we test
            {
                AddSetting(SettingDefinitions.InternalTraceLevel.WithValue(options.InternalTraceLevel ?? "Off"));
                if (options.WorkDirectory != null)
                    AddSetting(SettingDefinitions.WorkDirectory.WithValue(options.WorkDirectory));
                if (options.MaxAgents >= 0)
                    AddSetting(SettingDefinitions.MaxAgents.WithValue(options.MaxAgents));
                if (options.RunAsX86)
                    AddSetting(SettingDefinitions.RunAsX86.WithValue(true));
                if (options.DebugAgent)
                    AddSetting(SettingDefinitions.DebugAgent.WithValue(true));
                if (options.SimulateUnloadError)
                    AddSetting("SimulateUnloadError", true);
                if (options.SimulateUnloadTimeout)
                    AddSetting("SimulateUnloadError", true);
                if (options.TestParameters.Count > 0)
                {
                    string[] parms = new string[options.TestParameters.Count];
                    int index = 0;
                    foreach (string key in options.TestParameters.Keys)
                        parms[index++] = $"{key}={options.TestParameters[key]}";

                    AddSetting("TestParametersDictionary", options.TestParameters);
                    AddSetting("TestParameters", string.Join(";", parms));
                }
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

        public TestCentricProject()
        {

        }

        public void Load(string path)
        {
            ProjectPath = path;

            using (StreamReader reader = new StreamReader(ProjectPath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(NUnit.Engine.TestPackage));

                try
                {
                    var newPackage = (NUnit.Engine.TestPackage)serializer.Deserialize(reader);

                    foreach (var subPackage in newPackage.SubPackages)
                    {
                        AddSubPackage(subPackage.FullName);
                    }

                    LoadTests();
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to deserialize TestProject.", ex);
                }
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
        }

        //public void Save(TextWriter writer)
        //{
        //    XmlSerializer serializer = new XmlSerializer(typeof(NUnit.Engine.TestPackage));

        //    try
        //    {
        //        serializer.Serialize(writer, this);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Unable to serialize TestProject.", ex);
        //    }

        //    IsDirty = false;
        //}

        public void LoadTests()
        {
            _model.LoadTests(TestFiles);
        }

        public void UnloadTests()
        {

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
        }

        public void RemoveSetting(NUnit.Engine.SettingDefinition setting) => RemoveSetting(setting.Name);
    }
}
