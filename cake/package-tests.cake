
//////////////////////////////////////////////////////////////////////
// TESTING HELPER METHODS
//////////////////////////////////////////////////////////////////////

static void CheckTestErrors(ref List<string> errorDetail)
{
    if(errorDetail.Count != 0)
    {
        var copyError = new List<string>();
        copyError = errorDetail.Select(s => s).ToList();
        errorDetail.Clear();
        throw new Exception("One or more tests failed, breaking the build.\r\n"
                              + copyError.Aggregate((x,y) => x + "\r\n" + y));
    }
}

private void RunNUnitLite(string testName, string framework, string directory)
{
	bool isDotNetCore = framework.StartsWith("netcoreapp");
	string ext = isDotNetCore ? ".dll" : ".exe";
	string testPath = directory + testName + ext;

	Information("==================================================");
	Information("Running tests under " + framework);
	Information("==================================================");

	int rc = isDotNetCore
		? StartProcess("dotnet", testPath)
		: StartProcess(testPath);

	if (rc > 0)
		ErrorDetail.Add($"{testName}: {rc} tests failed running under {framework}");
	else if (rc < 0)
		ErrorDetail.Add($"{testName} returned rc = {rc} running under {framework}");
}

// Representation of a single test to be run against a pre-built package.
// Each test has a Level, with the following values defined...
//  0 Do not run - used for temporarily disabling a test
//  1 Run for all CI tests - that is every time we test packages
//  2 Run only on PRs, dev builds and when publishing
//  3 Run only when publishing
public struct PackageTest
{
    public int Level;
    public string Description;
    public string Arguments;
    public ExpectedResult ExpectedResult;
    public string[] ExtensionsNeeded;

    public PackageTest(int level, string description, string arguments, ExpectedResult expectedResult, params string[] extensionsNeeded)
    {
        if (description == null)
            throw new ArgumentNullException(nameof(description));
        if (arguments == null)
            throw new ArgumentNullException(nameof(arguments));
        if (expectedResult == null)
            throw new ArgumentNullException(nameof(expectedResult));

        Level = level;
        Description = description;
        Arguments = arguments;
        ExpectedResult = expectedResult;
        ExtensionsNeeded = extensionsNeeded;
    }
}

const string DEFAULT_TEST_RESULT_FILE = "TestResult.xml";

// Abstract base for all package testers.
public abstract class PackageTester
{
    protected BuildParameters _parameters;
    private ICakeContext _context;

