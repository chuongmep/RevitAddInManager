using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WixSharp;
using File = System.IO.File;

namespace QuickMsiBuilder.CLI
{
    public class Program
    {
        /// <summary>
        /// Prefix of the machine readable result line, so the UI can pick the produced file out of
        /// the output without parsing prose.
        /// </summary>
        public const string ResultPrefix = "MSI_PATH=";

        /// <summary>Extracts the built MSI path from CLI output, or null when there is none.</summary>
        public static string ParseResultPath(string output)
        {
            if (string.IsNullOrEmpty(output)) return null;

            foreach (var line in output.Split('\r', '\n'))
            {
                if (line.StartsWith(ResultPrefix, StringComparison.Ordinal))
                {
                    var path = line.Substring(ResultPrefix.Length).Trim();
                    if (path.Length > 0) return path;
                }
            }

            return null;
        }

        /// <summary>Output with the machine readable lines taken out, for showing to a human.</summary>
        public static string StripResultLines(string output)
        {
            if (string.IsNullOrEmpty(output)) return string.Empty;

            var kept = output
                .Split('\n')
                .Where(line => !line.TrimStart().StartsWith(ResultPrefix, StringComparison.Ordinal))
                .Select(line => line.TrimEnd('\r'));

            return string.Join(Environment.NewLine, kept.ToArray()).Trim();
        }

        static int Main(string[] args)
        {
            var log = BuildLog.Logger;

            try
            {
                MsiBuildOptions options;
                string error;
                if (!MsiBuildOptions.TryParse(args, out options, out error))
                {
                    // NLog also writes to the console, so nothing is echoed twice here.
                    log.Error("Invalid arguments: {0}", error);
                    return 1;
                }

                log.Info("Building MSI for {0} v{1}, Revit {2}, entry points: {3}",
                    options.AssemblyName, options.Version, options.RevitYearsText, options.EntriesText);
                log.Debug("Target assembly: {0}", options.DllPath);

                string wixLocation;
                string wixError;
                if (!TryLocateWixToolset(out wixLocation, out wixError))
                {
                    log.Error("WiX toolset not available: {0}", wixError);
                    return 1;
                }

                log.Debug("Using WiX toolset at {0}", wixLocation);

                var msiPath = BuildMsi(options, log);
                new BuildHistoryStore().Record(options, msiPath, DateTime.UtcNow);

                log.Info("MSI build process completed: {0}", msiPath);
                Console.WriteLine(ResultPrefix + msiPath);
                return 0;
            }
            catch (Exception ex)
            {
                log.Error(ex, "MSI build failed");
                log.Info("See log: {0}", BuildLog.LogFilePath);
                return 1;
            }
            finally
            {
                BuildLog.Shutdown();
            }
        }

        /// <summary>
        /// WixSharp only wraps candle/light, it does not ship them. The toolset is bundled in a "wix"
        /// folder next to this executable so the feature works on a machine that only installed the
        /// add-in; anything already configured locally still wins.
        /// </summary>
        static bool TryLocateWixToolset(out string location, out string error)
        {
            error = null;
            location = null;

            var bundled = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wix");
            if (File.Exists(Path.Combine(bundled, "candle.exe")))
            {
                Compiler.WixLocation = bundled;
                location = bundled;
                return true;
            }

            try
            {
                location = Compiler.WixLocation;
                if (!string.IsNullOrEmpty(location) && File.Exists(Path.Combine(location, "candle.exe"))) return true;
            }
            catch (Exception ex)
            {
                error = ex.Message + Environment.NewLine;
            }

            error += "WiX Toolset v3 was not found. Install WiX 3.11+ (https://wixtoolset.org/) " +
                     "or point the WIXSHARP_WIXDIR environment variable at a folder containing candle.exe.";
            return false;
        }

