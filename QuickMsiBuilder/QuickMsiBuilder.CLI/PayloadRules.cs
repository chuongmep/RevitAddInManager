using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuickMsiBuilder.CLI
{
    /// <summary>
    /// Checks on the folder that is about to be packaged.
    /// </summary>
    public static class PayloadRules
    {
        /// <summary>
        /// Assemblies Revit itself provides. A build folder often carries copies of them when
        /// Copy Local was left on, and shipping those into the Revit Addins folder can make Revit
        /// load the wrong build of its own API.
        /// </summary>
        public static readonly string[] RevitProvidedAssemblies =
        {
            "RevitAPI.dll",
            "RevitAPIUI.dll",
            "RevitAPIIFC.dll",
            "RevitAPIMacros.dll",
            "RevitAddInUtility.dll",
            "AdWindows.dll",
            "UIFramework.dll",
            "UIFrameworkServices.dll"
        };

        public static bool IsRevitProvided(string fileName)
        {
            return !string.IsNullOrEmpty(fileName)
                   && RevitProvidedAssemblies.Any(known =>
                       string.Equals(known, fileName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Revit assemblies sitting in the folder about to be packaged, file names only, sorted.
        /// </summary>
        public static List<string> FindRevitAssemblies(string folder)
        {
            if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return new List<string>();

            try
            {
                return Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories)
                    .Select(Path.GetFileName)
                    .Where(IsRevitProvided)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            catch
            {
                // An unreadable folder is the build's problem, not this check's.
                return new List<string>();
            }
        }
    }
}
