using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace QuickMsiBuilder.CLI
{
    public enum RevitAddinType
    {
        Command,
        Application
    }

    /// <summary>
    /// Positional command line arguments of the builder, validated once so that the rest of the
    /// pipeline never has to deal with malformed input.
    /// </summary>
    public class MsiBuildOptions
    {
        public const string Usage =
            "Usage: QuickMsiBuilder.CLI <dll_path> [version] [author] [description] [icon_path] [bg_image_path] [revit_years] [full_class_name] [addin_type]\r\n" +
            "       revit_years accepts one or more years, e.g. \"2024\" or \"2024,2025,2026\".";

        public string DllPath { get; private set; }
        public string Version { get; private set; }
        public string Author { get; private set; }
        public string Description { get; private set; }
        public string IconPath { get; private set; }
        public string BackgroundImagePath { get; private set; }
        public List<string> RevitYears { get; private set; }
        public string FullClassName { get; private set; }
        public RevitAddinType AddinType { get; private set; }

        public string AssemblyName
        {
            get { return Path.GetFileNameWithoutExtension(DllPath); }
        }

        /// <summary>The selected years as stored in history and shown to the user, e.g. "2024, 2026".</summary>
        public string RevitYearsText
        {
            get { return string.Join(", ", RevitYears.ToArray()); }
        }

        /// <summary>
        /// MSI file name without extension, e.g. "Contoso-1.0.0-R2024" or "Contoso-1.0.0-R2024-2026"
        /// for a package covering several Revit releases.
        /// </summary>
        public string OutputName
        {
            get
            {
                return string.Format("{0}-{1}-R{2}",
                    AssemblyName, Version, string.Join("-", RevitYears.ToArray()));
            }
        }

        public static bool TryParse(string[] args, out MsiBuildOptions options, out string error)
        {
            options = null;
            error = null;

            if (args == null || args.Length < 1 || string.IsNullOrEmpty(args[0]))
            {
                error = Usage;
                return false;
            }

            string dllPath;
            try
            {
                dllPath = Path.GetFullPath(args[0]);
            }
            catch (Exception ex)
            {
                error = "Invalid DLL path: " + ex.Message;
                return false;
            }

            if (!File.Exists(dllPath))
            {
                error = "DLL file not found at " + dllPath;
                return false;
            }

            var result = new MsiBuildOptions
            {
                DllPath = dllPath,
                Author = Fallback(Get(args, 2), DefaultAuthor),
                Description = Fallback(Get(args, 3), "Revit Add-in"),
                IconPath = Get(args, 4),
                BackgroundImagePath = Get(args, 5),
            };

            // Read once and reuse: the assembly supplies both the default version and the entry point.
            var details = AssemblyInspector.Inspect(dllPath);

            var versionArg = Get(args, 1);
            if (string.IsNullOrEmpty(versionArg)) versionArg = details.Version;

            string version;
            if (!TryNormalizeVersion(versionArg, out version, out error)) return false;
            result.Version = version;

            List<string> revitYears;
            if (!TryNormalizeRevitYears(Get(args, 6), out revitYears, out error)) return false;
            result.RevitYears = revitYears;

            var fullClassName = Get(args, 7);
            result.AddinType = ParseAddinType(Get(args, 8));

            if (string.IsNullOrEmpty(fullClassName))
            {
                // Nobody should have to type this: read the entry point out of the assembly, and
                // only fall back to a guess when the assembly declares none.
                var candidate = AssemblyInspector.PickDefault(details, result.AddinType);
                if (candidate != null)
                {
                    fullClassName = candidate.FullClassName;
                    result.AddinType = candidate.AddinType;
                }
                else
                {
                    fullClassName = result.AssemblyName +
                                    (result.AddinType == RevitAddinType.Application ? ".Application" : ".Command");
                }
            }

            result.FullClassName = fullClassName;

            options = result;
            return true;
        }

        /// <summary>
        /// MSI ProductVersion only honours major.minor.build, with 0-255 / 0-255 / 0-65535 ranges.
        /// Anything wider (or a SemVer suffix such as 1.2.0-beta) is rejected here instead of
        /// blowing up inside WiX with an unreadable message.
        /// </summary>
        public static bool TryNormalizeVersion(string value, out string normalized, out string error)
        {
            normalized = null;
            error = null;
            if (string.IsNullOrEmpty(value)) value = "1.0.0";

            System.Version parsed;
            if (!System.Version.TryParse(value, out parsed))
            {
                error = string.Format("Invalid version '{0}'. Expected a numeric version such as 1.0.0.", value);
                return false;
            }

            var major = parsed.Major;
            var minor = Math.Max(parsed.Minor, 0);
            var build = Math.Max(parsed.Build, 0);

            if (major > 255 || minor > 255 || build > 65535)
            {
                error = string.Format(
                    "Version '{0}' is not a valid MSI ProductVersion. Limits are 255.255.65535.", value);
                return false;
            }

            normalized = string.Format("{0}.{1}.{2}", major, minor, build);
            return true;
        }

        /// <summary>
        /// One MSI can target several Revit releases, so the year argument is a list.
        /// Duplicates are collapsed and the result is sorted so the output name is stable.
        /// </summary>
        public static bool TryNormalizeRevitYears(string value, out List<string> normalized, out string error)
        {
            normalized = null;
            error = null;
            if (string.IsNullOrEmpty(value))
            {
                normalized = new List<string> { DefaultRevitYear };
                return true;
            }

            var years = new List<string>();
            foreach (var raw in value.Split(new[] { ',', ';', ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // Revit reports its version as "2026"; accept that plus anything the UI can produce.
                var match = Regex.Match(raw.Trim(), @"^(20\d{2})");
                if (!match.Success)
                {
                    error = string.Format("Invalid Revit year '{0}'. Expected a four digit year such as 2024.", raw.Trim());
                    return false;
                }

                var year = match.Groups[1].Value;
                if (!years.Contains(year)) years.Add(year);
            }

            if (years.Count == 0)
            {
                error = "No Revit year was selected. Select at least one Revit version.";
                return false;
            }

            years.Sort(StringComparer.Ordinal);
            normalized = years;
            return true;
        }

        public static string DefaultRevitYear
        {
            get { return RevitYearRange.Default; }
        }

        /// <summary>
        /// Falls back to the Windows account name so the publisher field is filled in with something
        /// meaningful instead of a placeholder.
        /// </summary>
        public static string DefaultAuthor
        {
            get
            {
                try
                {
                    var user = Environment.UserName;
                    return string.IsNullOrEmpty(user) ? "Unknown Publisher" : user;
                }
                catch
                {
                    return "Unknown Publisher";
                }
            }
        }

        public static RevitAddinType ParseAddinType(string value)
        {
            return string.Equals(value, "Application", StringComparison.OrdinalIgnoreCase)
                ? RevitAddinType.Application
                : RevitAddinType.Command;
        }

        private static string Get(string[] args, int index)
        {
            return args.Length > index ? (args[index] ?? string.Empty).Trim() : string.Empty;
        }

        private static string Fallback(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
