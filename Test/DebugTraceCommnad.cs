using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class DebugTraceCommnad : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        Trace.Write("hello");
        return Result.Succeeded;
    }
}