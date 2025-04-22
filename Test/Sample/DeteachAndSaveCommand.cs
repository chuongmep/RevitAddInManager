using System;
using System.IO;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class DetachAndSaveCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var app = commandData.Application.Application;
        var uiapp = new UIApplication(app);
        var uidoc = uiapp.ActiveUIDocument;
        var doc = uidoc.Document;
        string dir = String.Empty;
        var listFiles = Directory.GetFiles(dir, "*.rvt");
        foreach (var file in listFiles)
        {
            DetachAndSaveAs(app, file);
        }
        return Result.Succeeded;
    }

    public void DetachAndSaveAs(Application app,string path)
    {
        OpenOptions options = new OpenOptions
        {
            DetachFromCentralOption = DetachFromCentralOption.ClearTransmittedSaveAsNewCentral,
            AllowOpeningLocalByWrongUser = true,
            IgnoreExtensibleStorageSchemaConflict = true,
            Audit = false,
        };
        app.FailuresProcessing += (sender, args) =>
        {
            var failuresAccessor = args.GetFailuresAccessor();
            var failures = failuresAccessor.GetFailureMessages();
            foreach (var failure in failures)
            {
                if (failure.GetSeverity() == FailureSeverity.Warning)
                {
                    failuresAccessor.DeleteWarning(failure);
                }
                else
                {
                    failuresAccessor.ResolveFailure(failure);
                }
            }
            args.SetProcessingResult(FailureProcessingResult.Continue);
        };
        var modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
        var detachedDoc = app.OpenDocumentFile(modelPath, options);
        // resolve  warnings with detachedDoc

        // if same version with app, continue
        if (detachedDoc.Application.VersionNumber == app.VersionNumber)
        {
            detachedDoc.Close(false);
            return;
        }
        var saveOptions = new SaveAsOptions
        {
            OverwriteExistingFile = true,
            MaximumBackups = 1,
        };
        string folder = Path.GetDirectoryName(path);
        string fileName = Path.GetFileName(path);
        // save over the same file
        detachedDoc.SaveAs(Path.Combine(folder, fileName), saveOptions);
        detachedDoc.Close(false);
    }
}