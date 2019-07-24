#tool nuget:?package=NUnit.ConsoleRunner&version=3.9.0
//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var packageVersion = Argument("packageVersion", "1.0.0");

//////////////////////////////////////////////////////////////////////
// SET PACKAGE VERSION
//////////////////////////////////////////////////////////////////////

var dash = packageVersion.IndexOf('-');
var version = dash > 0
    ? packageVersion.Substring(0, dash)
    : packageVersion;

if (configuration == "Debug")
    packageVersion += "-dbg";

//////////////////////////////////////////////////////////////////////
// DETERMINE BUILD ENVIRONMENT
//////////////////////////////////////////////////////////////////////

bool usingXBuild = EnvironmentVariable("USE_XBUILD") != null;

var msBuildSettings = new MSBuildSettings {
    Verbosity = Verbosity.Minimal,
    ToolVersion = MSBuildToolVersion.Default,//The highest available MSBuild tool version//VS2017
    Configuration = configuration,
    PlatformTarget = PlatformTarget.MSIL,
    MSBuildPlatform = MSBuildPlatform.Automatic,
    DetailedSummary = true,
};

var xBuildSettings = new XBuildSettings {
    Verbosity = Verbosity.Minimal,
    ToolVersion = XBuildToolVersion.Default,//The highest available XBuild tool version//NET40
    Configuration = configuration,
};

var nugetRestoreSettings = new NuGetRestoreSettings();
// Older Mono version was not picking up the testcentric source
if (usingXBuild)
    nugetRestoreSettings.Source = new string [] {
        "https://www.myget.org/F/testcentric/api/v2/",
        "https://www.myget.org/F/testcentric/api/v3/index.json",
        "https://www.nuget.org/api/v2/",
        "https://api.nuget.org/v3/index.json",
		"https://www.myget.org/F/nunit/api/v2/",
		"https://www.myget.org/F/nunit/api/v3/index.json"
    };

//////////////////////////////////////////////////////////////////////
// DEFINE RUN CONSTANTS
//////////////////////////////////////////////////////////////////////

// Directories
var PROJECT_DIR = Context.Environment.WorkingDirectory.FullPath + "/";
var PACKAGE_DIR = PROJECT_DIR + "package/";
var CHOCO_DIR = PROJECT_DIR + "choco/";
var BIN_DIR = PROJECT_DIR + "bin/" + configuration + "/";
var ENGINE_BIN_DIR = PROJECT_DIR + "src/TestEngine/bin/" + configuration + "/net20/";

// Packaging
var PACKAGE_NAME = "testcentric-gui";

// Solution
var SOLUTION = "testcentric-gui.sln";

// Test Assembly Pattern
var ALL_TESTS = BIN_DIR + "*.Tests.dll";

//////////////////////////////////////////////////////////////////////
// SETUP
//////////////////////////////////////////////////////////////////////
Setup(context =>
{
    if (BuildSystem.IsRunningOnAppVeyor)
    {
	    // The provided package version suffix is ignored when running
		// on AppVeyor and is determined from the type of build and the 
		// AppVeyor build number.

        var buildNumber = AppVeyor.Environment.Build.Number.ToString("00000");
        var branch = AppVeyor.Environment.Repository.Branch;
        var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;

        if (branch == "master" && !isPullRequest)
        {
            packageVersion = version + "-dev-" + buildNumber;
        }
        else
        {
            var suffix = "-ci-" + buildNumber;

            if (isPullRequest)
                suffix += "-pr-" + AppVeyor.Environment.PullRequest.Number;
            else if (AppVeyor.Environment.Repository.Branch.StartsWith("release", StringComparison.OrdinalIgnoreCase))
                suffix += "-pre-" + buildNumber;
            else
                suffix += "-" + System.Text.RegularExpressions.Regex.Replace(branch, "[^0-9A-Za-z-]+", "-");

            // Nuget limits "special version part" to 20 chars. Add one for the hyphen.
            if (suffix.Length > 21)
                suffix = suffix.Substring(0, 21);

            packageVersion = version + suffix;
        }

        AppVeyor.UpdateBuildVersion(packageVersion);
    }

    // Executed BEFORE the first task.
    Information("Building {0} version {1} of TestCentric GUI.", configuration, packageVersion);
});


//////////////////////////////////////////////////////////////////////
// CLEAN
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .Does(() =>
{
    CleanDirectory(BIN_DIR);
});

//////////////////////////////////////////////////////////////////////
// RESTORE NUGET PACKAGES
//////////////////////////////////////////////////////////////////////

Task("RestorePackages")
    .Does(() =>
{
    NuGetRestore(SOLUTION, nugetRestoreSettings);
});

//////////////////////////////////////////////////////////////////////
// BUILD
//////////////////////////////////////////////////////////////////////

