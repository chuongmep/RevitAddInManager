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

        /// <summary>Guards against a malformed or circular type hierarchy.</summary>
        private const int MaxHierarchyDepth = 32;

        public static AssemblyDetails Inspect(string dllPath)
        {
            var details = new AssemblyDetails();
            if (string.IsNullOrEmpty(dllPath) || !File.Exists(dllPath)) return details;

            ReadFileVersionInfo(dllPath, details);

            try
            {
                // The resolver needs the assembly's own folder: a command often derives from a base
                // class living in a sibling dll, and that base is where the interface is declared.
                using (var resolver = new DefaultAssemblyResolver())
                {
                    var directory = Path.GetDirectoryName(dllPath);
                    if (!string.IsNullOrEmpty(directory)) resolver.AddSearchDirectory(directory);

                    var parameters = new ReaderParameters { AssemblyResolver = resolver };
                    using (var assembly = AssemblyDefinition.ReadAssembly(dllPath, parameters))
                    {
                        // The assembly version is the one developers actually maintain; the Win32
                        // file version is often left at 0.0.0.0, so it only fills the gaps.
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
            // Revit only instantiates public, concrete classes.
            if (!type.IsClass || type.IsAbstract || !type.IsPublic && !type.IsNestedPublic) return null;

            var addinType = Classify(type);
            return addinType == null ? null : new AddinCandidate(ClassName(type), addinType.Value);
        }

        /// <summary>
        /// Walks the whole hierarchy, not just the interfaces declared on the class itself.
        /// Add-ins commonly put the interface on a shared base class - every derived command would
        /// otherwise go unnoticed - and an interface may also inherit the Revit one.
        /// </summary>
        private static RevitAddinType? Classify(TypeDefinition type)
        {
            var current = type;

            for (var depth = 0; current != null && depth < MaxHierarchyDepth; depth++)
            {
                foreach (var contract in current.Interfaces)
                {
                    var addinType = ClassifyInterface(contract.InterfaceType, 0);
                    if (addinType != null) return addinType;
                }

                current = Resolve(current.BaseType);
            }

            return null;
        }

        private static RevitAddinType? ClassifyInterface(TypeReference contract, int depth)
        {
            if (contract == null || depth >= MaxHierarchyDepth) return null;

            if (contract.FullName == CommandInterface) return RevitAddinType.Command;
            if (contract.FullName == ApplicationInterface) return RevitAddinType.Application;

            var definition = Resolve(contract);
            if (definition == null) return null;

            foreach (var inherited in definition.Interfaces)
            {
                var addinType = ClassifyInterface(inherited.InterfaceType, depth + 1);
                if (addinType != null) return addinType;
            }

            return null;
        }

        /// <summary>
        /// Resolving reaches outside the assembly, so it fails for types whose assembly is not
        /// beside the target - the Revit API itself, for one. A failure just ends that branch.
        /// </summary>
        private static TypeDefinition Resolve(TypeReference reference)
        {
            if (reference == null) return null;

            try
            {
                return reference.Resolve();
            }
            catch
            {
                return null;
            }
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
