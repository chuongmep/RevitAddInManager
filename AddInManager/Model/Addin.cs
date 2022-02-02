using System.IO;
using RevitAddinManager.Command;

namespace RevitAddinManager.Model;

public class Addin : IAddinNode
{
    public List<AddinItem> ItemList
    {
        get => _itemList;
        set => _itemList = value;
    }

    public string FilePath
    {
        get => _filePath;
        set => _filePath = value;
    }

    public bool Save
    {
        get => _save;
        set => _save = value;
    }

    public bool Hidden
    {
        get => _hidden;
        set => _hidden = value;
    }

    public Addin(string filePath)
    {
        _itemList = new List<AddinItem>();
        _filePath = filePath;
        _save = true;
    }

    public Addin(string filePath, List<AddinItem> itemList)
    {
        _itemList = itemList;
        _filePath = filePath;
        SortAddinItem();
        _save = true;
    }

    public void SortAddinItem()
    {
        _itemList.Sort(new AddinItemComparer());
    }
    public void RemoveItem(AddinItem item)
    {
        _itemList.Remove(item);
        if (_itemList.Count == 0)
        {
            AddinManagerBase.Instance.AddinManager.RemoveAddin(this);
        }
    }

    public void SaveToLocalIni(IniFile file)
    {
        if (_itemList == null || _itemList.Count == 0)
        {
            return;
        }
        var addinType = _itemList[0].AddinType;
        if (addinType == AddinType.Command)
        {
            file.WriteSection("ExternalCommands");
            file.Write("ExternalCommands", "ECCount", 0);
            var num = 0;
            foreach (var addinItem in _itemList)
            {
                if (addinItem.Save)
                {
                    WriteExternalCommand(file, addinItem, ++num);
                }
            }
            file.Write("ExternalCommands", "ECCount", num);
            return;
        }
        file.WriteSection("ExternalApplications");
        file.Write("ExternalApplications", "EACount", 0);
        var num2 = 0;
        foreach (var item in _itemList)
        {
            WriteExternalApplication(file, item, ++num2);
        }
        file.Write("ExternalApplications", "EACount", num2);
    }

    private void WriteExternalCommand(IniFile file, AddinItem item, int number)
    {
        file.Write("ExternalCommands", "ECName" + number, item.Name);
        file.Write("ExternalCommands", "ECClassName" + number, item.FullClassName);
        file.Write("ExternalCommands", "ECAssembly" + number, item.AssemblyName);
        file.Write("ExternalCommands", "ECDescription" + number, item.Description);
    }

    private void WriteExternalApplication(IniFile file, AddinItem item, int number)
    {
        file.Write("ExternalApplications", "EAClassName" + number, item.FullClassName);
        file.Write("ExternalApplications", "EAAssembly" + number, item.AssemblyName);
    }

    public void SaveToLocalManifest()
    {
        if (_itemList == null || _itemList.Count == 0)
        {
            return;
        }
        var addinType = _itemList[0].AddinType;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_filePath);
        var manifestFile = new ManifestFile(fileNameWithoutExtension + DefaultSetting.FormatExAddin);
        if (addinType == AddinType.Application)
        {
            manifestFile.Applications = _itemList;
        }
        else if (addinType == AddinType.Command)
        {
            manifestFile.Commands = _itemList;
        }
        manifestFile.Save();
    }

    private List<AddinItem> _itemList;

    private string _filePath;

    private bool _save;

    private bool _hidden;
}