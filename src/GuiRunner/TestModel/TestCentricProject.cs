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
    using System.Xml;
    using NUnit;
    using TestCentric.Gui.Model.Settings;

    public class TestCentricProject
    {
        public static bool IsProjectFile(string path) => Path.GetExtension(path).ToLower() == ".tcproj";

        public string ProjectPath { get; private set; }

        public TestPackage TopLevelPackage { get; private set; }

        public IList<String> TestFiles { get; } = new List<String>();

        #region Construction and Loading

        // NOTE: Originally, it was possible to construct a project in memory without specifying
        // the ProjectPath. The selection of a location was postponed until the project was saved.
        // Because of our decision to save the project and visual state with behind the scenes,
        // with as little user intervention as possible, we now require a valid ProjectPath for
        // every project. This allows us to save the project whenever an event occurs that 
        // could otherwise cause the loss of data. The VisualState is saved in the same directory
        // whenever the project is saved. With this approach, it must not be possible to create
        // a new project without specifying a ProjectPath.

        /// <summary>
        /// Construct a new project, specifying the ProjectPath and TestFiles.
        /// </summary>
        /// <param name="projectPath"></param>
        /// <param name="testFiles"></param>
        public TestCentricProject(string projectPath, params string[] testFiles)
        {
            Guard.ArgumentNotNullOrEmpty(projectPath, nameof(projectPath));

            ProjectPath = projectPath;
            TestFiles = [.. testFiles];
            TopLevelPackage = new TestPackage(testFiles);

            // TODO: This should not be in the constructor. Move elsewhere.
            ////Turn on shadow copy in new TestCentric project by default
            //AddSetting(SettingDefinitions.ShadowCopyFiles.WithValue(true));

            // TODO: Policy decisions should be at a higher level. Handling
            // setting definition for .sln files is definitely a policy decision
            // but it's not clear where the check for .tcproj belongs, so it
            // is kept here for the time being.
            foreach (var subpackage in TopLevelPackage.SubPackages)
                switch (Path.GetExtension(subpackage.Name))
                {
                    //case ".sln":
                    //    subpackage.AddSetting(SettingDefinitions.SkipNonTestAssemblies.WithValue(true));
                    //    break;
                    case ".tcproj":
                        throw new InvalidOperationException("A TestCentric project may not contain another TestCentric project.");
                }
        }

        /// <summary>
        /// Construct a new project, specifying the ProjectPath and GuiOptions.
        /// </summary>
        /// <param name="projectPath"></param>
        /// <param name="options"></param>
        public TestCentricProject(string projectPath, GuiOptions options)
        {
            Guard.ArgumentNotNullOrEmpty(projectPath, nameof(projectPath));

            ProjectPath = projectPath;
            TestFiles = [.. options.InputFiles];
            TopLevelPackage = new TestPackage(TestFiles);

            // TODO: Default setting needs to be moved elsewhere
            //// Turn on shadow copy in new TestCentric project by default
            //AddSetting(SettingDefinitions.ShadowCopyFiles.WithValue(true));

            if (options != null) // Happens when we test
            {
                AddSetting(SettingDefinitions.InternalTraceLevel.WithValue(options.InternalTraceLevel ?? "Off"));
                if (options.WorkDirectory != null)
                    AddSetting(SettingDefinitions.WorkDirectory.WithValue(options.WorkDirectory));
                if (options.MaxAgents >= 0)
                    SetTopLevelSetting(SettingDefinitions.MaxAgents.WithValue(options.MaxAgents));
                if (options.RunAsX86)
                    AddSetting(SettingDefinitions.RunAsX86.WithValue(true));
                if (options.DebugAgent)
                    SetTopLevelSetting(SettingDefinitions.DebugAgent.WithValue(true));
                if (options.TestParameters.Count > 0)
                    SetTopLevelSetting(SettingDefinitions.TestParametersDictionary.WithValue(options.TestParameters));
            }

            foreach (var subpackage in TopLevelPackage.SubPackages)
                switch (Path.GetExtension(subpackage.Name))
                {
                    case ".sln":
                        subpackage.AddSetting(SettingDefinitions.SkipNonTestAssemblies.WithValue(true));
                        break;
                    case ".tcproj":
                        throw new InvalidOperationException("A TestCentric project may not contain another TestCentric project.");
                }
        }

        // Empty project used by some tests
        // TODO: Phase this out?
        public TestCentricProject()
        {
            TestFiles = [];
            TopLevelPackage = new TestPackage();
        }

        public static TestCentricProject LoadFrom(string path)
        {
            var project = new TestCentricProject();
            project.Load(path);
            return project;
        }

        private void Load(string path)
        {
            ProjectPath = path;

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                StringReader reader = new StringReader(xmlDoc.OuterXml);
                XmlReader xmlReader = XmlReader.Create(reader);

                if (!FindTestCentricProjectElement())
                    throw new InvalidCastException("Invalid TestCentricProject XML");

                // Currently, we have no attributes on the TestCentricProject element
                // so proceed tot he next item in the reader, it will either be a TestPackage
                // or the TestCentricProject end element.
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xmlReader.Name == "TestPackage")
                                TopLevelPackage = ReadXml(xmlReader);
                            else
                                throw new Exception($"Invalid element: `{xmlReader.Name}`.");
                            break;
                        case XmlNodeType.EndElement:
                            if (xmlReader.Name == "TestCentricProject")
                                break;
                            else
                                throw new Exception($"Invalid end element '{xmlReader.Name}'");
                    }
                }

                // Update the list of test files
                TestFiles.Clear();
                foreach (TestPackage subPackage in TopLevelPackage.SubPackages)
                    TestFiles.Add(subPackage.FullName);

                bool FindTestCentricProjectElement()
                {
                    while (xmlReader.Read())
                        if (xmlReader.NodeType == XmlNodeType.Element)
                            return xmlReader.Name == "TestCentricProject";

                    return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to load TestProject from {path}", ex);
            }
        }

        #endregion

        public void SaveAs(string projectPath)
        {
            ProjectPath = projectPath;
            Save();
        }

        public void Save()
        {
            using (StreamWriter writer = new StreamWriter(ProjectPath))
            using (XmlWriter xmlWriter = XmlWriter.Create(writer))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("TestCentricProject");
                if (TopLevelPackage is not null)
                   xmlWriter.WriteRaw(TopLevelPackage.ToXml());
                xmlWriter.WriteEndElement();
            }
        }

        public void AddSubPackage(string fullName)
        {
            TopLevelPackage.AddSubPackage(fullName);
            TestFiles.Add(fullName);
        }
        public void AddSubPackage(TestPackage subPackage) => TopLevelPackage.AddSubPackage(subPackage);

        public void RemoveSubPackage(TestPackage subPackage)
        {
            if (subPackage != null)
            {
                TopLevelPackage.SubPackages.Remove(subPackage);
                TestFiles.Remove(subPackage.FullName);
            }
        }

        public void AddSetting(PackageSetting setting) => TopLevelPackage.AddSetting(setting);

        public void AddSetting(string key, string value) => TopLevelPackage.AddSetting(key, value);

        public void AddSetting(string key, bool value) => TopLevelPackage.AddSetting(key, value);

        public void AddSetting(string key, int value) => TopLevelPackage.AddSetting(key, value);

        public void ApplySetting(PackageSetting setting)
        {
            RemoveSetting(setting.Name);
            TopLevelPackage.AddSetting(setting);
        }

        public void RemoveSetting(string key)
        {
            TopLevelPackage.Settings.Remove(key);
            foreach (var subPackage in TopLevelPackage.SubPackages)
                subPackage.Settings.Remove(key);
        }

        public void RemoveSetting(SettingDefinition setting) => RemoveSetting(setting.Name);

        public void SetTopLevelSetting(PackageSetting setting) => TopLevelPackage.Settings.Set(setting);

        #region NUnit TestPackage Load Helpers

        // These two methods are private in NUnit and have been extracted for temporary use.
        // If NUnit can support their public use we'll be able to remove them.

        private static TestPackage ReadXml(XmlReader xmlReader)
        {
            TestPackage testPackage = new TestPackage();
            testPackage.ID = xmlReader.GetAttribute("id").ShouldNotBeNull("xmlReader.GetAttribute(\"id\")");
            testPackage.FullName = xmlReader.GetAttribute("fullname");
            if (!xmlReader.IsEmptyElement)
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            {
                                string name = xmlReader.Name;
                                if (!(name == "Settings"))
                                {
                                    if (name == "TestPackage")
                                    {
                                        testPackage.SubPackages.Add(ReadXml(xmlReader));
                                    }
                                }
                                else
                                {
                                    ReadSettings(testPackage.Settings, xmlReader);
                                }
                                break;
                            }
                        case XmlNodeType.EndElement:
                            if (xmlReader.Name == "TestPackage")
                            {
                                return testPackage;
                            }
                            throw new Exception("Unexpected EndElement: " + xmlReader.Name);
                    }
                }
                throw new Exception("Invalid XML: TestPackage Element not terminated.");
            }
            return testPackage;
        }

        private static void ReadSettings(PackageSettings packageSettings, XmlReader xmlReader)
        {
            while (xmlReader.MoveToNextAttribute())
            {
                string name = xmlReader.Name;
                string value = xmlReader.Value;
                SettingDefinition settingDefinition = SettingDefinitions.Lookup(name);
                bool result2;
                int result3;
                if (settingDefinition != null)
                {
                    switch (name)
                    {
                        case "LOAD":
                            packageSettings.Add(SettingDefinitions.LOAD.WithValue(value.Split(';')));
                            continue;
                        case "TestParametersDictionary":
                            {
                                XmlDocument xmlDocument = new XmlDocument();
                                xmlDocument.LoadXml(value);
                                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                                foreach (XmlNode item in xmlDocument.SelectNodes("parms/parm").ShouldNotBeNull("doc.SelectNodes(\"parms/parm\")"))
                                {
                                    dictionary.Add(item.GetAttribute("key").ShouldNotBeNull("node.GetAttribute(\"key\")"), item.GetAttribute("value").ShouldNotBeNull("node.GetAttribute(\"value\")"));
                                }
                                packageSettings.Add(SettingDefinitions.TestParametersDictionary.WithValue(dictionary));
                                continue;
                            }
                        case "InternalTraceWriter":
                            continue;
                    }
                    Type valueType = settingDefinition.ValueType;
                    if ((object)valueType == null)
                    {
                        continue;
                    }
                    if (valueType.IsAssignableFrom(typeof(string)))
                    {
                        packageSettings.Add(name, value);
                        continue;
                    }
                    Type type = valueType;
                    if (type.IsAssignableFrom(typeof(bool)))
                    {
                        packageSettings.Add(name, bool.Parse(value));
                        continue;
                    }
                    Type type2 = valueType;
                    if (type2.IsAssignableFrom(typeof(int)))
                    {
                        packageSettings.Add(name, int.Parse(value));
                    }
                }
                else if (bool.TryParse(value, out result2))
                {
                    packageSettings.Add(name, result2);
                }
                else if (int.TryParse(value, out result3))
                {
                    packageSettings.Add(name, result3);
                }
                else
                {
                    packageSettings.Add(name, value);
                }
            }
            xmlReader.MoveToElement();
        }

        #endregion
    }
}
