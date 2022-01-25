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
            AddInManagerViewModel vm = new AddInManagerViewModel(data);
            if (this._mActiveCmd != null && faceless)
            {
                return this.RunActiveCommand(vm, data, ref message, elements);
            }
            View.FrmAddInManager frmAddInManager = new View.FrmAddInManager(vm);
            frmAddInManager.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Process process = Process.GetCurrentProcess();
            new WindowInteropHelper(frmAddInManager).Owner = process.MainWindowHandle;
            bool? showDialog = frmAddInManager.ShowDialog();
            if (showDialog == false && this.ActiveCmd != null&& vm.IsRun)
            {
                return this.RunActiveCommand(vm, data, ref message, elements);
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
            string filePath = this._mActiveCmd.FilePath;
            Result result;
            try
            {
                vm.AssemLoader.HookAssemblyResolve();
                Assembly assembly = vm.AssemLoader.LoadAddinsToTempFolder(filePath, false);
                if (null == assembly)
                {
                    result = Result.Failed;
                }
                else
                {
                    this._mActiveTempFolder = vm.AssemLoader.TempFolder;
                    IExternalCommand externalCommand = assembly.CreateInstance(this._mActiveCmdItem.FullClassName) as IExternalCommand;
                    if (externalCommand == null)
                    {
                        result = Result.Failed;
                    }
                    else
                    {
                        this._mActiveEc = externalCommand;
                        result = this._mActiveEc.Execute(data, ref message, elements);
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
            this._mAddinManager = new ViewModel.AddinManager();
            this._mActiveCmd = null;
            this._mActiveCmdItem = null;
            this._mActiveApp = null;
            this._mActiveAppItem = null;
        }


        public IExternalCommand ActiveEC
        {
            get => this._mActiveEc;
            set => this._mActiveEc = value;
        }


        public Addin ActiveCmd
        {
            get => this._mActiveCmd;
            set => this._mActiveCmd = value;
        }

        public AddinItem ActiveCmdItem
        {
            get => this._mActiveCmdItem;
            set => this._mActiveCmdItem = value;
        }


        public Addin ActiveApp
        {
            get => this._mActiveApp;
            set => this._mActiveApp = value;
        }
        public AddinItem ActiveAppItem
        {
            get => this._mActiveAppItem;
            set => this._mActiveAppItem = value;
        }

        public ViewModel.AddinManager AddinManager
        {
            get => this._mAddinManager;
            set => this._mAddinManager = value;
        }

        private string _mActiveTempFolder = string.Empty;

        private static volatile AddinManagerBase _mInst;

        private IExternalCommand _mActiveEc;

        private Addin _mActiveCmd;

        private AddinItem _mActiveCmdItem;

        private Addin _mActiveApp;

        private AddinItem _mActiveAppItem;

        private ViewModel.AddinManager _mAddinManager;
    }
}
