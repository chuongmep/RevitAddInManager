using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitElementBipChecker.Model;
using RevitElementBipChecker.Viewmodel;

namespace RevitElementBipChecker.Command
{
    [Transaction(TransactionMode.Manual)]
    public class ComparePropertyCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;
            Reference r = uidoc.Selection.PickObject(ObjectType.Element);
            Element element = doc.GetElement(r);
            PropertyViewModel viewModel = new PropertyViewModel(element);
            List<BaseDataCompare> propertyData = viewModel.GetPropertyData();
            string path = propertyData.ToDataTable().ExportCsv();
            Process.Start(path);
            return Result.Succeeded;
        }
    }
}
