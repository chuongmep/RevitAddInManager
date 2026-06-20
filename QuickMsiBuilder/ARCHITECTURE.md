# Quick MSI Builder Architecture

The Quick MSI Builder is a decoupled, lightweight tool designed to automate the creation of MSI installers for Revit Add-ins.

## Components

1.  **AddInManager Integration**:
    *   Integrated into the existing Revit Add-In Manager UI via context menus.
    *   Triggers the `QuickMsiBuilder.UI` passing the selected assembly path.

2.  **QuickMsiBuilder.UI**:
    *   A standalone .NET 8 WPF application.
    *   Allows users to review and edit metadata (Version, Author, Description).
    *   Supports custom cosmetics (Icon and Background image).
    *   Automatically extracts default metadata from the target DLL.
    *   Invokes the CLI tool to perform the actual build.

3.  **QuickMsiBuilder.CLI**:
    *   A headless .NET 8 Console application.
    *   Accepts arguments for all metadata and file paths.
    *   Dynamically generates the required Revit `.addin` manifest.
    *   Generates a WiX source file (`.wxs`) and would ideally invoke WiX toolset (candle/light) to produce the `.msi`.

## Workflow

1.  User selects an assembly in Revit Add-In Manager and clicks **Build MSI...**.
2.  `AddInManager` launches `QuickMsiBuilder.UI.exe` with the assembly path.
3.  `QuickMsiBuilder.UI` extracts metadata and displays it to the user.
4.  User adjusts settings and clicks **Build MSI**.
5.  `QuickMsiBuilder.UI` launches `QuickMsiBuilder.CLI.exe` with all gathered parameters.
6.  `QuickMsiBuilder.CLI` generates the manifest, WiX source, and triggers the MSI build.
