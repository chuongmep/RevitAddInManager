using System.Diagnostics;
using System.IO;
using System.Reflection;
using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

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
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(folderPath, Resource.AppName);
        var filePath = Path.Combine(path, DefaultSetting.AimInternalName);
        _mAimIniFile = new IniFile(filePath);
        var currentProcess = Process.GetCurrentProcess();
        var fileName = currentProcess.MainModule.FileName;
        var filePath2 = fileName.Replace(".exe", ".ini");
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
        var addinType = AddinType.Invalid;
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

            var assembly = assemLoader.LoadAddinsToTempFolder(filePath, true);
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
            var addin = new Addin(filePath, list);
            _mCommands.AddAddIn(addin);
            addinType |= AddinType.Command;
        }
        if (list2 != null && list2.Count > 0)
        {
            var addin2 = new Addin(filePath, list2);
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
        foreach (var keyValuePair in _mCommands.AddinDict)
        {
            var key = keyValuePair.Key;
            var value = keyValuePair.Value;
            var directoryName = Path.GetDirectoryName(value.FilePath);
            if (string.IsNullOrEmpty(directoryName))
            {
                MessageBox.Show(@"Directory Not Found");
                return;
            }
            var file = new IniFile(Path.Combine(directoryName, DefaultSetting.IniName));
            value.SaveToLocalIni(file);
            if (_mApplications.AddinDict.ContainsKey(key))
            {
                var addin = _mApplications.AddinDict[key];
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
        foreach (var addin in _mCommands.AddinDict.Values)
        {
            if (addin.Save)
            {
                return true;
            }
        }
        foreach (var addin2 in _mApplications.AddinDict.Values)
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
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var filePaths = new List<string>();
        var folders = new List<string>();
        if (!vm.IsCurrentVersion)
        {
            var Directories = Directory.GetDirectories(Path.Combine(folderPath, DefaultSetting.AdskPath), "*",
                SearchOption.TopDirectoryOnly);
            foreach (var Directory in Directories) folders.Add(Directory);
        }
        else
        {
            var folder = Path.Combine(folderPath, DefaultSetting.AdskPath,
                vm.ExternalCommandData.Application.Application.VersionNumber);
            folders.Add(folder);
        }

        var manifestFile = new ManifestFile(false){VendorDescription = vm.VendorDescription};
        if (vm.IsTabCmdSelected)
        {
            foreach (var parrent in vm.CommandItems)
            {
                foreach (var chidrent in parrent.Children)
                {
                    if (chidrent.IsChecked == true) manifestFile.Commands.Add(chidrent.AddinItem);
                }
            }
        }
        else if(vm.IsTabAppSelected)
        {
            foreach (var parrent in vm.ApplicationItems)
            {
                foreach (var chidrent in parrent.Children)
                {
                    if (chidrent.IsChecked == true) manifestFile.Applications.Add(chidrent.AddinItem);
                }
            }

        }
        foreach (var folder in folders)
        {
            var filePath = GetProperFilePath(folder, DefaultSetting.FileName, DefaultSetting.FormatExAddin);
            manifestFile.SaveAs(filePath);
            filePaths.Add(filePath);
        }

        return filePaths;
    }

    public void SaveToLocalManifest()
    {
        var dictionary = new Dictionary<string, Addin>();
        var dictionary2 = new Dictionary<string, Addin>();
        foreach (var keyValuePair in _mCommands.AddinDict)
        {
            var key = keyValuePair.Key;
            var value = keyValuePair.Value;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(value.FilePath);
            var directoryName = Path.GetDirectoryName(value.FilePath);
            var filePath = Path.Combine(directoryName, fileNameWithoutExtension + DefaultSetting.FormatExAddin);
            var manifestFile = new ManifestFile(true);
            foreach (var addinItem in value.ItemList)
            {
                if (addinItem.Save)
                {
                    manifestFile.Commands.Add(addinItem);
                }
            }
            if (_mApplications.AddinDict.ContainsKey(key))
            {
                var addin = _mApplications.AddinDict[key];
                foreach (var addinItem2 in addin.ItemList)
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
        foreach (var keyValuePair2 in _mApplications.AddinDict)
        {
            var key2 = keyValuePair2.Key;
            var value2 = keyValuePair2.Value;
            if (!dictionary.ContainsKey(key2))
            {
                var fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(value2.FilePath);
                var directoryName2 = Path.GetDirectoryName(value2.FilePath);
                var filePath2 = Path.Combine(directoryName2, fileNameWithoutExtension2 + DefaultSetting.FormatExAddin);
                var manifestFile2 = new ManifestFile(true);
                foreach (var addinItem3 in value2.ItemList)
                {
                    if (addinItem3.Save)
                    {
                        manifestFile2.Applications.Add(addinItem3);
                    }
                }
                if (_mCommands.AddinDict.ContainsKey(key2))
                {
                    var addin2 = _mCommands.AddinDict[key2];
                    foreach (var addinItem4 in addin2.ItemList)
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
        var num = -1;
        do
        {
            num++;
            var path = num <= 0 ? fileNameWithoutExt + ext : fileNameWithoutExt + num + ext;
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