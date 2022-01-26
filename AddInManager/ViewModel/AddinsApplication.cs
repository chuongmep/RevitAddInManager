using RevitAddinManager.Model;

namespace RevitAddinManager.ViewModel
{
    public class AddinsApplication : Addins
    {
        public void ReadItems(IniFile file)
        {
            int num = file.ReadInt("ExternalApplications", "EACount");
            int i = 1;
            while (i <= num)
            {
                ReadExternalApplication(file, i++);
            }

            SortAddin();
        }

        private bool ReadExternalApplication(IniFile file, int nodeNumber)
        {
            string text = file.ReadString("ExternalApplications", "EAClassName" + nodeNumber);
            string text2 = file.ReadString("ExternalApplications", "EAAssembly" + nodeNumber);
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
            file.WriteSection("ExternalApplications");
            file.Write("ExternalApplications", "EACount", m_maxCount);
            int num = 0;
            foreach (Addin addin in m_addinDict.Values)
            {
                foreach (AddinItem addinItem in addin.ItemList)
                {
                    if (num >= m_maxCount)
                    {
                        break;
                    }

                    if (addinItem.Save)
                    {
                        WriteExternalApplication(file, addinItem, ++num);
                    }
                }
            }

            file.Write("ExternalApplications", "EACount", num);
        }

        private bool WriteExternalApplication(IniFile file, AddinItem item, int number)
        {
            file.Write("ExternalApplications", "EAClassName" + number, item.FullClassName);
            file.Write("ExternalApplications", "EAAssembly" + number, item.AssemblyPath);
            return true;
        }
    }
}