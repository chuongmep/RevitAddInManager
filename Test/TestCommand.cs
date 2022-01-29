using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Test
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
            TaskDialog.Show("Commnad",@"Class2");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class Class3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Command",@"Class3");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class Class4 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Command", @"Class4");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.ReadOnly)]
    public class Class5 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Command",@"Class5");
            return Result.Succeeded;
        }
    }
}
