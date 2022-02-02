using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class AddinsApplication : Addins
{
    public static string ExternalName = "ExternalApplications";
    public void ReadItems(IniFile file)
    {
        var num = file.ReadInt(ExternalName, "EACount");
        var i = 1;
        while (i <= num)
        {
            ReadExternalApplication(file, i++);
        }

        SortAddin();
    }

    private bool ReadExternalApplication(IniFile file, int nodeNumber)
    {
        var text = file.ReadString(ExternalName, "EAClassName" + nodeNumber);
        var text2 = file.ReadString(ExternalName, "EAAssembly" + nodeNumber);
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
        file.Write(ExternalName, "EACount", _maxCount);
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
                    WriteExternalApplication(file, addinItem, ++num);
                }
            }
        }

        file.Write(ExternalName, "EACount", num);
    }

    private bool WriteExternalApplication(IniFile file, AddinItem item, int number)
    {
        file.Write(ExternalName, "EAClassName" + number, item.FullClassName);
        file.Write(ExternalName, "EAAssembly" + number, item.AssemblyPath);
        return true;
    }
}