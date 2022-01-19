using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Xml;
using AddInManager.Command;
using AddInManager.Model;
using AddInManager.Properties;
using AddInManager.View.Control;
using Autodesk.Revit.UI;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AddInManager.ViewModel
{
    public class AddInManagerViewModel : ViewModelBase
    {
        public ExternalCommandData ExternalCommandData { get; set; }
        public View.FrmAddInManager FrmAddInManager { get; set; }
        public AssemLoader AssemLoader { get; set; }

        private AddinManagerBase MAddinManagerBase { get; set; }

        private ObservableCollection<AddinModel> _commandItems;

        public ObservableCollection<AddinModel> CommandItems
        {
            get
            {
                if (_commandItems == null)
                {
                    _commandItems = new ObservableCollection<AddinModel>();
                }
                return _commandItems;
            }
            set => OnPropertyChanged(ref _commandItems, value);
        }

        public ICommand LoadCommand => new RelayCommand(LoadCommandClick);
        public ICommand ManagerCommand => new RelayCommand(ManagerCommandClick);
        public ICommand ClearCommand => new RelayCommand(ClearCommandClick);


        public ICommand RemoveCommand => new RelayCommand(RemoveAddinClick);
        public ICommand SaveCommand => new RelayCommand(SaveCommandClick);



        public ICommand OpenAssemblyCommand => new RelayCommand(OpenAssemblyCommandClick);

       

        public ICommand ExecuteAddin => new RelayCommand(ExecuteAddinClick);


        public ICommand FreshSearch => new RelayCommand(FreshSearchClick);


        public string SearchText { get; set; }

        private bool _IsCurrentVersion = true;
        public bool IsCurrentVersion
        {
            get => _IsCurrentVersion;
            set => _IsCurrentVersion = value;
        }

        public ICommand ExploreCommand => new RelayCommand(ExploreCommandClick);

        private ObservableCollection<RevitAddin> _addinStartup;
        public ObservableCollection<RevitAddin> AddInStartUps
        {
            get
            {
                if (_addinStartup == null)
                {
                    _addinStartup = new ObservableCollection<RevitAddin>();
                }

                return _addinStartup;
            }
            set => OnPropertyChanged(ref _addinStartup, value);
        }

        public ICommand HelpCommand => new RelayCommand(HelpCommandClick);

        private void HelpCommandClick()
        {
            Process.Start("https://github.com/chuongmep/RevitAddInManager/wiki");
        }

        public AddInManagerViewModel(ExternalCommandData data)
        {
            AssemLoader = new AssemLoader();
            this.MAddinManagerBase = AddinManagerBase.Instance;
            CommandItems = FreshTreeItems(false);
            this.ExternalCommandData = data;
            ManagerCommandClick();
        }


        public ObservableCollection<AddinModel> FreshTreeItems(bool isSearchText)
        {
            Addins addins = this.MAddinManagerBase.AddinManager.Commands;
            ObservableCollection<AddinModel> MainTrees = new ObservableCollection<AddinModel>();
            foreach (KeyValuePair<string, Addin> keyValuePair in addins.AddinDict)
            {
                Addin addin = keyValuePair.Value;
                string Title = keyValuePair.Key;
                List<AddinItem> addinItemList = addin.ItemList;
                List<AddinModel> addinModels = new List<AddinModel>();
                foreach (AddinItem addinItem in addinItemList)
                {
                    if (isSearchText)
                    {
                        if (addinItem.FullClassName.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            addinModels.Add(new AddinModel(addinItem.FullClassName)
                            {
                                IsChecked = true,
                                Addin = addin,
                                AddinItem = addinItem,
                            });
                        }
                    }
                    else
                    {
                        addinModels.Add(new AddinModel(addinItem.FullClassName)
                        {
                            IsChecked = true,
                            Addin = addin,
                            AddinItem = addinItem,
                        });
                    }
                }
                AddinModel root = new AddinModel(Title)
                {
                    IsChecked = true,
                    Children = addinModels,
                    IsParentTree = true,
                    Addin = addin,
                };
                root.Initialize();
                MainTrees.Add(root);
            }

            return MainTrees;
        }
        private void ExecuteAddinClick()
        {
            try
            {
                foreach (AddinModel parent in CommandItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        //TODO : Auto Run All Command Selected Children
                        return;
                    }
                    foreach (AddinModel addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in command
                            this.MAddinManagerBase.ActiveCmd = parent.Addin;
                            this.MAddinManagerBase.ActiveCmdItem = addinChild.AddinItem;
                        }
                    }
                }

                CheckCountSelected(CommandItems, out int result);
                if (result > 0) FrmAddInManager.Close();
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void CheckCountSelected(ObservableCollection<AddinModel> addinModels, out int result)
        {
            result = 0;
            foreach (AddinModel addinModel in addinModels)
            {
                if (addinModel.IsInitiallySelected) result++;
                foreach (AddinModel modelChild in addinModel.Children)
                {
                    if (modelChild.IsInitiallySelected) result++;
                }
            }
        }

        void LoadCommandClick()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = Resources.LoadFileFilter;
            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }
            string fileName = openFileDialog.FileName;
            AddinType addinType = MAddinManagerBase.AddinManager.LoadAddin(fileName, AssemLoader);
            if (addinType == AddinType.Invalid)
            {
                MessageBox.Show(Resource.LoadInvalid);
                return;
            }
            this.MAddinManagerBase.AddinManager.SaveToAimIni();
            CommandItems = FreshTreeItems(false);

        }
        private void RemoveAddinClick()
        {
            try
            {
                foreach (AddinModel parent in CommandItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        this.MAddinManagerBase.ActiveCmd = parent.Addin;
                        this.MAddinManagerBase.ActiveCmdItem = parent.AddinItem;
                        this.MAddinManagerBase.AddinManager.Commands.RemoveAddIn(this.MAddinManagerBase.ActiveCmd);
                        this.MAddinManagerBase.ActiveCmd = null;
                        this.MAddinManagerBase.ActiveCmdItem = null;
                        this.MAddinManagerBase.AddinManager.SaveToAimIni();
                        CommandItems = FreshTreeItems(false);
                        return;
                    }
                    foreach (AddinModel addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in command
                            this.MAddinManagerBase.ActiveCmd = parent.Addin;
                            this.MAddinManagerBase.ActiveCmdItem = addinChild.AddinItem;
                        }
                    }
                }
                this.MAddinManagerBase.ActiveCmd.RemoveItem(this.MAddinManagerBase.ActiveCmdItem);
                this.MAddinManagerBase.ActiveCmd = null;
                this.MAddinManagerBase.ActiveCmdItem = null;
                this.MAddinManagerBase.AddinManager.SaveToAimIni();
                CommandItems = FreshTreeItems(false);

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private void SaveCommandClick()
        {
            DialogResult DialogResult = MessageBox.Show("It will create file addin and load to revit, do you want continue?", Resource.AppName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult == DialogResult.Yes)
            {
                if (!this.MAddinManagerBase.AddinManager.HasItemsToSave())
                {
                    MessageBox.Show(Resources.NoItemsSelected, Resources.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                this.MAddinManagerBase.AddinManager.SaveToAllUserManifest(this);
                FrmAddInManager.Close();
                System.Windows.MessageBox.Show(FrmAddInManager, "Save Successfully", Resource.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            }
           
        }
        private void FreshSearchClick()
        {
            if (string.IsNullOrEmpty(SearchText))
            {
                CommandItems = FreshTreeItems(false);
                return;
            }
            CommandItems = FreshTreeItems(true);
        }
        private void ManagerCommandClick()
        {
            //Get All AddIn
            if (_addinStartup == null) _addinStartup = new ObservableCollection<RevitAddin>();
            _addinStartup.Clear();
            string autodeskPath = "Autodesk\\Revit\\Addins";
            string AdskPluginPath = "Autodesk\\ApplicationPlugins\\";
            string version = ExternalCommandData.Application.Application.VersionNumber;
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path1 = Path.Combine(roaming, autodeskPath, version);
            string programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string path2 = Path.Combine(programdata, autodeskPath, version);
            string path3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                AdskPluginPath);
            List<RevitAddin> revitAddins = GetAddinFromFolder(path1);
            List<RevitAddin> addinsProgramData = GetAddinFromFolder(path2);
            List<RevitAddin> addinsPlugins = GetAddinFromFolder(path3);
            if (FrmAddInManager != null) { FrmAddInManager.TabControl.SelectedIndex = 2; }
            revitAddins.ForEach(x=>_addinStartup.Add(x));
            addinsProgramData.ForEach(x=>_addinStartup.Add(x));
            addinsPlugins.ForEach(x=>_addinStartup.Add(x));

        }

        List<RevitAddin> GetAddinFromFolder(string folder)
        {
            string fileFormat = ".addin";
            string XmlTagParent = "RevitAddIns";
            string[] strings = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(fileFormat)).ToArray();
            if (strings.Length == 0) return new List<RevitAddin>();
            List<RevitAddin> revitAddins = new List<RevitAddin>();
            foreach (string path_name in strings)
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path_name);
                foreach (XmlNode node in doc.ChildNodes)
                {

                    if (node.Name == XmlTagParent)
                    {
                        foreach (XmlNode addiNode in node.ChildNodes)
                        {
                            RevitAddin revitAddin = new RevitAddin();
                            foreach (XmlNode addin in addiNode.ChildNodes)
                            {
                                switch (addin.Name)
                                {
                                    case "Assembly":
                                        revitAddin.Assembly = Path.GetFileName(addin.InnerText);
                                        break;
                                    case "VendorId":
                                        revitAddin.VendorId = addin.InnerText;
                                        break;
                                    case "VendorDescription":
                                        revitAddin.VendorDescription = addin.InnerText;
                                        break;
                                    case "LanguageType":
                                        revitAddin.LanguageType = addin.InnerText;
                                        break;
                                    case "FullClassName":
                                        revitAddin.FullClassName = addin.InnerText;
                                        break;
                                    case "Text":
                                        revitAddin.Text = addin.InnerText;
                                        break;
                                    case "VisibilityMode":
                                        revitAddin.VisibilityMode = addin.InnerText;
                                        break;
                                    case "Name":
                                        revitAddin.FullClassName = addin.InnerText;
                                        break;

                                }
                            }

                            if (string.IsNullOrEmpty(revitAddin.Assembly) == false)
                            {
                                revitAddins.Add(revitAddin);
                            }
                            

                        }
                    }
                }
            }
            return revitAddins;
        }

        private void ClearCommandClick()
        {
            string FolderName = "RevitAddins";
            string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Temp", FolderName);
            if (Directory.Exists(tempFolder))
            {
                Process.Start(tempFolder);
            }

        }
        private void ExploreCommandClick()
        {
            string AdskPath = "Autodesk\\Revit\\Addins";
            string FileNameExtension = "ExternalTool";
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (IsCurrentVersion)
            {
               
                string folder = Path.Combine(folderPath, AdskPath,
                    ExternalCommandData.Application.Application.VersionNumber);
                if (Directory.Exists(folder))
                {
                    string[] filePaths = Directory.GetFiles(folder).Where(x => x.Contains(FileNameExtension)).ToArray();
                    if (filePaths.Length == 0)
                    {
                        System.Windows.MessageBox.Show(FrmAddInManager,"File Empty!", Resource.AppName, MessageBoxButton.OK,MessageBoxImage.Exclamation);
                        return;
                    }
                    foreach (string s in filePaths)
                        System.Diagnostics.Process.Start("explorer.exe", "/select, " + s);
                }
            }
            else
            {
                string folder = Path.Combine(folderPath, AdskPath);
                if (Directory.Exists(folder))
                {
                    Process.Start(folder);
                }
            }

        }
        private void OpenAssemblyCommandClick()
        {
            throw new NotImplementedException();
        }


    }
}
