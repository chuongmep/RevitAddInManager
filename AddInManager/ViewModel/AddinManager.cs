using RevitAddinManager.Model;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace RevitAddinManager.ViewModel;

public class AddinManager
{
    public AddinsApplication Applications => applications;
    public int AppCount => applications.Count;
    public AddinsCommand Commands => commands;
    public int CmdCount => commands.Count;

    public AddinManager()
    {
        commands = new AddinsCommand();
        applications = new AddinsApplication();
        GetIniFilePaths();
        ReadAddinsFromAimIni();
    }

    private IniFile AimIniFile => aimIniFile;

    public IniFile RevitIniFile
    {
        get => revitIniFile;
        set => revitIniFile = value;
    }

    private void GetIniFilePaths()
    {
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var path = Path.Combine(folderPath, Resource.AppName);
        var filePath = Path.Combine(path, DefaultSetting.AimInternalName);
        aimIniFile = new IniFile(filePath);
        var currentProcess = Process.GetCurrentProcess();
        var fileName = currentProcess.MainModule?.FileName;
        var filePath2 = fileName?.Replace(".exe", ".ini");
        revitIniFile = new IniFile(filePath2);
    }

    private void ReadAddinsFromAimIni()
    {
        commands.ReadItems(aimIniFile);
        applications.ReadItems(aimIniFile);
    }

    public void RemoveAddin(Addin addin)
    {
        if (!commands.RemoveAddIn(addin))
        {
            applications.RemoveAddIn(addin);
        }
    }

