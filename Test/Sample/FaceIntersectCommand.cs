using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class FaceIntersectCommand : IExternalCommand
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
        // try see intersect
        var face1 = GetTopFace(e1 as Extrusion);
        var face2 = GetTopFace(e2 as Extrusion);
        if (face1 != null && face2 != null)
        {
            Trace.WriteLine("CheckIntersect:");
            CheckIntersect(face1, face2);
        }
        // try with another method
        var poly1 = face1.Triangulate().Vertices.ToList();
        var poly2 = face2.Triangulate().Vertices.ToList();
        List<XYZ> intersection = GetIntersection(poly1, poly2);
        Trace.WriteLine("Intersection:");
        foreach (var xyz in intersection)
        {
            Trace.WriteLine(xyz.ToString());
        }
        // create direct shape
        using Autodesk.Revit.DB.Transaction trans = new Transaction(doc, "Create Direct Shape");
        trans.Start();
        DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
        ds.ApplicationId = "ApplicationId";
        ds.ApplicationDataId = "ApplicationDataId";
        var curveloop = new CurveLoop();
        for (int i = 0; i < intersection.Count; i++)
        {
            curveloop.Append(Line.CreateBound(intersection[i], intersection[(i + 1) % intersection.Count]));
        }
        Solid solid = GeometryCreationUtilities.CreateExtrusionGeometry(new List<CurveLoop> { curveloop }, XYZ.BasisZ, 1);
        ds.SetShape(new GeometryObject[] { solid });
        trans.Commit();
        return Result.Succeeded;
    }
    public PlanarFace? GetTopFace(Extrusion? extrusion)
    {
        Options options = new Options();
        options.ComputeReferences = true;
        options.DetailLevel = ViewDetailLevel.Fine;
        options.IncludeNonVisibleObjects = false;
        GeometryElement geomElem = extrusion.get_Geometry(options);
        foreach (GeometryObject geomObj in geomElem)
        {
            Solid? solid = geomObj as Solid;
            if (solid != null)
            {
                foreach (Face face in solid.Faces)
                {
                    PlanarFace? planarFace = face as PlanarFace;
                    if (planarFace != null)
                    {
                        // make sure it is top face
                        if (planarFace.FaceNormal.Z > 0)
                        {
                            return planarFace;
                        }
                    }
                }
            }
        }
        return null;
    }

    public void CheckIntersect(PlanarFace face1, PlanarFace face2)
    {
        FaceIntersectionFaceResult result = face1.Intersect(face2, out Curve curve);
        Trace.WriteLine(result.ToString());
        // fu...k, why ?????????? ^🫡🫡^
        //https://thebuildingcoder.typepad.com/blog/2019/09/face-intersect-face-is-unbounded.html

    }
    public static List<XYZ> GetIntersection(List<XYZ> poly1, List<XYZ> poly2)
    {
        List<XYZ> outputList = poly1;

        // Each segment of the clipping polygon
        for (int i = 0; i < poly2.Count; i++)
        {
            List<XYZ> inputList = new List<XYZ>(outputList);
            outputList.Clear();

            XYZ A = poly2[(i + poly2.Count - 1) % poly2.Count];
            XYZ B = poly2[i];

            for (int j = 0; j < inputList.Count; j++)
            {
                XYZ P = inputList[(j + inputList.Count - 1) % inputList.Count];
                XYZ Q = inputList[j];

                if (IsInside(A, B, Q))
                {
                    if (!IsInside(A, B, P))
                        outputList.Add(Intersection(A, B, P, Q));
                    outputList.Add(Q);
                }
                else if (IsInside(A, B, P))
                {
                    outputList.Add(Intersection(A, B, P, Q));
                }
            }
        }

        return outputList;
    }

    private static bool IsInside(XYZ A, XYZ B, XYZ C)
    {
        return (B.X - A.X) * (C.Y - A.Y) > (B.Y - A.Y) * (C.X - A.X);
    }

    private static XYZ Intersection(XYZ A, XYZ B, XYZ P, XYZ Q)
    {
        double A1 = B.Y - A.Y;
        double B1 = A.X - B.X;
        double C1 = A1 * A.X + B1 * A.Y;

        double A2 = Q.Y - P.Y;
        double B2 = P.X - Q.X;
        double C2 = A2 * P.X + B2 * P.Y;

        double determinant = A1 * B2 - A2 * B1;
        if (determinant == 0)
            return new XYZ();  // Lines are parallel
        else
        {
            double x = (B2 * C1 - B1 * C2) / determinant;
            double y = (A1 * C2 - A2 * C1) / determinant;
            return new XYZ(x, y,P.Z);
        }
    }


}

public class ExtrusionFaceSelectionFilter : ISelectionFilter
{
    public bool AllowElement(Element elem)
    {
        return elem is Autodesk.Revit.DB.Extrusion;
    }

    public bool AllowReference(Reference reference, XYZ position)
    {
        return false;
    }
}