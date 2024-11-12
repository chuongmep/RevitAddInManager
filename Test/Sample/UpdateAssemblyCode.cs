using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CsvHelper;
using CsvHelper.Configuration;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class UpdateAssemblyCode : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        string csvPath = @"C:\Users\vho2\Downloads\AseemblyCodeUpdate\Uniformat.csv";
        var app = commandData.Application.Application;
        var uiapp = new UIApplication(app);
        var uidoc = uiapp.ActiveUIDocument;
        var doc = uidoc.Document;
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
        doc.SynchronizeWithCentral(new TransactWithCentralOptions(), new SynchronizeWithCentralOptions());
        // publish model by command

        return Result.Succeeded;
    }
}
public class Uniformat
{
    public double Category { get; set; }
    public string UniformatCode { get; set; }
}