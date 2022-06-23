using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitAddinManager.Command;

[Transaction(TransactionMode.Manual)]
public class DockableCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiapp = commandData.Application;

        if (DockablePane.PaneIsRegistered(App.PaneId))
        {
            DockablePane docpanel = uiapp.GetDockablePane(App.PaneId);
#if R14 || R15
            docpanel.Show();
#else
            if (docpanel.IsShown())
                docpanel.Hide();
            else
                docpanel.Show();
#endif
        }

        else
        {
            return Result.Failed;
        }

        return Result.Succeeded;
    }
}