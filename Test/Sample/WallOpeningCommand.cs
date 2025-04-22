using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class WallOpeningCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uidoc = commandData.Application.ActiveUIDocument;
        Document doc = uidoc.Document;
        // create a wall opening
        Reference reference = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select wall");
        Wall wall = doc.GetElement(reference) as Wall;
        using Autodesk.Revit.DB.Transaction transaction = new Transaction(doc, "Create wall opening");
        transaction.Start();
        Reference? startPoints = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.PointOnElement, "Select start points");
        Reference? endPoints = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.PointOnElement, "Select end points");
        doc.Create.NewOpening(wall, startPoints.GlobalPoint, endPoints.GlobalPoint);
        transaction.Commit();
        return Result.Succeeded;
    }
}