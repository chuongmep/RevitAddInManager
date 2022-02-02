using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using RevitAddinManager.View;

namespace RevitAddinManager.Model;

public class AssemLoader
{
    public string OriginalFolder
    {
        get => _originalFolder;
        set => _originalFolder = value;
    }

    public string TempFolder
    {
        get => _tempFolder;
        set => _tempFolder = value;
    }


    public AssemLoader()
    {
        _tempFolder = string.Empty;
        _refedFolders = new List<string>();
        _copiedFiles = new Dictionary<string, DateTime>();
    }


    public void CopyGeneratedFilesBack()
    {
        var files = Directory.GetFiles(_tempFolder, "*.*", SearchOption.AllDirectories);
        foreach (var text in files)
        {
            if (_copiedFiles.ContainsKey(text))
            {
                var t = _copiedFiles[text];
                var fileInfo = new FileInfo(text);
                if (fileInfo.LastWriteTime > t)
                {
                    var str = text.Remove(0, _tempFolder.Length);
                    var destinationFilename = _originalFolder + str;
                    FileUtils.CopyFile(text, destinationFilename);
                }
            }
            else
            {
                var str2 = text.Remove(0, _tempFolder.Length);
                var destinationFilename2 = _originalFolder + str2;
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
        if (string.IsNullOrEmpty(originalFilePath) || originalFilePath.StartsWith("\\") || !File.Exists(originalFilePath))
        {
            return null;
        }
        _mParsingOnly = parsingOnly;
        _originalFolder = Path.GetDirectoryName(originalFilePath);
        var stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
        if (parsingOnly)
        {
            stringBuilder.Append("-Parsing-");
        }
        else
        {
            stringBuilder.Append("-Executing-");
        }
        _tempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
        var assembly = CopyAndLoadAddin(originalFilePath, parsingOnly);

        if (null == assembly || !IsAPIReferenced(assembly))
        {
            return null;
        }
        return assembly;
    }


    private Assembly CopyAndLoadAddin(string srcFilePath, bool onlyCopyRelated)
    {
        var text = string.Empty;
        if (!FileUtils.FileExistsInFolder(srcFilePath, _tempFolder))
        {
            var directoryName = Path.GetDirectoryName(srcFilePath);
            if (!_refedFolders.Contains(directoryName))
            {
                _refedFolders.Add(directoryName);
            }
            var list = new List<FileInfo>();
            text = FileUtils.CopyFileToFolder(srcFilePath, _tempFolder, onlyCopyRelated, list);
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            foreach (var fileInfo in list)
            {
                _copiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTime);
            }
        }
        return LoadAddin(text);
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
        var text = SearchAssemblyFileInTempFolder(args.Name);
        if (File.Exists(text))
        {
            result = LoadAddin(text);
        }
        else
        {
            text = SearchAssemblyFileInOriginalFolders(args.Name);
            if (string.IsNullOrEmpty(text))
            {
                var array = args.Name.Split(new char[]
                {
                    ','
                });
                var text2 = array[0];
                if (array.Length > 1)
                {
                    var text3 = array[2];
                    if (text2.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) && !text3.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                    {
                        text2 = text2.Substring(0, text2.Length - ".resources".Length);
                    }
                    text = SearchAssemblyFileInTempFolder(text2);
                    if (File.Exists(text))
                    {
                        return LoadAddin(text);
                    }
                    text = SearchAssemblyFileInOriginalFolders(text2);
                }
            }
            if (string.IsNullOrEmpty(text))
            {
                var loader = new AssemblyLoader(args.Name);
                loader.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                if (loader.ShowDialog() != true)
                {
                    return null;
                }
                text = loader.resultPath;
            }
            result = CopyAndLoadAddin(text, true);
        }

        return result;
    }

    private string SearchAssemblyFileInTempFolder(string assemName)
    {
        try
        {
               
            var array = new string[] {".dll", ".exe"};
            var text = string.Empty;
            var str = assemName.Substring(0, assemName.IndexOf(','));
            foreach (var str2 in array)
            {
                text = _tempFolder + "\\" + str + str2;

                if (File.Exists(text))
                {
                    return text;
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
        var array = new string[]
        {
            ".dll",
            ".exe"
        };
        string text;
        var text2 = assemName.Substring(0, assemName.IndexOf(','));
        foreach (var str in array)
        {
            text = _dotnetDir + "\\" + text2 + str;
            if (File.Exists(text))
            {
                return text;
            }
        }
        foreach (var str2 in array)
        {
            foreach (var str3 in _refedFolders)
            {
                text = str3 + "\\" + text2 + str2;
                if (File.Exists(text))
                {
                    return text;
                }
            }
        }
        try
        {
            var directoryInfo = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
            var path = directoryInfo.Parent?.FullName + "\\Regression\\_RegressionTools\\";
            if (Directory.Exists(path))
            {
                foreach (var text3 in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileNameWithoutExtension(text3).Equals(text2, StringComparison.OrdinalIgnoreCase))
                    {
                        return text3;
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
        if (string.IsNullOrEmpty(_RevitApiAssemblyFullName))
        {
            foreach (var assembly2 in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (String.Compare(assembly2.GetName().Name,AssRevitName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _RevitApiAssemblyFullName = assembly2.GetName().Name;
                    break;
                }
            }
        }
        foreach (var assemblyName in assembly.GetReferencedAssemblies())
        {
            if (_RevitApiAssemblyFullName == assemblyName.Name)
            {
                return true;
            }
        }
        return false;
    }

    private readonly List<string> _refedFolders;

    private readonly Dictionary<string, DateTime> _copiedFiles;

    private bool _mParsingOnly;

    private string _originalFolder;

    private string _tempFolder;

    private static string _dotnetDir = Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\Framework\\v2.0.50727";

    public static string ResolvedAssemPath = string.Empty;

    private string _RevitApiAssemblyFullName;
}