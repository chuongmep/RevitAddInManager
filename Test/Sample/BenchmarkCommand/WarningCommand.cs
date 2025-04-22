using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Test.BenchmarkCommand;

namespace Test.Sample.BenchmarkCommand;

[Transaction(TransactionMode.Manual)]
public class WarningCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        var warnings = doc.GetWarnings();
        List<WarningBenchmark> warningBenchmarks = new List<WarningBenchmark>();
        foreach (FailureMessage failureMessage in warnings)
        {
            var warningBenchmark = new WarningBenchmark();
            warningBenchmark.ModelName = doc.Title+".rvt";
            warningBenchmark.Severity = failureMessage.GetSeverity().ToString();
            warningBenchmark.WarningDescription = failureMessage.GetDescriptionText();
            foreach (ElementId? elementId in failureMessage.GetFailingElements())
            {
                warningBenchmark.ElementId = elementId.ToString();
                warningBenchmark.Category = doc.GetElement(elementId)?.Category?.Name ?? "Unknown";
                warningBenchmarks.Add(warningBenchmark);
            }
        }
        // save csv download
        string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string filePath = System.IO.Path.Combine(folderPath, "warning.csv");
        CsvUtils.WriteCsvWarnings(warningBenchmarks, filePath);
        Process.Start(filePath);
        return Result.Succeeded;
    }
    public class WarningBenchmark
    {
        public string ModelName { get; set; }
        public string Category { get; set; }
        public string Severity { get; set; }
        public string ElementId { get; set; }
        public string WarningDescription { get; set; }
    }
}