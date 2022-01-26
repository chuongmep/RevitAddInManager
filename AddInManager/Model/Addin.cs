using System.IO;
using RevitAddinManager.Command;

namespace RevitAddinManager.Model;

public class Addin : IAddinNode
{
    public List<AddinItem> ItemList
    {
        get => _mItemList;
        set => _mItemList = value;
    }

    public string FilePath
    {
        get => _FilePath;
        set => _FilePath = value;
    }

    public bool Save
    {
        get => _mSave;
        set => _mSave = value;
    }

    public bool Hidden
    {
        get => _mHidden;
        set => _mHidden = value;
    }

    public Addin(string filePath)
    {
        _mItemList = new List<AddinItem>();
        _FilePath = filePath;
        _mSave = true;
    }

    public Addin(string filePath, List<AddinItem> itemList)
    {
        _mItemList = itemList;
        _FilePath = filePath;
        SortAddinItem();
        _mSave = true;
    }

    public void SortAddinItem()
    {
        _mItemList.Sort(new AddinItemComparer());
    }
    public void RemoveItem(AddinItem item)
    {
        _mItemList.Remove(item);
        if (_mItemList.Count == 0)
        {
            AddinManagerBase.Instance.AddinManager.RemoveAddin(this);
        }
    }

    public void SaveToLocalIni(IniFile file)
    {
        if (_mItemList == null || _mItemList.Count == 0)
        {
            return;
        }
        var addinType = _mItemList[0].AddinType;
        if (addinType == AddinType.Command)
        {
            file.WriteSection("ExternalCommands");
            file.Write("ExternalCommands", "ECCount", 0);
            var num = 0;
            foreach (var addinItem in _mItemList)
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
        foreach (var item in _mItemList)
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
        if (_mItemList == null || _mItemList.Count == 0)
        {
            return;
        }
        var addinType = _mItemList[0].AddinType;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_FilePath);
        var manifestFile = new ManifestFile(fileNameWithoutExtension + DefaultSetting.FormatExAddin);
        if (addinType == AddinType.Application)
        {
            manifestFile.Applications = _mItemList;
        }
        else if (addinType == AddinType.Command)
        {
            manifestFile.Commands = _mItemList;
        }
        manifestFile.Save();
    }

    private List<AddinItem> _mItemList;

    private string _FilePath;

    private bool _mSave;

    private bool _mHidden;
}