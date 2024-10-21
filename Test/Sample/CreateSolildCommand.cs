using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class CreateSolidCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // // create a solid

        var uidoc = commandData.Application.ActiveUIDocument;
        var doc = uidoc.Document;
        using Autodesk.Revit.DB.Transaction tran = new Transaction(doc, "zz");
        tran.Start();
        var solid = SolidFromFace(commandData);
        CreateDirectShape(doc,solid);
        tran.Commit();

        return Result.Succeeded;
    }

    void CreateDirectShape(Document doc, Solid solid)
    {
        var ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
        ds.ApplicationId = "ApplicationId";
        ds.ApplicationDataId = "ApplicationDataId";
        ds.SetShape(new GeometryObject[] { solid });
    }

    Solid? GetSolid(Autodesk.Revit.DB.Element? element)
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

    public static Solid SolidFromFace(ExternalCommandData  commandData)
    {
        // pick a face
        double height = 1;
        var uidoc = commandData.Application.ActiveUIDocument;
        Reference reference = uidoc.Selection.PickObject(ObjectType.Face, "Select a face");
        var doc = uidoc.Document;
        var face = doc.GetElement(reference).GetGeometryObjectFromReference(reference) as Face;
        var vertices = face.Triangulate().Vertices.ToList();
        return SolidFromVertices(vertices, height);
    }
    public static Solid SolidFromVertices(List<XYZ> vertices, double height)
    {
        // create face
        List<CurveLoop> curveLoops = new List<CurveLoop>();
        CurveLoop curveLoop = new CurveLoop();
        for (int i = 0; i < vertices.Count; i++)
        {
            curveLoop.Append(Line.CreateBound(vertices[i], vertices[(i + 1) % vertices.Count]));
        }
        curveLoops.Add(curveLoop);
        Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, XYZ.BasisZ, height);
        return solid;

    }
    public Solid SolidFromBoundingBox(ExternalCommandData commandData)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        Reference reference =
            commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element,
                "Select a extrusion element");
        var extrusion = commandData.Application.ActiveUIDocument.Document.GetElement(reference);
        Solid? solid = GetSolid(extrusion);
        BoundingBoxXYZ bbox = solid.GetBoundingBox();
        // corners in BBox coords
        XYZ pt0 = new XYZ(bbox.Min.X, bbox.Min.Y, bbox.Min.Z);
        XYZ pt1 = new XYZ(bbox.Max.X, bbox.Min.Y, bbox.Min.Z);
        XYZ pt2 = new XYZ(bbox.Max.X, bbox.Max.Y, bbox.Min.Z);
        XYZ pt3 = new XYZ(bbox.Min.X, bbox.Max.Y, bbox.Min.Z);
        //edges in BBox coords
        Line edge0 = Line.CreateBound(pt0, pt1);
        Line edge1 = Line.CreateBound(pt1, pt2);
        Line edge2 = Line.CreateBound(pt2, pt3);
        Line edge3 = Line.CreateBound(pt3, pt0);
        //create loop, still in BBox coords
        List<Curve> edges = new List<Curve>();
        edges.Add(edge0);
        edges.Add(edge1);
        edges.Add(edge2);
        edges.Add(edge3);
        Double height = bbox.Max.Z - bbox.Min.Z;
        CurveLoop baseLoop = CurveLoop.Create(edges);
        List<CurveLoop> loopList = new List<CurveLoop>();
        loopList.Add(baseLoop);
        Solid preTransformBox = GeometryCreationUtilities.CreateExtrusionGeometry(loopList, XYZ.BasisZ, height);
        Solid transformBox = SolidUtils.CreateTransformed(preTransformBox, bbox.Transform);
        return transformBox;

    }
}