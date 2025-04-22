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
public class RvtLinksCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        var rvtLinks = new FilteredElementCollector(doc)
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>();
        List<RvtLinkBenchmark> rvtLinkBenchmarks = new List<RvtLinkBenchmark>();
        foreach (RevitLinkType rvtLink in rvtLinks)
        {
            var rvtLinkBenchmark = new RvtLinkBenchmark();
            rvtLinkBenchmark.ModelName = doc.Title + ".rvt";
            rvtLinkBenchmark.Name = rvtLink.Name;
            ExternalFileReference externalFileReference = rvtLink.GetExternalFileReference();
            string visiblePath = ModelPathUtils.ConvertModelPathToUserVisiblePath(externalFileReference.GetAbsolutePath());
            rvtLinkBenchmark.Path = visiblePath;
            rvtLinkBenchmark.Type = rvtLink.AttachmentType.ToString();
            rvtLinkBenchmark.Status = rvtLink.GetLinkedFileStatus().ToString();
            rvtLinkBenchmarks.Add(rvtLinkBenchmark);
        }
        // save csv
        string folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        string filePath = System.IO.Path.Combine(folderPath, "RvtLinks.csv");
        CsvUtils.WriteRvtLinks(rvtLinkBenchmarks, filePath);
        Process.Start(filePath);
        return Result.Succeeded;
    }

    public class RvtLinkBenchmark
    {
        public string ModelName { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
}