using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Installer;
using WixSharp;
using WixSharp.CommonTasks;
using WixSharp.Controls;


const string installationDir = @"%AppDataFolder%\Autodesk\Revit\Addins\";
const string projectName = "RevitAddinManager";
const string outputName = "RevitAddinManager";
const string outputDir = "output";

var version = GetAssemblyVersion(out var dllVersion);
var fileName = new StringBuilder().Append(outputName).Append("-").Append(dllVersion);
var project = new Project
{
    Name = projectName,
    OutDir = outputDir,
    Platform = Platform.x64,
    Description = "Project Support Developer Work With Revit API",
    UI = WUI.WixUI_InstallDir,
    ReinstallMode = "e",
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
project.BuildMsi();

WixEntity[] GenerateWixEntities()
{
    var versionRegex = new Regex(@"\d+");
    var versionStorages = new Dictionary<string, List<WixEntity>>();

    foreach (var directory in args)
    {
        Console.WriteLine($"Find dll in  directory: {directory}");
        //DirectoryInfo directoryInfo = new DirectoryInfo(directory);
        //string fileVersion = versionRegex.Match(directoryInfo.Name).Value;
        Files files = new Files($@"{directory}\*.*");
        foreach (int version in Enum.GetValues(typeof(BuildVersions.RevitVersion)))
        {
            Console.WriteLine($"Start addd file to folder {directory}/{version}");
            if (versionStorages.ContainsKey(version.ToString()))
                versionStorages[version.ToString()].Add(files);
            else
                versionStorages.Add(version.ToString(), new List<WixEntity> { files });
            Console.WriteLine($"Added '{version}' version files: ");
        }

        var assemblies = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
        foreach (var assembly in assemblies) Console.WriteLine($"'{assembly}'");
    }

    return versionStorages.Select(storage => new Dir(storage.Key, storage.Value.ToArray())).Cast<WixEntity>().ToArray();
}

string GetAssemblyVersion(out string originalVersion)
{
    string AssemblyName = @"RevitAddinManager.dll";
    foreach (var directory in args)
    {
        var assemblies = Directory.GetFiles(directory,AssemblyName, SearchOption.AllDirectories);
        if (assemblies.Length == 0) continue;
        var fileVersionInfo = FileVersionInfo.GetVersionInfo(assemblies[0]);
        var versionGroups = fileVersionInfo.ProductVersion.Split('.');
        var majorVersion = versionGroups[0];
        if (int.Parse(majorVersion) > 255) versionGroups[0] = majorVersion.Substring(majorVersion.Length - 2);
        originalVersion = fileVersionInfo.ProductVersion;
        var wixVersion = string.Join(".", versionGroups);
        if (!originalVersion.Equals(wixVersion)) Console.WriteLine($"Installer version trimmed from '{originalVersion}' to '{wixVersion}'");
        return wixVersion;
    }

    throw new Exception($"Cant find {AssemblyName} file");
}