using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RevitElementBipChecker.Model;
using RevitElementBipChecker.View;
using RevitElementBipChecker.Viewmodel;

namespace RevitElementBipChecker.Command;


/// <summary>
/// Compare Two Element with parameter instance different
/// </summary>
[Transaction(TransactionMode.Manual)]
public class CompareTwoEleCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIApplication uiapp = commandData.Application;
        UIDocument uidoc = uiapp.ActiveUIDocument;
        Document doc = uidoc.Document;
        Element element1 = null;
        Element element2 = null;
        try
        {
            Reference r1 = uidoc.Selection.PickObject(ObjectType.Element, "Select first element");
            element1 = doc.GetElement(r1);
            Reference r2 = uidoc.Selection.PickObject(ObjectType.Element, "Select second element");
            element2 = doc.GetElement(r2);
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            MessageBox.Show("Please Select Two Element");
            return Result.Failed;
        }
        catch (Exception e)
        {
            MessageBox.Show("Error: " + e.Message);
            return Result.Cancelled;
        }
        ParameterCompareViewModel viewModel = new ParameterCompareViewModel(uiapp,element1, element2);
        FrmCompareBip frmCompareBip = new FrmCompareBip(viewModel);
     
        frmCompareBip.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        frmCompareBip.SetRevitAsWindowOwner();
        frmCompareBip.Show();
        return Result.Succeeded;
    }
}