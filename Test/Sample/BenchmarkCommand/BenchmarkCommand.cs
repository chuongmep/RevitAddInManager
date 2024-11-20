using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Test.BenchmarkCommand;

public class BenchmarkCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        throw new System.NotImplementedException();
    }
}

public class ItemReport
{
    public string FolderId { get; set; }
    public string ItemId { get; set; }
    public string ProjectId { get; set; }
    public string ProjectGuid { get; set; }
    public string ModelGuid { get; set; }
    public string ModelName { get; set; }
}