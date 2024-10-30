using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
            string dir = @"D:\Development\Revit\Project\F7H-M";
            Guid accountId = new Guid("1715cf2b-cc12-46fd-9279-11bbc47e72f6");
            Guid projectId = new Guid("58450c0d-394f-41b2-a7e6-7aa53665dfb8");
            string folderId = "urn:adsk.wipprod:fs.folder:co.qslf4shtRpOHsZLUmwANiQ";
            List<string> revitPaths = GetAllRevitPaths(dir);
            List<string> report = new List<string>();

            foreach (string revitPath in revitPaths)
            {
                UnloadRevitLinks(revitPath);
                continue;
                string fileName = System.IO.Path.GetFileNameWithoutExtension(revitPath);

                Document? doc = OpenDocument(commandData.Application.Application, revitPath, report);
                if (doc == null)
                {
                    continue;
                }
                try
                {
                    doc.SaveAsCloudModel(accountId, projectId, folderId, fileName);
                    // sleep for 5 seconds to allow the cloud model to be created
                    Thread.Sleep(5000);
                    doc.Close(false);
                }
                catch (Exception e)
                {
                    Trace.WriteLine(e.Message);
                }
            }
            TaskDialog.Show("Done", "Process complete.");
            return Result.Succeeded;
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

        private static string UnloadRevitLinks(string path)
        {
            ModelPath mPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
            TransmissionData tData = TransmissionData.ReadTransmissionData(mPath);
            if (tData == null)
            {
                return path;
            }

            ICollection<ElementId> externalReferences = tData.GetAllExternalFileReferenceIds();
            foreach (ElementId refId in externalReferences)
            {
                ExternalFileReference extRef = tData.GetLastSavedReferenceData(refId);
                if (extRef.ExternalFileReferenceType == ExternalFileReferenceType.RevitLink)
                {
                    tData.SetDesiredReferenceData(refId, extRef.GetPath(), extRef.PathType, false);
                }
            }

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
