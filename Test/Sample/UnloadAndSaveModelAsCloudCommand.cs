using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CsvHelper;
using Microsoft.Win32;
using Application = Autodesk.Revit.ApplicationServices.Application;

namespace Test
{
    [Transaction(TransactionMode.Manual)]
    public class UnloadAndSaveModelAsCloudCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //UIApplication uiApp = commandData.Application;
            //uiApp.Idling += ApplicationIdling;
            Guid accountId = new Guid("1715cf2b-cc12-46fd-9279-11bbc47e72f6");
            Guid projectId = new Guid("ca790fb5-141d-4ad5-b411-0461af2e9748");
            string folderIdMech = "urn:adsk.wipprod:fs.folder:co.kHlWc1ajSHSxey-_bGjKwg";
            // string folderIdElec = "urn:adsk.wipprod:fs.folder:co.xm8eECPARESSL00xbO7qLw";
            string csvPath = OpenDialogGetPath();
            string dir = OpenDirectoryDialog();
            if(string.IsNullOrEmpty(csvPath))
            {
                TaskDialog.Show("Error", "Please select uniformat code a csv file.");
                return Result.Failed;
            }
            List<string> revitPaths = GetAllRevitPaths(dir);
            List<string> report = new List<string>();
            // start write a .txt log file
            string logPath = Path.Combine(dir, "log.txt");
            using (StreamWriter writer = new StreamWriter(logPath))
            {
                foreach (string revitPath in revitPaths)
                {
                    UnloadRevitLinks(revitPath,writer);
                    string fileName = Path.GetFileNameWithoutExtension(revitPath);

                    Document? doc = OpenDocument(commandData.Application.Application, revitPath, report,writer);
                    if (doc == null)
                    {
                        continue;
                    }
                    try
                    {

                        // string csvPath = @"C:\Users\vho2\Downloads\AseemblyCodeUpdate\Uniformat.csv";
                        // read csv file
                        var allElements = new FilteredElementCollector(doc).WhereElementIsElementType()
                            //group by id
                            .GroupBy(x => x.Id.IntegerValue).Select(x => x.First())
                            .Where(x => x.Category != null);


                        using (var reader = new StreamReader(csvPath))
                        {
                            using Autodesk.Revit.DB.Transaction tran = new Transaction(doc, "Update Assembly Code");
                            tran.Start();
                            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                            {
                                var records = csv.GetRecords<Uniformat>().ToList();
                                foreach (var record in records)
                                {
                                    var type_elements = allElements.Where(x => Math.Abs(x.Category.Id.IntegerValue - record.Category) < 0.001);
                                    foreach (var element in type_elements)
                                    {
                                        Parameter parameter = element.get_Parameter(BuiltInParameter.UNIFORMAT_CODE);
                                        if (parameter != null && !parameter.IsReadOnly)
                                        {
                                            parameter.Set(record.UniformatCode);
                                        }
                                    }
                                }
                            }
                            tran.Commit();
                        }
                        // sync to central
                        //doc.SynchronizeWithCentral(new TransactWithCentralOptions(), new SynchronizeWithCentralOptions());
                        // publish model by command
                        doc.SaveAsCloudModel(accountId, projectId, folderIdMech, fileName);
                        // sleep for 5 seconds to allow the cloud model to be created
                        Thread.Sleep(5000);
                        doc.Close(false);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.Message);
                    }
                }
            }

            TaskDialog.Show("Done", "Process complete.");
            Process.Start(logPath);
            return Result.Succeeded;
        }

        public string OpenDialogGetPath()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "CSV files (*.csv)|*.csv";
            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return string.Empty;
        }
        public string OpenDirectoryDialog()
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return folderBrowserDialog.SelectedPath;
            }
            return string.Empty;
        }
        private void ApplicationIdling(object sender, Autodesk.Revit.UI.Events.IdlingEventArgs e)
        {
            UIApplication? uiApp = sender as UIApplication;
            if (uiApp == null)
            {
                return;
            }
            uiApp.Idling -= ApplicationIdling;
            uiApp.Application.FailuresProcessing += Application_FailuresProcessing;
        }
        public void Application_FailuresProcessing(object sender, Autodesk.Revit.DB.Events.FailuresProcessingEventArgs e)
        {
            FailuresAccessor failuresAccessor = e.GetFailuresAccessor();
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();
            foreach (FailureMessageAccessor failureMessage in failureMessages)
            {
                if (failureMessage.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failureMessage);
                }
            }
            e.SetProcessingResult(FailureProcessingResult.Continue);
        }

        public List<string> GetAllRevitPaths(string folderPath)
        {
            List<string> revitPaths = new List<string>();
            string[] files = System.IO.Directory.GetFiles(folderPath, "*.rvt", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                revitPaths.Add(file);
            }
            return revitPaths;
        }

        private static string UnloadRevitLinks(string path,StreamWriter writer)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            writer.WriteLine($"Model'{fileName}'");
            ModelPath mPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            //bool isDocumentTransmitted = TransmissionData.IsDocumentTransmitted(mPath);
            // if (!isDocumentTransmitted)
            // {
            //     writer.WriteLine($"Model {fileName} is not transmitted");
            //     return path;
            // }
            TransmissionData tData = TransmissionData.ReadTransmissionData(mPath);
            ICollection<ElementId> externalReferences = tData.GetAllExternalFileReferenceIds();
            foreach (ElementId refId in externalReferences)
            {
                ExternalFileReference extRef = tData.GetLastSavedReferenceData(refId);
                LinkedFileStatus status = extRef.GetLinkedFileStatus();
                if (status == LinkedFileStatus.Loaded && extRef.ExternalFileReferenceType==ExternalFileReferenceType.RevitLink)
                {
                    string name = ModelPathUtils.ConvertModelPathToUserVisiblePath(extRef.GetPath());
                    writer.WriteLine($"{extRef.ExternalFileReferenceType.ToString()}:'{name}' Status: {status}");
                    //tData.SetDesiredReferenceData(elementid, extRef.GetPath(), extRef.PathType, false);
                }
            }
            tData.IsTransmitted = true;
            TransmissionData.WriteTransmissionData(mPath, tData);

            return path;
        }

        private static Document? OpenDocument(Application app, string filePath, List<string> errorMessages,StreamWriter writer)
        {
            UnloadRevitLinks(filePath,writer);

            ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(filePath);
            OpenOptions options = new OpenOptions
            {
                DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets,
                AllowOpeningLocalByWrongUser = true,
                IgnoreExtensibleStorageSchemaConflict = true,
                Audit = false,
            };

            try
            {
                Document? doc = app.OpenDocumentFile(modelPath, options);
                if (doc == null || doc.IsLinked)
                {
                    throw new Exception("Failed to open document or document is a linked file.");
                }
                return doc;
            }
            catch (Exception e)
            {
                errorMessages.Add($"Error opening Revit document '{System.IO.Path.GetFileName(filePath)}': {e.Message}");
                return null;
            }
        }
    }

    // Custom IFailuresPreprocessor implementation to ignore warnings
    public class IgnoreFailuresPreprocessor : IFailuresPreprocessor
    {
        public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
        {
            IList<FailureMessageAccessor> failureMessages = failuresAccessor.GetFailureMessages();

            foreach (FailureMessageAccessor failureMessage in failureMessages)
            {
                // This will delete all warning messages
                if (failureMessage.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failureMessage);
                }
            }

            return FailureProcessingResult.Continue;
        }
    }
}
