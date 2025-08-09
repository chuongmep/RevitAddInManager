using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;

internal partial class Build
{
    private Target Cleaning => _ => _
         .Executes(() =>
         {
             if (Directory.Exists(ArtifactsDirectory))
             {
                 Directory.Delete(ArtifactsDirectory, recursive: true);
                 Serilog.Log.Information($"Deleted artifacts directory: {ArtifactsDirectory}");
             }
             Directory.CreateDirectory(ArtifactsDirectory);
             Serilog.Log.Information($"Created clean artifacts directory: {ArtifactsDirectory}");

             if (IsServerBuild) return;
             foreach (var projectName in Projects)
             {
                 var project = Solution.GetProject(projectName);
                 if (project == null)
                 {
                     Serilog.Log.Warning($"Project {projectName} not found in solution.");
                     continue;
                 }

                 // Assume GetBinDirectory returns the bin directory path
                 var binDirectory = (AbsolutePath)project.Directory / "bin";
                 if (!Directory.Exists(binDirectory))
                 {
                     Serilog.Log.Information($"Bin directory does not exist: {binDirectory}");
                     continue;
                 }

                 // Glob and delete directories matching patterns
                 var directoriesToDelete = binDirectory.GlobDirectories($"{AddInBinPrefix}*", "Release*");
                 foreach (var dir in directoriesToDelete)
                 {
                     if (Directory.Exists(dir))
                     {
                         Directory.Delete(dir, recursive: true);
                         Serilog.Log.Information($"Deleted directory: {dir}");
                     }
                 }
             }
         });
}