using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Test;

[Transaction(TransactionMode.Manual)]
public class AssemblyCodeFamilyExport : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var doc = commandData.Application.ActiveUIDocument.Document;
        string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dataDir = Path.Combine(desktopDir, "data");
        if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
        string filePath = Path.Combine(dataDir, "assemblycode.csv");
        ExportAssemblyCodes(doc, filePath);
        ProcessStartInfo processStartInfo = new ProcessStartInfo()
        {
            FileName = filePath,
            WorkingDirectory = dataDir,
            UseShellExecute = true
        };
        Process.Start(processStartInfo);
        return Result.Succeeded;
    }
    private void ExportAssemblyCodes(Document doc, string filePath)
    {
        var familySymbols = new FilteredElementCollector(doc)
            .OfClass(typeof(FamilySymbol))
            .Cast<FamilySymbol>();

        var dataToExport = familySymbols.Select(symbol => new DataAssembly
        {
            Category = symbol.Category?.Name ?? "N/A",
            FamilyName = symbol.Family.Name,
            TypeName = symbol.Name,
            AssemblyCode = symbol.LookupParameter("Assembly Code")?.AsString() ?? "N/A",
            AssemblyCodeDescription = ""
        }).ToList();

        try
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(dataToExport);
            }
            TaskDialog.Show("Export Completed", $"Assembly codes exported to: {filePath}");
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Error", "Failed to export CSV: " + ex.Message);
        }
    }


}

[Transaction(TransactionMode.Manual)]
public class AssemblyCodeFamilyImport : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        var uiDoc = commandData.Application.ActiveUIDocument;
        var doc = uiDoc.Document;

        // Mở hộp thoại chọn file CSV
        string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        string dataDir = Path.Combine(desktopDir, "data");
        if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
        string filePath = Path.Combine(dataDir, "assemblycode.csv");
        List<DataAssembly> dataAssemblies = ReadCsv(filePath);
        if (!dataAssemblies.Any())
        {
            TaskDialog.Show("Error", "No valid data found in the CSV file.");
            return Result.Failed;
        }

        using (Transaction tx = new Transaction(doc, "Update Assembly Codes"))
        {
            tx.Start();
            var familySymbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>();

            int updatedCount = 0;
            foreach (var data in dataAssemblies)
            {
                var symbol = familySymbols.FirstOrDefault(fam =>
                    fam.Family.Name.Equals(data.FamilyName, StringComparison.OrdinalIgnoreCase) &&
                    fam.Name.Equals(data.TypeName, StringComparison.OrdinalIgnoreCase));

                if (symbol != null)
                {
                    Parameter assemblyCodeParam = symbol.get_Parameter(BuiltInParameter.UNIFORMAT_CODE);
                    if (assemblyCodeParam != null && !assemblyCodeParam.IsReadOnly)
                    {
                        if(data.AssemblyCode=="N/A") continue;
                        assemblyCodeParam.Set(data.AssemblyCode);
                        updatedCount++;
                    }
                }
            }

            tx.Commit();
            TaskDialog.Show("Completed", $"Updated {updatedCount} assembly codes.");
        }

        return Result.Succeeded;
    }
    private List<DataAssembly> ReadCsv(string filePath)
    {
        try
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                return csv.GetRecords<DataAssembly>().ToList();
            }
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Error", "Failed to read CSV: " + ex.Message);
            return null;
        }
    }


}

public class DataAssembly
{
    public string Category { get; set; }
    public string FamilyName { get; set; }
    public string TypeName { get; set; }
    public string AssemblyCode { get; set; }
    public string AssemblyCodeDescription { get; set; }
}
