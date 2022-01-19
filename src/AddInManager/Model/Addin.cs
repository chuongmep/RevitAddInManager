using System.Collections.Generic;
using System.IO;
using AddInManager.Command;

namespace AddInManager.Model
{
    public class Addin : IAddinNode
    {
        public List<AddinItem> ItemList
        {
            get => this.m_itemList;
            set => this.m_itemList = value;
        }

        public string FilePath
        {
            get => this.m_filePath;
            set => this.m_filePath = value;
        }

        public bool Save
        {
            get => this.m_save;
            set => this.m_save = value;
        }

        public bool Hidden
        {
            get => this.m_hidden;
            set => this.m_hidden = value;
        }

        public Addin(string filePath)
        {
            this.m_itemList = new List<AddinItem>();
            this.m_filePath = filePath;
            this.m_save = true;
        }

        public Addin(string filePath, List<AddinItem> itemList)
        {
            this.m_itemList = itemList;
            this.m_filePath = filePath;
            this.SortAddinItem();
            this.m_save = true;
        }

        public void SortAddinItem()
        {
            this.m_itemList.Sort(new AddinItemComparer());
        }
        public void RemoveItem(AddinItem item)
        {
            this.m_itemList.Remove(item);
            if (this.m_itemList.Count == 0)
            {
                AddinManagerBase.Instance.AddinManager.RemoveAddin(this);
            }
        }

        public void SaveToLocalIni(IniFile file)
        {
            if (this.m_itemList == null || this.m_itemList.Count == 0)
            {
                return;
            }
            AddinType addinType = this.m_itemList[0].AddinType;
            if (addinType == AddinType.Command)
            {
                file.WriteSection("ExternalCommands");
                file.Write("ExternalCommands", "ECCount", 0);
                int num = 0;
                foreach (AddinItem addinItem in this.m_itemList)
                {
                    if (addinItem.Save)
                    {
                        this.WriteExternalCommand(file, addinItem, ++num);
                    }
                }
                file.Write("ExternalCommands", "ECCount", num);
                return;
            }
            file.WriteSection("ExternalApplications");
            file.Write("ExternalApplications", "EACount", 0);
            int num2 = 0;
            foreach (AddinItem item in this.m_itemList)
            {
                this.WriteExternalApplication(file, item, ++num2);
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
            if (this.m_itemList == null || this.m_itemList.Count == 0)
            {
                return;
            }
            AddinType addinType = this.m_itemList[0].AddinType;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this.m_filePath);
            ManifestFile manifestFile = new ManifestFile(fileNameWithoutExtension + DefaultSetting.FormatExAddin);
            if (addinType == AddinType.Application)
            {
                manifestFile.Applications = this.m_itemList;
            }
            else if (addinType == AddinType.Command)
            {
                manifestFile.Commands = this.m_itemList;
            }
            manifestFile.Save();
        }

        private List<AddinItem> m_itemList;

        private string m_filePath;

        private bool m_save;

        private bool m_hidden;
    }
}
