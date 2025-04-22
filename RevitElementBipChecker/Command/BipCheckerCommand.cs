using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RevitElementBipChecker.Model;
using RevitElementBipChecker.View;
using RevitElementBipChecker.Viewmodel;

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
            BipCheckerViewmodel vm = new BipCheckerViewmodel(uidoc);
            FrmBipChecker frMainWindows = new FrmBipChecker(vm);
            frMainWindows.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            frMainWindows.SetRevitAsWindowOwner();
            frMainWindows.Show();
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