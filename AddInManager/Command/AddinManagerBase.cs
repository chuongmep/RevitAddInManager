using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinManager.Model;
using RevitAddinManager.ViewModel;
using MessageBox = System.Windows.Forms.MessageBox;

namespace RevitAddinManager.Command
{

    public sealed class AddinManagerBase
    {
        public Result ExecuteCommand(ExternalCommandData data, ref string message, ElementSet elements, bool faceless)
        {
            var vm = new AddInManagerViewModel(data);
            if (_mActiveCmd != null && faceless)
            {
                return RunActiveCommand(vm, data, ref message, elements);
            }
            var frmAddInManager = new View.FrmAddInManager(vm);
            frmAddInManager.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            var process = Process.GetCurrentProcess();
            new WindowInteropHelper(frmAddInManager).Owner = process.MainWindowHandle;
            var showDialog = frmAddInManager.ShowDialog();
            if (showDialog == false && ActiveCmd != null&& vm.IsRun)
            {
                return RunActiveCommand(vm, data, ref message, elements);
            }
            return Result.Failed;
        }

        public string ActiveTempFolder
        {
            get => _mActiveTempFolder;
            set => _mActiveTempFolder = value;
        }


        private Result RunActiveCommand(AddInManagerViewModel vm, ExternalCommandData data, ref string message, ElementSet elements)
        {
            var filePath = _mActiveCmd.FilePath;
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
                    _mActiveTempFolder = vm.AssemLoader.TempFolder;
                    var externalCommand = assembly.CreateInstance(_mActiveCmdItem.FullClassName) as IExternalCommand;
                    if (externalCommand == null)
                    {
                        result = Result.Failed;
                    }
                    else
                    {
                        _mActiveEc = externalCommand;
                        result = _mActiveEc.Execute(data, ref message, elements);
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
                if (_mInst == null)
                {
#pragma warning disable RCS1059 // Avoid locking on publicly accessible instance.
                    lock (typeof(AddinManagerBase))
                    {
                        if (_mInst == null)
                        {
                            _mInst = new AddinManagerBase();
                        }
                    }
#pragma warning restore RCS1059 // Avoid locking on publicly accessible instance.
                }
                return _mInst;
            }
        }

        private AddinManagerBase()
        {
            _mAddinManager = new AddinManager();
            _mActiveCmd = null;
            _mActiveCmdItem = null;
            _mActiveApp = null;
            _mActiveAppItem = null;
        }


        public IExternalCommand ActiveEC
        {
            get => _mActiveEc;
            set => _mActiveEc = value;
        }


        public Addin ActiveCmd
        {
            get => _mActiveCmd;
            set => _mActiveCmd = value;
        }

        public AddinItem ActiveCmdItem
        {
            get => _mActiveCmdItem;
            set => _mActiveCmdItem = value;
        }


        public Addin ActiveApp
        {
            get => _mActiveApp;
            set => _mActiveApp = value;
        }
        public AddinItem ActiveAppItem
        {
            get => _mActiveAppItem;
            set => _mActiveAppItem = value;
        }

        public AddinManager AddinManager
        {
            get => _mAddinManager;
            set => _mAddinManager = value;
        }

        private string _mActiveTempFolder = string.Empty;

        private static volatile AddinManagerBase _mInst;

        private IExternalCommand _mActiveEc;

        private Addin _mActiveCmd;

        private AddinItem _mActiveCmdItem;

        private Addin _mActiveApp;

        private AddinItem _mActiveAppItem;

        private AddinManager _mAddinManager;
    }
}
