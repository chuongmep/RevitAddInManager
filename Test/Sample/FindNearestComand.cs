using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class FindIntersectCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uidoc = commandData.Application.ActiveUIDocument;
        var doc = commandData.Application.ActiveUIDocument.Document;
        Reference? refElement = uidoc.Selection.PickObject(ObjectType.Element);
        var element = doc.GetElement(refElement);
        LocationPoint? locationPoint = element.Location as LocationPoint;
        if (locationPoint == null) throw new Exception("element location is null");
        var category = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Floors);
        var categories = new List<Category>() { category };
        Element? findNearest2 = FindNearest2(commandData, locationPoint.Point, categories, new XYZ(0, 0, -1), true);
        MessageBox.Show(findNearest2.Id.ToString());
        // Way 2
        IList<ReferenceWithContext> referenceWithContext = FindNearest(doc, locationPoint!.Point, categories,
            new XYZ(0, 0, 1), FindReferenceTarget.All, true);
        if (referenceWithContext.Count==0)
        {
            MessageBox.Show("Nothing");
        }
        else
        {
            StringBuilder sb = new StringBuilder();
            foreach (ReferenceWithContext context in referenceWithContext)
            {
                sb.AppendLine(context.GetReference().ElementId.ToString());
                sb.AppendLine(context.GetReference().LinkedElementId.ToString());
                sb.AppendLine(context.GetReference().GlobalPoint.ToString());
            }
            MessageBox.Show(sb.ToString());
        }

        return Result.Succeeded;
    }

    private Element? FindNearest2(ExternalCommandData commandData, XYZ point,
        List<Autodesk.Revit.DB.Category> categories, XYZ direction,bool isIncludeRevitLink)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        var categoryFilter = new ElementMulticategoryFilter(categories.Select(x => x.Id).ToList());
        var collector = new FilteredElementCollector(doc);
        IList<Element> elements = collector.WherePasses(categoryFilter)
            .WhereElementIsNotElementType().ToElements();
        if (isIncludeRevitLink)
        {
            var revitLinkTypes = new FilteredElementCollector(doc)
                .OfClass(typeof(RevitLinkType)).Cast<RevitLinkType>();
            foreach (RevitLinkType revitLinkType in revitLinkTypes)
            {
                var linkDoc = commandData.Application.Application.Documents.Cast<Document>()
                    .FirstOrDefault(x => x.Title+".rvt" == revitLinkType.Name);
                var collectorLink = new FilteredElementCollector(linkDoc);
                IList<Element> elementIsNotElementType = collectorLink
                    .WherePasses(categoryFilter)
                    .WhereElementIsNotElementType()
                    .ToElements();
                elements = elements.Concat(elementIsNotElementType).ToList();

            }
        }
        // check distance between point and element by direction
        Element? nearestElement = null;
        double minDistance = double.MaxValue;
        foreach (Element element in elements)
        {
            BoundingBoxXYZ boundingBoxXyz = element.get_BoundingBox(null);
            if (boundingBoxXyz == null) continue;
            var center = boundingBoxXyz.Max.Add(boundingBoxXyz.Min).Divide(2);
            // check distance by project direction from input
            var plane = Plane.CreateByNormalAndOrigin(direction, point);
            // if direction is opposite with element direction, skip
            if (IsOppositeDirection(direction, center - point)) continue;
            //calc  distance from point to plane by math
            double currentDis = CalculateDistanceFromPointToPlane(center, plane);
            if (currentDis < minDistance)
            {
                minDistance = currentDis;
                nearestElement = element;
            }
        }
        return nearestElement;
    }
    bool IsOppositeDirection(XYZ direction, XYZ vector)
    {
        return direction.DotProduct(vector) < 0;
    }

    public double CalculateDistanceFromPointToPlane(XYZ point, Plane plane)
    {
        // Get the normal vector of the plane
        XYZ normal = plane.Normal;

        // Get a point on the plane
        XYZ pointOnPlane = plane.Origin;

        // Calculate the vector from the point on the plane to the given point
        XYZ vectorToPoint = point - pointOnPlane;

        // Use the dot product to find the signed distance
        double signedDistance = normal.DotProduct(vectorToPoint);

        // The absolute value of the signed distance gives the shortest distance
        return Math.Abs(signedDistance);
    }

    private IList<ReferenceWithContext> FindNearest(Autodesk.Revit.DB.Document doc, XYZ point,
        List<Category> categories, XYZ direction, FindReferenceTarget referenceTarget,
        bool findReferencesInRevitLinks)
    {
        // Find a 3D view to use for the Reference Intersection constructor
        FilteredElementCollector collector = new FilteredElementCollector(doc);
        Func<View3D?, bool> isNotTemplate = v3 => !(v3.IsTemplate);
        View3D? view3D = collector.OfClass(typeof(View3D))?.Cast<View3D>()?.First<View3D>(isNotTemplate);
        if (view3D == null)
        {
            throw new ArgumentNullException($"FindNearest require 3D view open");
        }

        // Use the center of the skylight bounding box as the start point.
        // Project in the negative Z direction down to the ceiling.
        IEnumerable<string> categoriesEnum = categories.Select(x => x.Name);
        ElementFilter? elementFilter = FiltersElementByCategory(doc, categoriesEnum);
        ReferenceIntersector refIntersect = new ReferenceIntersector(elementFilter, referenceTarget, view3D);
        refIntersect.FindReferencesInRevitLinks = findReferencesInRevitLinks;
        IList<ReferenceWithContext> referenceWithContexts = refIntersect.Find(point, direction);
        // ReferenceWithContext referenceWithContext = refIntersect.FindNearest(point, direction);
        return referenceWithContexts;
    }

    /// <summary>
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="categoryNames"></param>
    /// <returns></returns>
    private ElementFilter? FiltersElementByCategory(Document doc, IEnumerable<string> categoryNames)
    {
        ElementFilter? categoriesFilter = null;
        if (categoryNames != null && categoryNames.Any())
        {
            var categoryIds = new List<ElementId>();
            foreach (var categoryName in categoryNames)
            {
                var category = doc.Settings.Categories.get_Item(categoryName);
                if (category != null)
                {
                    categoryIds.Add(category.Id);
                }
            }

            var categoryFilters = new List<ElementFilter>();
            if (categoryIds.Count > 0)
            {
                var categoryRule = new FilterCategoryRule(categoryIds);
                var categoryFilter = new ElementParameterFilter(categoryRule);
                categoryFilters.Add(categoryFilter);
            }

            if (categoryFilters.Count > 0)
            {
                categoriesFilter = new LogicalOrFilter(categoryFilters);
            }
        }

        return categoriesFilter;
    }
}