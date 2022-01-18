using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using AddInManager.Model;

namespace AddInManager.ViewModel
{
    public class AddinManager
    {
        public AddinsApplication Applications => this.m_applications;
        public int AppCount => this.m_applications.Count;
        public AddinsCommand Commands => this.m_commands;
        public int CmdCount => this.m_commands.Count;


        public AddinManager()
        {
            this.m_commands = new AddinsCommand();
            this.m_applications = new AddinsApplication();
            this.GetIniFilePaths();
            this.ReadAddinsFromAimIni();
        }


        public IniFile AimIniFile
        {
            get => this.m_aimIniFile;
            set => this.m_aimIniFile = value;
        }

        public IniFile RevitIniFile
        {
            get => this.m_revitIniFile;
            set => this.m_revitIniFile = value;
        }

        private void GetIniFilePaths()
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string path = Path.Combine(folderPath, Resource.AppName);
            string filePath = Path.Combine(path, "AimInternal.ini");
            this.m_aimIniFile = new IniFile(filePath);
            Process currentProcess = Process.GetCurrentProcess();
            string fileName = currentProcess.MainModule.FileName;
            string filePath2 = fileName.Replace(".exe", ".ini");
            this.m_revitIniFile = new IniFile(filePath2);
        }

        public void ReadAddinsFromAimIni()
        {
            this.m_commands.ReadItems(this.m_aimIniFile);
            this.m_applications.ReadItems(this.m_aimIniFile);
        }

        public void RemoveAddin(Addin addin)
        {
            if (!this.m_commands.RemoveAddIn(addin))
            {
                this.m_applications.RemoveAddIn(addin);
            }
        }

        public AddinType LoadAddin(string filePath, AssemLoader assemLoader)
        {
            AddinType addinType = AddinType.Invalid;
            if (!File.Exists(filePath))
            {
                return addinType;
            }
            //AssemLoader assemLoader = new AssemLoader();
            List<AddinItem> list = null;
            List<AddinItem> list2 = null;
            try
            {
                assemLoader.HookAssemblyResolve();

                Assembly assembly = assemLoader.LoadAddinsToTempFolder(filePath, true);
                list = this.m_commands.LoadItems(assembly, StaticUtil.commandFullName, filePath, AddinType.Command);
                list2 = this.m_applications.LoadItems(assembly, StaticUtil.appFullName, filePath, AddinType.Application);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                assemLoader.UnhookAssemblyResolve();
            }
            if (list != null && list.Count > 0)
            {
                Addin addin = new Addin(filePath, list);
                this.m_commands.AddAddIn(addin);
                addinType |= AddinType.Command;
            }
            if (list2 != null && list2.Count > 0)
            {
                Addin addin2 = new Addin(filePath, list2);
                this.m_applications.AddAddIn(addin2);
                addinType |= AddinType.Application;
            }
            return addinType;
        }

        public void SaveToRevitIni()
        {
            if (!File.Exists(this.m_revitIniFile.FilePath))
            {
                throw new System.IO.FileNotFoundException("can't find the revit.ini file from: " + this.m_revitIniFile.FilePath);
            }
            this.m_commands.Save(this.m_revitIniFile);
            this.m_applications.Save(this.m_revitIniFile);
        }

        public void SaveToLocal()
        {
            this.SaveToLocalManifest();
        }

        public void SaveToLocalRevitIni()
        {
            foreach (KeyValuePair<string, Addin> keyValuePair in this.m_commands.AddinDict)
            {
                string key = keyValuePair.Key;
                Addin value = keyValuePair.Value;
                string directoryName = Path.GetDirectoryName(value.FilePath);
                IniFile file = new IniFile(Path.Combine(directoryName, "revit.ini"));
                value.SaveToLocalIni(file);
                if (this.m_applications.AddinDict.ContainsKey(key))
                {
                    Addin addin = this.m_applications.AddinDict[key];
                    addin.SaveToLocalIni(file);
                }
            }
        }

        public void SaveToAimIni()
        {
            if (!File.Exists(this.AimIniFile.FilePath))
            {
                new FileInfo(this.AimIniFile.FilePath).Create();
                FileUtils.SetWriteable(this.AimIniFile.FilePath);
            }
            this.m_commands.Save(this.m_aimIniFile);
            this.m_applications.Save(this.m_aimIniFile);
        }

        public bool HasItemsToSave()
        {
            foreach (Addin addin in this.m_commands.AddinDict.Values)
            {
                if (addin.Save)
                {
                    return true;
                }
            }
            foreach (Addin addin2 in this.m_applications.AddinDict.Values)
            {
                if (addin2.Save)
                {
                    return true;
                }
            }
            return false;
        }