Task("Build")
	.IsDependentOn("Clean")
    .IsDependentOn("RestorePackages")
    .Does(() =>
{
    if(usingXBuild)
    {
        XBuild(SOLUTION, xBuildSettings);
    }
    else
    {
        MSBuild(SOLUTION, msBuildSettings);
    }

	CopyFiles(ENGINE_BIN_DIR + "nunit-agent.*", BIN_DIR);
	CopyFiles(ENGINE_BIN_DIR + "nunit-agent-x86.*", BIN_DIR);

    CopyFileToDirectory("LICENSE.txt", BIN_DIR);
    CopyFileToDirectory("NOTICES.txt", BIN_DIR);
    CopyFileToDirectory("CHANGES.txt", BIN_DIR);
});

//////////////////////////////////////////////////////////////////////
// TEST
//////////////////////////////////////////////////////////////////////

Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
{
    NUnit3(ALL_TESTS, new NUnit3Settings {
        NoResults = true
        });
});

//////////////////////////////////////////////////////////////////////
// PACKAGING
//////////////////////////////////////////////////////////////////////

var baseFiles = new string[]
{
    BIN_DIR + "LICENSE.txt",
    BIN_DIR + "NOTICES.txt",
    BIN_DIR + "CHANGES.txt",
    BIN_DIR + "testcentric.exe",
    BIN_DIR + "testcentric.exe.config",
    BIN_DIR + "tc-next.exe",
    BIN_DIR + "tc-next.exe.config",
    BIN_DIR + "TestCentric.Common.dll",
    BIN_DIR + "TestCentric.Gui.Components.dll",
    BIN_DIR + "TestCentric.Gui.Runner.dll",
    BIN_DIR + "Experimental.Gui.Runner.dll",
    BIN_DIR + "nunit.uiexception.dll",
    BIN_DIR + "TestCentric.Gui.Model.dll",
    BIN_DIR + "nunit.engine.api.dll",
    BIN_DIR + "nunit.engine.dll",
    BIN_DIR + "Mono.Cecil.dll",
    BIN_DIR + "nunit-agent.exe",
    BIN_DIR + "nunit-agent.exe.config",
    BIN_DIR + "nunit-agent-x86.exe",
    BIN_DIR + "nunit-agent-x86.exe.config"
};

var chocoFiles = new string[]
{
    CHOCO_DIR + "VERIFICATION.txt",
    CHOCO_DIR + "nunit-agent.exe.ignore",
    CHOCO_DIR + "nunit-agent-x86.exe.ignore",
    CHOCO_DIR + "nunit.choco.addins"
};

var pdbFiles = new string[]
{
    BIN_DIR + "testcentric.pdb",
    BIN_DIR + "tc-next.pdb",
    BIN_DIR + "TestCentric.Common.pdb",
    BIN_DIR + "TestCentric.Gui.Components.pdb",
    BIN_DIR + "TestCentric.Gui.Runner.pdb",
    BIN_DIR + "Experimental.Gui.Runner.pdb",
    BIN_DIR + "nunit.uiexception.pdb",
    BIN_DIR + "TestCentric.Gui.Model.pdb",
};

//////////////////////////////////////////////////////////////////////
// PACKAGE ZIP
//////////////////////////////////////////////////////////////////////

Task("PackageZip")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CreateDirectory(PACKAGE_DIR);

        var zipFiles = new List<string>(baseFiles);
        if (!usingXBuild)
            zipFiles.AddRange(pdbFiles);

        Zip(BIN_DIR, File(PACKAGE_DIR + PACKAGE_NAME + "-" + packageVersion + ".zip"), zipFiles);
    });

//////////////////////////////////////////////////////////////////////
// PACKAGE CHOCOLATEY
//////////////////////////////////////////////////////////////////////

Task("PackageChocolatey")
    .IsDependentOn("Build")
    .Does(() =>
    {
        CreateDirectory(PACKAGE_DIR);

        var content = new List<ChocolateyNuSpecContent>();
        var sources = usingXBuild
            ? new[] { baseFiles, chocoFiles }
            : new[] { baseFiles, chocoFiles, pdbFiles };

        foreach (var source in sources)
            foreach (string file in source)
                content.Add(new ChocolateyNuSpecContent() { Source = file, Target = "tools" });

        ChocolateyPack(CHOCO_DIR + PACKAGE_NAME + ".nuspec", 
            new ChocolateyPackSettings()
            {
                Version = packageVersion,
                OutputDirectory = PACKAGE_DIR,
                Files = content
            });
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Package")
    .IsDependentOn("PackageZip")
    .IsDependentOn("PackageChocolatey");

Task("Appveyor")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Package");

Task("Travis")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("PackageZip");

Task("All")
    .IsDependentOn("Build")
    .IsDependentOn("Test")
    .IsDependentOn("Package");

Task("Default")
    .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);
