using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class TestEvaCommand  : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        
        UIApplication uiApplication = commandData.Application;
        MessageBox.Show(uiApplication.Application.Username);
        return Result.Succeeded;
    }
}