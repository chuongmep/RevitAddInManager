using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AddInManager.Model
{
    public class IniFile
	{
        public string FilePath => this._mFilePath;

        public IniFile(string filePath)
		{
			this._mFilePath = filePath;
			if (!File.Exists(this._mFilePath))
			{
				FileUtils.CreateFile(this._mFilePath);
				FileUtils.SetWriteable(this._mFilePath);
			}
		}

		public void WriteSection(string iniSection)
		{
			IniFile.WritePrivateProfileSection(iniSection, null, this._mFilePath);
		}

		
		public void Write(string iniSection, string iniKey, object iniValue)
		{
			IniFile.WritePrivateProfileString(iniSection, iniKey, iniValue.ToString(), this._mFilePath);
		}

		
		public string ReadString(string iniSection, string iniKey)
		{
			StringBuilder stringBuilder = new StringBuilder(255);
			IniFile.GetPrivateProfileString(iniSection, iniKey, "", stringBuilder, 255, this._mFilePath);
			return stringBuilder.ToString();
		}

		public int ReadInt(string iniSection, string iniKey)
		{
			return IniFile.GetPrivateProfileInt(iniSection, iniKey, 0, this._mFilePath);
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
}
