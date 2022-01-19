using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AddInManager.Model
{
    public static class FileUtils
    {
        public static DateTime GetModifyTime(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }

        public static string CreateTempFolder(string prefix)
        {
            string folderPath = Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            string tempPath = Path.Combine(folderPath, "Temp");
            DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(tempPath, "RevitAddins"));
            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
            foreach (DirectoryInfo directoryInfo2 in directoryInfo.GetDirectories())
            {
                try
                {
                    Directory.Delete(directoryInfo2.FullName, true);
                }
                catch
                {
                    // ignored
                }
            }
            string str = $"{DateTime.Now:yyyyMMdd_HHmmss_ffff}";
            string path = Path.Combine(directoryInfo.FullName, prefix + str);
            DirectoryInfo directoryInfo3 = new DirectoryInfo(path);
            directoryInfo3.Create();
            return directoryInfo3.FullName;
        }


        public static void SetWriteable(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
                File.SetAttributes(fileName, fileAttributes);
            }
        }

        public static bool SameFile(string file1, string file2)
        {
            return 0 == string.Compare(file1.Trim(), file2.Trim(), true);
        }

        public static bool CreateFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return true;
            }
            try
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (new FileInfo(filePath).Create())
                {
                    FileUtils.SetWriteable(filePath);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return File.Exists(filePath);
        }

        public static void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                FileAttributes fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
                File.SetAttributes(fileName, fileAttributes);
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        public static bool FileExistsInFolder(string filePath, string destFolder)
        {
            string path = Path.Combine(destFolder, Path.GetFileName(filePath));
            return File.Exists(path);
        }

        public static string CopyFileToFolder(string sourceFilePath, string destFolder, bool onlyCopyRelated, List<FileInfo> allCopiedFiles)
        {
            if (!File.Exists(sourceFilePath))
            {
                return null;
            }
            string directoryName = Path.GetDirectoryName(sourceFilePath);
            if (onlyCopyRelated)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
                string searchPattern = fileNameWithoutExtension + ".*";
                string[] files = Directory.GetFiles(directoryName, searchPattern, SearchOption.TopDirectoryOnly);
                foreach (string text in files)
                {
                    string fileName = Path.GetFileName(text);
                    string text2 = Path.Combine(destFolder, fileName);
                    bool flag = FileUtils.CopyFile(text, text2);
                    if (flag)
                    {
                        FileInfo item = new FileInfo(text2);
                        allCopiedFiles.Add(item);
                    }
                }
            }
            else
            {
                long folderSize = FileUtils.GetFolderSize(directoryName);
                if (folderSize > 50L)
                {
                    switch (FolderTooBigDialog.Show(directoryName, folderSize))
                    {
                        case DialogResult.Yes:
                            FileUtils.CopyDirectory(directoryName, destFolder, allCopiedFiles);
                            break;
                        case DialogResult.No:
                            FileUtils.CopyFileToFolder(sourceFilePath, destFolder, true, allCopiedFiles);
                            break;
                        default:
                            return null;
                    }
                }
                else
                {
                    FileUtils.CopyDirectory(directoryName, destFolder, allCopiedFiles);
                }
            }
            string text3 = Path.Combine(destFolder, Path.GetFileName(sourceFilePath));
            if (File.Exists(text3))
            {
                return text3;
            }
            return null;
        }

        public static bool CopyFile(string sourceFilename, string destinationFilename)
        {
            if (!File.Exists(sourceFilename))
            {
                return false;
            }
            FileAttributes fileAttributes = File.GetAttributes(sourceFilename) & ~FileAttributes.ReadOnly;
            File.SetAttributes(sourceFilename, fileAttributes);
            if (File.Exists(destinationFilename))
            {
                FileAttributes fileAttributes2 = File.GetAttributes(destinationFilename) & ~FileAttributes.ReadOnly;
                File.SetAttributes(destinationFilename, fileAttributes2);
                File.Delete(destinationFilename);
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(destinationFilename)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFilename));
                }
                File.Copy(sourceFilename, destinationFilename, true);
            }
            catch (Exception)
            {
                return false;
            }
            return File.Exists(destinationFilename);
        }

        public static void CopyDirectory(string sourceDir, string desDir, List<FileInfo> allCopiedFiles)
        {
            try
            {
                string[] directories = Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories);
                foreach (string text in directories)
                {
                    string str = text.Replace(sourceDir, "");
                    string path = desDir + str;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                foreach (string text2 in files)
                {
                    string str2 = text2.Replace(sourceDir, "");
                    string text3 = desDir + str2;
                    if (!Directory.Exists(Path.GetDirectoryName(text3)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(text3));
                    }
                    if (FileUtils.CopyFile(text2, text3))
                    {
                        allCopiedFiles.Add(new FileInfo(text3));
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        public static long GetFolderSize(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            long num = 0L;
            foreach (FileSystemInfo fileSystemInfo in directoryInfo.GetFileSystemInfos())
            {
                if (fileSystemInfo is FileInfo)
                {
                    num += ((FileInfo)fileSystemInfo).Length;
                }
                else
                {
                    num += FileUtils.GetFolderSize(fileSystemInfo.FullName);
                }
            }
            return num / 1024L / 1024L;
        }

        private const string TempFolderName = "RevitAddins";
    }
}