    public AddinType LoadAddin(string filePath, AssemLoader assemLoader)
    {
        var addinType = AddinType.Invalid;
        if (!File.Exists(filePath))
        {
            return addinType;
        }
        List<AddinItem> commandItems = null;
        List<AddinItem> appItems = null;
        try
        {
            assemLoader.HookAssemblyResolve();

            var assembly = assemLoader.LoadAddinsToTempFolder(filePath, true);
            commandItems = commands.LoadItems(assembly, StaticUtil.CommandFullName, filePath, AddinType.Command);
            appItems = applications.LoadItems(assembly, StaticUtil.AppFullName, filePath, AddinType.Application);
        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
        finally
        {
            assemLoader.UnhookAssemblyResolve();
        }
        if (commandItems != null && commandItems.Count > 0)
        {
            var addin = new Addin(filePath, commandItems);
            commands.AddAddIn(addin);
            addinType |= AddinType.Command;
        }
        if (appItems != null && appItems.Count > 0)
        {
            var addin2 = new Addin(filePath, appItems);
            applications.AddAddIn(addin2);
            addinType |= AddinType.Application;
        }
        return addinType;
    }

    public void SaveToRevitIni()
    {
        if (!File.Exists(revitIniFile.FilePath))
        {
            throw new FileNotFoundException("can't find the revit.ini file from: " + revitIniFile.FilePath);
        }
        commands.Save(revitIniFile);
        applications.Save(revitIniFile);
    }

    public void SaveAsLocal(AddInManagerViewModel vm, string filepath)
    {
        ManifestFile manifestFile = AddManifestFile(vm);
        manifestFile.SaveAs(filepath);
    }

    public void SaveToLocal()
    {
        SaveToLocalManifest();
    }

    public void SaveToLocalRevitIni()
    {
        foreach (var keyValuePair in commands.AddinDict)
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
            if (applications.AddinDict.ContainsKey(key))
            {
                var addin = applications.AddinDict[key];
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
        commands.Save(aimIniFile);
        applications.Save(aimIniFile);
    }

    public bool HasItemsToSave()
    {
        foreach (var addin in commands.AddinDict.Values)
        {
            if (addin.Save)
            {
                return true;
            }
        }
        foreach (var addin2 in applications.AddinDict.Values)
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

        ManifestFile manifestFile = AddManifestFile(vm);
        foreach (var folder in folders)
        {
            var filePath = GetProperFilePath(folder, DefaultSetting.FileName, DefaultSetting.FormatExAddin);
            manifestFile.SaveAs(filePath);
            filePaths.Add(filePath);
        }

        return filePaths;
    }

    private ManifestFile AddManifestFile(AddInManagerViewModel vm)
    {
        var manifestFile = new ManifestFile(false) { VendorDescription = vm.VendorDescription };
        if (vm.IsTabCmdSelected)
        {
            foreach (var parent in vm.CommandItems)
            {
                foreach (var children in parent.Children)
                {
                    if (children.IsChecked == true) manifestFile.Commands.Add(children.AddinItem);
                }
            }
        }
        else if (vm.IsTabAppSelected)
        {
            foreach (var parent in vm.ApplicationItems)
            {
                foreach (var children in parent.Children)
                {
                    if (children.IsChecked == true) manifestFile.Applications.Add(children.AddinItem);
                }
            }
        }

        return manifestFile;
    }

    private void SaveToLocalManifest()
    {
        var dictionary = new Dictionary<string, Addin>();
        var dictionary2 = new Dictionary<string, Addin>();
        foreach (var keyValuePair in commands.AddinDict)
        {
            var key = keyValuePair.Key;
            var value = keyValuePair.Value;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(value.FilePath);
            var directoryName = Path.GetDirectoryName(value.FilePath);
            if (string.IsNullOrEmpty(directoryName)) throw new ArgumentNullException(nameof(directoryName));
            var filePath = Path.Combine(directoryName, fileNameWithoutExtension + DefaultSetting.FormatExAddin);
            var manifestFile = new ManifestFile(true);
            foreach (var addinItem in value.ItemList)
            {
                if (addinItem.Save)
                {
                    manifestFile.Commands.Add(addinItem);
                }
            }
            if (applications.AddinDict.ContainsKey(key))
            {
                var addin = applications.AddinDict[key];
                foreach (var addinItem2 in addin.ItemList)
                {
                    if (addinItem2.Save)
                    {
                        manifestFile.Applications.Add(addinItem2);
                    }
                }
                dictionary.Add(key, applications.AddinDict[key]);
            }
            manifestFile.SaveAs(filePath);
        }
        foreach (var keyValuePair2 in applications.AddinDict)
        {
            var key2 = keyValuePair2.Key;
            var value2 = keyValuePair2.Value;
            if (!dictionary.ContainsKey(key2))
            {
                var fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(value2.FilePath);
                var directoryName2 = Path.GetDirectoryName(value2.FilePath);
                if (string.IsNullOrEmpty(directoryName2)) throw new ArgumentNullException(nameof(directoryName2));
                var filePath2 = Path.Combine(directoryName2, fileNameWithoutExtension2 + DefaultSetting.FormatExAddin);
                var manifestFile2 = new ManifestFile(true);
                foreach (var addinItem3 in value2.ItemList)
                {
                    if (addinItem3.Save)
                    {
                        manifestFile2.Applications.Add(addinItem3);
                    }
                }
                if (commands.AddinDict.ContainsKey(key2))
                {
                    var addin2 = commands.AddinDict[key2];
                    foreach (var addinItem4 in addin2.ItemList)
                    {
                        if (addinItem4.Save)
                        {
                            manifestFile2.Commands.Add(addinItem4);
                        }
                    }
                    dictionary2.Add(key2, commands.AddinDict[key2]);
                }
                manifestFile2.SaveAs(filePath2);
            }
        }
    }

    private string GetProperFilePath(string folder, string fileNameWithoutExt, string ext)
    {
        string filePath;
        var num = -1;
        do
        {
            num++;
            var path = num <= 0 ? fileNameWithoutExt + ext : fileNameWithoutExt + num + ext;
            filePath = Path.Combine(folder, path);
        }
        while (File.Exists(filePath));
        return filePath;
    }

    private readonly AddinsApplication applications;

    private readonly AddinsCommand commands;

    private IniFile aimIniFile;

    private IniFile revitIniFile;
}