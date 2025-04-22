using System.Diagnostics.Contracts;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class SoildIntersectCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // select a extrusion element
        Reference reference =
            commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element,
                new ExtrusionFaceSelectionFilter(),
                "Select a extrusion element");
        var doc = commandData.Application.ActiveUIDocument.Document;
        var e1 = doc.GetElement(reference);
        var r2 = commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element,
            new ExtrusionFaceSelectionFilter(), "Select another extrusion element");
        var e2 = doc.GetElement(r2);
        // try see intersect soild
        var solid1 = GetSolid(e1 as Extrusion);
        var solid2 = GetSolid(e2 as Extrusion);
        if (solid1 != null && solid2 != null)
        {
            Solid operation = BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Intersect);
            // create direct shape
            using Autodesk.Revit.DB.Transaction trans = new Transaction(doc, "Create DirectShape");
            trans.Start();
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.ApplicationId = "ApplicationId";
            ds.ApplicationDataId = "ApplicationDataId";
            ds.SetShape(new GeometryObject[] { operation });
            trans.Commit();
        }
        return Result.Succeeded;
    }
    Solid? GetSolid(Extrusion? element)
    {
        Options options = new Options();
        options.ComputeReferences = true;
        options.DetailLevel = ViewDetailLevel.Fine;
        options.IncludeNonVisibleObjects = false;
        GeometryElement geomElem = element.get_Geometry(options);
        foreach (GeometryObject geomObj in geomElem)
        {
            Solid? solid = geomObj as Solid;
            if (solid != null)
            {
                return solid;
            }
        }
        return null;
    }
}