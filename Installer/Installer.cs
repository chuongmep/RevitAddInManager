using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;
using File = System.IO.File;

const string installationDir = @"%AppDataFolder%\Autodesk\Revit\Addins\";
const string projectName = "RevitAddinManager";
const string outputName = "RevitAddinManager";
const string outputDir = "output";
const string version = "1.5.6";

var fileName = new StringBuilder().Append(outputName).Append("-").Append(version);
var project = new Project
{
    Name = projectName,
    OutDir = outputDir,
    Platform = Platform.x64,
    Description = "Project Support Developer Work With Revit API",
    UI = WUI.WixUI_InstallDir,
    Version = new Version(version),
    OutFileName = fileName.ToString(),
    InstallScope = InstallScope.perUser,
    MajorUpgrade = MajorUpgrade.Default,
    GUID = new Guid("A0176A8B-2483-4073-B6BB-4A481D9B4439"),
    BackgroundImage = @"Installer\Resources\Icons\BackgroundImage.png",
    BannerImage = @"Installer\Resources\Icons\BannerImage.png",
    ControlPanelInfo =
    {
        Manufacturer = "Autodesk",
        HelpLink = "https://github.com/chuongmep/RevitAddInManager/issues",
        Comments = "Project Support Developer With Revit API",
        ProductIcon = @"Installer\Resources\Icons\ShellIcon.ico"
    },
    Dirs = new Dir[]
    {
        new InstallDir(installationDir, GenerateWixEntities())
    }
};

MajorUpgrade.Default.AllowSameVersionUpgrades = true;
project.RemoveDialogsBetween(NativeDialogs.WelcomeDlg, NativeDialogs.InstallDirDlg);
string buildMsi = project.BuildMsi();
FileInfo fileInfo = new FileInfo(buildMsi);
string zipfileName = Path.Combine(fileInfo.Directory.FullName, fileName + ".zip");
CompressFile(buildMsi, zipfileName);
WixEntity[] GenerateWixEntities()
{
    var versionRegex = new Regex(@"\d+");
    var versionStorages = new Dictionary<string, List<WixEntity>>();

    foreach (var directory in args)
    {
        var directoryInfo = new DirectoryInfo(directory);
        var fileVersion = versionRegex.Match(directoryInfo.Name).Value;
        var files = new Files($@"{directory}\*.*");
        if (versionStorages.ContainsKey(fileVersion))
            versionStorages[fileVersion].Add(files);
        else
            versionStorages.Add(fileVersion, new List<WixEntity> { files });

        var assemblies = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        Console.WriteLine($"Added '{fileVersion}' version files: ");
        foreach (var assembly in assemblies) Console.WriteLine($"'{assembly}'");
    }

    return versionStorages.Select(storage => new Dir(storage.Key, storage.Value.ToArray())).Cast<WixEntity>().ToArray();
}


void CompressFile(string filePath, string OutputFilePath, int compressLevel = 9)
{
    try
    {
        using (ZipOutputStream OutputStream = new ZipOutputStream(File.Create(OutputFilePath)))
        {
            // Define the compression level
            // 0 - store only to 9 - means best compression
            OutputStream.SetLevel(compressLevel);
            byte[] buffer = new byte[4096];
            ZipEntry entry = new ZipEntry(Path.GetFileName(filePath));
            entry.DateTime = DateTime.Now;
            OutputStream.PutNextEntry(entry);

            using (FileStream fs = File.OpenRead(filePath))
            {
                // Using a fixed size buffer here makes no noticeable difference for output
                // but keeps a lid on memory usage.
                int sourceBytes;
                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    OutputStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }

            OutputStream.Finish();
            OutputStream.Close();
            Console.WriteLine("Zip file has been built: " + OutputFilePath);
        }
    }
    catch (Exception ex)
    {
        // No need to rethrow the exception as for our purposes its handled.
        Console.WriteLine("Exception during processing {0}", ex);
    }
}