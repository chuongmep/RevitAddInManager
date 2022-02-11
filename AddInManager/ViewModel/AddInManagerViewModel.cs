using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using RevitAddinManager.Command;
using RevitAddinManager.Model;
using RevitAddinManager.View;
using RevitAddinManager.View.Control;

namespace RevitAddinManager.ViewModel;

public class AddInManagerViewModel : ViewModelBase
{
    public ExternalCommandData ExternalCommandData { get; set; }
    private string Message { get; set; }
    private ElementSet Elements { get; set; }
    private RevitEvent RevitEvent = new RevitEvent();
    public FrmAddInManager FrmAddInManager { get; set; }
    public AssemLoader AssemLoader { get; set; }

    public AddinManagerBase MAddinManagerBase { get; set; }

    private ObservableCollection<AddinModel> commandItems;

    public ObservableCollection<AddinModel> CommandItems
    {
        get => commandItems;
        set
        {
            if (value == commandItems) return;
            commandItems = value;
            OnPropertyChanged();
        }
    }

    private AddinModel selectedCommandItem;

    public AddinModel SelectedCommandItem
    {
        get
        {
            if (selectedCommandItem != null && selectedCommandItem.IsParentTree == true && IsTabCmdSelected)
            {
                IsCanRun = false;
                MAddinManagerBase.ActiveCmd = selectedCommandItem.Addin;
            }
            else if (selectedCommandItem != null && selectedCommandItem.IsParentTree == false && IsTabCmdSelected)
            {
                IsCanRun = true;
                MAddinManagerBase.ActiveCmdItem = selectedCommandItem.AddinItem;
                MAddinManagerBase.ActiveCmd = selectedCommandItem.Addin;
                VendorDescription = MAddinManagerBase.ActiveCmdItem.Description;
            }
            else IsCanRun = false;
            return selectedCommandItem;
        }
        set => OnPropertyChanged(ref selectedCommandItem, value);
    }

    private ObservableCollection<AddinModel> applicationItems;
    public ObservableCollection<AddinModel> ApplicationItems
    {
        get => applicationItems;
        set
        {
            if (value == applicationItems) return;
            applicationItems = value;
            OnPropertyChanged();
        }
    }

    private AddinModel selectedAppItem;

    public AddinModel SelectedAppItem
    {
        get
        {
            if (selectedAppItem != null && selectedAppItem.IsParentTree == true && IsTabAppSelected)
            {
                MAddinManagerBase.ActiveApp = selectedAppItem.Addin;
            }
            else if (selectedAppItem != null && selectedAppItem.IsParentTree == false && IsTabAppSelected)
            {
                MAddinManagerBase.ActiveAppItem = selectedAppItem.AddinItem;
                MAddinManagerBase.ActiveApp = selectedAppItem.Addin;
                VendorDescription = MAddinManagerBase.ActiveAppItem.Description;
            }
            return selectedAppItem;
        }
        set => OnPropertyChanged(ref selectedAppItem, value);
    }

    public ICommand LoadCommand => new RelayCommand(LoadCommandClick);
    public ICommand ManagerCommand => new RelayCommand(ManagerCommandClick);
    public ICommand ClearCommand => new RelayCommand(ClearCommandClick);


    public ICommand RemoveCommand => new RelayCommand(RemoveAddinClick);
    public ICommand SaveCommand => new RelayCommand(SaveCommandClick);



    public ICommand OpenLocalAddinCommand => new RelayCommand(OpenLocalAddinCommandClick);
    public ICommand EditAddinCommand => new RelayCommand(EditAddinCommandClick);

    private readonly ICommand _executeAddinCommand = null;
    public ICommand ExecuteAddinCommand => _executeAddinCommand ?? new RelayCommand(ExecuteAddinCommandClick);
    public ICommand OpenLcAssemblyCommand => new RelayCommand(OpenLcAssemblyCommandClick);
    public ICommand ReloadCommand => new RelayCommand(()=>ReloadCommandClick());
    public ICommand OpenLcAssemblyApp => new RelayCommand(OpenLcAssemblyAppClick);
    public ICommand ExecuteAddinApp => new RelayCommand(ExecuteAddinAppClick);
    public ICommand FreshSearch => new RelayCommand(FreshSearchClick);
    public ICommand VisibleToggle => new RelayCommand(SetToggleVisible);
    public ICommand ExploreCommand => new RelayCommand(ExploreCommandClick);

    private string searchText;
    public string SearchText
    {
        get
        {
            FreshSearchClick();
            return searchText;
        }
        set => OnPropertyChanged(ref searchText, value);
    }

    private bool isCurrentVersion = true;
    public bool IsCurrentVersion
    {
        get => isCurrentVersion;
        set => OnPropertyChanged(ref isCurrentVersion, value);
    }

