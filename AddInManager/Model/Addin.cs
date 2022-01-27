using System.IO;
using RevitAddinManager.Command;

namespace RevitAddinManager.Model
{
    public class Addin : IAddinNode
    {
        public List<AddinItem> ItemList
        {
            get => this._mItemList;
            set => this._mItemList = value;
        }

        public string FilePath
        {
            get => this._FilePath;
            set => this._FilePath = value;
        }

        public bool Save
        {
            get => this._mSave;
            set => this._mSave = value;
        }

        public bool Hidden
        {
            get => this._mHidden;
            set => this._mHidden = value;
        }

        public Addin(string filePath)
        {
            this._mItemList = new List<AddinItem>();
            this._FilePath = filePath;
            this._mSave = true;
        }

        public Addin(string filePath, List<AddinItem> itemList)
        {
            this._mItemList = itemList;
            this._FilePath = filePath;
            this.SortAddinItem();
            this._mSave = true;
        }

        public void SortAddinItem()
        {
            this._mItemList.Sort(new AddinItemComparer());
        }
        public void RemoveItem(AddinItem item)
        {
            this._mItemList.Remove(item);
            if (this._mItemList.Count == 0)
            {
                AddinManagerBase.Instance.AddinManager.RemoveAddin(this);
            }
        }

        public void SaveToLocalIni(IniFile file)
        {
            if (this._mItemList == null || this._mItemList.Count == 0)
            {
                return;
            }
            AddinType addinType = this._mItemList[0].AddinType;
            if (addinType == AddinType.Command)
            {
                file.WriteSection("ExternalCommands");
                file.Write("ExternalCommands", "ECCount", 0);
                int num = 0;
                foreach (AddinItem addinItem in this._mItemList)
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
            foreach (AddinItem item in this._mItemList)
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
            if (this._mItemList == null || this._mItemList.Count == 0)
            {
                return;
            }
            AddinType addinType = this._mItemList[0].AddinType;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(this._FilePath);
            ManifestFile manifestFile = new ManifestFile(fileNameWithoutExtension + DefaultSetting.FormatExAddin);
            if (addinType == AddinType.Application)
            {
                manifestFile.Applications = this._mItemList;
            }
            else if (addinType == AddinType.Command)
            {
                manifestFile.Commands = this._mItemList;
            }
            manifestFile.Save();
        }

        private List<AddinItem> _mItemList;

        private string _FilePath;

        private bool _mSave;

        private bool _mHidden;
    }
}
