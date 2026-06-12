using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CsvHelper;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class UpdateUFCodeBaseFamily : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        // load csv path

        string? browseFile = BrowseFileCsv();
        if (browseFile == null)
        {
            return Result.Cancelled;
        }

        // browse fil

        // KeyBasedTreeEntriesLoadContent loadContent = new KeyBasedTreeEntriesLoadContent();
        // // load configuration assembly code :
        // ClassificationEntries.LoadClassificationEntriesFromFile(doc.PathName, classificationEntry);
        // load assembly code setting

        // read csv file
        Execute(doc, browseFile);

        MessageBox.Show("Done");

        return Result.Succeeded;
    }

    public void Execute(Autodesk.Revit.DB.Document doc, string browseFile)
    {
        List<InputAssembly> inputAssemblies = new List<InputAssembly>();
        using (var reader = new StreamReader(browseFile))
        {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                var records = csv.GetRecords<InputAssembly>().ToList();
                inputAssemblies.AddRange(records);
            }
        }

        // update assembly code base on family name and type name
        using (var tran = new Transaction(doc, "Update Assembly Code"))
        {
            tran.Start();
            // get all family types
            var familyTypes = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                .ToList();
            foreach (FamilySymbol familySymbol in familyTypes)
            {
                var inputAssembly = inputAssemblies.FirstOrDefault(x =>
                    x.Family == familySymbol.FamilyName && x.FamilyType == familySymbol.Name);
                if (inputAssembly != null)
                {
                    Parameter parameter = familySymbol.get_Parameter(BuiltInParameter.UNIFORMAT_CODE);
                    if (parameter != null && !parameter.IsReadOnly)
                    {
                        parameter.Set(inputAssembly.UF2Code);
                    }

                    var descriptionParameter = familySymbol.get_Parameter(BuiltInParameter.ALL_MODEL_DESCRIPTION);
                    if (descriptionParameter != null && !descriptionParameter.IsReadOnly)
                    {
                        descriptionParameter.Set(inputAssembly.UF2Description);
                    }
                }
            }
            tran.Commit();
        }
    }

    string? BrowseFileCsv()
    {
        Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
        openFileDialog.Filter = "CSV files (*.csv)|*.csv";
        if (openFileDialog.ShowDialog() == true)
        {
            return openFileDialog.FileName;
        }

        return null;
    }
}

public class InputAssembly
{
    public string ModelName { get; set; }
    public string Category { get; set; }
    public string Family { get; set; }
    public string FamilyType { get; set; }
    public string UF2Code { get; set; }
    public string UF2Description { get; set; }
}