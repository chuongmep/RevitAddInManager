using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class AddinsCommand : Addins
{
    private static string ExternalName = "ExternalCommands";
    private static string ExternalCName = "ECName";
    private static string ExternalCount = "ECCount";
    private static string ExternalClassName = "ECClassName";
    private static string ExternalAssembly = "ECAssembly";
    private static string ExternalDescription = "ECDescription";

    public void ReadItems(IniFile file)
    {
        var num = file.ReadInt(ExternalName, ExternalCount);
        var i = 1;
        while (i <= num)
        {
            ReadExternalCommand(file, i++);
        }
        SortAddin();
    }

    void ReadExternalCommand(IniFile file, int nodeNumber)
    {
        var name = file.ReadString(ExternalName, ExternalCName + nodeNumber);
        var text = file.ReadString(ExternalName, ExternalAssembly + nodeNumber);
        var text2 = file.ReadString(ExternalName, ExternalClassName + nodeNumber);
        var description = file.ReadString(ExternalName, ExternalDescription + nodeNumber);
        if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text))
        {
            return;
        }
        AddItem(new AddinItem(AddinType.Command)
        {
            Name = name,
            AssemblyPath = text,
            FullClassName = text2,
            Description = description
        });
    }

    public void Save(IniFile file)
    {
        file.WriteSection(ExternalName);
        file.Write(ExternalName, ExternalCount, maxCount);
        var num = 0;
        foreach (var addin in addinDict.Values)
        {
            foreach (var addinItem in addin.ItemList)
            {
                if (num >= maxCount)
                {
                    break;
                }
                if (addinItem.Save)
                {
                    WriteExternalCommand(file, addinItem, ++num);
                }
            }
        }
        file.Write(ExternalName, ExternalCount, num);
    }

    private bool WriteExternalCommand(IniFile file, AddinItem item, int number)
    {
        file.Write(ExternalName, ExternalCName + number, item.Name);
        file.Write(ExternalName, ExternalClassName + number, item.FullClassName);
        file.Write(ExternalName, ExternalAssembly + number, item.AssemblyPath);
        file.Write(ExternalName, ExternalDescription + number, item.Description);
        return true;
    }
}