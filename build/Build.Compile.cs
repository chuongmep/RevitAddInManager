using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.MSBuild;
using static Nuke.Common.Tools.MSBuild.MSBuildTasks;

internal partial class Build
{
    private Target Compile => _ => _
         .TriggeredBy(Cleaning)
         .Executes(() =>
         {
             var configurations = GetConfigurations(BuildConfiguration, InstallerConfiguration);
             configurations.ForEach(configuration =>
             {
                 Console.WriteLine($"Compiling {configuration} configuration...");
             });
             
             configurations.ForEach(configuration =>
             {
                 Console.WriteLine($"Compiling project {configuration}...");
                 MSBuild(s => s
                     .SetTargets("Rebuild")
                     .SetProcessToolPath(MsBuildPath.Value)
                     .SetConfiguration(configuration)
                     .SetVerbosity(MSBuildVerbosity.Minimal)
                     .DisableNodeReuse()
                     .EnableRestore());
                 Console.WriteLine($"Project {configuration} has been compiled successfully.");
             });
         });
}