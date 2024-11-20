using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            string dir = OpenDirectoryDialog();
            List<string> revitPaths = GetAllRevitPaths(dir);
            List<string> report = new List<string>();
            // start write a .txt log file
            string logPath = Path.Combine(dir, "log.txt");
            // clear log file
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
            using (StreamWriter writer = new StreamWriter(logPath))
            {
                foreach (string revitPath in revitPaths)
                {
                    UnloadRevitLinks(revitPath);
                    string fileName = Path.GetFileNameWithoutExtension(revitPath);

                    Document? doc = OpenDocument(commandData.Application.Application, revitPath, report);
                    if (doc == null)
                    {
                        continue;
                    }
                    try
                    {
                        // sync to central
                        //doc.SynchronizeWithCentral(new TransactWithCentralOptions(), new SynchronizeWithCentralOptions());
                        // publish model by command
                        doc.SaveAsCloudModel(accountId, projectId, folderIdMech, fileName);
                        // write to log format DAteTime.Now - ModelName - Status
                        writer.WriteLine($"{DateTime.Now} - {fileName} - Success");
                        // sleep for 5 seconds to allow the cloud model to be created
                        // Thread.Sleep(5000);
                        doc.Close(false);
                    }
                    catch (Exception e)
                    {
                        writer.WriteLine($"{DateTime.Now} - {fileName} - Failed: {e.Message}");
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

        private static string UnloadRevitLinks(string path)
        {
            ModelPath mPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            //bool isDocumentTransmitted = TransmissionData.IsDocumentTransmitted(mPath);
            // if (!isDocumentTransmitted)
            // {
            //     writer.WriteLine($"Model {fileName} is not transmitted");
            //     return path;
            // }
            TransmissionData tData = TransmissionData.ReadTransmissionData(mPath);
            tData.IsTransmitted = true;
            TransmissionData.WriteTransmissionData(mPath, tData);

            return path;
        }

        private static Document? OpenDocument(Application app, string filePath, List<string> errorMessages)
        {
            UnloadRevitLinks(filePath);

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
