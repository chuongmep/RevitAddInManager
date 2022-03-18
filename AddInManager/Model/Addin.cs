using RevitAddinManager.Command;
using System.IO;

namespace RevitAddinManager.Model;

public class Addin : IAddinNode
{
    public List<AddinItem> ItemList
    {
        get => itemList;
        set => itemList = value;
    }

    public string FilePath
    {
        get => filePath;
        set => filePath = value;
    }

    public bool Save
    {
        get => save;
        set => save = value;
    }

    public bool Hidden
    {
        get => hidden;
        set => hidden = value;
    }

    public Addin(string filePath)
    {
        itemList = new List<AddinItem>();
        this.filePath = filePath;
        save = true;
    }

    public Addin(string filePath, List<AddinItem> itemList)
    {
        this.itemList = itemList;
        this.filePath = filePath;
        SortAddinItem();
        save = true;
    }

    public void SortAddinItem()
    {
        itemList.Sort(new AddinItemComparer());
    }

    public void RemoveItem(AddinItem item)
    {
        itemList.Remove(item);
        if (itemList.Count == 0)
        {
            AddinManagerBase.Instance.AddinManager.RemoveAddin(this);
        }
    }

    public void SaveToLocalIni(IniFile file)
    {
        if (itemList == null || itemList.Count == 0)
        {
            return;
        }
        var addinType = itemList[0].AddinType;
        if (addinType == AddinType.Command)
        {
            file.WriteSection("ExternalCommands");
            file.Write("ExternalCommands", "ECCount", 0);
            var num = 0;
            foreach (var addinItem in itemList)
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
        foreach (var item in itemList)
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
        if (itemList == null || itemList.Count == 0)
        {
            return;
        }
        var addinType = itemList[0].AddinType;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
        var manifestFile = new ManifestFile(fileNameWithoutExtension + DefaultSetting.FormatExAddin);
        if (addinType == AddinType.Application)
        {
            manifestFile.Applications = itemList;
        }
        else if (addinType == AddinType.Command)
        {
            manifestFile.Commands = itemList;
        }
        manifestFile.Save();
    }

    private List<AddinItem> itemList;

    private string filePath;

    private bool save;

    private bool hidden;
}