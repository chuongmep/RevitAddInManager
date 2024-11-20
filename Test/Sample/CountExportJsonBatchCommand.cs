using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using CsvHelper;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class CountExportJsonBatchCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        string region = "US";
        // browser to item path
        string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        // create new folder data
        string dataFolder = Path.Combine(folder, "data");
        if (!Directory.Exists(dataFolder))
        {
            Directory.CreateDirectory(dataFolder);
        }
        using (var reader = new StreamReader(BrowsePathItems()))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<DataInput>().ToList();
                foreach (DataInput dataInput in records)
                {
                    try
                    {
                        var projectGuid = new Guid(dataInput.project_guid);
                        var modelGuid = new Guid(dataInput.model_guid);
                        var modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(region, projectGuid, modelGuid);
                        Document document = doc.Application.OpenDocumentFile(modelPath, new OpenOptions());
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
                                output.PipeLenght +=
                                    ConvertToRealLength(
                                        pipe.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble() ?? 0);
                            }
                            else if (ele is Duct duct)
                            {
                                output.DuctLenght +=
                                    ConvertToRealLength(
                                        duct.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble() ?? 0);
                            }
                            else if (ele is CableTray cableTray)
                            {
                                output.CableTrayLenght +=
                                    ConvertToRealLength(cableTray.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)
                                        ?.AsDouble() ?? 0);
                            }
                            else if (ele is Conduit conduit)
                            {
                                output.ConduitsLenght +=
                                    ConvertToRealLength(conduit.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)
                                        ?.AsDouble() ?? 0);
                            }
                            else if (ele is ConduitRun conduitRun)
                            {
                                output.ConduitRunsLength += ConvertToRealLength(
                                    conduitRun.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble() ?? 0);
                            }
                            else if (ele is CableTrayRun cableTrayRun)
                            {
                                output.CableTrayRunsLength += ConvertToRealLength(
                                    cableTrayRun.get_Parameter(BuiltInParameter.CURVE_ELEM_LENGTH)?.AsDouble() ?? 0);
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
                        output.CountByCategory =
                            output.CountByCategory.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        // save to json
                        string json =
                            JsonConvert.SerializeObject(output, Formatting.Indented);
                        string filePath = Path.Combine(dataFolder, $"{document.Title}.json");
                        File.WriteAllText(filePath, json, Encoding.UTF8);
                        OpenLogFileAndWrite($"Save to {filePath}", folder);
                    }
                    catch (Exception e)
                    {
                        OpenLogFileAndWrite(e.Message, folder);
                    }
                }
            }
        }

        MessageBox.Show("Done");

       // Process.Start("output.json");
        // close model
        //doc.Close(false);
        return Result.Succeeded;
    }
    public void OpenLogFileAndWrite(string message,string folder)
    {
        string fileName = "log.txt";
        string logFile = Path.Combine(folder, fileName);
        if (!File.Exists(logFile))
        {
            using (StreamWriter sw = File.CreateText(logFile))
            {
                sw.WriteLine(message);
            }
        }
        else
        {
            using (StreamWriter sw = File.AppendText(logFile))
            {
                sw.WriteLine(message);
            }
        }
    }

    public class DataInput
    {
        public string item_id { get; set; }
        public string item_name { get; set; }
        public string project_guid { get; set; }
        public string model_guid { get; set; }
    }

    public string BrowsePathItems()
    {
        var dialog = new OpenFileDialog();
        dialog.Filter = "Revit Files (*.csv)|*.csv";
        dialog.Title = "Select a items file";
        dialog.ShowDialog();
        return dialog.FileName;
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

    public class Output
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