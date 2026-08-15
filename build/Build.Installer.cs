using JetBrains.Annotations;
using Nuke.Common;
using Nuke.Common.Git;
using Serilog;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

internal partial class Build
{
    private readonly Regex StreamRegex = new("'(.+?)'", RegexOptions.Compiled);

    private Target CreateInstaller => _ => _
         .TriggeredBy(Compile)
         .OnlyWhenStatic(() => IsLocalBuild || GitRepository.IsOnMainOrMasterBranch())
         .Executes(() =>
         {
             var installerProject = BuilderExtensions.GetProject(Solution, InstallerProject);
             var buildDirectories = GetBuildDirectories();
             var configurations = GetConfigurations(InstallerConfiguration);

             var sharedDirectories = GetSharedDirectories();

             foreach (var directoryGroup in buildDirectories)
             {
                 var directories = directoryGroup.ToList();
                 var payloadDirectories = directories.Select(info => info.FullName).Concat(sharedDirectories).ToList();
                 var exeArguments = BuildExeArguments(payloadDirectories);
                 var exeFile = installerProject.GetExecutableFile(configurations, directories);
                 if (string.IsNullOrEmpty(exeFile))
                 {
                     Log.Warning("No installer executable was found for these packages:\n {Directories}", string.Join("\n", directories));
                     continue;
                 }

                 var proc = new Process();
                 proc.StartInfo.FileName = exeFile;
                 proc.StartInfo.Arguments = exeArguments;
                 proc.StartInfo.RedirectStandardOutput = true;
                 proc.Start();
                 while (!proc.StandardOutput.EndOfStream) ParseProcessOutput(proc.StandardOutput.ReadLine());
                 proc.WaitForExit();
                 if (proc.ExitCode != 0) throw new Exception("The installer creation failed.");
             }
         });

    /// <summary>
    /// Payload that is the same for every Revit version and therefore installed once, beside the
    /// version folders instead of inside each of them. Staged by the add-in build.
    /// </summary>
    private List<string> GetSharedDirectories()
    {
        var addInProject = BuilderExtensions.GetProject(Solution, Projects[0]);
        var sharedRoot = addInProject.GetBinDirectory() / SharedBinFolder;
        var directories = new List<string>();

        if (!Directory.Exists(sharedRoot))
        {
            Log.Warning("No shared payload directory found at {Directory}", sharedRoot.ToString());
            return directories;
        }

        foreach (var directory in Directory.GetDirectories(sharedRoot))
        {
            Log.Information("Including shared payload: {Directory}", directory);
            directories.Add(directory);
        }

        return directories;
    }

    private void ParseProcessOutput([CanBeNull] string value)
    {
        if (value is null) return;
        var matches = StreamRegex.Matches(value);
        if (matches.Count > 0)
        {
            var parameters = matches.Select(match => match.Value
                    .Substring(1, match.Value.Length - 2))
                .Cast<object>()
                .ToArray();
            var line = StreamRegex.Replace(value, match => $"{{Parameter{match.Index}}}");
            Log.Information(line, parameters);
        }
        else
        {
            Log.Debug(value);
        }
    }

    private static string BuildExeArguments(IReadOnlyList<string> args)
    {
        var argumentBuilder = new StringBuilder();
        for (var i = 0; i < args.Count; i++)
        {
            if (i > 0) argumentBuilder.Append(' ');
            var value = args[i];
            if (value.Contains(' ')) value = $"\"{value}\"";
            argumentBuilder.Append(value);
        }

        return argumentBuilder.ToString();
    }
}