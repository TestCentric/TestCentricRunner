public static class PackageTests
{
    // Tests run for the each package
    public static List<PackageTest> GuiTests = new List<PackageTest>();
    public static List<PackageTest> EngineTests = new List<PackageTest>();

    // Define Package Tests
    //   Level 1 tests are run each time we build the packages
    //   Level 2 tests are run for PRs and when packages will be published
    //   Level 3 tests are run only when publishing a release

    static PackageTests()
    {
        var GuiAndEngineTests = new ListWrapper(GuiTests, EngineTests);

        // Tests of single assemblies targeting each runtime we support

        GuiAndEngineTests.Add(new PackageTest(1, "Net462Test", "Run net462 mock-assembly.dll under .NET 4.6.2",
            "net462/mock-assembly.dll",
            MockAssemblyExpectedResult("Net462AgentLauncher")));

        GuiAndEngineTests.Add(new PackageTest(1, "Net35Test", "Run net35 mock-assembly.dll under .NET 4.6.2",
        "net35/mock-assembly.dll",
            MockAssemblyExpectedResult("Net462AgentLauncher")));

        GuiAndEngineTests.Add(new PackageTest(1, "Net462X86Test", "Run net462 mock-assembly-x86.dll under .NET 4.6.2",
            "net462/mock-assembly-x86.dll",
            MockAssemblyX86ExpectedResult("Net462AgentLauncher")));

        GuiAndEngineTests.Add(new PackageTest(1, "Net35X86Test", "Run net35 mock-assembly-x86.dll under .NET 4.6.2",
        "net35/mock-assembly-x86.dll",
            MockAssemblyX86ExpectedResult("Net462AgentLauncher")));

        if (BuildSettings.IsLocalBuild)
        {
            GuiAndEngineTests.Add(new PackageTest(1, "NetCore21Test", "Run .NET Core 2.1 mock-assembly.dll under .NET Core 3.1",
                "netcoreapp2.1/mock-assembly.dll",
                MockAssemblyExpectedResult("Net80AgentLauncher")));

            GuiAndEngineTests.Add(new PackageTest(1, "NetCore31Test", "Run mock-assembly.dll under .NET Core 3.1",
                "netcoreapp3.1/mock-assembly.dll",
                MockAssemblyExpectedResult("Net80AgentLauncher")));

            GuiAndEngineTests.Add(new PackageTest(1, "Net50Test", "Run mock-assembly.dll under .NET 5.0",
                "net5.0/mock-assembly.dll",
                MockAssemblyExpectedResult("Net80AgentLauncher")));
        }

        GuiAndEngineTests.Add(new PackageTest(1, "Net60Test", "Run mock-assembly.dll under .NET 6.0",
            "net6.0/mock-assembly.dll",
            MockAssemblyExpectedResult("Net80AgentLauncher")));

        GuiAndEngineTests.Add(new PackageTest(1, "Net70Test", "Run mock-assembly.dll under .NET 7.0",
            "net7.0/mock-assembly.dll",
            MockAssemblyExpectedResult("Net80AgentLauncher")));

        GuiAndEngineTests.Add(new PackageTest(1, "Net80Test", "Run mock-assembly.dll under .NET 8.0",
            "net8.0/mock-assembly.dll",
            MockAssemblyExpectedResult("Net80AgentLauncher")));

        // AspNetCore tests

        if (BuildSettings.IsLocalBuild)
        {
            GuiAndEngineTests.Add(new PackageTest(1, "AspNetCore31Test", "Run test using AspNetCore under .NET Core 3.1",
                "netcoreapp3.1/aspnetcore-test.dll",
                new ExpectedResult("Passed")
                {
                    Assemblies = new[] { new ExpectedAssemblyResult("aspnetcore-test.dll", "Net80AgentLauncher") }
                }));

            GuiAndEngineTests.Add(new PackageTest(1, "AspNetCore50Test", "Run test using AspNetCore under .NET 5.0",
                "net5.0/aspnetcore-test.dll",
                new ExpectedResult("Passed")
                {
                    Assemblies = new[] { new ExpectedAssemblyResult("aspnetcore-test.dll", "Net80AgentLauncher") }
                }));
        }

        GuiAndEngineTests.Add(new PackageTest(1, "AspNetCore60Test", "Run test using AspNetCore under .NET 6.0",
            "net6.0/aspnetcore-test.dll",
            new ExpectedResult("Passed")
            {
                Assemblies = new[] { new ExpectedAssemblyResult("aspnetcore-test.dll", "Net80AgentLauncher") }
            }));

        GuiAndEngineTests.Add(new PackageTest(1, "AspNetCore70Test", "Run test using AspNetCore under .NET 7.0",
            "net7.0/aspnetcore-test.dll",
            new ExpectedResult("Passed")
            {
                Assemblies = new[] { new ExpectedAssemblyResult("aspnetcore-test.dll", "Net80AgentLauncher") }
            }));

        GuiAndEngineTests.Add(new PackageTest(1, "AspNetCore80Test", "Run test using AspNetCore under .NET 8.0",
            "net8.0/aspnetcore-test.dll",
            new ExpectedResult("Passed")
            {
                Assemblies = new[] { new ExpectedAssemblyResult("aspnetcore-test.dll", "Net80AgentLauncher") }
            }));

        // Windows Forms Tests

        //if (BuildSettings.IsLocalBuild)
        //    GuiAndEngineTests.Add(new PackageTest(1, "Net50WindowsFormsTest", "Run test using windows forms under .NET 5.0",
        //        "net5.0-windows/windows-forms-test.dll",
        //        new ExpectedResult("Passed")
        //        {
        //            Assemblies = new[] { new ExpectedAssemblyResult("windows-forms-test.dll", "Net80AgentLauncher") }
        //        }));

        GuiAndEngineTests.Add(new PackageTest(1, "Net60WindowsFormsTest", "Run test using windows forms under .NET 6.0",
            "net6.0-windows/windows-forms-test.dll",
            new ExpectedResult("Passed")
            {
                Assemblies = new[] { new ExpectedAssemblyResult("windows-forms-test.dll", "Net80AgentLauncher") }
            }));

        GuiAndEngineTests.Add(new PackageTest(1, "Net70WindowsFormsTest", "Run test using windows forms under .NET 7.0",
            "net7.0-windows/windows-forms-test.dll",
            new ExpectedResult("Passed")
            {
                Assemblies = new[] { new ExpectedAssemblyResult("windows-forms-test.dll", "Net80AgentLauncher") }
            }));

        GuiAndEngineTests.Add(new PackageTest(1, "Net80WindowsFormsTest", "Run test using windows forms under .NET 8.0",
            "net8.0-windows/windows-forms-test.dll",
            new ExpectedResult("Passed")
            {
                Assemblies = new[] { new ExpectedAssemblyResult("windows-forms-test.dll", "Net80AgentLauncher") }
            }));

        // Multiple assembly tests

        GuiAndEngineTests.Add(new PackageTest(1, "Net462PlusNet35Test", "Run .NET 4.6.2 and .NET 3.5 builds of mock-assembly.dll together",
            "net462/mock-assembly.dll net35/mock-assembly.dll",
            MockAssemblyExpectedResult("Net462AgentLauncher", "Net462AgentLauncher")));

        GuiAndEngineTests.Add(new PackageTest(1, "Net462PlusNet60Test", "Run .NET 4.6.2 and .NET 6.0 builds of mock-assembly.dll together",
            "net462/mock-assembly.dll net6.0/mock-assembly.dll",
            MockAssemblyExpectedResult("Net462AgentLauncher", "Net80AgentLauncher")));

        // TODO: Suppress V2 tests until driver is working
        //GuiTests.Add(new PackageTest(1, "NUnitV2Test", "Run mock-assembly.dll built for NUnit V2",
        //	"v2-tests/mock-assembly.dll",
        //	new ExpectedResult("Failed")
        //	{
        //		Total = 28,
        //		Passed = 18,
        //		Failed = 5,
        //		Warnings = 0,
        //		Inconclusive = 1,
        //		Skipped = 4
        //	},
        //	EngineExtensions.NUnitV2Driver));

        // TODO: Use --config option when it's supported by the extension.
        // Current test relies on the fact that the Release config appears
        // first in the project file.
        // Completely suppressed for the time being
        //if (BuildSettings.Configuration == "Release")
        //{
        //    GuiTests.Add(new PackageTest(1, "NUnitProjectTest", "Run an NUnit project",
        //        "../../TestProject.nunit --trace:Debug",
        //        MockAssemblyExpectedResult(
        //            "Net462AgentLauncher", "Net462AgentLauncher", "Net80AgentLauncher", "Net80AgentLauncher"),
        //        NUnitProjectLoader));
        //}

        ExpectedResult MockAssemblyExpectedResult(params string[] agentNames)
        {
            int ncopies = agentNames.Length;

            var assemblies = new ExpectedAssemblyResult[ncopies];
            for (int i = 0; i < ncopies; i++)
                assemblies[i] = new ExpectedAssemblyResult("mock-assembly.dll", agentNames[i]);

            return new ExpectedResult("Failed")
            {
                Total = 42 * ncopies,
                Passed = 22 * ncopies,
                Failed = 7 * ncopies,
                Warnings = 1 * ncopies,
                Inconclusive = 5 * ncopies,
                Skipped = 7 * ncopies,
                Assemblies = assemblies
            };
        }

        ExpectedResult MockAssemblyX86ExpectedResult(params string[] agentNames)
        {
            int ncopies = agentNames.Length;

            var assemblies = new ExpectedAssemblyResult[ncopies];
            for (int i = 0; i < ncopies; i++)
                assemblies[i] = new ExpectedAssemblyResult("mock-assembly-x86.dll", agentNames[i]);

            return new ExpectedResult("Failed")
            {
                Total = 31 * ncopies,
                Passed = 18 * ncopies,
                Failed = 5 * ncopies,
                Warnings = 0 * ncopies,
                Inconclusive = 1 * ncopies,
                Skipped = 7 * ncopies,
                Assemblies = assemblies
            };
        }
    }

    class ListWrapper
    {
        private List<PackageTest>[] _lists;

        public ListWrapper(params List<PackageTest>[] lists)
        {
            _lists = lists;
        }

        public void Add(PackageTest test)
        {
            foreach (var list in _lists)
                list.Add(test);
        }
    }

}
