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
        double plus = DependLib.Plus();
        Console.WriteLine(plus);
        MessageBox.Show(plus.ToString());
        return Result.Succeeded;
    }
}