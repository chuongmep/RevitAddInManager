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
        get => _mOriginalFolder;
        set => _mOriginalFolder = value;
    }

    public string TempFolder
    {
        get => _mTempFolder;
        set => _mTempFolder = value;
    }


    public AssemLoader()
    {
        _mTempFolder = string.Empty;
        _mRefedFolders = new List<string>();
        _mCopiedFiles = new Dictionary<string, DateTime>();
    }


    public void CopyGeneratedFilesBack()
    {
        var files = Directory.GetFiles(_mTempFolder, "*.*", SearchOption.AllDirectories);
        foreach (var text in files)
        {
            if (_mCopiedFiles.ContainsKey(text))
            {
                var t = _mCopiedFiles[text];
                var fileInfo = new FileInfo(text);
                if (fileInfo.LastWriteTime > t)
                {
                    var str = text.Remove(0, _mTempFolder.Length);
                    var destinationFilename = _mOriginalFolder + str;
                    FileUtils.CopyFile(text, destinationFilename);
                }
            }
            else
            {
                var str2 = text.Remove(0, _mTempFolder.Length);
                var destinationFilename2 = _mOriginalFolder + str2;
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
        _mOriginalFolder = Path.GetDirectoryName(originalFilePath);
        var stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
        if (parsingOnly)
        {
            stringBuilder.Append("-Parsing-");
        }
        else
        {
            stringBuilder.Append("-Executing-");
        }
        _mTempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
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
        if (!FileUtils.FileExistsInFolder(srcFilePath, _mTempFolder))
        {
            var directoryName = Path.GetDirectoryName(srcFilePath);
            if (!_mRefedFolders.Contains(directoryName))
            {
                _mRefedFolders.Add(directoryName);
            }
            var list = new List<FileInfo>();
            text = FileUtils.CopyFileToFolder(srcFilePath, _mTempFolder, onlyCopyRelated, list);
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }
            foreach (var fileInfo in list)
            {
                _mCopiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTime);
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
            //Do not use method LoadFile, See https://github.com/chuongmep/RevitAddInManager/issues/7
            result = Assembly.LoadFrom(filePath);
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
               
            var array = new string[]
            {
                ".dll",
                ".exe"
            };
            var text = string.Empty;
            var str = assemName.Substring(0, assemName.IndexOf(','));
            foreach (var str2 in array)
            {
                text = _mTempFolder + "\\" + str + str2;

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
        return String.Empty;
    }


    private string SearchAssemblyFileInOriginalFolders(string assemName)
    {
        var array = new string[]
        {
            ".dll",
            ".exe"
        };
        var text = string.Empty;
        var text2 = assemName.Substring(0, assemName.IndexOf(','));
        foreach (var str in array)
        {
            text = _mDotnetDir + "\\" + text2 + str;
            if (File.Exists(text))
            {
                return text;
            }
        }
        foreach (var str2 in array)
        {
            foreach (var str3 in _mRefedFolders)
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
            var path = directoryInfo.Parent.FullName + "\\Regression\\_RegressionTools\\";
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
        if (string.IsNullOrEmpty(_mRevitApiAssemblyFullName))
        {
            foreach (var assembly2 in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (String.Compare(assembly2.GetName().Name,AssRevitName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    _mRevitApiAssemblyFullName = assembly2.GetName().Name;
                    break;
                }
            }
        }
        foreach (var assemblyName in assembly.GetReferencedAssemblies())
        {
            if (_mRevitApiAssemblyFullName == assemblyName.Name)
            {
                return true;
            }
        }
        return false;
    }

    private List<string> _mRefedFolders;

    private Dictionary<string, DateTime> _mCopiedFiles;

    private bool _mParsingOnly;

    private string _mOriginalFolder;

    private string _mTempFolder;

    private static string _mDotnetDir = Environment.GetEnvironmentVariable("windir") + "\\Microsoft.NET\\Framework\\v2.0.50727";

    public static string MResolvedAssemPath = string.Empty;

    private string _mRevitApiAssemblyFullName;
}