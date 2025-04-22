using System;
using System.IO;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class OpenModelAndSync : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        string region = "US";
        var projectGuid = new Guid("f10b5c85-fd34-435a-9206-e4a8c21d761c");
        string input = @"D:\API\Revit\RevitAddInManager\Test\Sample\data\input.txt";
        string guidModelInput = File.ReadAllText(input).Trim();
        var modelGuid = new Guid(guidModelInput);
        var modelPath = ModelPathUtils.ConvertCloudGUIDsToCloudPath(region, projectGuid, modelGuid);
        // Document document = doc.Application.OpenDocumentFile(modelPath, new OpenOptions());
        // active document
        Document document = doc.Application.OpenDocumentFile(modelPath, new OpenOptions());
        // reload latest
        document.ReloadLatest(new ReloadLatestOptions() { });
        // sync model
        TransactWithCentralOptions twcOpts = new TransactWithCentralOptions();
        SynchronizeWithCentralOptions syncopt = new SynchronizeWithCentralOptions();
        RelinquishOptions rOptions = new RelinquishOptions(true);
        rOptions.UserWorksets = true;
        syncopt.SetRelinquishOptions(rOptions);
        // syncopt.SaveLocalBefore = false;
        // syncopt.SaveLocalAfter = false;
        document.SynchronizeWithCentral(twcOpts, syncopt);
        // publish model post command
        // close model
        document.Close(false);
        return Result.Succeeded;
    }
}