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

    public class TestCentricProject
    {
        public static bool IsProjectFile(string path) => Path.GetExtension(path).ToLower() == ".tcproj";

        public string ProjectPath { get; private set; }

        public TestPackage TopLevelPackage { get; private set; }

        public IList<String> TestFiles { get; }

        public bool IsDirty { get; private set; }

        public TestCentricProject(GuiOptions options = null)
        {
            if (options is null)
                options = new GuiOptions();

            TestFiles = [.. options.InputFiles];
            TopLevelPackage = new TestPackage(TestFiles);

            // Turn on shadow copy in new TestCentric project by default
            AddSetting(SettingDefinitions.ShadowCopyFiles.WithValue(true));

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

            IsDirty = false;
        }

        public void Load(string path)
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
            using (XmlWriter xmlWriter = XmlWriter.Create(writer))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("TestCentricProject");
                if (TopLevelPackage is not null)
                   xmlWriter.WriteRaw(TopLevelPackage.ToXml());
                xmlWriter.WriteEndElement();
            }

            IsDirty = false;
        }

        public void AddSubPackage(string fullName)
        {
            TopLevelPackage.AddSubPackage(fullName);
            TestFiles.Add(fullName);
            IsDirty = true;
        }
        public void AddSubPackage(TestPackage subPackage)
        {
            TopLevelPackage.AddSubPackage(subPackage);
            IsDirty = true;
        }

        public void RemoveSubPackage(TestPackage subPackage)
        {
            if (subPackage != null)
            {
                TopLevelPackage.SubPackages.Remove(subPackage);
                TestFiles.Remove(subPackage.FullName);
                IsDirty = true;
            }
        }

        public void AddSetting(PackageSetting setting)
        {
            TopLevelPackage.AddSetting(setting);
            IsDirty = true;
        }

        public void AddSetting(string key, string value)
        {
            TopLevelPackage.AddSetting(key, value);
            IsDirty = true;
        }

        public void AddSetting(string key, bool value)
        {
            TopLevelPackage.AddSetting(key, value);
            IsDirty = true;
        }

        public void AddSetting(string key, int value)
        {
            TopLevelPackage.AddSetting(key, value);
            IsDirty = true;
        }

        public void RemoveSetting(string key)
        {
            TopLevelPackage.Settings.Remove(key);
            foreach (var subPackage in TopLevelPackage.SubPackages)
                subPackage.Settings.Remove(key);

            IsDirty = true;
        }

        public void RemoveSetting(SettingDefinition setting) => RemoveSetting(setting.Name);

        public void SetTopLevelSetting(PackageSetting setting)
        {
            TopLevelPackage.Settings.Set(setting);
            IsDirty = true;
        }

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
