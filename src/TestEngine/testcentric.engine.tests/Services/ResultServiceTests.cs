// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

using System;
using System.IO;
using NUnit.Engine;
using NUnit.Framework;

namespace TestCentric.Engine.Services
{
    public class ResultServiceTests
    {
        private ResultService _resultService;

        [SetUp]
        public void CreateService()
        {
            var services = new ServiceContext();
            services.Add(new ResultService());
            services.ServiceManager.StartServices();
            _resultService = services.GetService<ResultService>();
        }

        [Test]
        public void ServiceIsStarted()
        {
            Assert.That(_resultService.Status, Is.EqualTo(ServiceStatus.Started));
        }

        [Test]
        public void AvailableFormats()
        {
            Assert.That(_resultService.Formats, Is.EquivalentTo(new string[] { "nunit3", "cases", "user" }));
        }

        [TestCase("nunit3", null, ExpectedResult = "NUnit3XmlResultWriter")]
        //[TestCase("nunit2", null, ExpectedResult = "NUnit2XmlResultWriter")]
        [TestCase("cases", null, ExpectedResult = "TestCaseResultWriter")]
        //[TestCase("user", new object[] { "TextSummary.xslt" }, ExpectedResult = "XmlTransformResultWriter")]
        public string CanGetWriter(string format, object[] args)
        {
            var writer = _resultService.GetResultWriter(format, args);

            Assert.That(writer, Is.Not.Null);
            return writer.GetType().Name;
        }

#if !NETCOREAPP2_1
        [Test]
        public void CanGetWriterUser()
        {
            string actual = CanGetWriter("user", new object[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "TextSummary.xslt") });
            Assert.That(actual, Is.EqualTo("XmlTransformResultWriter"));
        }

        [Test]
        public void NUnit3Format_NonExistentTransform_ThrowsArgumentException()
        {
            Assert.That(
                () => _resultService.GetResultWriter("user", new object[] { "junk.xslt" }),
                Throws.ArgumentException);
        }

        [Test]
        public void NUnit3Format_NullArgument_ThrowsArgumentNullException()
        {
            Assert.That(
                () => _resultService.GetResultWriter("user", null),
                Throws.TypeOf<ArgumentNullException>());
        }
#endif
    }
}
