using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AddInManager.ViewModel;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddInManager.Command
{
    [Transaction(TransactionMode.Manual)]
    public class AddInManagerManual : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            AddInManager.Model.StaticUtil.RegenOption = RegenerationOption.Manual;
            AddInManager.Model.StaticUtil.RegenOption = RegenerationOption.Manual;
            AddInManager.Model.StaticUtil.TransactMode = TransactionMode.Manual;
            return AddinManagerBase.Instance.ExecuteCommand(commandData, ref message, elements, false);
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class AddInManagerFaceless : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return AddinManagerBase.Instance.ExecuteCommand(commandData, ref message, elements, true);
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class AddInManagerReadOnly : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            AddInManager.Model.StaticUtil.RegenOption = RegenerationOption.Manual;
            AddInManager.Model.StaticUtil.TransactMode = TransactionMode.ReadOnly;
            return AddinManagerBase.Instance.ExecuteCommand(commandData, ref message, elements, true);
        }
    }
}