    public PackageTester(BuildParameters parameters)
    {
        _parameters = parameters;
        _context = parameters.SetupContext;

        PackageTests = new List<PackageTest>();

        //Level 1 tests are run each time we build the packages
        PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting .NET 4.6.2",
            "engine-tests/net462/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 36,
                Passed = 23,
                Failed = 5,
                Warnings = 1,
                Inconclusive = 1,
                Skipped = 7,
                Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "Net462AgentLauncher") }
            }));

        PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting .NET 3.5",
            "engine-tests/net35/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 36,
                Passed = 23,
                Failed = 5,
                Warnings = 1,
                Inconclusive = 1,
                Skipped = 7,
                Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "Net462AgentLauncher") }
            }));

        PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting .NET Core 2.1",
            "engine-tests/netcoreapp2.1/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 36,
                Passed = 23,
                Failed = 5,
                Warnings = 1,
                Inconclusive = 1,
                Skipped = 7,
                Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "NetCore31AgentLauncher") }
            }));

        PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting .NET Core 3.1",
            "engine-tests/netcoreapp3.1/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 36,
                Passed = 23,
                Failed = 5,
                Warnings = 1,
                Inconclusive = 1,
                Skipped = 7,
                Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "NetCore31AgentLauncher") }
            }));

        PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting .NET Core 1.1",
            "engine-tests/netcoreapp1.1/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 36,
                Passed = 23,
                Failed = 5,
                Warnings = 1,
                Inconclusive = 1,
                Skipped = 7,
                Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "NetCore31AgentLauncher") }
            }));

        PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting .NET 5.0",
            "engine-tests/net5.0/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 32,
                Passed = 19,
                Failed = 5,
                Warnings = 1,
                Inconclusive = 1,
                Skipped = 7,
                Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "Net50AgentLauncher") }
            }));

        PackageTests.Add(new PackageTest(1, "Run different builds of mock-assembly.dll together",
            "engine-tests/net35/mock-assembly.dll engine-tests/netcoreapp2.1/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 72,
                Passed = 46,
                Failed = 10,
                Warnings = 2,
                Inconclusive = 2,
                Skipped = 14,
                Assemblies = new[] {
                            new ExpectedAssemblyResult("mock-assembly.dll", "Net462AgentLauncher"),
                            new ExpectedAssemblyResult("mock-assembly.dll", "NetCore31AgentLauncher") }
            }));

        //		// Level 2 tests are run for PRs and when packages will be published

        //		//PackageTests.Add(new PackageTest(2, "Run mock-assembly.dll built for NUnit V2"
        //		//	"v2-tests/mock-assembly.dll",
        //		//	new ExpectedResult("Failed")
        //		//	{
        //		//		Total = 28,
        //		//		Passed = 18,
        //		//		Failed = 5,
        //		//		Warnings = 0,
        //		//		Inconclusive = 1,
        //		//		Skipped = 4
        //		//	},
        //		//	NUnitV2Driver));

        // TODO: Use --config option when it's supported by the extension.
        // Current test relies on the fact that the Release config appears
        // first in the project file.
        // TODO: Reinstate this when extension is updated to work.
        if (_parameters.Configuration == "Release")
        {
            PackageTests.Add(new PackageTest(1, "Run an NUnit project",
                "TestProject.nunit --trace",
                new ExpectedResult("Failed")
                {
                    Assemblies = new[] {
                                    new ExpectedAssemblyResult("mock-assembly.dll", "Net462AgentLauncher"),
                                    new ExpectedAssemblyResult("mock-assembly.dll", "Net462AgentLauncher"),
                                    new ExpectedAssemblyResult("mock-assembly.dll", "NetCore31AgentLauncher"),
                                    new ExpectedAssemblyResult("mock-assembly.dll", "Net50AgentLauncher") }
                },
                NUnitProjectLoader));
        }

        // NOTE: Package tests using a pluggable agent must be run after all tests
        // that assume no pluggable agents are installed!

        PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting net35 using Net20PluggableAgent",
            "engine-tests/net35/mock-assembly.dll",
            new ExpectedResult("Failed")
            {
                Total = 36,
                Passed = 23,
                Failed = 5,
                Warnings = 1,
                Inconclusive = 1,
                Skipped = 7,
                Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "Net20AgentLauncher") }
            },
            Net20PluggableAgent));

        // TODO: NetCore21PluggableAgent is not yet available
        //PackageTests.Add(new PackageTest(1, "Run mock-assembly.dll targeting Net Core 2.1 using NetCore21PluggableAgent",
        //    "engine-tests/netcoreapp2.1/mock-assembly.dll --trace",
        //    new ExpectedResult("Failed")
        //    {
        //        Total = 36,
        //        Passed = 23,
        //        Failed = 5,
        //        Warnings = 1,
        //        Inconclusive = 1,
        //        Skipped = 7,
        //        Assemblies = new[] { new ExpectedAssemblyResult("mock-assembly.dll", "NetCore21AgentLauncher") }
        //    },
        //    NetCore21PluggableAgent));
    }

    protected abstract string PackageName { get; }
    protected abstract FilePath PackageUnderTest { get; }
    protected abstract string PackageTestDirectory { get; }
    protected abstract string PackageTestBinDirectory { get; }
    protected abstract string ExtensionInstallDirectory { get; }

    protected virtual string NUnitV2Driver => "NUnit.Extension.NUnitV2Driver";
    protected virtual string NUnitProjectLoader => "NUnit.Extension.NUnitProjectLoader";
    protected virtual string Net20PluggableAgent => "NUnit.Extension.Net20PluggableAgent";
    protected virtual string NetCore21PluggableAgent => "NUnit.Extension.NetCore21PluggableAgent";

    // NOTE: Currently, we use the same tests for all packages. There seems to be
    // no reason for the three packages to differ in capability so the only reason
    // to limit tests on some of them would be efficiency... so far not a problem.
    private List<PackageTest> PackageTests { get; }

    public void RunAllTests()
    {
        Console.WriteLine("Testing package " + PackageName);

        RunPackageTests(_parameters.PackageTestLevel);

        CheckTestErrors(ref ErrorDetail);
    }

    private void CheckExtensionIsInstalled(string extension)
    {
        bool alreadyInstalled = _context.GetDirectories($"{ExtensionInstallDirectory}{extension}.*").Count > 0;

        if (!alreadyInstalled)
        {
            DisplayBanner($"Installing {extension}");
            InstallEngineExtension(extension);
        }
    }

    protected abstract void InstallEngineExtension(string extension);

    private void RunPackageTests(int testLevel)
    {
        //string pathToRunner = _parameters.OutputDirectory + TEST_BED_EXE;
        _context.CopyFileToDirectory(
            _parameters.OutputDirectory + TEST_BED_EXE, 
            PackageTestBinDirectory);

        var reporter = new ResultReporter(PackageName);

        foreach (var packageTest in PackageTests)
        {
            if (packageTest.Level > 0 && packageTest.Level <= testLevel)
            {
                foreach (string extension in packageTest.ExtensionsNeeded)
                    CheckExtensionIsInstalled(extension);

                var resultFile = _parameters.OutputDirectory + DEFAULT_TEST_RESULT_FILE;
                // Delete result file ahead of time so we don't mistakenly
                // read a left-over file from another test run. Leave the
                // file after the run in case we need it to debug a failure.
                if (_context.FileExists(resultFile))
                    _context.DeleteFile(resultFile);

                DisplayBanner(packageTest.Description);

                try
                {
                    Console.WriteLine($"Launching {PackageTestBinDirectory}{TEST_BED_EXE}");
                    _context.StartProcess(PackageTestBinDirectory + TEST_BED_EXE, new ProcessSettings()
                    {
                        Arguments = packageTest.Arguments,
                        WorkingDirectory = _parameters.OutputDirectory
                    });

                    var result = new ActualResult(resultFile);
                    var report = new PackageTestReport(packageTest, result);                    reporter.AddReport(report);

                    Console.WriteLine(report.Errors.Count == 0
                        ? "\nSUCCESS: Test Result matches expected result!"
                        : "\nERROR: Test Result not as expected!");
                }
                catch (Exception ex)
                {
                    reporter.AddReport(new PackageTestReport(packageTest, ex));

                    Console.WriteLine("\nERROR: Unexpected Exception thrown.");
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        bool anyErrors = reporter.ReportResults();
        Console.WriteLine();

        // All package tests are run even if one of them fails. If there are
        // any errors,  we stop the run at this point.
        if (anyErrors)
            throw new Exception("One or more package tests had errors!");
    }

    private void DisplayBanner(string message)
    {
        Console.WriteLine("\n=======================================================");
        Console.WriteLine(message);
        Console.WriteLine("=======================================================");
    }
}

public class NuGetPackageTester : PackageTester
{
    public NuGetPackageTester(BuildParameters parameters) : base(parameters) { }

    protected override string PackageName => _parameters.EnginePackageName;
    protected override FilePath PackageUnderTest => _parameters.EnginePackage;
    protected override string PackageTestDirectory => _parameters.NuGetTestDirectory;
    protected override string PackageTestBinDirectory => PackageTestDirectory + "tools/";
    protected override string ExtensionInstallDirectory => PackageTestBinDirectory + "addins";

    protected override void InstallEngineExtension(string extension)
    {
        _parameters.SetupContext.NuGetInstall(extension,
            new NuGetInstallSettings()
            {
                OutputDirectory = ExtensionInstallDirectory,
                Prerelease = true
            });
    }
}
