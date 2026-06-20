using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using WixSharp;
using WixSharp.CommonTasks;
using File = System.IO.File;

namespace QuickMsiBuilder.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: QuickMsiBuilder.CLI <dll_path> [version] [author] [description] [icon_path] [bg_image_path] [revit_year] [full_class_name]");
                return;
            }

            string dllPath = Path.GetFullPath(args[0]);
            string version = args.Length > 1 ? args[1] : "1.0.0";
            string author = args.Length > 2 ? args[2] : "Autodesk";
            string description = args.Length > 3 ? args[3] : "Revit Add-in";
            string iconPath = args.Length > 4 ? args[4] : "";
            string bgImagePath = args.Length > 5 ? args[5] : "";
            string revitYear = args.Length > 6 ? args[6] : "2024";
            string fullClassName = args.Length > 7 ? args[7] : "";

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"Error: DLL file not found at {dllPath}");
                return;
            }

            try
            {
                BuildMsi(dllPath, version, author, description, iconPath, bgImagePath, revitYear, fullClassName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building MSI: {ex.Message}");
            }
        }

        static void BuildMsi(string dllPath, string version, string author, string description, string iconPath, string bgImagePath, string revitYear, string fullClassName)
        {
            string assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            string assemblyDir = Path.GetDirectoryName(dllPath);
            string outputDir = Path.Combine(assemblyDir, "InstallerOutput");
            Directory.CreateDirectory(outputDir);

            string addinFilePath = Path.Combine(outputDir, assemblyName + ".addin");
            GenerateAddinManifest(addinFilePath, assemblyName, author, description, fullClassName);

            string installDir = $@"%AppDataFolder%\Autodesk\Revit\Addins\{revitYear}";

            // Stable UpgradeCode based on assembly name
            Guid upgradeCode = GenerateGuidFromName(assemblyName);

            var project = new Project
            {
                Name = assemblyName,
                OutDir = outputDir,
                Platform = Platform.x64,
                Description = description,
                UI = WUI.WixUI_InstallDir,
                Version = new Version(version),
                OutFileName = $"{assemblyName}-{version}",
                InstallScope = InstallScope.perUser,
                MajorUpgrade = MajorUpgrade.Default,
                GUID = upgradeCode,
                ControlPanelInfo =
                {
                    Manufacturer = author,
                    Comments = description
                },
                Dirs = new Dir[]
                {
                    new InstallDir(installDir,
                        new WixSharp.File(dllPath),
                        new WixSharp.File(addinFilePath)
                    )
                }
            };

            if (File.Exists(iconPath)) project.ControlPanelInfo.ProductIcon = iconPath;
            if (File.Exists(bgImagePath)) project.BackgroundImage = bgImagePath;

            Compiler.BuildMsi(project);
            Console.WriteLine("MSI build process completed.");
        }

        static void GenerateAddinManifest(string filePath, string assemblyName, string author, string description, string fullClassName)
        {
            if (string.IsNullOrEmpty(fullClassName)) fullClassName = assemblyName + ".Command";

            XNamespace ns = "http://www.autodesk.com/revit/2009/addin";
            XElement root = new XElement(ns + "RevitAddIns",
                new XElement(ns + "AddIn", new XAttribute("Type", "Application"),
                    new XElement(ns + "Text", assemblyName),
                    new XElement(ns + "Description", description),
                    new XElement(ns + "Assembly", assemblyName + ".dll"),
                    new XElement(ns + "FullClassName", fullClassName),
                    new XElement(ns + "ClientId", Guid.NewGuid().ToString()),
                    new XElement(ns + "VendorId", "ADSK"),
                    new XElement(ns + "VendorDescription", author)
                )
            );

            root.Save(filePath);
        }

        static Guid GenerateGuidFromName(string name)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(name));
                return new Guid(hash);
            }
        }
    }
}
