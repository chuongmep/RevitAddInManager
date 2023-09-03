using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;

namespace RevitElementBipChecker.Model
{
    public static class CsvUtils
    {
        public static string ExportCsv(this DataTable dataTable,string name="BipChecker.csv")
        {
            string PathDocument = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(PathDocument, name);
            using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                // Write the column headers
                foreach (DataColumn column in dataTable.Columns)
                {
                    writer.Write(column.ColumnName);
                    writer.Write(",");
                }
                writer.WriteLine();

                // Write the data rows
                foreach (DataRow row in dataTable.Rows)
                {
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        writer.Write(row[i]);
                        writer.Write(",");
                    }
                    writer.WriteLine();
                }
            }
            return filePath;
        }
        
        /// <summary>
        /// convert optimize data object to data table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection properties = 
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}
