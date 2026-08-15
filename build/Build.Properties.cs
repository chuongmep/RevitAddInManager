internal partial class Build
{
    private readonly string[] Projects =
    {
        "RevitAddinManager"
    };

    public const string InstallerProject = "Installer";

    public const string BuildConfiguration = "Release";
    public const string InstallerConfiguration = "Installer";

    private const string AddInBinPrefix = "AddIn";

    //Payload installed once for all Revit versions, staged by the add-in build under bin/
    private const string SharedBinFolder = "AddInShared";
    private const string ArtifactsFolder = "output";

    //Specify the path to the MSBuild.exe file here if you are not using VisualStudio
    private const string CustomMsBuildPath = @"C:\Program Files\JetBrains\JetBrains Rider\tools\MSBuild\Current\Bin\MSBuild.exe";
}