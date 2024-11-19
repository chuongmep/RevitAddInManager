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
public class OpenModelFromCloudSimple : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // open model from acc
        var doc = commandData.Application.ActiveUIDocument.Document;
        string region = "US";
        var projectGuid = new Guid("f10b5c85-fd34-435a-9206-e4a8c21d761c");
        string input = @"D:\API\Revit\RevitAddInManager\Test\Sample\data\input.txt";
        string guidModelInput = File.ReadAllText(input).Trim();
        var modelGuid = new Guid(guidModelInput);
        var modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(region, projectGuid, modelGuid);
        // Document document = doc.Application.OpenDocumentFile(modelPath, new OpenOptions());
        // active document
        commandData.Application.OpenAndActivateDocument(modelPath, new OpenOptions(), false);
        // sync model
        // close model
        doc.Close(false);

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