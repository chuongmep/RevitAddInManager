using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Test.BenchmarkCommand;

namespace Test.Sample.BenchmarkCommand;

[Transaction(TransactionMode.Manual)]
public class CategoriesCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        List<CategoryBenchmark> categoryBenchmarks = new List<CategoryBenchmark>();
        var categories = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .Where(x=>x.Category!=null)
            .GroupBy(x => x.Category.Name);
        FilteredWorksetCollector fwc = new FilteredWorksetCollector(doc);
        WorksetKindFilter wf = new WorksetKindFilter(WorksetKind.UserWorkset);
        ICollection<Workset> worksets = fwc.WherePasses(wf).ToWorksets();
        foreach (IGrouping<string, Element>? category in categories)
        {
            var categoryBenchmark = new CategoryBenchmark();
            categoryBenchmark.ModelName = doc.Title + ".rvt";
            categoryBenchmark.Count = category.Count();
            categoryBenchmark.Id = category.First().Category.Id.ToString();
            categoryBenchmark.Name = category.Key;
            categoryBenchmark.Workset = CountWorksetName(worksets,category.ToList());
            categoryBenchmark.Family = CountFamilyName(category.ToList());
            categoryBenchmark.Type = CountFamilyTypeName(category.ToList());
            categoryBenchmark.AssemblyCode = CountAssemblyCode(category.ToList());
            categoryBenchmark.Nestest = CountNestestElement(category.ToList());
            categoryBenchmark.Length = category.Sum(x=> UnitFeetToMeter(x.LookupParameter("Length")?.AsDouble() ?? 0));
            categoryBenchmark.Width = category.Sum(x=> UnitFeetToMeter(x.LookupParameter("Width")?.AsDouble() ?? 0));
            categoryBenchmark.Height = category.Sum(x=> UnitFeetToMeter(x.LookupParameter("Height")?.AsDouble() ?? 0));
            categoryBenchmarks.Add(categoryBenchmark);
        }
        // save csv
        string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string filePath = System.IO.Path.Combine(folderPath, "categories.csv");
        CsvUtils.WriteCsvCategories(categoryBenchmarks, filePath);
        Process.Start(filePath);
        return Result.Succeeded;
    }
    private double CountWorksetName(ICollection<Workset> worksets,List<Element> elements)
    {
        List<string> names = new List<string>();
        // get user workset created
        foreach (Element element in elements)
        {
            WorksetId worksetId = element.WorksetId;
            if (worksetId == WorksetId.InvalidWorksetId) continue;
            var workset = worksets.FirstOrDefault(x => x.Id == worksetId);
            if (workset != null) names.Add(workset.Name);
        }
        return names.Distinct().Count();
    }
    private double CountFamilyTypeName(List<Element> elements)
    {
        List<string> names = new List<string>();
        foreach (Element element in elements)
        {
            string? familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM)?.AsValueString();
            string? typeName = element.get_Parameter(BuiltInParameter.ELEM_TYPE_PARAM)?.AsValueString();
            if (familyName != null && typeName != null) names.Add(familyName + typeName);
        }
        return names.Distinct().Count();
    }
    private double CountFamilyName(List<Element> elements)
    {
        List<string?> names = new List<string?>();
        foreach (Element element in elements)
        {
            string? familyName = element.get_Parameter(BuiltInParameter.ELEM_FAMILY_PARAM)?.AsValueString();
            if (familyName != null) names.Add(familyName);

        }
        return names.Distinct().Count();
    }

    private double CountNestestElement(List<Autodesk.Revit.DB.Element> elements)
    {
        List<string> names = new List<string>();
        foreach (Element element in elements)
        {
            if(element is FamilyInstance familyInstance)
            {
                ICollection<ElementId> subComponentIds = familyInstance.GetSubComponentIds();
                foreach (ElementId subComponentId in subComponentIds)
                {
                    names.Add(element.Document.GetElement(subComponentId).Name);
                }
            }
        }
        return names.Distinct().Count();
    }

    private double CountAssemblyCode(List<Element> elements)
    {
        List<string> names = new List<string>();
        var familySymbols = elements.Select(x => x.Document.GetElement(x.GetTypeId()) as FamilySymbol);
        foreach (FamilySymbol familySymbol in familySymbols)
        {
            string? assemblyCode = familySymbol?.get_Parameter(BuiltInParameter.UNIFORMAT_CODE)?.AsValueString();
            if (assemblyCode != null) names.Add(assemblyCode);
        }
        return names.Distinct().Count();
    }


    private double UnitFeetToMeter(double value)
    {
        return UnitUtils.Convert(value,UnitTypeId.Feet,UnitTypeId.Meters);
    }
    public class CategoryBenchmark
    {
        public string ModelName { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public double Count { get; set; } = 0;
        public double Workset { get; set; }
        public double Family { get; set; } = 0;

        public double Type { get; set; } = 0;
        public double Nestest { get; set; } = 0;

        public double AssemblyCode { get; set; } = 0;
        // set feild name csv
        [CsvHelper.Configuration.Attributes.Name("Length(m)")]
        public double Length { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Width(m)")]
        public double Width { get; set; }
        [CsvHelper.Configuration.Attributes.Name("Height(m)")]
        public double Height { get; set; }
    }
}