        public string SaveToAllUserManifest(AddInManagerViewModel vm)
        {
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string folder = Path.Combine(folderPath, "Autodesk\\Revit\\Addins\\2014");
            if (vm.IsCurrentVersion)
            {

                folder = Path.Combine(folderPath, "Autodesk\\Revit\\Addins", vm.ExternalCommandData.Application.Application.VersionNumber);
            }
            ManifestFile manifestFile = new ManifestFile(false);
            foreach (AddinModel parrent in vm.CommandItems)
            {
                foreach (AddinModel chidrent in parrent.Children)
                {
                    if (chidrent.IsChecked == true)
                    {
                        manifestFile.Commands.Add(chidrent.AddinItem);
                    }
                }
            }
            //         int num2 = 0;
            //Addin addin3 = null;
            //foreach (Addin addin4 in this.m_applications.AddinDict.Values)
            //{
            //	if (addin4.Save)
            //	{
            //		num++;
            //		addin3 = addin4;
            //	}
            //	foreach (AddinItem addinItem2 in addin4.ItemList)
            //	{
            //		if (addinItem2.Save)
            //		{
            //			manifestFile.Applications.Add(addinItem2);
            //			num2++;
            //			addin3 = addin4;
            //		}
            //	}
            //}
            //string text = string.Empty;
            //string text2 = string.Empty;
            //if (num <= 1 && num2 <= 1 && num + num2 > 0)
            //{
            //	if (addin != null)
            //	{
            //		if (addin3 == null || addin.FilePath.Equals(addin3.FilePath, StringComparison.OrdinalIgnoreCase))
            //		{
            //			text = Path.GetFileNameWithoutExtension(addin.FilePath);
            //                 }
            //	}	
            //	else if (addin3 != null && addin == null)
            //	{
            //		text = Path.GetFileNameWithoutExtension(addin3.FilePath);
            //             }
            //	if (string.IsNullOrEmpty(text))
            //	{
            //		return string.Empty;
            //	}
            //             MessageBox.Show(text2);
            //         }
            //else
            //         {
            //             text2 = this.GetProperFilePath(folder, "ExternalTool", ".addin");
            //}
            string FilePath = this.GetProperFilePath(folder, "ExternalTool", ".addin");
            manifestFile.SaveAs(FilePath);
            return FilePath;
        }

        public void SaveToLocalManifest()
        {
            Dictionary<string, Addin> dictionary = new Dictionary<string, Addin>();
            Dictionary<string, Addin> dictionary2 = new Dictionary<string, Addin>();
            foreach (KeyValuePair<string, Addin> keyValuePair in this.m_commands.AddinDict)
            {
                string key = keyValuePair.Key;
                Addin value = keyValuePair.Value;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(value.FilePath);
                string directoryName = Path.GetDirectoryName(value.FilePath);
                string filePath = Path.Combine(directoryName, fileNameWithoutExtension + ".addin");
                ManifestFile manifestFile = new ManifestFile(true);
                foreach (AddinItem addinItem in value.ItemList)
                {
                    if (addinItem.Save)
                    {
                        manifestFile.Commands.Add(addinItem);
                    }
                }
                if (this.m_applications.AddinDict.ContainsKey(key))
                {
                    Addin addin = this.m_applications.AddinDict[key];
                    foreach (AddinItem addinItem2 in addin.ItemList)
                    {
                        if (addinItem2.Save)
                        {
                            manifestFile.Applications.Add(addinItem2);
                        }
                    }
                    dictionary.Add(key, this.m_applications.AddinDict[key]);
                }
                manifestFile.SaveAs(filePath);
            }
            foreach (KeyValuePair<string, Addin> keyValuePair2 in this.m_applications.AddinDict)
            {
                string key2 = keyValuePair2.Key;
                Addin value2 = keyValuePair2.Value;
                if (!dictionary.ContainsKey(key2))
                {
                    string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(value2.FilePath);
                    string directoryName2 = Path.GetDirectoryName(value2.FilePath);
                    string filePath2 = Path.Combine(directoryName2, fileNameWithoutExtension2 + ".addin");
                    ManifestFile manifestFile2 = new ManifestFile(true);
                    foreach (AddinItem addinItem3 in value2.ItemList)
                    {
                        if (addinItem3.Save)
                        {
                            manifestFile2.Applications.Add(addinItem3);
                        }
                    }
                    if (this.m_commands.AddinDict.ContainsKey(key2))
                    {
                        Addin addin2 = this.m_commands.AddinDict[key2];
                        foreach (AddinItem addinItem4 in addin2.ItemList)
                        {
                            if (addinItem4.Save)
                            {
                                manifestFile2.Commands.Add(addinItem4);
                            }
                        }
                        dictionary2.Add(key2, this.m_commands.AddinDict[key2]);
                    }
                    manifestFile2.SaveAs(filePath2);
                }
            }
        }

        private string GetProperFilePath(string folder, string fileNameWithoutExt, string ext)
        {
            string text;
            int num = -1;
            do
            {
                num++;
                string path = (num <= 0) ? (fileNameWithoutExt + ext) : (fileNameWithoutExt + num + ext);
                text = Path.Combine(folder, path);
            }
            while (File.Exists(text));
            return text;
        }

        private AddinsApplication m_applications;

        private AddinsCommand m_commands;

        private IniFile m_aimIniFile;

        private IniFile m_revitIniFile;
    }
}
