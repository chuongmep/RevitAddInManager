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
                Serilog.Log.Information($"Compiling {configuration} configuration...");

                try
                {
                    MSBuild(s => s
                        .SetTargetPath(Solution) // Explicitly set solution file
                        .SetTargets("Rebuild")
                        .SetProcessToolPath(MSBuildTasks.MSBuildPath) // Use standard MSBuild path
                        .SetConfiguration(configuration)
                        .SetVerbosity(MSBuildVerbosity.Minimal)
                        .DisableNodeReuse()
                        .EnableRestore()
                        .SetMaxCpuCount(Environment.ProcessorCount));

                    Serilog.Log.Information($"Project {configuration} compiled successfully.");
                }
                catch (ProcessException ex)
                {
                    Serilog.Log.Error(ex, $"Failed to compile {configuration} configuration.");
                    throw; // Rethrow to fail the build
                }
            });
        });
}