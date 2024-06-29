using System;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Test2;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class TestEvaCommand  : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        MessageBox.Show("TestEvaCommand");
        string? value = DependLib.ShowDialogFolder();
        Console.WriteLine(value);
        MessageBox.Show(value.ToString());
        return Result.Succeeded;
    }
}