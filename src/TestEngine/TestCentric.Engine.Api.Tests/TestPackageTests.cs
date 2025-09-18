// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NUnit.Framework;

namespace TestCentric.Engine.Api
{
    [TestFixtureSource(nameof(FixtureData))]
    public class TestPackageTests
    {
        private TestPackage _package;
        private string _expectedXml;

        private const string HEADER = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";

        public TestPackageTests(TestPackage package, string expectedXml)
        {
            _package = package;
            _expectedXml = expectedXml;
        }

        [Test]
        public void TestPackageIsXmlSerializable()
        {
            Assert.That(_package, Is.XmlSerializable);
        }

        [Test]
        public void PackageIDsAreUnique()
        {
            string[] fileNames = _package.SubPackages.Select(p => p.FullName).ToArray();
            var anotherPackage = new TestPackage(fileNames);
            Assert.That(anotherPackage.ID, Is.Not.EqualTo(_package.ID));
        }

        [Test]
        public void TopLevelPackageIsAnonymous()
        {
            Assert.That(_package.Name, Is.Null);
            Assert.That(_package.FullName, Is.Null);
        }

        [Test]
        public void TopLevelPackageHasSubPackages()
        {
            Assert.That(_package.HasSubPackages);
        }

        [Test]
        public void TopLevelPackageIsNotAnAssembly()
        {
            Assert.That(_package.IsAssemblyPackage, Is.False);
        }

        [Test]
        public void CanSelectPackages()
        {
            var selection = _package.Select(p => p.IsAssemblyPackage);

            Assert.That(selection.Count, Is.GreaterThan(0));
            foreach (var package in selection)
                Assert.That(package.IsAssemblyPackage);
        }

        [Test]
        public void CanSerializePackage()
        {
            StringWriter writer = new StringWriter();
            // Creating our own XmlWriter makes the test simpler. Using
            // the StringWriter directly would introduce the XML declaration
            // and format the output into multiple indented lines.
            XmlWriter xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings() { OmitXmlDeclaration = true });
            XmlSerializer serializer = new XmlSerializer(typeof(TestPackage));
            serializer.Serialize(xmlWriter, _package);

            Assert.That(writer.ToString(), Is.EqualTo(_expectedXml));
        }

        [Test]
        public void CanDeserializePackage()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(TestPackage));
            StringReader reader = new StringReader(_expectedXml);
            TestPackage newPackage = (TestPackage)serializer.Deserialize(reader);

            CheckPackage(newPackage, _package);
        }

        [Test]
        public void RoundTrip()
        {
            // This test performs a round trip serializing and then
            // deserializing the original package using default settings.
            XmlSerializer serializer = new XmlSerializer(typeof(TestPackage));

            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, _package);

            StringReader reader = new StringReader(writer.ToString());
            TestPackage newPackage = (TestPackage)serializer.Deserialize(reader);

            CheckPackage(newPackage, _package);
        }

        private void CheckPackage(TestPackage actual, TestPackage expected)
        {
            Assert.That(actual.ID, Is.EqualTo(expected.ID));
            Assert.That(actual.FullName, Is.EqualTo(expected.FullName));
            foreach (PackageSetting setting in expected.Settings)
            {
                Assert.That(actual.Settings.HasSetting(setting.Name), Is.True);
                Assert.That(actual.Settings.GetSetting(setting.Name), Is.EqualTo(setting.Value));
            }

            Assert.That(actual.SubPackages.Count, Is.EqualTo(expected.SubPackages.Count));

            for (int i = 0; i < expected.SubPackages.Count; i++)
                CheckPackage(actual.SubPackages[i], expected.SubPackages[i]);
        }

        static IEnumerable<TestFixtureData> FixtureData()
        {
            TestPackage package;

            package = new TestPackage("test.dll");
            yield return new TestFixtureData(package, GetExpectedXml(package)) { TestName = "TestPackageTests(Single Assembly, no Settings)" };

            package = new TestPackage("test.dll");
            package.AddSetting("StringSetting", "xyz");
            package.AddSetting("IntSetting", 123);
            yield return new TestFixtureData(package, GetExpectedXml(package)) { TestName = "TestPackageTests(Single Assembly with Settings)" };

            package = new TestPackage("test1.dll", "test2.dll", "test3.dll");
            yield return new TestFixtureData(package, GetExpectedXml(package)) { TestName = "TestPackageTests(Three Assemblies, no Settings)" };

            package = new TestPackage("test1.dll", "test2.dll", "test3.dll");
            package.AddSetting("StringSetting", "xyz");
            package.AddSetting("IntSetting", 123);
            package.AddSetting("BoolSetting", true);
            package.SubPackages[0].AddSetting("Comment", "This is test1");
            package.SubPackages[1].AddSetting("Comment", "This is test2");
            package.SubPackages[2].AddSetting("Comment", "This is test3");
            yield return new TestFixtureData(package, GetExpectedXml(package)) { TestName = "TestPackageTests(Three Assemblies with Settings)" };

            package = new TestPackage("test1.dll", "project.nunit");
            package.SubPackages[1].AddSubPackage("test2.dll");
            package.SubPackages[1].AddSubPackage("test3.dll");
            yield return new TestFixtureData(package, GetExpectedXml(package)) { TestName = "TestPackageTests(One Assembly and One Project))" };
        }

        private static string GetExpectedXml(TestPackage package)
        {
            int nextId = int.Parse(package.ID);

            var sb = new StringBuilder();
            sb.Append($"<TestPackage id=\"{package.ID}\"");
            if (package.FullName != null)
                sb.Append($" fullname=\"{package.FullName}\"");

            if (package.Settings.Count == 0 && package.SubPackages.Count == 0)
            {
                sb.Append(" />");
            }
            else
            {
                sb.Append(">");

                if (package.Settings.Count > 0)
                {
                    // Terminate TestPackage and start Settings
                    sb.Append("<Settings");
                    foreach (var setting in package.Settings)
                        sb.Append($" {setting.Name}=\"{setting.Value}\"");
                    sb.Append(" />");
                }

                foreach (TestPackage subPackage in package.SubPackages)
                    sb.Append(GetExpectedXml(subPackage));

                sb.Append("</TestPackage>");
            }

            return sb.ToString();
        }
    }

    public class AddAndRemoveTestPackageTests
    {
        [Test]
        public void AddSubPackage()
        {
            var package = new TestPackage("test.dll");
            Assert.That(package.SubPackages.Count, Is.EqualTo(1));
            package.AddSubPackage("another.dll");
            Assert.That(package.SubPackages.Count, Is.EqualTo(2));
            Assert.That(package.SubPackages[1].Name, Is.EqualTo("another.dll"));
        }

        [Test]
        public void RemoveSubPackage()
        {
            var package = new TestPackage(new[] { "test1.dll", "test2.dll" });
            package.SubPackages.Remove(package.SubPackages[0]);
            Assert.That(package.SubPackages.Count, Is.EqualTo(1));
            Assert.That(package.SubPackages[0].Name, Is.EqualTo("test2.dll"));
        }

        [Test]
        public void RemoveSubPackageAt()
        {
            var package = new TestPackage(new[] { "test1.dll", "test2.dll" });
            package.SubPackages.RemoveAt(0);
            Assert.That(package.SubPackages.Count, Is.EqualTo(1));
            Assert.That(package.SubPackages[0].Name, Is.EqualTo("test2.dll"));
        }
    }
}
