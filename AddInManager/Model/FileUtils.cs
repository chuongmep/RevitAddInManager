using System.IO;
using System.Windows;

namespace RevitAddinManager.Model;

public static class FileUtils
{
    public static DateTime GetModifyTime(string filePath)
    {
        return File.GetLastWriteTime(filePath);
    }

    public static string CreateTempFolder(string prefix)
    {
        var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var tempPath = Path.Combine(folderPath, "Temp");
        var directoryInfo = new DirectoryInfo(Path.Combine(tempPath, DefaultSetting.TempFolderName));
        if (!directoryInfo.Exists)
        {
            directoryInfo.Create();
        }
        foreach (var directoryInfo2 in directoryInfo.GetDirectories())
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
        var str = $"{DateTime.Now:yyyyMMdd_HHmmss_ffff}";
        var path = Path.Combine(directoryInfo.FullName, prefix + str);
        var directoryInfo3 = new DirectoryInfo(path);
        directoryInfo3.Create();
        return directoryInfo3.FullName;
    }

    public static void SetWriteable(string fileName)
    {
        if (File.Exists(fileName))
        {
            var fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
            File.SetAttributes(fileName, fileAttributes);
        }
    }

    public static bool SameFile(string file1, string file2)
    {
        return 0 == String.Compare(file1.Trim(), file2.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool CreateFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            return true;
        }
        try
        {
            var directoryName = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            using (new FileInfo(filePath).Create())
            {
                SetWriteable(filePath);
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
            var fileAttributes = File.GetAttributes(fileName) & ~FileAttributes.ReadOnly;
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
        var path = Path.Combine(destFolder, Path.GetFileName(filePath));
        return File.Exists(path);
    }

    public static string CopyFileToFolder(string sourceFilePath, string destFolder, bool onlyCopyRelated, List<FileInfo> allCopiedFiles)
    {
        if (!File.Exists(sourceFilePath))
        {
            return null;
        }
        var directoryName = Path.GetDirectoryName(sourceFilePath);
        if (onlyCopyRelated)
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFilePath);
            var searchPattern = fileNameWithoutExtension + ".*";
            var files = Directory.GetFiles(directoryName, searchPattern, SearchOption.TopDirectoryOnly);
            foreach (var srcFileName in files)
            {
                var fileName = Path.GetFileName(srcFileName);
                var desFileName = Path.Combine(destFolder, fileName);
                var flag = CopyFile(srcFileName, desFileName);
                if (flag)
                {
                    var item = new FileInfo(desFileName);
                    allCopiedFiles.Add(item);
                }
            }
        }
        else
        {
            var folderSize = GetFolderSize(directoryName);
            if (folderSize > 50L)
            {
                switch (FolderTooBigDialog.Show(directoryName, folderSize))
                {
                    case MessageBoxResult.Yes:
                        CopyDirectory(directoryName, destFolder, allCopiedFiles);
                        break;

                    case MessageBoxResult.No:
                        CopyFileToFolder(sourceFilePath, destFolder, true, allCopiedFiles);
                        break;

                    default:
                        return null;
                }
            }
            else
            {
                CopyDirectory(directoryName, destFolder, allCopiedFiles);
            }
        }
        var text3 = Path.Combine(destFolder, Path.GetFileName(sourceFilePath));
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
        var fileAttributes = File.GetAttributes(sourceFilename) & ~FileAttributes.ReadOnly;
        File.SetAttributes(sourceFilename, fileAttributes);
        if (File.Exists(destinationFilename))
        {
            var fileAttributes2 = File.GetAttributes(destinationFilename) & ~FileAttributes.ReadOnly;
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
            var directories = Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories);
            foreach (var text in directories)
            {
                var str = text.Replace(sourceDir, "");
                var path = desDir + str;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            var files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
            foreach (var text2 in files)
            {
                var str2 = text2.Replace(sourceDir, "");
                var text3 = desDir + str2;
                if (!Directory.Exists(Path.GetDirectoryName(text3)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(text3));
                }
                if (CopyFile(text2, text3))
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
        var directoryInfo = new DirectoryInfo(folderPath);
        var folderSize = 0L;
        foreach (var fileSystemInfo in directoryInfo.GetFileSystemInfos())
        {
            if (fileSystemInfo is FileInfo)
            {
                folderSize += ((FileInfo)fileSystemInfo).Length;
            }
            else
            {
                folderSize += GetFolderSize(fileSystemInfo.FullName);
            }
        }
        return folderSize / 1024L / 1024L;
    }
}