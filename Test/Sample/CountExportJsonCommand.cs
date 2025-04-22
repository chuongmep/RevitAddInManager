using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class CountExportJsonCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        string region = "US";
        var projectGuid = new Guid("f10b5c85-fd34-435a-9206-e4a8c21d761c");
        var modelGuid = new Guid(File.ReadAllText(@"D:\API\Revit\RevitAddInManager\Test\Sample\data\input.txt").Trim());
        var modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(region, projectGuid, modelGuid);
        Document document = doc.Application.OpenDocumentFile(modelPath, new OpenOptions());
        // active document
        // check model is opend
        //var openDocs = commandData.Application.Application.Documents;
        // try
        // {
        //commandData.Application.OpenAndActivateDocument(modelPath, new OpenOptions(), false);
        //doc = commandData.Application.ActiveUIDocument.Document;
        // }
        // catch (Exception e)
        // {
        //     //doc = commandData.Application.ActiveUIDocument.Document;
        //     // Console.WriteLine(e);
        // }
        // sync model
        var eles = new FilteredElementCollector(document).WhereElementIsNotElementType().ToElements();
        // save json with length of ducts unit meter, pipes length unit meter, all count by category
        Output output = new Output();
        output.modelName = document.Title;
        output.PipeLenght = 0;
        output.DuctLenght = 0;
        output.CableTrayLenght = 0;
        output.ConduitsLenght = 0;
        output.ConduitRunsLength = 0;
        output.CableTrayRunsLength = 0;
        output.CountByCategory = new Dictionary<string?, string>();
        foreach (var ele in eles)
        {
            if (ele is Pipe pipe)
            {
                output.PipeLenght +=ConvertToRealLength( pipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble()??0);
            }
            else if (ele is Duct duct)
            {
                output.DuctLenght += ConvertToRealLength(duct.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble()??0);
            }
            else if (ele is CableTray cableTray)
            {
                output.CableTrayLenght += ConvertToRealLength(cableTray.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble()??0);
            }
            else if (ele is Conduit conduit)
            {
                output.ConduitsLenght += ConvertToRealLength(conduit.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble()??0);
            }
            else if (ele is ConduitRun conduitRun)
            {
                output.ConduitRunsLength += ConvertToRealLength(conduitRun.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble() ?? 0);
            }
            else if (ele is CableTrayRun cableTrayRun)
            {
                output.CableTrayRunsLength += ConvertToRealLength(cableTrayRun.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble()??0);
            }

        }
        // group by category and count
        var groupByCategory = eles.GroupBy(x => x.Category?.Name);
        foreach (var group in groupByCategory)
        {
            if (group.Key == null)
            {
                continue;
            }
            output.CountByCategory.Add(group.Key!, group.Count().ToString());
        }
        // sort by key
        output.CountByCategory = output.CountByCategory.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        // save to json
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(output, Newtonsoft.Json.Formatting.Indented);
        System.IO.File.WriteAllText("output.json", json, System.Text.Encoding.UTF8);
        Process.Start("output.json");
        // close model
        //doc.Close(false);
        return Result.Succeeded;
    }

    public double ConvertToRealLength(double length)
    {
        // convert to meter
        if (length == null)
        {
            return 0;
        }
        double convert = UnitUtils.Convert(length, UnitTypeId.Feet, UnitTypeId.Meters);
        return convert;
    }
    public class  Output
    {
        public string modelName { get; set; }
        public double? PipeLenght { get; set; }
        public double? DuctLenght { get; set; }
        public double? CableTrayLenght { get; set; }
        public double? ConduitsLenght { get; set; }
        public double? ConduitRunsLength { get; set; }
        public double? CableTrayRunsLength { get; set; }
        public Dictionary<string?, string> CountByCategory { get; set; }
    }
}