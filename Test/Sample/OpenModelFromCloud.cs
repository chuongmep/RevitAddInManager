using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CsvHelper;
using Microsoft.Win32;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class OpenModelFromCloud : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // open model from acc
        var doc = commandData.Application.ActiveUIDocument.Document;
        string region = "US";
        List<DataInput> dataInputs = new List<DataInput>();
        // browser to item path
        using (var reader = new StreamReader(BrowsePath()))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<DataInput>().ToList();
                dataInputs.AddRange(records);
            }

        }

        string csvFamily = BrowsePath();

        foreach (DataInput dataInput in dataInputs)
        {
            // Guid projectGuid = new Guid("ca790fb5-141d-4ad5-b411-0461af2e9748");
            // Guid modelGuid = new Guid("53d475e0-67b2-4971-b91a-b7f32a8ca5c2");
            try
            {
                var projectGuid = new Guid(dataInput.project_guid);
                var modelGuid = new Guid(dataInput.model_guid);
                var modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(region, projectGuid, modelGuid);
                Document document = doc.Application.OpenDocumentFile(modelPath, new OpenOptions());
                OpenLogFileAndWrite($"Open model {modelPath}");
                UpdateUFCodeBaseFamily updateUfCodeBaseFamily = new UpdateUFCodeBaseFamily();
                updateUfCodeBaseFamily.Execute(document, csvFamily);
                // sync model
                doc.SynchronizeWithCentral(new TransactWithCentralOptions(), new SynchronizeWithCentralOptions());
                OpenLogFileAndWrite("Sync model to central is done");
                // close model
                doc.Close(false);
            }
            catch (Exception e)
            {
                Trace.Write(e.Message);
            }
        }

        return Result.Succeeded;

    }
    public  void OpenLogFileAndWrite(string message)
    {
        string fileName = "log.txt";
        string logFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
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
    public string BrowsePath()
    {
        var dialog = new OpenFileDialog();
        dialog.Filter = "Revit Files (*.csv)|*.csv";
        dialog.Title = "Select a items file";
        dialog.ShowDialog();
        return dialog.FileName;
    }
    public class  DataInput
    {
        public string item_id { get; set; }
        public string item_name { get; set; }
        public string project_guid { get; set; }
        public string model_guid { get; set; }
    }
}