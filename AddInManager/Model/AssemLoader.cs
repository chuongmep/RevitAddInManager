using RevitAddinManager.View;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace RevitAddinManager.Model;

public class AssemLoader
{
    public string OriginalFolder
    {
        get => originalFolder;
        set => originalFolder = value;
    }

    public string TempFolder
    {
        get => tempFolder;
        set => tempFolder = value;
    }

    public AssemLoader()
    {
        tempFolder = string.Empty;
        refedFolders = new List<string>();
        copiedFiles = new Dictionary<string, DateTime>();
    }

    public void CopyGeneratedFilesBack()
    {
        var files = Directory.GetFiles(tempFolder, "*.*", SearchOption.AllDirectories);
        if(!files.Any()) return;
        foreach (var text in files)
        {
            if (copiedFiles.ContainsKey(text))
            {
                var t = copiedFiles[text];
                var fileInfo = new FileInfo(text);
                if (fileInfo.LastWriteTime > t)
                {
                    var str = text.Remove(0, tempFolder.Length);
                    var destinationFilename = originalFolder + str;
                    FileUtils.CopyFile(text, destinationFilename);
                }
            }
            else
            {
                var str2 = text.Remove(0, tempFolder.Length);
                var destinationFilename2 = originalFolder + str2;
                FileUtils.CopyFile(text, destinationFilename2);
            }
        }
    }

    public void HookAssemblyResolve()
    {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    public void UnhookAssemblyResolve()
    {
        AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
    }

    public Assembly LoadAddinsToTempFolder(string originalFilePath, bool parsingOnly)
    {
        if (string.IsNullOrEmpty(originalFilePath)|| !File.Exists(originalFilePath))
        {
            return null;
        }
        this.parsingOnly = parsingOnly;
        originalFolder = Path.GetDirectoryName(originalFilePath);
        var stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
        if (parsingOnly)
        {
            stringBuilder.Append("-Parsing-");
        }
        else
        {
            stringBuilder.Append("-Executing-");
        }
        tempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
        var assembly = CopyAndLoadAddin(originalFilePath, parsingOnly);

        if (null == assembly || !IsAPIReferenced(assembly))
        {
            return null;
        }
        return assembly;
    }

    private Assembly CopyAndLoadAddin(string srcFilePath, bool onlyCopyRelated)
    {
        var filePath = string.Empty;
        if (!FileUtils.FileExistsInFolder(srcFilePath, tempFolder))
        {
            var directoryName = Path.GetDirectoryName(srcFilePath);
            if (!refedFolders.Contains(directoryName))
            {
                refedFolders.Add(directoryName);
            }
            var list = new List<FileInfo>();
            filePath = FileUtils.CopyFileToFolder(srcFilePath, tempFolder, onlyCopyRelated, list);
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            foreach (var fileInfo in list)
            {
                copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTime);
            }
        }
        return LoadAddin(filePath);
    }

    private Assembly LoadAddin(string filePath)
    {
        Assembly result = null;
        try
        {
            Monitor.Enter(this);
            //Agree this error to load depend event assembly, see https://github.com/chuongmep/RevitAddInManager/issues/7
            result = Assembly.LoadFile(filePath);
        }
        finally
        {
            Monitor.Exit(this);
        }
        return result;
    }

