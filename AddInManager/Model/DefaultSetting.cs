using System.IO;
using System.Net;

namespace RevitAddinManager.Model;

/// <summary>
/// All setting name default for addin
/// </summary>
public static class DefaultSetting
{
    public static string FileName = "ExternalTool";

    public static string FormatExAddin = ".addin";
    public static string FormatDisable = ".disable";
    public static string AdskPath = "Autodesk\\Revit\\Addins";
    public static string IniName = "revit.ini";
    public static string TempFolderName = "RevitAddins";
    public static string DirLogFile = Path.Combine(Path.GetTempPath(), TempFolderName);

    private static string pathLogFile;
    public static string PathLogFile
    {
        get
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(DirLogFile);
            FileInfo fileInfo = directoryInfo.GetFiles("*.txt",SearchOption.TopDirectoryOnly).OrderBy(x=>x.LastWriteTime).LastOrDefault();
            if (fileInfo==null)
            {
                pathLogFile = Path.Combine(DirLogFile, $"{Guid.NewGuid()}.txt");
                File.Create(pathLogFile).Close();
            }
            return pathLogFile;
        }
    }
    public static string AimInternalName = "AimInternal.ini";
}