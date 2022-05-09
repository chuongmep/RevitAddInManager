using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Test
{
    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Command", "Hello Word");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TestCommandTransaction : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document document = uidoc.Document;
            using (Transaction tran = new Transaction(document, "tran"))
            {
                tran.Start();
                TaskDialog.Show("Add-in Manager", document.Title);
                Reference r = uidoc.Selection.PickObject(ObjectType.Element);
                Element element = document.GetElement(r);
                Parameter p = element.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS);
                p.Set("Hello Word");
                TaskDialog.Show("Add-in Manager", "Assigned Value");
                tran.Commit();
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Class2 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Commnad", @"Class2");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class Class3 : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Command", @"Class3");
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
    public class TestReadOnly : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Command", @"Class5");
            return Result.Succeeded;
        }
    }
    [Transaction(TransactionMode.Manual)]
    public class TestDebugTrace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
          
            Debug.WriteLine("This is a test debug");
            Trace.WriteLine("This is a test trace");
            return Result.Succeeded;
        }
    }
}