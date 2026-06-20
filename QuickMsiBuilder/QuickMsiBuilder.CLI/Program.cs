using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Diagnostics;

namespace QuickMsiBuilder.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: QuickMsiBuilder.CLI <dll_path> [version] [author] [description] [icon_path] [bg_image_path] [revit_year]");
                return;
            }

            string dllPath = Path.GetFullPath(args[0]);
            string version = args.Length > 1 ? args[1] : "1.0.0";
            string author = args.Length > 2 ? args[2] : "Autodesk";
            string description = args.Length > 3 ? args[3] : "Revit Add-in";
            string iconPath = args.Length > 4 ? args[4] : "";
            string bgImagePath = args.Length > 5 ? args[5] : "";
            string revitYear = args.Length > 6 ? args[6] : "2024";

            if (!File.Exists(dllPath))
            {
                Console.WriteLine($"Error: DLL file not found at {dllPath}");
                return;
            }

            try
            {
                BuildMsi(dllPath, version, author, description, iconPath, bgImagePath, revitYear);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error building MSI: {ex.Message}");
            }
        }

        static void BuildMsi(string dllPath, string version, string author, string description, string iconPath, string bgImagePath, string revitYear)
        {
            string assemblyName = Path.GetFileNameWithoutExtension(dllPath);
            string assemblyDir = Path.GetDirectoryName(dllPath);
            string outputDir = Path.Combine(assemblyDir, "InstallerOutput");
            Directory.CreateDirectory(outputDir);

            string addinFilePath = Path.Combine(outputDir, assemblyName + ".addin");
            GenerateAddinManifest(addinFilePath, assemblyName, author, description);

            Console.WriteLine("Generating Wix Source...");
            string wxsPath = Path.Combine(outputDir, assemblyName + ".wxs");
            string installDir = $@"AppDataFolder\Autodesk\Revit\Addins\{revitYear}";

            string wxsContent = $@"<?xml version='1.0' encoding='UTF-8'?>
<Wix xmlns='http://schemas.microsoft.com/wix/2006/wi'>
    <Product Id='*' Name='{assemblyName}' Language='1033' Version='{version}' Manufacturer='{author}' UpgradeCode='{Guid.NewGuid()}'>
        <Package InstallerVersion='200' Compressed='yes' InstallScope='perUser' />
        <MajorUpgrade DowngradeErrorMessage='A newer version of [ProductName] is already installed.' />
        <MediaTemplate EmbedCab='yes' />

        <Feature Id='ProductFeature' Title='{assemblyName}' Level='1'>
            <ComponentGroupRef Id='ProductComponents' />
        </Feature>

        <Icon Id='ProductIcon' SourceFile='{(File.Exists(iconPath) ? iconPath : "")}' />
        <Property Id='ARPPRODUCTICON' Value='ProductIcon' />
    </Product>

    <Fragment>
        <Directory Id='TARGETDIR' Name='SourceDir'>
            <Directory Id='AppDataFolder'>
                <Directory Id='AutodeskDir' Name='Autodesk'>
                    <Directory Id='RevitDir' Name='Revit'>
                        <Directory Id='AddinsDir' Name='Addins'>
                            <Directory Id='INSTALLFOLDER' Name='{revitYear}' />
                        </Directory>
                    </Directory>
                </Directory>
            </Directory>
        </Directory>
    </Fragment>

    <Fragment>
        <ComponentGroup Id='ProductComponents' Directory='INSTALLFOLDER'>
            <Component Id='MainDll' Guid='{Guid.NewGuid()}'>
                <File Source='{dllPath}' KeyPath='yes' />
            </Component>
            <Component Id='AddinManifest' Guid='{Guid.NewGuid()}'>
                <File Source='{addinFilePath}' KeyPath='yes' />
            </Component>
        </ComponentGroup>
    </Fragment>
</Wix>";
            File.WriteAllText(wxsPath, wxsContent);

            Console.WriteLine($"Wxs generated: {wxsPath}");
            Console.WriteLine("In a production environment, candle.exe and light.exe would be called here.");
        }

        static void GenerateAddinManifest(string filePath, string assemblyName, string author, string description)
        {
            XNamespace ns = "http://www.autodesk.com/revit/2009/addin";
            XElement root = new XElement(ns + "RevitAddIns",
                new XElement(ns + "AddIn", new XAttribute("Type", "Command"),
                    new XElement(ns + "Text", assemblyName),
                    new XElement(ns + "Description", description),
                    new XElement(ns + "Assembly", assemblyName + ".dll"),
                    new XElement(ns + "FullClassName", assemblyName + ".Command"),
                    new XElement(ns + "ClientId", Guid.NewGuid().ToString()),
                    new XElement(ns + "VendorId", "ADSK"),
                    new XElement(ns + "VendorDescription", author)
                )
            );

            root.Save(filePath);
        }
    }
}
