using System.Diagnostics;
using System.IO;
using System.Reflection;
using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel
{
    public class AddinManager
    {
        public AddinsApplication Applications => _mApplications;
        public int AppCount => _mApplications.Count;
        public AddinsCommand Commands => _mCommands;
        public int CmdCount => _mCommands.Count;


        public AddinManager()
        {
            _mCommands = new AddinsCommand();
            _mApplications = new AddinsApplication();
            GetIniFilePaths();
            ReadAddinsFromAimIni();
        }


        public IniFile AimIniFile
        {
            get => _mAimIniFile;
            set => _mAimIniFile = value;
        }

        public IniFile RevitIniFile
        {
            get => _mRevitIniFile;
            set => _mRevitIniFile = value;
        }

        private void GetIniFilePaths()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(folderPath, Resource.AppName);
            string filePath = Path.Combine(path, DefaultSetting.AimInternalName);
            _mAimIniFile = new IniFile(filePath);
            Process currentProcess = Process.GetCurrentProcess();
            string fileName = currentProcess.MainModule.FileName;
            string filePath2 = fileName.Replace(".exe", ".ini");
            _mRevitIniFile = new IniFile(filePath2);
        }

        public void ReadAddinsFromAimIni()
        {
            _mCommands.ReadItems(_mAimIniFile);
            _mApplications.ReadItems(_mAimIniFile);
        }

        public void RemoveAddin(Addin addin)
        {
            if (!_mCommands.RemoveAddIn(addin))
            {
                _mApplications.RemoveAddIn(addin);
            }
        }

        public AddinType LoadAddin(string filePath, AssemLoader assemLoader)
        {
            AddinType addinType = AddinType.Invalid;
            if (!File.Exists(filePath))
            {
                return addinType;
            }
            //TODO : Need Quick Check why assembly it not load in some case
            //AssemLoader assemLoader = new AssemLoader();
            List<AddinItem> list = null;
            List<AddinItem> list2 = null;
            try
            {
                assemLoader.HookAssemblyResolve();

                Assembly assembly = assemLoader.LoadAddinsToTempFolder(filePath, true);
                list = _mCommands.LoadItems(assembly, StaticUtil.CommandFullName, filePath, AddinType.Command);
                list2 = _mApplications.LoadItems(assembly, StaticUtil.AppFullName, filePath, AddinType.Application);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                assemLoader.UnhookAssemblyResolve();
            }
            if (list != null && list.Count > 0)
            {
                Addin addin = new Addin(filePath, list);
                _mCommands.AddAddIn(addin);
                addinType |= AddinType.Command;
            }
            if (list2 != null && list2.Count > 0)
            {
                Addin addin2 = new Addin(filePath, list2);
                _mApplications.AddAddIn(addin2);
                addinType |= AddinType.Application;
            }
            return addinType;
        }

        public void SaveToRevitIni()
        {
            if (!File.Exists(_mRevitIniFile.FilePath))
            {
                throw new FileNotFoundException("can't find the revit.ini file from: " + _mRevitIniFile.FilePath);
            }
            _mCommands.Save(_mRevitIniFile);
            _mApplications.Save(_mRevitIniFile);
        }

        public void SaveToLocal()
        {
            SaveToLocalManifest();
        }

        public void SaveToLocalRevitIni()
        {
            foreach (KeyValuePair<string, Addin> keyValuePair in _mCommands.AddinDict)
            {
                string key = keyValuePair.Key;
                Addin value = keyValuePair.Value;
                string directoryName = Path.GetDirectoryName(value.FilePath);
                if (string.IsNullOrEmpty(directoryName))
                {
                    MessageBox.Show(@"Directory Not Found");
                    return;
                }
                IniFile file = new IniFile(Path.Combine(directoryName, DefaultSetting.IniName));
                value.SaveToLocalIni(file);
                if (_mApplications.AddinDict.ContainsKey(key))
                {
                    Addin addin = _mApplications.AddinDict[key];
                    addin.SaveToLocalIni(file);
                }
            }
        }

        public void SaveToAimIni()
        {
            if (!File.Exists(AimIniFile.FilePath))
            {
                new FileInfo(AimIniFile.FilePath).Create();
                FileUtils.SetWriteable(AimIniFile.FilePath);
            }
            _mCommands.Save(_mAimIniFile);
            _mApplications.Save(_mAimIniFile);
        }

        public bool HasItemsToSave()
        {
            foreach (Addin addin in _mCommands.AddinDict.Values)
            {
                if (addin.Save)
                {
                    return true;
                }
            }
            foreach (Addin addin2 in _mApplications.AddinDict.Values)
            {
                if (addin2.Save)
                {
                    return true;
                }
            }
            return false;
        }

        public List<string> SaveToAllUserManifest(AddInManagerViewModel vm)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            List<string> filePaths = new List<string>();
            List<string> folders = new List<string>();
            if (!vm.IsCurrentVersion)
            {
                string[] Directories = Directory.GetDirectories(Path.Combine(folderPath, DefaultSetting.AdskPath), "*",
                    SearchOption.TopDirectoryOnly);
                foreach (string Directory in Directories) folders.Add(Directory);
            }
            else
            {
                string folder = Path.Combine(folderPath, DefaultSetting.AdskPath,
                    vm.ExternalCommandData.Application.Application.VersionNumber);
                folders.Add(folder);
            }

            ManifestFile manifestFile = new ManifestFile(false){VendorDescription = vm.VendorDescription};
            if (vm.IsTabCmdSelected)
            {
                foreach (AddinModel parrent in vm.CommandItems)
                {
                    foreach (AddinModel chidrent in parrent.Children)
                    {
                        if (chidrent.IsChecked == true) manifestFile.Commands.Add(chidrent.AddinItem);
                    }
                }
            }
            else if(vm.IsTabAppSelected)
            {
                foreach (AddinModel parrent in vm.ApplicationItems)
                {
                    foreach (AddinModel chidrent in parrent.Children)
                    {
                        if (chidrent.IsChecked == true) manifestFile.Applications.Add(chidrent.AddinItem);
                    }
                }

            }
            foreach (string folder in folders)
            {
                string filePath = GetProperFilePath(folder, DefaultSetting.FileName, DefaultSetting.FormatExAddin);
                manifestFile.SaveAs(filePath);
                filePaths.Add(filePath);
            }

            return filePaths;
        }

        public void SaveToLocalManifest()
        {
            Dictionary<string, Addin> dictionary = new Dictionary<string, Addin>();
            Dictionary<string, Addin> dictionary2 = new Dictionary<string, Addin>();
            foreach (KeyValuePair<string, Addin> keyValuePair in _mCommands.AddinDict)
            {
                string key = keyValuePair.Key;
                Addin value = keyValuePair.Value;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(value.FilePath);
                string directoryName = Path.GetDirectoryName(value.FilePath);
                string filePath = Path.Combine(directoryName, fileNameWithoutExtension + DefaultSetting.FormatExAddin);
                ManifestFile manifestFile = new ManifestFile(true);
                foreach (AddinItem addinItem in value.ItemList)
                {
                    if (addinItem.Save)
                    {
                        manifestFile.Commands.Add(addinItem);
                    }
                }
                if (_mApplications.AddinDict.ContainsKey(key))
                {
                    Addin addin = _mApplications.AddinDict[key];
                    foreach (AddinItem addinItem2 in addin.ItemList)
                    {
                        if (addinItem2.Save)
                        {
                            manifestFile.Applications.Add(addinItem2);
                        }
                    }
                    dictionary.Add(key, _mApplications.AddinDict[key]);
                }
                manifestFile.SaveAs(filePath);
            }
            foreach (KeyValuePair<string, Addin> keyValuePair2 in _mApplications.AddinDict)
            {
                string key2 = keyValuePair2.Key;
                Addin value2 = keyValuePair2.Value;
                if (!dictionary.ContainsKey(key2))
                {
                    string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(value2.FilePath);
                    string directoryName2 = Path.GetDirectoryName(value2.FilePath);
                    string filePath2 = Path.Combine(directoryName2, fileNameWithoutExtension2 + DefaultSetting.FormatExAddin);
                    ManifestFile manifestFile2 = new ManifestFile(true);
                    foreach (AddinItem addinItem3 in value2.ItemList)
                    {
                        if (addinItem3.Save)
                        {
                            manifestFile2.Applications.Add(addinItem3);
                        }
                    }
                    if (_mCommands.AddinDict.ContainsKey(key2))
                    {
                        Addin addin2 = _mCommands.AddinDict[key2];
                        foreach (AddinItem addinItem4 in addin2.ItemList)
                        {
                            if (addinItem4.Save)
                            {
                                manifestFile2.Commands.Add(addinItem4);
                            }
                        }
                        dictionary2.Add(key2, _mCommands.AddinDict[key2]);
                    }
                    manifestFile2.SaveAs(filePath2);
                }
            }
        }

        private string GetProperFilePath(string folder, string fileNameWithoutExt, string ext)
        {
            string text;
            int num = -1;
            do
            {
                num++;
                string path = (num <= 0) ? (fileNameWithoutExt + ext) : (fileNameWithoutExt + num + ext);
                text = Path.Combine(folder, path);
            }
            while (File.Exists(text));
            return text;
        }

        private readonly AddinsApplication _mApplications;

        private readonly AddinsCommand _mCommands;

        private IniFile _mAimIniFile;

        private IniFile _mRevitIniFile;
    }
}
