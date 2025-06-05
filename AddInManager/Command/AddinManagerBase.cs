using System.Diagnostics;
using System.IO;
using System.Reflection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinManager.Model;
using RevitAddinManager.ViewModel;
using System.Windows;
using static RevitAddinManager.App;

#if R25 || R26
using AssemblyLoadContext = RevitAddinManager.Model.AssemblyLoadContext;
using System.Runtime.Loader;
#endif

namespace RevitAddinManager.Command;

public sealed class AddinManagerBase
{
    public Result ExecuteCommand(ExternalCommandData data, ref string message, ElementSet elements, bool faceless)
    {
        if (FormControl.Instance.IsOpened) return Result.Succeeded;
        var vm = new AddInManagerViewModel(data, ref message, elements);
        if (_activeCmd != null && faceless)
        {
#if R19 || R20 || R21 || R22 || R23 || R24
            return RunActiveCommand(vm, data, ref message, elements);
#else
            return RunActiveCommand(data, ref message, elements);
#endif

        }
        FrmAddInManager = new View.FrmAddInManager(vm);
        FrmAddInManager.SetRevitAsWindowOwner();
        FrmAddInManager.SetMonitorSize();
        FrmAddInManager.Show();
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
        if (!File.Exists(filePath))
        {
            MessageBox.Show("File not found: " + filePath,DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            return 0;
        }
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

#if R25 || R26
    public Result RunActiveCommand(ExternalCommandData data, ref string message, ElementSet elements)
    {
        var filePath = _activeCmd.FilePath;
        if (!File.Exists(filePath))
        {
            MessageBox.Show("File not found: " + filePath,DefaultSetting.AppName, MessageBoxButton.OK, MessageBoxImage.Error);
            return 0;
        }
        Result result = Result.Failed;
        var alc = new AssemblyLoadContext(filePath);
        Stream stream = null;
        try
        {
            stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            Assembly assembly = alc.LoadFromStream(stream);
            object instance = assembly.CreateInstance(_activeCmdItem.FullClassName);
            WeakReference alcWeakRef = new WeakReference(alc, trackResurrection: true);
            if (instance is IExternalCommand externalCommand)
            {
                _activeEc = externalCommand;
                result = _activeEc.Execute(data, ref message, elements);
                alc.Unload();
            }
            int counter = 0;
            for (counter = 0; alcWeakRef.IsAlive && (counter < 10); counter++)
            {
                alc = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            stream.Close();
        }
        catch (Exception ex)
        {
            // unload the assembly
            MessageBox.Show(ex.ToString());
            alc?.Unload();
            result = Result.Failed;
            WeakReference alcWeakRef = new WeakReference(alc, trackResurrection: true);
            for (int counter = 0; alcWeakRef.IsAlive && (counter < 10); counter++)
            {
                alc = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            stream?.Close();
            Debug.WriteLine("Assembly unloaded");

        }
        // finally
        // {
        //     alc?.Unload();
        // }
        return result;
    }
#endif

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