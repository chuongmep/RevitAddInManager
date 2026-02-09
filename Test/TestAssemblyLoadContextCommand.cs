using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace TestLib;

[Transaction(TransactionMode.Manual)]
public class TestAssemblyLoadContextCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var loadedLibrariesCount = AppDomain.CurrentDomain
            .GetAssemblies()
            .Count(a => a.GetName().Name == "TestLib");

        TaskDialog.Show("dev", $"Loaded Libraries n Count: {loadedLibrariesCount}");

        return Result.Succeeded;
    }
}