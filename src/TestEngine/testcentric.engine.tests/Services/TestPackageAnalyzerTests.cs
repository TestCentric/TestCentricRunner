// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric contributors.
// Licensed under the MIT License. See LICENSE file in root directory.
// ***********************************************************************

#if !NETCOREAPP2_1
using NUnit.Engine;
using NUnit.Framework;
using NSubstitute;

using SettingDefinitions = NUnit.Common.SettingDefinitions;

namespace TestCentric.Engine.Services
{
    public class TestPackageAnalyzerTests
    {
        // NOTE: Tests are a bit fragile, since we can't inject a 
        // current framework to the validator. Need to find a way
        // to fake current framework for better tests.
        private const string VALID_RUNTIME = "net-4.0";
        private const string INVALID_RUNTIME = "invalid-5.0";
        //private static readonly string CURRENT_RUNTIME = RuntimeFramework.CurrentFramework.Id;

        private NUnit.Engine.TestPackage _package;
        private TestPackageAnalyzer _analyzer;

        public interface ITestRuntimeService : IRuntimeFrameworkService, IService { }

        [SetUp]
        public void Initialize()
        {
            // Validation doesn't look at the files specified, only settings
            _package = new NUnit.Engine.TestPackage("any.dll");

            var runtimeService = Substitute.For<ITestRuntimeService>();
            runtimeService.IsAvailable("net-2.0", false).Returns(true);
            runtimeService.IsAvailable("net-4.0", false).Returns(true);
            runtimeService.IsAvailable("net-4.5", false).Returns(true);
            //runtimeService.IsAvailable(CURRENT_RUNTIME).Returns(true);
            runtimeService.IsAvailable("netcore-3.0", false).Returns(true); // Not actually available yet, but used to test

            var context = new ServiceContext();
            context.Add(runtimeService);
            context.Add(Substitute.For<NUnit.Engine.Services.ProjectService>());

            context.Add(new TestPackageAnalyzer());
            
            context.ServiceManager.StartServices();

            _analyzer = context.GetService<TestPackageAnalyzer>();
        }

        [Test]
        public void RequestedFrameworkNotSpecified()
        {
            Assert.That(() => Validate(), Throws.Nothing);
        }

        [Test]
        public void RequestedFrameworkInvalid()
        {
            _package.AddSetting(SettingDefinitions.RequestedRuntimeFramework.WithValue(INVALID_RUNTIME));

            var exception = Assert.Throws<NUnitEngineException>(() => Validate());

            CheckMessageContent(exception.Message, $"The requested framework {INVALID_RUNTIME} is unknown or not available.");
        }

        [Test]
        public void AllPossibleErrors()
        {
            _package.AddSetting(SettingDefinitions.RequestedRuntimeFramework.WithValue(INVALID_RUNTIME));

            var exception = Assert.Throws<NUnitEngineException>(() => Validate());

            CheckMessageContent(exception.Message,
                $"The requested framework {INVALID_RUNTIME} is unknown or not available.");
        }

        [Test]
        public void RequestedFrameworkValid()
        {
            _package.AddSetting(SettingDefinitions.RequestedRuntimeFramework.WithValue(VALID_RUNTIME));
            Assert.That(() => Validate(), Throws.Nothing);
        }

        private void Validate()
        {
            _analyzer.ValidatePackageSettings(_package);
        }

        private void CheckMessageContent(string message, params string[] errors)
        {
            Assert.That(message, Does.StartWith("The following errors were detected in the TestPackage:\n\n"));

            foreach (string error in errors)
                Assert.That(message, Contains.Substring($"\n* {error}\n"));
        }
    }
}
#endif
