using System;
using System.Collections.Generic;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Point = Autodesk.Revit.DB.Point;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class TestConorBoundingBoxCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        using Autodesk.Revit.DB.Transaction transaction =
            new Autodesk.Revit.DB.Transaction(commandData.Application.ActiveUIDocument.Document);
        transaction.Start("hack");
        var doc = commandData.Application.ActiveUIDocument.Document;
        Reference r = commandData.Application.ActiveUIDocument.Selection.PickObject(ObjectType.Element);
        var instance = doc.GetElement(r) as Autodesk.Revit.DB.FamilyInstance;
        // Collect 4x corners of symbol Bbox
        BoundingBoxXYZ bbox = instance.Symbol.get_BoundingBox(doc.ActiveView);
        (double, double) widthAndHeight = GetWidthAndHeight(bbox);
        double rectangleWidth = widthAndHeight.Item1;
        double rectangleHeight = widthAndHeight.Item2;
        var location = instance.Location as LocationPoint;
        double rotationAngle = location.Rotation;

        bbox = instance.get_BoundingBox(doc.ActiveView);
        var center = bbox.Min + (bbox.Max - bbox.Min) / 2;
        (double topRightX, double topRightY) =
            RotatePoint(rectangleWidth / 2, rectangleHeight / 2, center, rotationAngle);
        (double bottomRightX, double bottomRightY) =
            RotatePoint(rectangleWidth / 2, -rectangleHeight / 2, center, rotationAngle);
        (double topLeftX, double topLeftY) =
            RotatePoint(-rectangleWidth / 2, rectangleHeight / 2, center, rotationAngle);
        (double bottomLeftX, double bottomLeftY) =
            RotatePoint(-rectangleWidth / 2, -rectangleHeight / 2, center, rotationAngle);

        topRightX += center.X;
        topRightY += center.Y;
        bottomRightX += center.X;
        bottomRightY += center.Y;
        topLeftX += center.X;
        topLeftY += center.Y;
        bottomLeftX += center.X;
        bottomLeftY += center.Y;
        List<XYZ> corners = new List<XYZ>();
        corners.Add(new XYZ(bottomLeftX, bottomLeftY, 0));
        corners.Add(new XYZ(bottomRightX, bottomRightY, 0));
        corners.Add(new XYZ(topRightX, topRightY, 0));
        corners.Add(new XYZ(topLeftX, topLeftY, 0));
        foreach (XYZ corner in corners)
        {
            DirectShape ds =
                DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_GenericModel));
            ds.SetShape(new List<GeometryObject>(new[] { Point.Create(corner), }));
            ds.Name = "BoundingBox";
            // doc.ActiveView.IsolateElementsTemporary(new List<ElementId>(){ds.Id, instance.Id});
        }

        transaction.Commit();
        return Result.Succeeded;
    }

    public static List<XYZ> Corners(BoundingBoxXYZ boundingBox)
    {
        if (boundingBox == null)
        {
            throw new ArgumentNullException("BoundingBox is null");
        }

        XYZ minPoint = boundingBox.Min;
        XYZ maxPoint = boundingBox.Max;

        List<XYZ> corners = new List<XYZ>();
        corners.Add(minPoint);
        corners.Add(new XYZ(minPoint.X, minPoint.Y, maxPoint.Z));
        corners.Add(new XYZ(minPoint.X, maxPoint.Y, minPoint.Z));
        corners.Add(new XYZ(minPoint.X, maxPoint.Y, maxPoint.Z));
        corners.Add(new XYZ(maxPoint.X, minPoint.Y, minPoint.Z));
        corners.Add(new XYZ(maxPoint.X, minPoint.Y, maxPoint.Z));
        corners.Add(new XYZ(maxPoint.X, maxPoint.Y, minPoint.Z));
        corners.Add(maxPoint);

        return corners;
    }

    public double GetArea(BoundingBoxXYZ boundingBox)
    {
        if (boundingBox == null)
        {
            throw new ArgumentNullException("BoundingBox is null");
        }

        double width = boundingBox.Max.X - boundingBox.Min.X;
        double height = boundingBox.Max.Y - boundingBox.Min.Y;

        return width * height;
    }

    public (double, double) GetWidthAndHeight(BoundingBoxXYZ boundingBox)
    {
        if (boundingBox == null)
        {
            throw new ArgumentNullException("BoundingBox is null");
        }

        XYZ minPoint = boundingBox.Min;
        XYZ maxPoint = boundingBox.Max;

        double width = maxPoint.X - minPoint.X;
        double height = maxPoint.Y - minPoint.Y;

        return (width, height);
    }

    public static (double, double) RotatePoint(double x, double y, XYZ center, double angleRad)
    {
        double xRotated = center.X + (x - center.X) * Math.Cos(angleRad) - (y - center.Y) * Math.Sin(angleRad);
        double yRotated = center.Y + (x - center.X) * Math.Sin(angleRad) + (y - center.Y) * Math.Cos(angleRad);
        return (xRotated, yRotated);
    }
}