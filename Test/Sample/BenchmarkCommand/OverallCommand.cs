using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.UI;
using Test.BenchmarkCommand;

namespace Test.Sample.BenchmarkCommand;

[Transaction(TransactionMode.Manual)]
public class OverallCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        BenchmarkReport benchmarkReport = new BenchmarkReport();
        benchmarkReport.FolderId = GetFolderId(doc);
        benchmarkReport.ItemId = GetItemId(doc);
        benchmarkReport.ProjectId = GetProjectId(doc);
        benchmarkReport.ProjectGuid = GetProjectGuid(doc);
        benchmarkReport.ModelGuid = GetModelGuid(doc);
        benchmarkReport.ModelName = doc.Title + ".rvt";
        benchmarkReport.Version = doc.Application.VersionNumber;
        benchmarkReport.Path = GetPath(doc);
        benchmarkReport.Unit = GetUnit(doc);
        benchmarkReport.Categories = GetCountCategories(doc).ToString();
        benchmarkReport.Worksets = GetCountWorksets(doc).ToString();
        benchmarkReport.Levels = GetCountLevels(doc).ToString();
        benchmarkReport.Warnings = GetCountWarnings(doc).ToString();
        benchmarkReport.NestedElements = GetCountNestedElements(doc).ToString();
        benchmarkReport.Families = GetCountFamilies(doc).ToString();
        benchmarkReport.FamilyTypes = GetCountFamilyTypes(doc).ToString();
        benchmarkReport.RevitLinks = GetCountRevitLinks(doc).ToString();
        benchmarkReport.CadLinked = GetCountCadLinked(doc).ToString();
        benchmarkReport.Groups = GetCountGroups(doc).ToString();
        benchmarkReport.CoordinationModel = GetCountCoordinationModel(doc).ToString();
        benchmarkReport.Schedules = GetCountSchedules(doc).ToString();
        benchmarkReport.Views = GetCountViews(doc).ToString();
        benchmarkReport.Sheets = GetCountSheets(doc).ToString();
        benchmarkReport.ViewFilters = GetCountViewFilters(doc).ToString();
        // save json
        string fileName = "OverallBenchmark.csv";
        string filePath =
            System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                fileName);
        CsvUtils.WriteOverallBenmark(new List<BenchmarkReport>(){benchmarkReport}, filePath);
        Process.Start(filePath);
        return Result.Succeeded;
    }

    public string? GetFolderId(Document doc)
    {
        return doc.GetCloudFolderId(true);
    }

    public string GetItemId(Document doc)
    {
        return doc.GetCloudModelUrn();
    }

    public string GetModelGuid(Document doc)
    {
        if (!doc.IsModelInCloud)
        {
            return "";
        }
        ModelPath modelPath = doc.GetCloudModelPath();
        return modelPath.GetModelGUID().ToString();
    }

    public string GetProjectGuid(Document doc)
    {
        if (!doc.IsModelInCloud)
        {
            return "";
        }
        ModelPath modelPath = doc.GetCloudModelPath();
        return modelPath.GetProjectGUID().ToString();
    }

    public string GetProjectId(Document doc)
    {
        return doc.GetProjectId();
    }

    public string GetPath(Document doc)
    {
        if (doc.IsModelInCloud)
        {
            return doc.GetCloudModelPath().ToString();
        }

        return doc.PathName;
    }

    public string GetUnit(Document doc)
    {
        return doc.DisplayUnitSystem.ToString();
    }

    public double GetCountCategories(Document doc)
    {
        var categories = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(x=>x.Category!=null)
            .Select(x => x.Category.Name)
            .Distinct();
        return categories.Count();
    }
    public double GetCountWorksets(Document doc)
    {
        var worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
        return worksets.Count;
    }
    public double GetCountLevels(Document doc)
    {
        var levels = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_Levels).ToElements();
        return levels.Count;
    }
    public double GetCountWarnings(Document doc)
    {
        var warnings = doc.GetWarnings();
        return warnings.Count;
    }
    public double GetCountNestedElements(Document doc)
    {
        // filter family instance
        var familyInstances = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilyInstance))
            .Cast<FamilyInstance>();
        List<string> names = new List<string>();
        var nestedElements = familyInstances.Where(x => x.GetSubComponentIds().Count > 0);
        foreach (var nestedElement in nestedElements)
        {
            var subComponentIds = nestedElement.GetSubComponentIds();
            foreach (var subComponentId in subComponentIds)
            {
                var subComponent = doc.GetElement(subComponentId);
                names.Add(subComponent.Name);
            }
        }
        // count unique nested elements
        return names.Distinct().Count();
    }
    public double GetCountFamilies(Document doc)
    {
        var families = new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .ToElements();
        return families.Count;
    }
    public double GetCountFamilyTypes(Document doc)
    {
        var familySymbols = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .ToElements();
        return familySymbols.Count;
    }
    public double GetCountRevitLinks(Document doc)
    {
        var revitLinks = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkInstance))
            .ToElements();
        return revitLinks.Count;
    }
    public double GetCountCadLinked(Document doc)
    {
        var cadLinks = new FilteredElementCollector(doc)
            .OfClass(typeof(CADLinkType))
            .ToElements();
        return cadLinks.Count;
    }
    public double GetCountGroups(Document doc)
    {
        var groups = new FilteredElementCollector(doc)
            .OfClass(typeof(Group))
            .ToElements();
        return groups.Count;
    }
    public double GetCountCoordinationModel(Document doc)
    {
        ICollection<ElementId> instanceIds = DirectContext3DDocumentUtils.GetDirectContext3DHandleInstances(doc, new ElementId(BuiltInCategory.OST_Coordination_Model));
        foreach (var id in instanceIds)
        {
            Element elem = doc.GetElement(id);
            if (null != elem)
            {
                Element typeElem = doc.GetElement(elem.GetTypeId());
                if (null != typeElem)
                {
                    Parameter param = typeElem.LookupParameter("Path");
                    string path = param.AsValueString();
                    // Check if this is the CM you're looking for by evaluating 'path'
                }
            }
        }
        return instanceIds.Count;
    }
    public double GetCountSchedules(Document doc)
    {
        var schedules = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSchedule))
            .ToElements();
        return schedules.Count;
    }
    public double GetCountViews(Document doc)
    {
        var views = new FilteredElementCollector(doc)
            .OfClass(typeof(View))
            .ToElements();
        return views.Count;
    }
    public double GetCountSheets(Document doc)
    {
        var sheets = new FilteredElementCollector(doc)
            .OfClass(typeof(ViewSheet))
            .ToElements();
        return sheets.Count;
    }
    public double GetCountViewFilters(Document doc)
    {
        var viewFilters = new FilteredElementCollector(doc)
            .OfClass(typeof(ParameterElement))
            .ToElements();
        return viewFilters.Count;
    }
}

public class BenchmarkReport
{
    public string? FolderId { get; set; }
    public string? ItemId { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectGuid { get; set; }
    public string? ModelGuid { get; set; }
    public string? ModelName { get; set; }
    public string? Version { get; set; }
    public string? Path { get; set; }
    public string? Unit { get; set; }
    public string? Categories { get; set; }
    public string? Worksets { get; set; }
    public string? Levels { get; set; }
    public string? Warnings { get; set; }
    public string? NestedElements { get; set; }
    public string? Families { get; set; }
    public string? FamilyTypes { get; set; }
    public string? RevitLinks { get; set; }
    public string? CadLinked { get; set; }
    public string? Groups { get; set; }
    public string? CoordinationModel { get; set; }
    public string? Schedules { get; set; }
    public string? Views { get; set; }
    public string? Sheets { get; set; }
    public string? ViewFilters { get; set; }
}