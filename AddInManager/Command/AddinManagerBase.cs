using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinManager.Model;
using RevitAddinManager.ViewModel;
namespace RevitAddinManager.Command;

public sealed class AddinManagerBase
{
    public Result ExecuteCommand(ExternalCommandData data, ref string message, ElementSet elements, bool faceless)
    {
        var vm = new AddInManagerViewModel(data,ref message,elements);
        if (_activeCmd != null && faceless)
        {
            return RunActiveCommand(vm, data, ref message, elements);
        }
        var frmAddInManager = new View.FrmAddInManager(vm);
        frmAddInManager.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        var process = Process.GetCurrentProcess();
        new WindowInteropHelper(frmAddInManager).Owner = process.MainWindowHandle;
        frmAddInManager.Show();
        return Result.Failed;
    }

    public string ActiveTempFolder
    {
        get => _activeTempFolder;
        set => _activeTempFolder = value;
    }


    public Result RunActiveCommand(AddInManagerViewModel vm, ExternalCommandData data, ref string message, ElementSet elements)
    {
        var filePath = _activeCmd.FilePath;
        Result result;
        try
        {
            vm.AssemLoader.HookAssemblyResolve();
            var assembly = vm.AssemLoader.LoadAddinsToTempFolder(filePath, false);
            if (null == assembly)
            {
                result = Result.Failed;
            }
            else
            {
                _activeTempFolder = vm.AssemLoader.TempFolder;
                if (assembly.CreateInstance(_activeCmdItem.FullClassName) is not IExternalCommand externalCommand)
                {
                    result = Result.Failed;
                }
                else
                {
                    _activeEc = externalCommand;
                    result = _activeEc.Execute(data, ref message, elements);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
            result = Result.Failed;
        }
        finally
        {
            vm.AssemLoader.UnhookAssemblyResolve();
            vm.AssemLoader.CopyGeneratedFilesBack();
        }
        return result;
    }


    public static AddinManagerBase Instance
    {
        get
        {
            if (_instance == null)
            {
#pragma warning disable RCS1059 // Avoid locking on publicly accessible instance.
                lock (typeof(AddinManagerBase))
                {
                    if (_instance == null)
                    {
                        _instance = new AddinManagerBase();
                    }
                }
#pragma warning restore RCS1059 // Avoid locking on publicly accessible instance.
            }
            return _instance;
        }
    }

    private AddinManagerBase()
    {
        _addinManager = new AddinManager();
        _activeCmd = null;
        _activeCmdItem = null;
        _activeApp = null;
        _activeAppItem = null;
    }


    public IExternalCommand ActiveEC
    {
        get => _activeEc;
        set => _activeEc = value;
    }


    public Addin ActiveCmd
    {
        get => _activeCmd;
        set => _activeCmd = value;
    }

    public AddinItem ActiveCmdItem
    {
        get => _activeCmdItem;
        set => _activeCmdItem = value;
    }


    public Addin ActiveApp
    {
        get => _activeApp;
        set => _activeApp = value;
    }
    public AddinItem ActiveAppItem
    {
        get => _activeAppItem;
        set => _activeAppItem = value;
    }

    public AddinManager AddinManager
    {
        get => _addinManager;
        set => _addinManager = value;
    }

    private string _activeTempFolder = string.Empty;

    private static volatile AddinManagerBase _instance;

    private IExternalCommand _activeEc;

    private Addin _activeCmd;

    private AddinItem _activeCmdItem;

    private Addin _activeApp;

    private AddinItem _activeAppItem;

    private AddinManager _addinManager;
}