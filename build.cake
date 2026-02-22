// Load the recipe
#load nuget:?package=TestCentric.Cake.Recipe&version=1.5.0-dev00010
// Comment out above line and uncomment below for local tests of recipe changes
//#load ../TestCentric.Cake.Recipe/recipe/*.cake

#load "./package-tests.cake"
#load "KnownExtensions.cake"

//////////////////////////////////////////////////////////////////////
// INITIALIZATION
//////////////////////////////////////////////////////////////////////

BuildSettings.Initialize(
	context: Context,
	title: "TestCentric.GuiRunner",
	solutionFile: "TestCentric-Gui.sln",
	githubRepository: "testcentric-gui",
	exemptFiles: ["Resource.cs", "TextCode.cs"]
);

//////////////////////////////////////////////////////////////////////
// COMMON DEFINITIONS USED IN BOTH PACKAGES
//////////////////////////////////////////////////////////////////////

static readonly FilePath[] GUI_FILES = {
        "testcentric.exe", "testcentric.exe.config", "nunit.uiexception.dll",
        "TestCentric.Gui.Runner.dll", "TestCentric.Gui.Model.dll" };
static readonly FilePath[] TREE_ICONS_PNG = {
        "Success.png", "Failure.png", "Warning.png", "Ignored.png", "Inconclusive.png", "Running.png", "Success_NotLatestRun.png", "Failure_NotLatestRun.png", "Warning_NotLatestRun.png", "Ignored_NotLatestRun.png", "Inconclusive_NotLatestRun.png", "Skipped.png" };

private const string GUI_DESCRIPTION =
	"The TestCentric Runner for NUnit (**TestCentric**) is a GUI runner aimed at eventually supporting a range of .NET testing frameworks. In the 1.x release series, we are concentrating on support of NUnit tests. The user interface is based on the layout and feature set of the of the original NUnit GUI, with the internals modified so as to run NUnit 3 tests." +
	"\r\n\nThis package includes the both the standard TestCentric GUI runner (`testcentric.exe`) and an experiental runner (`tc-next.exe`) which is available for... wait for it... experimentation! The package incorporates the TestCentric test engine, a modified version of the NUnit engine." +
	"\r\n\n### Features" +
	"\r\n\nMost features of the NUnit V2 Gui runner are supported. See CHANGES.txt for more detailed information." +
	"\r\n\nNUnit engine extensions are supported but no extensions are bundled with the GUI itself. They must be installed separately **using chocolatey**. In particular, to run NUnit V2 tests, you should install the **NUnit V2 Framework Driver Extension**." + 
	"\r\n\n**Warning:** When using the GUI chocolatey package, **only** chocolatey-packaged extensions will be available. This is by design." +
	"\r\n\n### Prerequisites" +
	"\r\n\n**TestCentric** requires .NET 4.5 or later in order to function, although your tests may run in a separate process under other framework versions." +
	"\r\n\nProjects with tests to be run under **TestCentric** must already have some version of the NUnit framework installed separtely.";

//////////////////////////////////////////////////////////////////////
// DEFINE PACKAGES
//////////////////////////////////////////////////////////////////////

