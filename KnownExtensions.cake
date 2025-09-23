using System.Reflection;

// Static class holding information about known extensions.
public static class KnownExtensions
{
    // Static Variables representing well-known Extensions with the latest tested version
    public static ExtensionSpecifier NUnitV2Driver = new ExtensionSpecifier(
        "NUnit.Extension.NUnitV2Driver", "nunit-extension-nunit-v2-driver", "3.9.0");
    public static ExtensionSpecifier NUnitProjectLoader = new ExtensionSpecifier(
        "NUnit.Extension.NUnitProjectLoader", "nunit-extension-nunit-project-loader", "4.0.0-dev00016");
    public static ExtensionSpecifier VSProjectLoader = new ExtensionSpecifier(
        "NUnit.Extension.VSProjectLoader", "nunit-extension-vs-project-loader", "3.9.0");
    public static ExtensionSpecifier NUnitV2ResultWriter = new ExtensionSpecifier(
        "NUnit.Extension.NUnitV2ResultWriter", "nunit-extension-nunit-v2-result-writer", "4.0.0-beta.1");
    public static ExtensionSpecifier Net20PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.Net20PluggableAgent", "nunit-extension-net20-pluggable-agent", "2.1.1");
    public static ExtensionSpecifier Net462PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.Net462PluggableAgent", "nunit-extension-net462-pluggable-agent", "4.1.0-alpha.3");
    public static ExtensionSpecifier NetCore21PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.NetCore21PluggableAgent", "nunit-extension-netcore21-pluggable-agent", "2.1.1");
    public static ExtensionSpecifier NetCore31PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.NetCore31PluggableAgent", "nunit-extension-netcore31-pluggable-agent", "2.1.0");
    public static ExtensionSpecifier Net50PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.Net50PluggableAgent", "nunit-extension-net50-pluggable-agent", "2.1.0");
    public static ExtensionSpecifier Net60PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.Net60PluggableAgent", "nunit-extension-net60-pluggable-agent", "2.1.0");
    public static ExtensionSpecifier Net70PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.Net70PluggableAgent", "nunit-extension-net70-pluggable-agent", "2.1.0");
    public static ExtensionSpecifier Net80PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.Net80PluggableAgent", "nunit-extension-net80-pluggable-agent", "4.1.0-alpha.4");
    public static ExtensionSpecifier Net90PluggableAgent = new ExtensionSpecifier(
        "NUnit.Extension.Net90PluggableAgent", "nunit-extension-net90-pluggable-agent", "4.1.0-alpha.3");

    private static FieldInfo[] ExtensionFields =>
        typeof(KnownExtensions).GetFields(BindingFlags.Static | BindingFlags.Public).ToArray();

    private static ExtensionSpecifier[] BundledAgents =>
    [
        Net462PluggableAgent,
        Net80PluggableAgent
    ];

    public static IEnumerable<PackageReference> BundledNuGetAgents =>
        BundledAgents.Select(a => a.NuGetPackage);

    public static IEnumerable<PackageReference> BundledChocolateyAgents =>
        BundledAgents.Select(a => a.ChocoPackage);

    public static IEnumerable<ExtensionSpecifier> AllExtensions =>
        ExtensionFields.Select(f => (ExtensionSpecifier)f.GetValue("Value")).ToArray();

    public static IEnumerable<ExtensionSpecifier> AllAgents =>
        AllExtensions.Where(ex => ex.NuGetId.EndsWith("PluggableAgent"));
}

Task("InstallBundledAgents")
    .Description("Installs just the agents we bundle with the GUI runner.")
    .Does(() =>
    {
        foreach (var agent in KnownExtensions.BundledNuGetAgents)
            agent.Install(BuildSettings.ProjectDirectory + BIN_DIR);
    });

Task("InstallAllAgents")
    .Description("Installs all known agents.")
    .Does(() =>
    {
        foreach (var agent in KnownExtensions.AllAgents)
            agent.NuGetPackage.Install(BuildSettings.ProjectDirectory + BIN_DIR);
    });

Task("InstallAllExtensions")
    .Description("Installs all known extensions, both agents and others.")
    .Does(() =>
    {
        foreach (var extension in KnownExtensions.AllExtensions)
            extension.NuGetPackage.Install(BuildSettings.ProjectDirectory + BIN_DIR);
    });

