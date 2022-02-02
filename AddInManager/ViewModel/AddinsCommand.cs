using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class AddinsCommand : Addins
{
    public static string ExternalName = "ExternalCommands";
    public void ReadItems(IniFile file)
    {
        var num = file.ReadInt(ExternalName, "ECCount");
        var i = 1;
        while (i <= num)
        {
            ReadExternalCommand(file, i++);
        }
        SortAddin();
    }

    private bool ReadExternalCommand(IniFile file, int nodeNumber)
    {
        var name = file.ReadString(ExternalName, "ECName" + nodeNumber);
        var text = file.ReadString(ExternalName, "ECAssembly" + nodeNumber);
        var text2 = file.ReadString(ExternalName, "ECClassName" + nodeNumber);
        var description = file.ReadString(ExternalName, "ECDescription" + nodeNumber);
        if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text))
        {
            return false;
        }
        AddItem(new AddinItem(AddinType.Command)
        {
            Name = name,
            AssemblyPath = text,
            FullClassName = text2,
            Description = description
        });
        return true;
    }

    public void Save(IniFile file)
    {
        file.WriteSection(ExternalName);
        file.Write(ExternalName, "ECCount", _maxCount);
        var num = 0;
        foreach (var addin in addinDict.Values)
        {
            foreach (var addinItem in addin.ItemList)
            {
                if (num >= _maxCount)
                {
                    break;
                }
                if (addinItem.Save)
                {
                    WriteExternalCommand(file, addinItem, ++num);
                }
            }
        }
        file.Write(ExternalName, "ECCount", num);
    }

    private bool WriteExternalCommand(IniFile file, AddinItem item, int number)
    {
        file.Write(ExternalName, "ECName" + number, item.Name);
        file.Write(ExternalName, "ECClassName" + number, item.FullClassName);
        file.Write(ExternalName, "ECAssembly" + number, item.AssemblyPath);
        file.Write(ExternalName, "ECDescription" + number, item.Description);
        return true;
    }
}