var NuGetGuiPackage = new NuGetPackage(
	id: "TestCentric.GuiRunner",
	description: GUI_DESCRIPTION,
	packageContent: new PackageContent()
		.WithRootFiles("../../LICENSE.txt", "../../NOTICES.txt", "../../CHANGES.txt", "../../testcentric.png")
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"testcentric.exe", "testcentric.exe.config", "TestCentric.Gui.Runner.dll",
				"nunit.uiexception.dll", "TestCentric.Gui.Model.dll",
				"TestCentric.Metadata.dll", "NUnit.Extensibility.dll", "NUnit.Extensibility.Api.dll",
				"nunit.common.dll", "nunit.engine.api.dll", "nunit.engine.dll"),
			new DirectoryContent("tools/Images/Tree/Circles").WithFiles(
				"Images/Tree/Circles/Success.png", "Images/Tree/Circles/Failure.png", "Images/Tree/Circles/Warning.png", "Images/Tree/Circles/Ignored.png", "Images/Tree/Circles/Inconclusive.png", 
				"Images/Tree/Circles/Success_NotLatestRun.png", "Images/Tree/Circles/Failure_NotLatestRun.png", "Images/Tree/Circles/Warning_NotLatestRun.png", "Images/Tree/Circles/Ignored_NotLatestRun.png", "Images/Tree/Circles/Inconclusive_NotLatestRun.png", 
				"Images/Tree/Circles/Running.png", "Images/Tree/Circles/Skipped.png"),
			new DirectoryContent("tools/Images/Tree/Classic").WithFiles(
				"Images/Tree/Classic/Success.png", "Images/Tree/Classic/Failure.png", "Images/Tree/Classic/Warning.png", "Images/Tree/Classic/Ignored.png", "Images/Tree/Classic/Inconclusive.png",
				"Images/Tree/Classic/Success_NotLatestRun.png", "Images/Tree/Classic/Failure_NotLatestRun.png", "Images/Tree/Classic/Warning_NotLatestRun.png", "Images/Tree/Classic/Ignored_NotLatestRun.png", "Images/Tree/Classic/Inconclusive_NotLatestRun.png",
				"Images/Tree/Classic/Running.png", "Images/Tree/Classic/Skipped.png"),
			new DirectoryContent("tools/Images/Tree/Visual Studio").WithFiles(
				"Images/Tree/Visual Studio/Success.png", "Images/Tree/Visual Studio/Failure.png", "Images/Tree/Visual Studio/Warning.png", "Images/Tree/Visual Studio/Ignored.png", "Images/Tree/Visual Studio/Inconclusive.png", 
				"Images/Tree/Visual Studio/Success_NotLatestRun.png", "Images/Tree/Visual Studio/Failure_NotLatestRun.png", "Images/Tree/Visual Studio/Warning_NotLatestRun.png", "Images/Tree/Visual Studio/Ignored_NotLatestRun.png", "Images/Tree/Visual Studio/Inconclusive_NotLatestRun.png", 
				"Images/Tree/Visual Studio/Running.png",  "Images/Tree/Visual Studio/Skipped.png") )
		.WithDependencies( KnownExtensions.BundledNuGetAgents ),
    testRunner: new GuiSelfTester(BuildSettings.PackageTestDirectory + "TestCentric.GuiRunner/TestCentric.GuiRunner." + BuildSettings.PackageVersion + "/tools/testcentric.exe"),
	checks: new PackageCheck[] {
		HasFiles("CHANGES.txt", "LICENSE.txt", "NOTICES.txt", "testcentric.png"),
        HasDirectory("tools").WithFiles(GUI_FILES).AndFile("TestCentric.Metadata.dll"),
        HasDirectory("tools/Images/Tree/Circles").WithFiles(TREE_ICONS_PNG),
		HasDirectory("tools/Images/Tree/Classic").WithFiles(TREE_ICONS_PNG),
		HasDirectory("tools/Images/Tree/Visual Studio").WithFiles(TREE_ICONS_PNG)
	},
	tests: PackageTests.GuiTests
);

