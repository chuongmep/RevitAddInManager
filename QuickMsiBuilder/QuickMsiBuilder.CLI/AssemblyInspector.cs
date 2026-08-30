using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Mono.Cecil;

namespace QuickMsiBuilder.CLI
{
    /// <summary>An add-in entry point found inside the target assembly.</summary>
    public class AddinCandidate
    {
        public AddinCandidate(string fullClassName, RevitAddinType addinType)
        {
            FullClassName = fullClassName;
            AddinType = addinType;
        }

        public string FullClassName { get; private set; }
        public RevitAddinType AddinType { get; private set; }

        /// <summary>Shown in the class name picker, e.g. "Contoso.MyCommand (Command)".</summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", FullClassName, AddinType);
        }
    }

    public class AssemblyDetails
    {
        public string Version { get; set; }
        public string Company { get; set; }
        public string Description { get; set; }
        public List<AddinCandidate> Candidates { get; set; }

        public AssemblyDetails()
        {
            Candidates = new List<AddinCandidate>();
        }
    }

    /// <summary>
    /// Reads metadata and add-in entry points straight out of the assembly's IL.
    /// Cecil is used rather than reflection because the target references the Revit API, which is not
    /// loadable from this process - reflection would throw before a single type could be listed.
    /// </summary>
    public static class AssemblyInspector
    {
        private const string CommandInterface = "Autodesk.Revit.UI.IExternalCommand";
        private const string ApplicationInterface = "Autodesk.Revit.UI.IExternalApplication";

        public static AssemblyDetails Inspect(string dllPath)
        {
            var details = new AssemblyDetails();
            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath)) return details;

            ReadFileVersionInfo(dllPath, details);

            try
            {
                using (var assembly = AssemblyDefinition.ReadAssembly(dllPath))
                {
                    // The assembly version is the one developers actually maintain; the Win32 file
                    // version is often left at 0.0.0.0, so it only fills the gaps.
                    var version = assembly.Name.Version;
                    if (version != null && (version.Major > 0 || version.Minor > 0 || version.Build > 0))
                    {
                        details.Version = UsableVersion(FormatVersion(version)) ?? details.Version;
                    }

                    foreach (var module in assembly.Modules)
                    {
                        foreach (var type in module.Types.SelectMany(Flatten))
                        {
                            var candidate = Match(type);
                            if (candidate != null) details.Candidates.Add(candidate);
                        }
                    }
                }
            }
            catch
            {
                // Not a managed assembly, or unreadable: the caller just gets whatever was found.
            }

            details.Candidates = details.Candidates
                .OrderBy(candidate => candidate.AddinType == RevitAddinType.Command ? 0 : 1)
                .ThenBy(candidate => candidate.FullClassName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return details;
        }

        private static AddinCandidate Match(TypeDefinition type)
        {
            if (!type.IsClass || type.IsAbstract || !type.IsPublic && !type.IsNestedPublic) return null;

            foreach (var contract in type.Interfaces)
            {
                var name = contract.InterfaceType.FullName;
                if (name == CommandInterface) return new AddinCandidate(ClassName(type), RevitAddinType.Command);
                if (name == ApplicationInterface) return new AddinCandidate(ClassName(type), RevitAddinType.Application);
            }

            return null;
        }

        /// <summary>
        /// Cecil separates a nested type with '/', while Revit resolves the manifest class name
        /// through Assembly.GetType, which expects '+'.
        /// </summary>
        private static string ClassName(TypeDefinition type)
        {
            return type.FullName.Replace('/', '+');
        }

        private static IEnumerable<TypeDefinition> Flatten(TypeDefinition type)
        {
            yield return type;
            foreach (var nested in type.NestedTypes.SelectMany(Flatten)) yield return nested;
        }

        private static void ReadFileVersionInfo(string dllPath, AssemblyDetails details)
        {
            try
            {
                var info = FileVersionInfo.GetVersionInfo(dllPath);
                if (info.FileMajorPart > 0 || info.FileMinorPart > 0 || info.FileBuildPart > 0)
                {
                    details.Version = UsableVersion(string.Format("{0}.{1}.{2}",
                        info.FileMajorPart, info.FileMinorPart, info.FileBuildPart));
                }

                if (!string.IsNullOrEmpty(info.CompanyName)) details.Company = info.CompanyName.Trim();
                if (!string.IsNullOrEmpty(info.FileDescription)) details.Description = info.FileDescription.Trim();
            }
            catch
            {
                // Version info is optional.
            }
        }

        /// <summary>
        /// Revit add-ins commonly stamp the Revit year as their assembly version (2027.0.0), which is
        /// not a legal MSI ProductVersion - the major field stops at 255. Such a version is dropped
        /// so the caller falls back to 1.0.0 instead of producing a package WiX would reject.
        /// </summary>
        private static string UsableVersion(string version)
        {
            string normalized;
            string error;
            return MsiBuildOptions.TryNormalizeVersion(version, out normalized, out error) ? normalized : null;
        }

        private static string FormatVersion(Version version)
        {
            return string.Format("{0}.{1}.{2}",
                Math.Max(version.Major, 0), Math.Max(version.Minor, 0), Math.Max(version.Build, 0));
        }
    }
}
