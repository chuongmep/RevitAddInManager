using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Application = Autodesk.Revit.ApplicationServices.Application;
using TaskDialog = Autodesk.Revit.UI.TaskDialog;

namespace Test
{
    [Transaction(TransactionMode.Manual)]
    public class UnloadAndSaveModelAsCloudCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // read file .env to get account id and project id
            VistaOpenFileDialog openFileDialog = new VistaOpenFileDialog();
            openFileDialog.Filter = "Text files (*.env)|*.env";
            openFileDialog.Title = "Select a text file with FolderId";
            string envPath = string.Empty;
            string folderIdStr = string.Empty;
            Guid accountId = Guid.Empty;
            Guid projectId = Guid.Empty;
            string? dirPath = string.Empty;
            if (openFileDialog.ShowDialog() == true)
            {
                envPath = openFileDialog.FileName;
                string[] envLines = File.ReadAllLines(openFileDialog.FileName);
                string accountIdStr = envLines[0].Split('=')[1];
                string projectIdStr = envLines[1].Split('=')[1];
                folderIdStr = envLines[2].Split('=')[1];
                accountId = new Guid(accountIdStr);
                projectId = new Guid(projectIdStr);
                dirPath = Path.GetDirectoryName(openFileDialog.FileName);
            }
            else
            {
                return Result.Cancelled;
            }
            if (string.IsNullOrEmpty(envPath))
            {
                return Result.Cancelled;
            }
            // string[] envLines = File.ReadAllLines(envPath);
            // string accountIdStr = envLines[0].Split('=')[1];
            // string projectIdStr = envLines[1].Split('=')[1];
            // string folderIdStr = envLines[2].Split('=')[1];
            // Guid accountId = new Guid(accountIdStr);
            // Guid projectId = new Guid(projectIdStr);
            //string folderIdMech = "urn:adsk.wipprod:fs.folder:co.kHlWc1ajSHSxey-_bGjKwg";
            // create a new temp folder if it does not exist
            List<string> revitPaths = GetAllRevitPaths(dirPath);
            string logPath = Path.Combine(dirPath, "log.txt");
            dirPath = Path.Combine(dirPath, "Temp");
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }

            List<string> report = new List<string>();
            // start write a .txt log file
            // clear log file
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
            using (StreamWriter writer = new StreamWriter(logPath))
            {
                foreach (string revitPath in revitPaths)
                {
                    // if revit file name exist in temp./../ folder include scan subfolder
                    var fileNameTemp = Path.GetFileName(revitPath);
                    bool flag = IsInsideFolder(dirPath, fileNameTemp);
                    if (flag)
                    {
                        writer.WriteLine($"{DateTime.Now} - {fileNameTemp} - Skipped Because it is already processed");
                        continue;
                    }
                    // create temp folder in current directory, tempfodler name is guid
                    string tempFolder = Path.Combine(dirPath, Guid.NewGuid().ToString());
                    if (!Directory.Exists(tempFolder))
                    {
                        Directory.CreateDirectory(tempFolder);
                    }
                    // copy revit file to temp folder and set the path to the copied file
                    string copiedRevitPath = Path.Combine(tempFolder, Path.GetFileName(revitPath));
                    File.Copy(revitPath, copiedRevitPath);
                    var newRevitPath = copiedRevitPath;
                    UnloadRevitLinks(revitPath);
                    string fileName = Path.GetFileNameWithoutExtension(newRevitPath);

                    Document? doc = OpenDocument(commandData.Application.Application, newRevitPath, report);
                    if (doc == null)
                    {
                        continue;
                    }
                    try
                    {
                        // sync to central
                        //doc.SynchronizeWithCentral(new TransactWithCentralOptions(), new SynchronizeWithCentralOptions());
                        // publish model by command
                        doc.SaveAsCloudModel(accountId, projectId, folderIdStr, fileName);
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
        public bool IsInsideFolder(string folderPath, string fileName)
        {
            string[] files = System.IO.Directory.GetFiles(folderPath, "*.rvt", SearchOption.AllDirectories);
            // compare ==
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                if (name == fileName)
                {
                    return true;
                }
            }
            return false;
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
