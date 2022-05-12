using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
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
            string tempPath = Path.GetTempPath();
            MessageBox.Show(tempPath);
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
    public class DebugTrace : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Debug.WriteLine($"This is a test debug Test");
            Trace.WriteLine("This is a test trace writeline");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class DebugWrite : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            for (int i = 0; i < 10; i++)
            {
                Debug.Write($"Error: This is a test DebugWrite Test {i}");
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class DebugWriteLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            for (int i = 0; i < 10; i++)
            {
                Debug.WriteLine($"This is a test DebugWriteLine Test {i}");
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TraceWrite : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            for (int i = 0; i < 10; i++)
            {
                Debug.Write($"This is a test TraceWrite Test {i}");
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TraceWriteLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Debug.WriteLine($"Warning: is a test TraceWriteLine Test");
            for (int i = 0; i < 20; i++)
            {
                Debug.WriteLine($"This is a test TraceWriteLine Test {i}");
            }

            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class ColorWriteLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Debug.WriteLine($"Warning: This is a warning");
            Debug.WriteLine($"Error: This is a error");
            Debug.WriteLine($"Add: This is a add");
            Debug.WriteLine($"Modify: This is a modify");
            Debug.WriteLine($"Delete: This is a delete");
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class DebugAssert : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                Reference r =
                    commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element,
                        "Please pick an element");
                Element element = commandData.Application.ActiveUIDocument.Document.GetElement(r);
                GetCurve(element);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return Result.Succeeded;
        }

        public void GetCurve(Element element)
        {
            Debug.Assert(null != element.Location,
                "expected an element with a valid Location");

            Debug.Assert(element.Location is LocationCurve lc,
                "expected an element with a valid LocationCurve");
        }
    }
}