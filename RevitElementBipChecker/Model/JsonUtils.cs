using System;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace RevitElementBipChecker.Model
{
    public static class JsonUtils
    {
        public static void WriteJson(this DataTable dataTable, out string path, string filename = "Report.json")
        {
            string PathDocument = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string namesave;
            if (filename.ToLower().Contains(".json"))
            {
                namesave = filename;
            }
            else
            {
                namesave = filename + ".json";
            }
            path = Path.Combine(PathDocument, namesave);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            string serializeObject = JsonConvert.SerializeObject(dataTable,Formatting.Indented);
            File.WriteAllText(path, serializeObject);
        }


    }
}
