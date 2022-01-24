using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using AddinManager.Command;
using AddinManager.Model;
using AddinManager.View.Control;
using Autodesk.Revit.UI;
using MessageBox = System.Windows.Forms.MessageBox;

namespace AddinManager.ViewModel
{
    public class AddInManagerViewModel : ViewModelBase
    {
        public bool IsRun { get; set; }
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

        private AddinModel _selectedCommandItem;

        public AddinModel SelectedCommandItem
        {
            get
            {

                if (_selectedCommandItem != null && _selectedCommandItem.IsParentTree == false && SelectedTab==0)
                {
                    MAddinManagerBase.ActiveCmdItem = _selectedCommandItem.AddinItem;
                    MAddinManagerBase.ActiveCmd = _selectedCommandItem.Addin;
                    VendorDescription = MAddinManagerBase.ActiveCmdItem.Description;
                }
                return _selectedCommandItem;
            }
            set => OnPropertyChanged(ref _selectedCommandItem, value);
        }

        private ObservableCollection<AddinModel> _applicationItems;
        public ObservableCollection<AddinModel> ApplicationItems
        {
            get
            {
                if (_applicationItems == null)
                {
                    _applicationItems = new ObservableCollection<AddinModel>();
                }
                return _applicationItems;
            }
            set => OnPropertyChanged(ref _applicationItems, value);
        }

        private AddinModel _selectedAppItem;

        public AddinModel SelectedAppItem
        {
            get
            {
                
                if (_selectedAppItem != null &&_selectedAppItem.IsParentTree == false && SelectedTab==1)
                {
                   
                    MAddinManagerBase.ActiveAppItem = _selectedAppItem.AddinItem;
                    MAddinManagerBase.ActiveApp = _selectedAppItem.Addin;
                    VendorDescription = MAddinManagerBase.ActiveAppItem.Description;
                }
                return _selectedAppItem;
            }
            set => OnPropertyChanged(ref _selectedAppItem, value);
        }

        public ICommand LoadCommand => new RelayCommand(LoadCommandClick);
        public ICommand ManagerCommand => new RelayCommand(() => FreshItemStartupClick(false));
        public ICommand ClearCommand => new RelayCommand(ClearCommandClick);


        public ICommand RemoveCommand => new RelayCommand(RemoveAddinClick);
        public ICommand SaveCommand => new RelayCommand(SaveCommandClick);



        public ICommand OpenLocalAddinCommand => new RelayCommand(OpenLocalAddinCommandClick);
        public ICommand EditAddinCommand => new RelayCommand(EditAddinCommandClick);



        public ICommand ExecuteAddinCommand => new RelayCommand(ExecuteAddinCommandClick);
        public ICommand ExecuteAddinApp => new RelayCommand(ExecuteAddinAppClick);


        public ICommand FreshSearch => new RelayCommand(FreshSearchClick);

        public ICommand VisableToggle => new RelayCommand(SetToggleVisible);

        private string _searchText;

        public string SearchText
        {
            get
            {
                FreshSearchClick();
                return _searchText;
            }
            set => OnPropertyChanged(ref _searchText, value);
        }