    private ObservableCollection<RevitAddin> addinStartup;
    public ObservableCollection<RevitAddin> AddInStartUps
    {
        get { return addinStartup ??= new ObservableCollection<RevitAddin>(); }
        set => OnPropertyChanged(ref addinStartup, value);
    }
    public ICommand HelpCommand => new RelayCommand(HelpCommandClick);


    private string vendorDescription = string.Empty;
    public string VendorDescription
    {
        get => vendorDescription;
        set => OnPropertyChanged(ref vendorDescription, value);
    }

    private bool isTabCmdSelected = true;
    public bool IsTabCmdSelected
    {
        get => isTabCmdSelected;
        set => OnPropertyChanged(ref isTabCmdSelected, value);
    }

    private bool isTabAppSelected;
    public bool IsTabAppSelected
    {
        get
        {
            if (isTabAppSelected) IsCanRun = false;
            return isTabAppSelected;
        }
        set => OnPropertyChanged(ref isTabAppSelected, value);
    }

    private bool isCanRun;

    public bool IsCanRun
    {
        get => isCanRun;
        set => OnPropertyChanged(ref isCanRun, value);
    }
    private bool isTabStartSelected;
    public bool IsTabStartSelected
    {
        get => isTabStartSelected;
        set => OnPropertyChanged(ref isTabStartSelected, value);
    }

    private void HelpCommandClick()
    {
        Process.Start("https://github.com/chuongmep/RevitAddInManager/wiki");
    }

