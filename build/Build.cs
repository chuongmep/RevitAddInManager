using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.VSWhere;
using System.Text.RegularExpressions;

internal partial class Build : NukeBuild
{
    private readonly AbsolutePath ArtifactsDirectory = RootDirectory / ArtifactsFolder;
    private readonly AbsolutePath ChangeLogPath = RootDirectory / "CHANGELOG.md";
    [GitRepository] private readonly GitRepository GitRepository;
    [Solution] private readonly Solution Solution;

    private static readonly Lazy<string> MsBuildPath = new(() =>
       {
           if (IsServerBuild) return null;
           var (_, output) = VSWhereTasks.VSWhere(settings => settings
               .EnableLatest()
               .AddRequires("Microsoft.Component.MSBuild")
               .SetProperty("installationPath")
           );

           if (output.Count > 0) return null;
           if (!File.Exists(CustomMsBuildPath)) throw new Exception($"Missing file: {CustomMsBuildPath}. Change the path to the build platform or install Visual Studio.");
           return CustomMsBuildPath;
       });

    public static int Main() => Execute<Build>(x => x.Cleaning);

    private List<string> GetConfigurations(params string[] startPatterns)
    {
        var configurations = Solution.Configurations
            .Select(pair => pair.Key)
            .Where(s => startPatterns.Any(s.StartsWith))
            .Select(s =>
            {
                var platformIndex = s.LastIndexOf('|');
                return s.Remove(platformIndex);
            })
            .ToList();
        if (configurations.Count == 0) throw new Exception($"Can't find configurations in the solution by patterns: {string.Join(" | ", startPatterns)}.");
        return configurations;
    }

    private IEnumerable<IGrouping<string, DirectoryInfo>> GetBuildDirectories()
    {
        var directories = new List<DirectoryInfo>();
        foreach (var projectName in Projects)
        {
            var project = BuilderExtensions.GetProject(Solution, projectName);
            var directoryInfo = new DirectoryInfo(project.GetBinDirectory()).GetDirectories();
            directories.AddRange(directoryInfo);
        }

        if (directories.Count == 0) throw new Exception("There are no packaged assemblies in the project. Try to build the project again.");

        var versionRegex = new Regex(@"^.*R\d+ ?");
        var addInsDirectory = directories
            .Where(dir => dir.Name.StartsWith(AddInBinPrefix))
            .Where(dir => dir.Name.Contains(BuildConfiguration))
            .GroupBy(dir => versionRegex.Replace(dir.Name, string.Empty));

        return addInsDirectory;
    }
}