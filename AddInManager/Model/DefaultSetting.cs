using System.IO;

namespace RevitAddinManager.Model;

/// <summary>
/// All setting name default for addin
/// </summary>
public static class DefaultSetting
{
    public static string Application = "RevitAddinManager";
    public static string Version = " ";
    public static string NewVersion = "";
    public static bool IsNewVersion = false;
    public static string AppName = "Revit Add-in Manager";
    public static string FileName = "ExternalTool";
    public static string FormatExAddin = ".addin";
    public static string FormatDisable = ".disable";
    public static string AdskPath = "Autodesk\\Revit\\Addins";
    public static string IniName = "revit.ini";
    public static string TempFolderName = "RevitAddins";

    //fix from revit 2020 over with Path.GetTempPath() auto create guid temp
    public static string DirLogFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "AppData",
        "Local",
        "Temp",TempFolderName);

    private static string pathLogFile;
    public static string PathLogFile
    {
        get
        {
            
            bool flag = Directory.Exists(DirLogFile);
            if (!flag) Directory.CreateDirectory(DirLogFile);
            DirectoryInfo directoryInfo = new DirectoryInfo(DirLogFile);
            FileInfo fileInfo = directoryInfo.GetFiles("*.txt",SearchOption.TopDirectoryOnly).OrderBy(x=>x.LastWriteTime).LastOrDefault();
            if (fileInfo==null)
            {
                pathLogFile = Path.Combine(DirLogFile, $"{Guid.NewGuid()}.txt");
                File.Create(pathLogFile).Close();
                return pathLogFile;
            }
            else
            {
                pathLogFile = fileInfo.FullName;
            }
            return fileInfo.FullName;
        }
        set => pathLogFile = value;
    }
    public static string AimInternalName = "AimInternal.ini";
}