    public AddInManagerViewModel(ExternalCommandData data, ref string message, ElementSet elements)
    {
        AssemLoader = new AssemLoader();
        MAddinManagerBase = AddinManagerBase.Instance;
        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
        ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);
        ExternalCommandData = data;
        Message = message;
        Elements = elements;
        FreshItemStartupClick(false);
    }

    private ObservableCollection<AddinModel> FreshTreeItems(bool isSearchText, Addins addins)
    {
        var MainTrees = new ObservableCollection<AddinModel>();
        foreach (var keyValuePair in addins.AddinDict)
        {
            var addin = keyValuePair.Value;
            var title = keyValuePair.Key;
            var addinItemList = addin.ItemList;
            var addinModels = new List<AddinModel>();
            foreach (var addinItem in addinItemList)
            {
                if (isSearchText)
                {
                    if (addinItem.FullClassName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
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
            var root = new AddinModel(title)
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
            if (SelectedCommandItem?.IsParentTree == false)
            {
                MAddinManagerBase.ActiveCmd = SelectedCommandItem.Addin;
                MAddinManagerBase.ActiveCmdItem = SelectedCommandItem.AddinItem;
                CheckCountSelected(CommandItems, out var result);
                if (result > 0)
                {
                    FrmAddInManager.Close();
                    RevitEvent.Run(Execute, false, null, null, false);
                }
            }
        }

        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }

    void Execute()
    {
        string message = Message;
        MAddinManagerBase.RunActiveCommand(this, ExternalCommandData, ref message, Elements);
    }
    private void OpenLcAssemblyCommandClick()
    {
        bool flag = MAddinManagerBase.ActiveCmd == null;
        if (flag) return;
        string path = MAddinManagerBase.ActiveCmd.FilePath;
        if (!File.Exists(path))
        {
            ShowFileNotExit(path);
            return;
        }
        Process.Start("explorer.exe", "/select, " + path);
    }
    private void OpenLcAssemblyAppClick()
    {
        bool flag = MAddinManagerBase.ActiveApp == null;
        if (flag) return;
        string path = MAddinManagerBase.ActiveApp.FilePath;
        if (!File.Exists(path))
        {
            ShowFileNotExit(path);
            return;
        }
        Process.Start("explorer.exe", "/select, " + path);
    }

    void ShowFileNotExit(string path)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(Resource.FileNotExit);
        sb.AppendLine("Path :");
        sb.AppendLine(path);
        MessageBox.Show(sb.ToString(), Resource.AppName,MessageBoxButton.OK,MessageBoxImage.Exclamation);
    }
    void ExecuteAddinAppClick()
    {
        //TODO: Whether we need support load app or not,
        // May be need create a new feature with console
    }

    void CheckCountSelected(ObservableCollection<AddinModel> addinModels, out int result)
    {
        result = 0;
        foreach (var addinModel in addinModels)
        {
            if (addinModel.IsInitiallySelected) result++;
            foreach (var modelChild in addinModel.Children)
            {
                if (modelChild.IsInitiallySelected) result++;
            }
        }
    }

    void LoadCommandClick()
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();
        openFileDialog.Filter = @"assembly files (*.dll)|*.dll|All files (*.*)|*.*";
        if (openFileDialog.ShowDialog() != true)
        {
            return;
        }
        var fileName = openFileDialog.FileName;
        if(!File.Exists(fileName))return;
        ReloadCommandClick(fileName);

    }

    void ReloadCommandClick(string fileName =null)
    {
        bool flag = MAddinManagerBase.ActiveCmd == null;
        if (flag) return;
        string path = MAddinManagerBase.ActiveCmd.FilePath;
        if(!File.Exists(path)) return;
        if (fileName == null) fileName = path;
        var addinType = MAddinManagerBase.AddinManager.LoadAddin(fileName, AssemLoader);
        if (addinType == AddinType.Invalid)
        {
            MessageBox.Show(Resource.LoadInvalid);
            return;
        }

        switch (addinType)
        {
            case AddinType.Command:
                IsTabCmdSelected = true;
                FrmAddInManager.TabCommand.Focus();
                break;
            case AddinType.Application:
                IsTabAppSelected = true;
                FrmAddInManager.TabApp.Focus();
                break;
            case AddinType.Mixed:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        MAddinManagerBase.AddinManager.SaveToAimIni();
        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
        ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);
    }
    private void RemoveAddinClick()
    {
        try
        {
            if (IsTabCmdSelected)
            {
                foreach (var parent in CommandItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        MAddinManagerBase.ActiveCmd = parent.Addin;
                        MAddinManagerBase.ActiveCmdItem = parent.AddinItem;
                        if (MAddinManagerBase.ActiveCmd != null)
                        {
                            MAddinManagerBase.AddinManager.Commands.RemoveAddIn(MAddinManagerBase.ActiveCmd);
                        }
                        MAddinManagerBase.ActiveCmd = null;
                        MAddinManagerBase.ActiveCmdItem = null;
                        CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
                        return;
                    }
                    foreach (var addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in command
                            MAddinManagerBase.ActiveCmd = parent.Addin;
                            MAddinManagerBase.ActiveCmdItem = addinChild.AddinItem;
                        }
                    }
                }

                if (MAddinManagerBase.ActiveCmdItem != null)
                {
                    MAddinManagerBase.ActiveCmd.RemoveItem(MAddinManagerBase.ActiveCmdItem);
                    MAddinManagerBase.ActiveCmd = null;
                    MAddinManagerBase.ActiveCmdItem = null;
                }
                CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
            }
            if (IsTabAppSelected)
            {
                foreach (var parent in ApplicationItems)
                {
                    if (parent.IsInitiallySelected)
                    {
                        MAddinManagerBase.ActiveApp = parent.Addin;
                        MAddinManagerBase.ActiveAppItem = parent.AddinItem;
                        if (MAddinManagerBase.ActiveApp != null)
                        {
                            MAddinManagerBase.AddinManager.Applications.RemoveAddIn(MAddinManagerBase.ActiveApp);
                        }
                        MAddinManagerBase.ActiveApp = null;
                        MAddinManagerBase.ActiveAppItem = null;
                        ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);
                        return;
                    }
                    foreach (var addinChild in parent.Children)
                    {
                        if (addinChild.IsInitiallySelected)
                        {
                            //Set Value to run for add-in app
                            MAddinManagerBase.ActiveApp = parent.Addin;
                            MAddinManagerBase.ActiveAppItem = addinChild.AddinItem;
                        }
                    }
                }

                if (MAddinManagerBase.ActiveAppItem != null)
                {
                    MAddinManagerBase.ActiveApp.RemoveItem(MAddinManagerBase.ActiveAppItem);
                    MAddinManagerBase.ActiveApp = null;
                    MAddinManagerBase.ActiveAppItem = null;
                }
                ApplicationItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Applications);
            }
            //Save All SetTings
            MAddinManagerBase.AddinManager.SaveToAimIni();

        }
        catch (Exception e)
        {
            MessageBox.Show(e.ToString());
        }
    }
    private void SaveCommandClick()
    {
        var messageBoxResult = MessageBox.Show(@"It will create file addin and load to Revit, do you want continue?", Resource.AppName,
            MessageBoxButton.YesNo,MessageBoxImage.Question);
        if (messageBoxResult == MessageBoxResult.Yes)
        {

            if (!MAddinManagerBase.AddinManager.HasItemsToSave())
            {
                MessageBox.Show(Resource.NoItemSelected, Resource.AppName, MessageBoxButton.OK,MessageBoxImage.Exclamation);
                return;
            }
            MAddinManagerBase.AddinManager.SaveToAllUserManifest(this);
            MessageBox.Show(FrmAddInManager, "Save Successfully", Resource.AppName, MessageBoxButton.OK, MessageBoxImage.Information);
            FrmAddInManager.Close();
        }

    }
    private void FreshSearchClick()
    {
        var flag = string.IsNullOrEmpty(searchText);
        if (IsTabCmdSelected)
        {
            if (flag)
            {
                CommandItems = FreshTreeItems(false, MAddinManagerBase.AddinManager.Commands);
                return;
            }
            CommandItems = FreshTreeItems(true, MAddinManagerBase.AddinManager.Commands);
        }
        else if (IsTabAppSelected)
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

    void ManagerCommandClick()
    {
        IsTabStartSelected = true;
        FreshItemStartupClick(false);
    }
    private void FreshItemStartupClick(bool isSearch)
    {

        //Get All AddIn
        if (addinStartup == null) addinStartup = new ObservableCollection<RevitAddin>();
        addinStartup.Clear();
        var autodeskPath = "Autodesk\\Revit\\Addins";
        const string AdskPluginPath = "Autodesk\\ApplicationPlugins\\";
        var version = ExternalCommandData.Application.Application.VersionNumber;
        var roaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var Folder1 = Path.Combine(roaming, autodeskPath, version);
        var programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        var Folder2 = Path.Combine(programdata, autodeskPath, version);
        var Folder3 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            AdskPluginPath);
        var revitAddins = GetAddinFromFolder(Folder1);
        var addinsProgramData = GetAddinFromFolder(Folder2);
        var addinsPlugins = GetAddinFromFolder(Folder3);
        revitAddins.ForEach(delegate (RevitAddin x)
        {
            addinStartup.Add(x);
            x.IsReadOnly = true;
        });
        addinsProgramData.ForEach(x => addinStartup.Add(x));
        addinsPlugins.ForEach(x => addinStartup.Add(x));
        if (isSearch)
        {
            addinStartup = addinStartup.Where(x => x.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.Name).ToObservableCollection();
            OnPropertyChanged(nameof(AddInStartUps));
            return;
        }
        addinStartup = addinStartup.OrderBy(x => x.Name).ToObservableCollection();
    }

    List<RevitAddin> GetAddinFromFolder(string folder)
    {

        var revitAddins = new List<RevitAddin>();
        if (!Directory.Exists(folder)) return revitAddins;
        var AddinFilePathsVisible = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
        .Where(x => x.EndsWith(DefaultSetting.FormatExAddin)).ToArray();
        foreach (var AddinFilePath in AddinFilePathsVisible)
        {
            var revitAddin = new RevitAddin();
            revitAddin.FilePath = AddinFilePath;
            revitAddin.Name = Path.GetFileName(AddinFilePath);
            revitAddin.NameNotEx =
                revitAddin.Name.Replace(DefaultSetting.FormatExAddin, string.Empty);
            revitAddin.State = VisibleModel.Enable;
            revitAddins.Add(revitAddin);
        }
        var AddinFilePathsDisable = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
            .Where(x => x.EndsWith(DefaultSetting.FormatDisable)).ToArray();
        foreach (var AddinFilePath in AddinFilePathsDisable)
        {
            var revitAddin = new RevitAddin();
            revitAddin.FilePath = AddinFilePath;
            revitAddin.Name = Path.GetFileName(AddinFilePath);
            revitAddin.NameNotEx =
                revitAddin.Name.Replace(DefaultSetting.FormatDisable, string.Empty);
            revitAddin.State = VisibleModel.Disable;
            revitAddins.Add(revitAddin);
        }
        if (AddinFilePathsVisible.Length == 0) return new List<RevitAddin>();
        return revitAddins;
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
        var tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Temp", DefaultSetting.TempFolderName);
        if (Directory.Exists(tempFolder))
        {
            Process.Start(tempFolder);
        }

    }
    private void ExploreCommandClick()
    {
        const string AdskPath = "Autodesk\\Revit\\Addins";
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        if (IsCurrentVersion)
        {

            var folder = Path.Combine(folderPath, AdskPath,
                ExternalCommandData.Application.Application.VersionNumber);
            if (Directory.Exists(folder))
            {
                var filePaths = Directory.GetFiles(folder).Where(x => x.Contains(DefaultSetting.FileName)).ToArray();
                if (filePaths.Length == 0)
                {
                    MessageBox.Show(FrmAddInManager, "File Empty!", Resource.AppName, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }
                foreach (var s in filePaths)
                    Process.Start("explorer.exe", "/select, " + s);
            }
        }
        else
        {
            var folder = Path.Combine(folderPath, AdskPath);
            if (Directory.Exists(folder))
            {
                Process.Start(folder);
            }
        }

    }
    void EditAddinCommandClick()
    {
        if (FrmAddInManager.DataGridStartup.SelectedItem is RevitAddin revitAddin && File.Exists(revitAddin.FilePath))
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
        if (FrmAddInManager.DataGridStartup.SelectedItem is RevitAddin revitAddin && File.Exists(revitAddin.FilePath))
        {
            Process.Start("explorer.exe", "/select, " + revitAddin.FilePath);
        }
        else
        {
            MessageBox.Show(Resource.FileNotFound, Resource.AppName);
        }
    }


}