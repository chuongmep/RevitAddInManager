using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Test.BenchmarkCommand;

namespace Test.Sample.BenchmarkCommand;

[Transaction(TransactionMode.Manual)]
public class WorksetsCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        var worksets = new FilteredWorksetCollector(doc).OfKind(WorksetKind.UserWorkset).ToWorksets();
        List<WorksetsBenchmark> worksetsBenchmarks = new List<WorksetsBenchmark>();
        foreach (Workset workset in worksets)
        {
            var worksetBenchmark = new WorksetsBenchmark();
            worksetBenchmark.ModelName = doc.Title + ".rvt";
            worksetBenchmark.Id = workset.Id.ToString();
            worksetBenchmark.Name = workset.Name;
            worksetBenchmark.Owner = workset.Owner;
            worksetsBenchmarks.Add(worksetBenchmark);
        }
        // save csv download
        string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string filePath = System.IO.Path.Combine(folderPath, "worksets.csv");
        CsvUtils.WriteCsvWorksets(worksetsBenchmarks, filePath);
        Process.Start(filePath);
        return Result.Succeeded;
    }
    public class WorksetsBenchmark
    {
        public string ModelName { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
    }
}