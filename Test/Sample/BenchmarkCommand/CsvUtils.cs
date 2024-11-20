using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;
using Test.Sample.BenchmarkCommand;

namespace Test.BenchmarkCommand;

public static class CsvUtils
{
    public static void WriteOverallBenmark(List<BenchmarkReport> items, string fileName)
    {
        // get from csv, if rows not exist, overwrite, else append
        var csvFile = new FileInfo(fileName);
        if (csvFile.Exists && csvFile.Length > 0)
        {
            // delete data have same model name
            var records = new List<BenchmarkReport>();
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            records.AddRange(csv.GetRecords<BenchmarkReport>());
            records.RemoveAll(x => x.ModelName == items[0].ModelName);
            records.AddRange(items);
            reader.Close();
            using var writer = new StreamWriter(fileName);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(records);

        }
        else
        {
            using var writer = new StreamWriter(fileName,true);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(items);
        }
    }
    public static void WriteCsvWarnings(List<WarningCommand.WarningBenchmark> items, string fileName)
    {
        // get from csv, if rows not exist, overwrite, else append
        var csvFile = new FileInfo(fileName);
        if (csvFile.Exists && csvFile.Length > 0)
        {
            // delete data have same model name
            var records = new List<WarningCommand.WarningBenchmark>();
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            records.AddRange(csv.GetRecords<WarningCommand.WarningBenchmark>());
            records.RemoveAll(x => x.ModelName == items[0].ModelName);
            records.AddRange(items);
            reader.Close();
            using var writer = new StreamWriter(fileName);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(records);

        }
        else
        {
            using var writer = new StreamWriter(fileName,true);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(items);
        }
    }
    public static void WriteCsvWorksets(List<WorksetsCommand.WorksetsBenchmark> items, string fileName)
    {
        // get from csv, if rows not exist, overwrite, else append
        var csvFile = new FileInfo(fileName);
        if (csvFile.Exists && csvFile.Length > 0)
        {
            // delete data have same model name
            var records = new List<WorksetsCommand.WorksetsBenchmark>();
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            records.AddRange(csv.GetRecords<WorksetsCommand.WorksetsBenchmark>());
            records.RemoveAll(x => x.ModelName == items[0].ModelName);
            records.AddRange(items);
            reader.Close();
            using var writer = new StreamWriter(fileName);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(records);

        }
        else
        {
            using var writer = new StreamWriter(fileName,true);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(items);
        }
    }
    public static void WriteCsvCategories(List<CategoriesCommand.CategoryBenchmark> items, string fileName)
    {
        // get from csv, if rows not exist, overwrite, else append
        var csvFile = new FileInfo(fileName);
        if (csvFile.Exists && csvFile.Length > 0)
        {
            // delete data have same model name
            var records = new List<CategoriesCommand.CategoryBenchmark>();
            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            records.AddRange(csv.GetRecords<CategoriesCommand.CategoryBenchmark>());
            records.RemoveAll(x => x.ModelName == items[0].ModelName);
            records.AddRange(items);
            reader.Close();
            using var writer = new StreamWriter(fileName);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(records);

        }
        else
        {
            using var writer = new StreamWriter(fileName,true);
            using var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture);
            csvWriter.WriteRecords(items);
        }
    }
    public static void WriteRvtLinks(List<RvtLinksCommand.RvtLinkBenchmark>? items, string fileName)
    {
        // Ensure items is not null or empty
        if (items == null || items.Count == 0)
            return;

        var csvFile = new FileInfo(fileName);

        if (csvFile.Exists && csvFile.Length > 0)
        {
            // Read existing records from the CSV file
            var records = new List<RvtLinksCommand.RvtLinkBenchmark>();
            using (var reader = new StreamReader(fileName))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                records.AddRange(csv.GetRecords<RvtLinksCommand.RvtLinkBenchmark>());
            }

            // Remove existing records with the same ModelName
            var modelName = items[0].ModelName; // Assuming all items have the same ModelName
            records.RemoveAll(x => x.ModelName == modelName);

            // Append the new items to the list
            records.AddRange(items);

            // Overwrite the file with updated records
            using (var writer = new StreamWriter(fileName))
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(records);
            }
        }
        else
        {
            // File doesn't exist or is empty, write new records
            using (var writer = new StreamWriter(fileName, false)) // Use `false` to overwrite or create a new file
            using (var csvWriter = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csvWriter.WriteRecords(items);
            }
        }
    }
}