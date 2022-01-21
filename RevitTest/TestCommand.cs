using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace RevitTest
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand:  IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Command","Hello Word");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class Class2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show(@"Class2");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class Class3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show(@"Class3");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class Class4 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show(@"Class4");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.ReadOnly)]
    public class Class5 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            MessageBox.Show(@"Class5");
            return Result.Succeeded;
        }
    }
}
