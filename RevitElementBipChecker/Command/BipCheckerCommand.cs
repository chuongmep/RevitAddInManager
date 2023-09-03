using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using KellermanSoftware.CompareNetObjects;
using ObjectsComparer;
using RevitElementBipChecker.Model;
using RevitElementBipChecker.View;
using RevitElementBipChecker.Viewmodel;
using Difference = KellermanSoftware.CompareNetObjects.Difference;

namespace RevitElementBipChecker.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    public class BipCheckerCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;
            
      
            

            // write to csv
            
            // DataTable dataTable = new DataTable();
            // dataTable.Columns.Add("Name");
            // dataTable.Columns.Add("Type");
            // dataTable.Columns.Add("Value1");
            // dataTable.Columns.Add("Value2");
            // foreach (var difference in differences)
            // {
            //     DataRow dataRow = dataTable.NewRow();
            //     dataRow["Name"] = difference.Name;
            //     dataRow["Type"] = difference.Type;
            //     dataRow["Value1"] = difference.Value1;
            //     dataRow["Value2"] = difference.Value2;
            //     dataTable.Rows.Add(dataRow);
            // }
            // dataTable.ExportCsv();
            // BipCheckerViewmodel vm = new BipCheckerViewmodel(uidoc);
            // FrmBipChecker frMainWindows = new FrmBipChecker(vm);
            // frMainWindows.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            // frMainWindows.SetRevitAsWindowOwner();
            // frMainWindows.Show();
            return Result.Succeeded;
        }
        
    }
    
    
    public class BipCheckerCommandAvail : IExternalCommandAvailability
    {
        public BipCheckerCommandAvail()
        {
        }

        public bool IsCommandAvailable(UIApplication uiApp, CategorySet selectedCategories)
        {
            if (uiApp.ActiveUIDocument != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}