var ChocolateyGuiPackage = new ChocolateyPackage(
	id: "testcentric-gui",
	description: GUI_DESCRIPTION,
	packageContent: new PackageContent()
		.WithDirectories(
			new DirectoryContent("tools").WithFiles(
				"../../LICENSE.txt", "../../NOTICES.txt", "../../CHANGES.txt", "../../testcentric.png",
				"../../choco/VERIFICATION.txt",
				"../../choco/testcentric-agent.exe.ignore",	"../../choco/testcentric-agent-x86.exe.ignore",
				"testcentric.exe", "testcentric.exe.config", "TestCentric.Gui.Runner.dll",
				"nunit.uiexception.dll", "TestCentric.Gui.Model.dll", "nunit.engine.api.dll", "nunit.engine.dll",
				"TestCentric.Metadata.dll", "NUnit.Extensibility.dll", "NUnit.Extensibility.Api.dll", "NUnit.Common.dll"),
            new DirectoryContent("tools/Images/Tree/Circles").WithFiles(
                "Images/Tree/Circles/Success.png", "Images/Tree/Circles/Failure.png", "Images/Tree/Circles/Warning.png", "Images/Tree/Circles/Ignored.png", "Images/Tree/Circles/Inconclusive.png", 
				"Images/Tree/Circles/Success_NotLatestRun.png", "Images/Tree/Circles/Failure_NotLatestRun.png", "Images/Tree/Circles/Warning_NotLatestRun.png", "Images/Tree/Circles/Ignored_NotLatestRun.png", "Images/Tree/Circles/Inconclusive_NotLatestRun.png", 
				"Images/Tree/Circles/Running.png", "Images/Tree/Circles/Skipped.png"),
            new DirectoryContent("tools/Images/Tree/Classic").WithFiles(
                "Images/Tree/Classic/Success.png", "Images/Tree/Classic/Failure.png", "Images/Tree/Classic/Warning.png", "Images/Tree/Classic/Ignored.png", "Images/Tree/Classic/Inconclusive.png", 
				"Images/Tree/Classic/Success_NotLatestRun.png", "Images/Tree/Classic/Failure_NotLatestRun.png", "Images/Tree/Classic/Warning_NotLatestRun.png", "Images/Tree/Classic/Ignored_NotLatestRun.png", "Images/Tree/Classic/Inconclusive_NotLatestRun.png",
				"Images/Tree/Classic/Running.png", "Images/Tree/Classic/Skipped.png"),
            new DirectoryContent("tools/Images/Tree/Visual Studio").WithFiles(
                "Images/Tree/Visual Studio/Success.png", "Images/Tree/Visual Studio/Failure.png", "Images/Tree/Visual Studio/Warning.png", "Images/Tree/Visual Studio/Ignored.png", "Images/Tree/Visual Studio/Inconclusive.png", 
				"Images/Tree/Visual Studio/Success_NotLatestRun.png", "Images/Tree/Visual Studio/Failure_NotLatestRun.png", "Images/Tree/Visual Studio/Warning_NotLatestRun.png", "Images/Tree/Visual Studio/Ignored_NotLatestRun.png", "Images/Tree/Visual Studio/Inconclusive_NotLatestRun.png", 
				"Images/Tree/Visual Studio/Running.png", "Images/Tree/Visual Studio/Skipped.png"))
        .WithDependencies( KnownExtensions.BundledChocolateyAgents ),
    testRunner: new GuiSelfTester(BuildSettings.PackageTestDirectory + "testcentric-gui/testcentric-gui." + BuildSettings.PackageVersion + "/tools/testcentric.exe"),
	checks: new PackageCheck[] {
		HasDirectory("tools").WithFiles("CHANGES.txt", "LICENSE.txt", "NOTICES.txt", "VERIFICATION.txt").AndFiles(GUI_FILES).AndFile("TestCentric.Metadata.dll"),
        HasDirectory("tools/Images/Tree/Circles").WithFiles(TREE_ICONS_PNG),
		HasDirectory("tools/Images/Tree/Classic").WithFiles(TREE_ICONS_PNG),
		HasDirectory("tools/Images/Tree/Visual Studio").WithFiles(TREE_ICONS_PNG),
	},
	tests: PackageTests.GuiTests
);

BuildSettings.Packages.Add(NuGetGuiPackage);
BuildSettings.Packages.Add(ChocolateyGuiPackage);

//////////////////////////////////////////////////////////////////////
// PACKAGE TEST RUNNER
//////////////////////////////////////////////////////////////////////

public class GuiSelfTester : TestRunner, IPackageTestRunner
{
    private FilePath _executablePath;

    // NOTE: When constructed as an argument to BuildSettings.Initialize(),
    // the executable path is not yet known and should not be provided.
    public GuiSelfTester(string executablePath = null)
    {
        _executablePath = executablePath;
    }

    public int RunPackageTest(string arguments)
    {
        if (!arguments.Contains(" --run"))
            arguments += " --run";
        if (!arguments.Contains(" --unattended"))
            arguments += " --unattended";
        if (!arguments.Contains(" --full-gui"))
            arguments += " --full-gui";

        if (_executablePath == null)
            _executablePath = BuildSettings.OutputDirectory + "testcentric.exe";

        Console.WriteLine($"Running {_executablePath} with arguments {arguments}");
        return base.RunTest(_executablePath, arguments);
    }
}

//////////////////////////////////////////////////////////////////////
// ADDITIONAL TARGETS USED IN DEVELOPMENT
//////////////////////////////////////////////////////////////////////

Task("PackageNuGet")
	.IsDependentOn("Build")
	.Does(() =>
	{
		NuGetGuiPackage.BuildVerifyAndTest();
	});

Task("PackageChocolatey")
	.IsDependentOn("Build")
	.Does(() =>
	{
		ChocolateyGuiPackage.BuildVerifyAndTest();
	});

// The following task installs an individual extensions in the bin
// directory, where it will be found by both Debug and Release builds.
// You can modify or duplicate this task to install a different extension.
// Note that 'KnownExtensions.cake' contains additional installation targets.
Task("InstallNUnitV2ResultWriter")
	.Description("Installs just the NUnitV2ResultWriter. Modify as needed.")
	.Does(() =>
	{
		KnownExtensions.NUnitV2ResultWriter.NuGetPackage.Install(BuildSettings.ProjectDirectory + BIN_DIR);
	});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

Build.Run();
