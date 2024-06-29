//////////////////////////////////////////////////////////////////////
// PACKAGING METHODS AND CLASSES
//////////////////////////////////////////////////////////////////////

var RootFiles = new string[]
{
    "LICENSE.txt",
    "NOTICES.txt",
    "CHANGES.txt"
};

var baseFiles = new string[]
{
    "testcentric.exe",
    "testcentric.exe.config",
    "TestCentric.Common.dll",
    "TestCentric.Gui.Components.dll",
    "TestCentric.Gui.Runner.dll",
    "nunit.uiexception.dll",
    "TestCentric.Gui.Model.dll",
    "nunit.engine.api.dll",
    "testcentric.engine.metadata.dll",
    "testcentric.engine.core.dll",
    "testcentric.engine.dll",
};

var PdbFiles = new string[]
{
    "testcentric.pdb",
    "TestCentric.Common.pdb",
    "TestCentric.Gui.Components.pdb",
    "TestCentric.Gui.Runner.pdb",
    "nunit.uiexception.pdb",
    "TestCentric.Gui.Model.pdb",
    //"testcentric.engine.metadata.pdb",
    "testcentric.engine.core.pdb",
    "testcentric.engine.pdb",
};

private void CreateImage(BuildParameters parameters)
{
	string imageDir = parameters.ImageDirectory;
	string imageBinDir = imageDir + "bin/";

    CreateDirectory(imageDir);
    CleanDirectory(imageDir);

	CopyFiles(RootFiles, imageDir);

	var copyFiles = new List<string>(baseFiles);
	if (!parameters.UsingXBuild)
		copyFiles.AddRange(PdbFiles);

	CreateDirectory(imageBinDir);

	foreach (string file in copyFiles)
		CopyFileToDirectory(parameters.OutputDirectory + file, imageBinDir);

	CopyDirectory(parameters.OutputDirectory + "Images", imageBinDir + "Images");

	foreach (var runtime in parameters.SupportedAgentRuntimes)
    {
        var targetDir = imageBinDir + "agents/" + Directory(runtime);
        var sourceDir = parameters.OutputDirectory + "agents/" + Directory(runtime);
        CopyDirectory(sourceDir, targetDir);
	}

	// NOTE: Files specific to a particular package are not copied
	// into the image directory but are added separately.
}

private void PushNuGetPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	NuGetPush(package, new NuGetPushSettings() { ApiKey=apiKey, Source=url });
}

private void PushChocolateyPackage(FilePath package, string apiKey, string url)
{
	CheckPackageExists(package);
	ChocolateyPush(package, new ChocolateyPushSettings() { ApiKey=apiKey, Source=url });
}

private void CheckPackageExists(FilePath package)
{
	if (!FileExists(package))
		throw new InvalidOperationException(
			$"Package not found: {package.GetFilename()}.\nCode may have changed since package was last built.");
}
