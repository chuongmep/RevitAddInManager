using System.IO;

namespace RevitAddinManager.Model;

/// <summary>
/// Turns the assembly path carried by an add-in entry into a path that exists on disk.
/// A .addin manifest may declare its assembly relatively - "MyAddin\MyAddin.dll" is the layout the
/// installers produce - and that value is stored verbatim, so it only resolves against the folder
/// holding the manifest, never against the working directory of the Revit process.
/// </summary>
public static class AssemblyPathResolver
{
    /// <summary>
    /// Returns an existing full path, or null when the assembly cannot be found.
    /// </summary>
    /// <param name="assemblyPath">Value taken from the add-in entry, absolute or relative.</param>
    /// <param name="revitVersion">Revit release used to build the Addins folders to search.</param>
    public static string Resolve(string assemblyPath, string revitVersion)
    {
        if (string.IsNullOrEmpty(assemblyPath)) return null;

        foreach (var candidate in Candidates(assemblyPath, revitVersion))
        {
            if (string.IsNullOrEmpty(candidate)) continue;

            try
            {
                if (File.Exists(candidate)) return Path.GetFullPath(candidate);
            }
            catch
            {
                // A malformed candidate is simply not the answer.
            }
        }

        return null;
    }

    private static IEnumerable<string> Candidates(string assemblyPath, string revitVersion)
    {
        yield return assemblyPath;

        if (Path.IsPathRooted(assemblyPath)) yield break;

        foreach (var root in AddinRoots(revitVersion))
        {
            yield return Path.Combine(root, assemblyPath);
        }
    }

    /// <summary>
    /// The folders Revit reads manifests from, which is what a relative assembly path is based on.
    /// </summary>
    private static IEnumerable<string> AddinRoots(string revitVersion)
    {
        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        if (!string.IsNullOrEmpty(revitVersion))
        {
            yield return Path.Combine(roaming, DefaultSetting.AdskPath, revitVersion);
            yield return Path.Combine(programData, DefaultSetting.AdskPath, revitVersion);
        }

        yield return Path.Combine(programData, "Autodesk", "ApplicationPlugins");
    }
}
