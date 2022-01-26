using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel;

public class AddinsCommand : Addins
{
    public void ReadItems(IniFile file)
    {
        var num = file.ReadInt("ExternalCommands", "ECCount");
        var i = 1;
        while (i <= num)
        {
            ReadExternalCommand(file, i++);
        }
        SortAddin();
    }

    private bool ReadExternalCommand(IniFile file, int nodeNumber)
    {
        var name = file.ReadString("ExternalCommands", "ECName" + nodeNumber);
        var text = file.ReadString("ExternalCommands", "ECAssembly" + nodeNumber);
        var text2 = file.ReadString("ExternalCommands", "ECClassName" + nodeNumber);
        var description = file.ReadString("ExternalCommands", "ECDescription" + nodeNumber);
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
        file.WriteSection("ExternalCommands");
        file.Write("ExternalCommands", "ECCount", m_maxCount);
        var num = 0;
        foreach (var addin in m_addinDict.Values)
        {
            foreach (var addinItem in addin.ItemList)
            {
                if (num >= m_maxCount)
                {
                    break;
                }
                if (addinItem.Save)
                {
                    WriteExternalCommand(file, addinItem, ++num);
                }
            }
        }
        file.Write("ExternalCommands", "ECCount", num);
    }

    private bool WriteExternalCommand(IniFile file, AddinItem item, int number)
    {
        file.Write("ExternalCommands", "ECName" + number, item.Name);
        file.Write("ExternalCommands", "ECClassName" + number, item.FullClassName);
        file.Write("ExternalCommands", "ECAssembly" + number, item.AssemblyPath);
        file.Write("ExternalCommands", "ECDescription" + number, item.Description);
        return true;
    }
}