    private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        Assembly result;
        new AssemblyName(args.Name);
        var filePath = SearchAssemblyFileInTempFolder(args.Name);
        if (File.Exists(filePath))
        {
            result = LoadAddin(filePath);
        }
        else
        {
            filePath = SearchAssemblyFileInOriginalFolders(args.Name);
            if (string.IsNullOrEmpty(filePath))
            {
                var array = args.Name.Split(new char[]
                {
                    ','
                });
                var ass = array[0];
                if (array.Length > 1)
                {
                    var text3 = array[2];
                    if (ass.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) && !text3.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                    {
                        ass = ass.Substring(0, ass.Length - ".resources".Length);
                    }
                    // Skip searching for the assembly if assembly with specified name is already loaded
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        if (String.Compare(assembly.GetName().Name, ass, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            return null;
                        }
                    }
                    filePath = SearchAssemblyFileInTempFolder(ass);
                    if (File.Exists(filePath))
                    {
                        return LoadAddin(filePath);
                    }
                    filePath = SearchAssemblyFileInOriginalFolders(ass);
                }
            }
            if (string.IsNullOrEmpty(filePath))
            {
                string assName = args.Name.Split(',').FirstOrDefault();
                bool contains = copiedFiles.Select(x => x.Key.Split('\\').LastOrDefault()).Contains(assName);
                if (contains)
                {
                    // Just allow manual load assembly in folder copied, because we checked same domain conflict with another add-in.
                    var loader = new AssemblyLoader(args.Name);
                    loader.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    if (loader.ShowDialog() != true)
                    {
                        return null;
                    }
                    filePath = loader.resultPath;
                }
            }
            result = CopyAndLoadAddin(filePath, true);
        }

        return result;
    }

    private string SearchAssemblyFileInTempFolder(string assemName)
    {
        try
        {
            var array = new string[] { ".dll", ".exe" };
            var filePath = string.Empty;
            if(string.IsNullOrEmpty(assemName)) return String.Empty;
            // Avoid ArgumentOutOfRangeException from .Substring() by checking length parameter
            var strLength = assemName.IndexOf(',');
            var str = strLength == -1 ? assemName : assemName.Substring(0, strLength);
            foreach (var str2 in array)
            {
                filePath = tempFolder + "\\" + str + str2;

                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.ToString());
        }
        return string.Empty;
    }

    private string SearchAssemblyFileInOriginalFolders(string assemName)
    {
        var extensions = new string[]
        {
            ".dll",
            ".exe"
        };
        string filePath;
        // Avoid ArgumentOutOfRangeException from .Substring() by checking length parameter
        var assLength = assemName.IndexOf(',');
        var ass = assLength == -1 ? assemName : assemName.Substring(0, assLength);
        foreach (var str in extensions)
        {
            filePath = dotnetDir + "\\" + ass + str;
            if (File.Exists(filePath))
            {
                return filePath;
            }
        }
        foreach (var ex in extensions)
        {
            foreach (var folder in refedFolders)
            {
                filePath = folder + "\\" + ass + ex;
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }
        }
        try
        {
            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            var path = directoryInfo.Parent?.FullName + "\\Regression\\_RegressionTools\\";
            if (Directory.Exists(path))
            {
                foreach (var fileName in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileNameWithoutExtension(fileName).Equals(ass, StringComparison.OrdinalIgnoreCase))
                    {
                        return fileName;
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.ToString());
        }

        try
        {
            var num = assemName.IndexOf("XMLSerializers", StringComparison.OrdinalIgnoreCase);
            if (num != -1)
            {
                assemName = "System.XML" + assemName.Substring(num + "XMLSerializers".Length);
                return SearchAssemblyFileInOriginalFolders(assemName);
            }
        }
        catch (Exception e)
        {
            throw new ArgumentException(e.ToString());
        }
        return null;
    }

    private bool IsAPIReferenced(Assembly assembly)
    {
        var AssRevitName = "RevitAPI";
        if (string.IsNullOrEmpty(revitApiAssemblyFullName))
        {
            foreach (var assembly2 in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (String.Compare(assembly2.GetName().Name, AssRevitName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    revitApiAssemblyFullName = assembly2.GetName().Name;
                    break;
                }
            }
        }
        foreach (var assemblyName in assembly.GetReferencedAssemblies())
        {
            if (revitApiAssemblyFullName == assemblyName.Name)
            {
                return true;
            }
        }
        return false;
    }

    private readonly List<string> refedFolders;

    private readonly Dictionary<string, DateTime> copiedFiles;

    private bool parsingOnly;

    private string originalFolder;

    private string tempFolder;

    private static string dotnetDir = Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\Framework\\v2.0.50727";

    public static string ResolvedAssemPath = string.Empty;

    private string revitApiAssemblyFullName;
}