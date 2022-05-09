using System.Diagnostics;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitAddinManager.Model;

namespace RevitAddinManager.Command;

[Transaction(TransactionMode.Manual)]
public class AddInManagerManual : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        Debug.Listeners.Clear();
        Trace.Listeners.Clear();
        CodeListener codeListener = new CodeListener();
        Debug.Listeners.Add(codeListener);
        StaticUtil.RegenOption = RegenerationOption.Manual;
        StaticUtil.TransactMode = TransactionMode.Manual;
        Result result = AddinManagerBase.Instance.ExecuteCommand(commandData, ref message, elements, false);
        Debug.Close();
        Trace.Close();
        return result;

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
        StaticUtil.RegenOption = RegenerationOption.Manual;
        StaticUtil.TransactMode = TransactionMode.ReadOnly;
        return AddinManagerBase.Instance.ExecuteCommand(commandData, ref message, elements, false);
    }
}