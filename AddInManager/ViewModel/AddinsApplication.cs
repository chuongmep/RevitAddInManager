using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class AddinsApplication : Addins
{
    private static string ExternalName = "ExternalApplications";
    private static string ExternalCount = "EACount";
    private static string ExternalClassName = "EAClassName";
    private static string ExternalAssembly = "EAAssembly";

    public void ReadItems(IniFile file)
    {
        var num = file.ReadInt(ExternalName, ExternalCount);
        var i = 1;
        while (i <= num)
        {
            ReadExternalApplication(file, i++);
        }

        SortAddin();
    }

    private bool ReadExternalApplication(IniFile file, int nodeNumber)
    {
        var text = file.ReadString(ExternalName, ExternalClassName + nodeNumber);
        var text2 = file.ReadString(ExternalName, ExternalAssembly + nodeNumber);
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text2))
        {
            return false;
        }

        AddItem(new AddinItem(AddinType.Application)
        {
            Name = string.Empty,
            AssemblyPath = text2,
            FullClassName = text
        });
        return true;
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
                    WriteExternalApplication(file, addinItem, ++num);
                }
            }
        }

        file.Write(ExternalName, ExternalCount, num);
    }

    private bool WriteExternalApplication(IniFile file, AddinItem item, int number)
    {
        file.Write(ExternalName, ExternalClassName + number, item.FullClassName);
        file.Write(ExternalName, ExternalAssembly + number, item.AssemblyPath);
        return true;
    }
}