        private bool _IsCurrentVersion = true;
        public bool IsCurrentVersion
        {
            get => _IsCurrentVersion;
            set => OnPropertyChanged(ref _IsCurrentVersion, value);
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


        private string _vendorDescription = string.Empty;
        public string VendorDescription
        {
            get
            {
                if (MAddinManagerBase.ActiveCmdItem != null && SelectedTab==0)
                {
                    MAddinManagerBase.ActiveCmdItem.Description = _vendorDescription;
                }
                if (MAddinManagerBase.ActiveAppItem != null && SelectedTab==1)
                {
                    MAddinManagerBase.ActiveAppItem.Description = _vendorDescription;
                }
                MAddinManagerBase.AddinManager.SaveToAimIni();
                return _vendorDescription;
            }
            set => OnPropertyChanged(ref _vendorDescription, value);
        }

        private int _selectedTab;
        public int SelectedTab
        {
            get => _selectedTab;
            set => OnPropertyChanged(ref _selectedTab, value);
        }

        private void HelpCommandClick()
        {
            Process.Start("https://github.com/chuongmep/RevitAddInManager/wiki");
        }

        public AddInManagerViewModel(ExternalCommandData data)
        {
            AssemLoader = new AssemLoader();
            this.MAddinManagerBase = AddinManagerBase.Instance;
            CommandItems = FreshTreeItems(false, this.MAddinManagerBase.AddinManager.Commands);
            ApplicationItems = FreshTreeItems(false, this.MAddinManagerBase.AddinManager.Applications);
            this.ExternalCommandData = data;
            FreshItemStartupClick(false);
        }

        public ObservableCollection<AddinModel> FreshTreeItems(bool isSearchText, Addins addins)
        {
            //Addins addins = this.MAddinManagerBase.AddinManager.Commands;
            ObservableCollection<AddinModel> MainTrees = new ObservableCollection<AddinModel>();
            foreach (KeyValuePair<string, Addin> keyValuePair in addins.AddinDict)
            {
                Addin addin = keyValuePair.Value;
                string title = keyValuePair.Key;
                List<AddinItem> addinItemList = addin.ItemList;
                List<AddinModel> addinModels = new List<AddinModel>();
                foreach (AddinItem addinItem in addinItemList)
                {
                    if (isSearchText)
                    {
                        if (addinItem.FullClassName.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
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
                AddinModel root = new AddinModel(title)
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
        void ExecuteAddinCommandClick()
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
                if (result > 0)
                {
                    IsRun = true;
                    FrmAddInManager.Close();
                }
            }

            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void ExecuteAddinAppClick()
        {
            //TODO: Whether we need support load app or not,
            // May be need create a new feature with console
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
            openFileDialog.Filter = @"assembly files (*.dll)|*.dll|All files (*.*)|*.*";
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

            switch (addinType)
            {
                case AddinType.Command:
                    this.SelectedTab = 0;
                    this.FrmAddInManager.TabCommand.Focus();
                    break;
                case AddinType.Application:
                    this.SelectedTab = 1;
                    this.FrmAddInManager.TabApp.Focus();
                    break;
                case AddinType.Mixed:
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }
            this.MAddinManagerBase.AddinManager.SaveToAimIni();
            CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
            ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);

        }
        private void RemoveAddinClick()
        {
            try
            {
                //TODO: Check null Selected
                if (SelectedTab == 0)
                {
                    foreach (AddinModel parent in CommandItems)
                    {
                        if (parent.IsInitiallySelected)
                        {
                            this.MAddinManagerBase.ActiveCmd = parent.Addin;
                            this.MAddinManagerBase.ActiveCmdItem = parent.AddinItem;
                            if (this.MAddinManagerBase.ActiveCmd != null)
                            {
                                this.MAddinManagerBase.AddinManager.Commands.RemoveAddIn(this.MAddinManagerBase.ActiveCmd);
                            }
                            this.MAddinManagerBase.ActiveCmd = null;
                            this.MAddinManagerBase.ActiveCmdItem = null;
                            CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
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

                    if (this.MAddinManagerBase.ActiveCmdItem != null)
                    {
                        this.MAddinManagerBase.ActiveCmd.RemoveItem(this.MAddinManagerBase.ActiveCmdItem);
                        this.MAddinManagerBase.ActiveCmd = null;
                        this.MAddinManagerBase.ActiveCmdItem = null;
                    }
                    CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
                }
                if (SelectedTab == 1)
                {
                    foreach (AddinModel parent in ApplicationItems)
                    {
                        if (parent.IsInitiallySelected)
                        {
                            this.MAddinManagerBase.ActiveApp = parent.Addin;
                            this.MAddinManagerBase.ActiveAppItem = parent.AddinItem;
                            if (this.MAddinManagerBase.ActiveApp != null)
                            {
                                this.MAddinManagerBase.AddinManager.Applications.RemoveAddIn(this.MAddinManagerBase.ActiveApp);
                            }
                            this.MAddinManagerBase.ActiveApp = null;
                            this.MAddinManagerBase.ActiveAppItem = null;
                            ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);
                            return;
                        }
                        foreach (AddinModel addinChild in parent.Children)
                        {
                            if (addinChild.IsInitiallySelected)
                            {
                                //Set Value to run for add-in app
                                this.MAddinManagerBase.ActiveApp = parent.Addin;
                                this.MAddinManagerBase.ActiveAppItem = addinChild.AddinItem;
                            }
                        }
                    }

                    if (this.MAddinManagerBase.ActiveAppItem != null)
                    {
                        this.MAddinManagerBase.ActiveApp.RemoveItem(this.MAddinManagerBase.ActiveAppItem);
                        this.MAddinManagerBase.ActiveApp = null;
                        this.MAddinManagerBase.ActiveAppItem = null;
                    }
                    ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);
                }
                //Save All SetTings
                this.MAddinManagerBase.AddinManager.SaveToAimIni();

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
        private void SaveCommandClick()
        {
            DialogResult DialogResult = MessageBox.Show($@"It will create file addin and load to Revit, do you want continue?", Resource.AppName,
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (DialogResult == DialogResult.Yes)
            {

                if (!this.MAddinManagerBase.AddinManager.HasItemsToSave())
                {
                    MessageBox.Show(Resource.NoItemSelected, Resource.AppName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                MAddinManagerBase.AddinManager.SaveToAllUserManifest(this);
                System.Windows.MessageBox.Show(FrmAddInManager, "Save Successfully", Resource.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
                FrmAddInManager.Close();
            }

        }
        private void FreshSearchClick()
        {
            bool flag = string.IsNullOrEmpty(_searchText);
            if (SelectedTab == 0)
            {
                if (flag)
                {
                    CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
                    return;
                }
                CommandItems = FreshTreeItems(true, MAddinManagerBase.AddinManager.Commands);
            }
            else if (SelectedTab == 1)
            {
                if (flag)
                {
                    ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);
                    return;
                }
                ApplicationItems = FreshTreeItems(true, MAddinManagerBase.AddinManager.Applications);
            }
            else
            {
                if (flag) FreshItemStartupClick(false);
                else FreshItemStartupClick(true);
            }

        }
        private void FreshItemStartupClick(bool isSearch)
        {

            //Get All AddIn
            if (_addinStartup == null) _addinStartup = new ObservableCollection<RevitAddin>();
            _addinStartup.Clear();
            string autodeskPath = "Autodesk\\Revit\\Addins";
            string AdskPluginPath = "Autodesk\\ApplicationPlugins\\";
            string version = ExternalCommandData.Application.Application.VersionNumber;
            string roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string Folder1 = Path.Combine(roaming, autodeskPath, version);
            string programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string Folder2 = Path.Combine(programdata, autodeskPath, version);
            string Folder3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                AdskPluginPath);
            List<RevitAddin> revitAddins = GetAddinFromFolder(Folder1);
            List<RevitAddin> addinsProgramData = GetAddinFromFolder(Folder2);
            List<RevitAddin> addinsPlugins = GetAddinFromFolder(Folder3);
            if (FrmAddInManager != null) { SelectedTab = 2; }
            revitAddins.ForEach(x => _addinStartup.Add(x));
            addinsProgramData.ForEach(x => _addinStartup.Add(x));
            addinsPlugins.ForEach(x => _addinStartup.Add(x));
            if (isSearch)
            {
                _addinStartup = _addinStartup.Where(x => x.Name.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .OrderBy(x => x.Name).ToObservableCollection();
                OnPropertyChanged(nameof(AddInStartUps));
                return;
            }
            _addinStartup = _addinStartup.OrderBy(x => x.Name).ToObservableCollection();
        }

        List<RevitAddin> GetAddinFromFolder(string folder)
        {
            List<RevitAddin> revitAddins = new List<RevitAddin>();
            string[] AddinFilePathsVisiable = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(DefaultSetting.FormatExAddin)).ToArray();
            foreach (string AddinFilePath in AddinFilePathsVisiable)
            {
                RevitAddin revitAddin = new RevitAddin();
                revitAddin.FilePath = AddinFilePath;
                revitAddin.Name = Path.GetFileName(AddinFilePath);
                revitAddin.NameNotEx =
                    revitAddin.Name.Replace(DefaultSetting.FormatExAddin, String.Empty);
                revitAddin.State = VisibleModel.Enable;
                revitAddins.Add(revitAddin);
            }
            string[] AddinFilePathsDisable = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(DefaultSetting.FormatDisable)).ToArray();
            foreach (string AddinFilePath in AddinFilePathsDisable)
            {
                //string checkFile = Path.GetFileName(AddinFilePath).Replace(DefaultSetting.FormatDisable, String.Empty);
                //if(File.Exists(checkFile)) continue;
                RevitAddin revitAddin = new RevitAddin();
                revitAddin.FilePath = AddinFilePath;
                revitAddin.Name = Path.GetFileName(AddinFilePath);
                revitAddin.NameNotEx =
                    revitAddin.Name.Replace(DefaultSetting.FormatDisable, String.Empty);
                revitAddin.State = VisibleModel.Disable;
                revitAddins.Add(revitAddin);
            }
            if (AddinFilePathsVisiable.Length == 0) return new List<RevitAddin>();
            return revitAddins;
        }

        [Obsolete("Remove In Feature")]
        void GetCommandAppInside(string AddinFilePath)
        {
            List<RevitAddin> revitAddins = new List<RevitAddin>();
            string XmlTagParent = "RevitAddIns";
            XmlDocument doc = new XmlDocument();
            doc.Load(AddinFilePath);
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
                            if (string.IsNullOrEmpty(revitAddin.Assembly) == false)
                            {
                                revitAddins.Add(revitAddin);
                            }
                        }
                    }
                }
            }
        }

        private void SetToggleVisible()
        {
            foreach (RevitAddin revitAddin in FrmAddInManager.DataGridStartup.SelectedItems)
            {
                revitAddin.SetToggleState();
            }
            FrmAddInManager.Close();
            MessageBox.Show(Resource.Successfully, Resource.AppName);
        }
        private void ClearCommandClick()
        {
            string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Temp", DefaultSetting.TempFolderName);
            if (Directory.Exists(tempFolder))
            {
                Process.Start(tempFolder);
            }

        }
        private void ExploreCommandClick()
        {
            string AdskPath = "Autodesk\\Revit\\Addins";
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (IsCurrentVersion)
            {

                string folder = Path.Combine(folderPath, AdskPath,
                    ExternalCommandData.Application.Application.VersionNumber);
                if (Directory.Exists(folder))
                {
                    string[] filePaths = Directory.GetFiles(folder).Where(x => x.Contains(DefaultSetting.FileName)).ToArray();
                    if (filePaths.Length == 0)
                    {
                        System.Windows.MessageBox.Show(FrmAddInManager, "File Empty!", Resource.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
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

        void EditAddinCommandClick()
        {
            RevitAddin revitAddin = FrmAddInManager.DataGridStartup.SelectedItem as RevitAddin;
            if (revitAddin != null && File.Exists(revitAddin.FilePath))
            {
                Process.Start(revitAddin.FilePath);
            }
            else
            {
                MessageBox.Show(Resource.FileNotFound, Resource.AppName);
            }
        }
        private void OpenLocalAddinCommandClick()
        {
            RevitAddin revitAddin = FrmAddInManager.DataGridStartup.SelectedItem as RevitAddin;
            if (revitAddin != null && File.Exists(revitAddin.FilePath))
            {
                System.Diagnostics.Process.Start("explorer.exe", "/select, " + revitAddin.FilePath);
            }
            else
            {
                MessageBox.Show(Resource.FileNotFound, Resource.AppName);
            }
        }


    }
}
