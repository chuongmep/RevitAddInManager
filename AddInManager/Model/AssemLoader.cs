using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using RevitAddinManager.View;

namespace RevitAddinManager.Model
{
    public class AssemLoader
    {
        public string OriginalFolder
        {
            get => this._mOriginalFolder;
            set => this._mOriginalFolder = value;
        }

        public string TempFolder
        {
            get => this._mTempFolder;
            set => this._mTempFolder = value;
        }


        public AssemLoader()
        {
            this._mTempFolder = string.Empty;
            this._mRefedFolders = new List<string>();
            this._mCopiedFiles = new Dictionary<string, DateTime>();
        }


        public void CopyGeneratedFilesBack()
        {
            string[] files = Directory.GetFiles(this._mTempFolder, "*.*", SearchOption.AllDirectories);
            foreach (string text in files)
            {
                if (this._mCopiedFiles.ContainsKey(text))
                {
                    DateTime t = this._mCopiedFiles[text];
                    FileInfo fileInfo = new FileInfo(text);
                    if (fileInfo.LastWriteTime > t)
                    {
                        string str = text.Remove(0, this._mTempFolder.Length);
                        string destinationFilename = this._mOriginalFolder + str;
                        FileUtils.CopyFile(text, destinationFilename);
                    }
                }
                else
                {
                    string str2 = text.Remove(0, this._mTempFolder.Length);
                    string destinationFilename2 = this._mOriginalFolder + str2;
                    FileUtils.CopyFile(text, destinationFilename2);
                }
            }
        }

        public void HookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;
        }

        public void UnhookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
        }

        public Assembly LoadAddinsToTempFolder(string originalFilePath, bool parsingOnly)
        {
            if (string.IsNullOrEmpty(originalFilePath) || originalFilePath.StartsWith("\\") || !File.Exists(originalFilePath))
            {
                return null;
            }
            this._mParsingOnly = parsingOnly;
            this._mOriginalFolder = Path.GetDirectoryName(originalFilePath);
            StringBuilder stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
            if (parsingOnly)
            {
                stringBuilder.Append("-Parsing-");
            }
            else
            {
                stringBuilder.Append("-Executing-");
            }
            this._mTempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
            Assembly assembly = this.CopyAndLoadAddin(originalFilePath, parsingOnly);

            if (null == assembly || !this.IsAPIReferenced(assembly))
            {
                return null;
            }
            return assembly;
        }


        private Assembly CopyAndLoadAddin(string srcFilePath, bool onlyCopyRelated)
        {
            string text = string.Empty;
            if (!FileUtils.FileExistsInFolder(srcFilePath, this._mTempFolder))
            {
                string directoryName = Path.GetDirectoryName(srcFilePath);
                if (!this._mRefedFolders.Contains(directoryName))
                {
                    this._mRefedFolders.Add(directoryName);
                }
                List<FileInfo> list = new List<FileInfo>();
                text = FileUtils.CopyFileToFolder(srcFilePath, this._mTempFolder, onlyCopyRelated, list);
                if (string.IsNullOrEmpty(text))
                {
                    return null;
                }
                foreach (FileInfo fileInfo in list)
                {
                    this._mCopiedFiles.Add(fileInfo.FullName, fileInfo.LastWriteTime);
                }
            }
            return this.LoadAddin(text);
        }

        private Assembly LoadAddin(string filePath)
        {
            Assembly result = null;
            try
            {
                Monitor.Enter(this);
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
            string text = this.SearchAssemblyFileInTempFolder(args.Name);
            if (File.Exists(text))
            {
                result = this.LoadAddin(text);
            }
            else
            {
                text = this.SearchAssemblyFileInOriginalFolders(args.Name);
                if (string.IsNullOrEmpty(text))
                {
                    string[] array = args.Name.Split(new char[]
                    {
                            ','
                    });
                    string text2 = array[0];
                    if (array.Length > 1)
                    {
                        string text3 = array[2];
                        if (text2.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) && !text3.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                        {
                            text2 = text2.Substring(0, text2.Length - ".resources".Length);
                        }
                        text = this.SearchAssemblyFileInTempFolder(text2);
                        if (File.Exists(text))
                        {
                            return this.LoadAddin(text);
                        }
                        text = this.SearchAssemblyFileInOriginalFolders(text2);
                    }
                }
                if (string.IsNullOrEmpty(text))
                {
                    AssemblyLoader loader = new AssemblyLoader(args.Name);
                    loader.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    if (loader.ShowDialog() != true)
                    {
                        return null;
                    }
                    text = loader.resultPath;
                }
                result = this.CopyAndLoadAddin(text, true);
            }

            return result;
        }

        private string SearchAssemblyFileInTempFolder(string assemName)
        {
            try
            {
               
                string[] array = new string[]
                {
                    ".dll",
                    ".exe"
                };
                string text = string.Empty;
                string str = assemName.Substring(0, assemName.IndexOf(','));
                foreach (string str2 in array)
                {
                    text = this._mTempFolder + "\\" + str + str2;

                    if (File.Exists(text))
                    {
                        return text;
                    }
                }
            }
            catch (Exception e)
            {
                throw new System.ArgumentException(e.ToString());
            }
            return String.Empty;
        }


        private string SearchAssemblyFileInOriginalFolders(string assemName)
        {
            string[] array = new string[]
            {
                ".dll",
                ".exe"
            };
            string text = string.Empty;
            string text2 = assemName.Substring(0, assemName.IndexOf(','));
            foreach (string str in array)
            {
                text = AssemLoader._mDotnetDir + "\\" + text2 + str;
                if (File.Exists(text))
                {
                    return text;
                }
            }
            foreach (string str2 in array)
            {
                foreach (string str3 in this._mRefedFolders)
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
                DirectoryInfo directoryInfo = new DirectoryInfo(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                string path = directoryInfo.Parent.FullName + "\\Regression\\_RegressionTools\\";
                if (Directory.Exists(path))
                {
                    foreach (string text3 in Directory.GetFiles(path, "*.*", SearchOption.AllDirectories))
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
                throw new System.ArgumentException(e.ToString());
            }

            try
            {
                int num = assemName.IndexOf("XMLSerializers", StringComparison.OrdinalIgnoreCase);
                if (num != -1)
                {
                    assemName = "System.XML" + assemName.Substring(num + "XMLSerializers".Length);
                    return this.SearchAssemblyFileInOriginalFolders(assemName);
                }
            }
            catch (Exception e)
            {
                throw new System.ArgumentException(e.ToString());
            }
            return null;
        }

        private bool IsAPIReferenced(Assembly assembly)
        {
            string AssRevitName = "RevitAPI";
            if (string.IsNullOrEmpty(this._mRevitApiAssemblyFullName))
            {
                foreach (Assembly assembly2 in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (String.Compare(assembly2.GetName().Name,AssRevitName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this._mRevitApiAssemblyFullName = assembly2.GetName().Name;
                        break;
                    }
                }
            }
            foreach (AssemblyName assemblyName in assembly.GetReferencedAssemblies())
            {
                if (this._mRevitApiAssemblyFullName == assemblyName.Name)
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
}
