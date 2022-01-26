using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace RevitAddinManager.Model;

public class IniFile
{
    public string FilePath => _mFilePath;

    public IniFile(string filePath)
    {
        _mFilePath = filePath;
        if (!File.Exists(_mFilePath))
        {
            FileUtils.CreateFile(_mFilePath);
            FileUtils.SetWriteable(_mFilePath);
        }
    }

    public void WriteSection(string iniSection)
    {
        WritePrivateProfileSection(iniSection, null, _mFilePath);
    }


    public void Write(string iniSection, string iniKey, object iniValue)
    {
        WritePrivateProfileString(iniSection, iniKey, iniValue.ToString(), _mFilePath);
    }


    public string ReadString(string iniSection, string iniKey)
    {
        var stringBuilder = new StringBuilder(255);
        GetPrivateProfileString(iniSection, iniKey, "", stringBuilder, 255, _mFilePath);
        return stringBuilder.ToString();
    }

    public int ReadInt(string iniSection, string iniKey)
    {
        return GetPrivateProfileInt(iniSection, iniKey, 0, _mFilePath);
    }


    [DllImport("kernel32.dll")]
    private static extern int WritePrivateProfileSection(string lpAppName, string lpString, string lpFileName);


    [DllImport("kernel32", CharSet = CharSet.Auto)]
    private static extern int WritePrivateProfileString(string section, string key, string val, string filePath);


    [DllImport("kernel32")]
    private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);


    [DllImport("kernel32", CharSet = CharSet.Auto)]
    private static extern int GetPrivateProfileString(string section, string key, string defaultValue, StringBuilder retVal, int size, string filePath);


    private readonly string _mFilePath;
}