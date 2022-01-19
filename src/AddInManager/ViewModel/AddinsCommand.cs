using AddInManager.Model;

namespace AddInManager.ViewModel
{
    public class AddinsCommand : Addins
    {
        public void ReadItems(IniFile file)
        {
            int num = file.ReadInt("ExternalCommands", "ECCount");
            int i = 1;
            while (i <= num)
            {
                this.ReadExternalCommand(file, i++);
            }
            base.SortAddin();
        }

        private bool ReadExternalCommand(IniFile file, int nodeNumber)
        {
            string name = file.ReadString("ExternalCommands", "ECName" + nodeNumber);
            string text = file.ReadString("ExternalCommands", "ECAssembly" + nodeNumber);
            string text2 = file.ReadString("ExternalCommands", "ECClassName" + nodeNumber);
            string description = file.ReadString("ExternalCommands", "ECDescription" + nodeNumber);
            if (string.IsNullOrEmpty(text2) || string.IsNullOrEmpty(text))
            {
                return false;
            }
            base.AddItem(new AddinItem(AddinType.Command)
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
            file.Write("ExternalCommands", "ECCount", this.m_maxCount);
            int num = 0;
            foreach (Addin addin in this.m_addinDict.Values)
            {
                foreach (AddinItem addinItem in addin.ItemList)
                {
                    if (num >= this.m_maxCount)
                    {
                        break;
                    }
                    if (addinItem.Save)
                    {
                        this.WriteExternalCommand(file, addinItem, ++num);
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
}