        static string BuildMsi(MsiBuildOptions options, NLog.Logger log)
        {
            var assemblyName = options.AssemblyName;
            var assemblyDir = Path.GetDirectoryName(options.DllPath);
            var outputDir = Path.Combine(assemblyDir, "InstallerOutput");
            Directory.CreateDirectory(outputDir);

            var addinFilePath = Path.Combine(outputDir, assemblyName + ".addin");
            AddinManifest.Create(options).Save(addinFilePath);
            log.Debug("Generated manifest: {0}", addinFilePath);

            LogPayload(assemblyDir, outputDir, log);

            // One InstallDir at the Addins root with a child folder per selected Revit release,
            // the same layout the Revit Add-in Manager installer itself uses. Everything sitting
            // next to the assembly is packaged, so add-ins with their own dependencies install
            // complete; the folder keeps the assembly name to avoid clashing with other add-ins.
            var yearDirs = options.RevitYears
                // A fresh entity tree per year: WixSharp entities carry parent state, so the same
                // instance must not be attached to two directories.
                .Select(year => (WixEntity)new Dir(year,
                    new WixSharp.File(addinFilePath),
                    CreatePayload(assemblyName, assemblyDir, outputDir)))
                .ToArray();

            // The upgrade code is tied to the add-in only: re-running the builder with a different
            // set of years upgrades the existing install instead of stacking a second product.
            var upgradeCode = AddinManifest.CreateDeterministicGuid(assemblyName);

            var project = new Project
            {
                Name = assemblyName,
                OutDir = outputDir,
                Platform = Platform.x64,
                Description = options.Description,
                UI = WUI.WixUI_InstallDir,
                Version = new Version(options.Version),
                OutFileName = options.OutputName,
                InstallScope = InstallScope.perUser,
                MajorUpgrade = MajorUpgrade.Default,
                GUID = upgradeCode,
                ControlPanelInfo =
                {
                    Manufacturer = options.Author,
                    Comments = options.Description
                },
                Dirs = new Dir[]
                {
                    // InstallDir (not Dir) is what defines INSTALLDIR, which WixUI_InstallDir requires.
                    new InstallDir(@"%AppDataFolder%\Autodesk\Revit\Addins\", yearDirs)
                }
            };

            MajorUpgrade.Default.AllowSameVersionUpgrades = true;

            // Icon and background are optional; a missing or cleared path just means no cosmetics.
            if (!string.IsNullOrEmpty(options.IconPath) && File.Exists(options.IconPath))
            {
                project.ControlPanelInfo.ProductIcon = options.IconPath;
                log.Debug("Using product icon: {0}", options.IconPath);
            }

            if (!string.IsNullOrEmpty(options.BackgroundImagePath) && File.Exists(options.BackgroundImagePath))
            {
                project.BackgroundImage = options.BackgroundImagePath;
                log.Debug("Using background image: {0}", options.BackgroundImagePath);
            }

            return project.BuildMsi();
        }

        /// <summary>
        /// Everything beside the assembly is packaged, subfolders included, except the folder this
        /// builder writes into - packaging it would swallow the previous MSI and grow the package on
        /// every run. The tree is walked by hand because WixSharp's Files only filters file names,
        /// which would still leave an empty InstallerOutput folder on the user machine.
        /// </summary>
        static Dir CreatePayload(string name, string sourceDir, string outputDir)
        {
            var entities = new List<WixEntity>();

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                entities.Add(new WixSharp.File(file));
            }

            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                if (IsSameDirectory(directory, outputDir)) continue;

                var child = CreatePayload(Path.GetFileName(directory), directory, outputDir);
                if (child != null) entities.Add(child);
            }

            return entities.Count == 0 ? null : new Dir(name, entities.ToArray());
        }

        static bool IsSameDirectory(string left, string right)
        {
            return string.Equals(
                Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar),
                Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar),
                StringComparison.OrdinalIgnoreCase);
        }

        static IEnumerable<string> EnumeratePayload(string assemblyDir, string outputDir)
        {
            return Directory.GetFiles(assemblyDir, "*", SearchOption.AllDirectories)
                .Where(path => !IsInside(path, outputDir));
        }

        static bool IsInside(string path, string directory)
        {
            var prefix = directory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            return Path.GetFullPath(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        static void LogPayload(string assemblyDir, string outputDir, NLog.Logger log)
        {
            var files = EnumeratePayload(assemblyDir, outputDir).ToList();
            long bytes = 0;
            foreach (var file in files)
            {
                try
                {
                    bytes += new FileInfo(file).Length;
                }
                catch
                {
                    // A file we cannot stat is still packaged; the size is only informational.
                }
            }

            log.Info("Packaging {0} file(s), {1:N1} MB, from {2}",
                files.Count, bytes / 1024d / 1024d, assemblyDir);

            foreach (var revitAssembly in PayloadRules.FindRevitAssemblies(assemblyDir))
            {
                log.Warn("  {0} is provided by Revit and should not be packaged with an add-in.", revitAssembly);
            }

            foreach (var file in files) log.Debug("  packaged: {0}", file.Substring(assemblyDir.Length).TrimStart(Path.DirectorySeparatorChar));
        }
    }
}
