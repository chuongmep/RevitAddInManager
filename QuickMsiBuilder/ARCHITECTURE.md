# Quick MSI Builder Architecture

The Quick MSI Builder is a decoupled, lightweight tool designed to automate the creation of MSI installers for Revit Add-ins.

## Components

1.  **AddInManager Integration**:
    *   Integrated into the existing Revit Add-In Manager UI via context menus.
    *   Triggers `QuickMsiBuilder.UI`, passing the selected assembly path, the running Revit version,
        the full class name of the selected add-in and its type (Command or Application).

2.  **QuickMsiBuilder.UI**:
    *   A standalone WPF application targeting `net48`.
    *   Uses the Add-in Manager's light theme dictionary (linked, not copied), so the two windows
        share one look. Light only - there is no theme switch here. `Esc` closes the window.
    *   Allows users to review and edit metadata (Version, Author, Description, Add-in Type, Full Class Name).
    *   **Revit versions** is a multi-select list: one MSI can install into several Revit releases.
    *   Author defaults to the Windows account name, overridden by the assembly's `Company` attribute
        and then by the previous release.
    *   Icon and background are optional, each with a **Clear** button; empty means the default
        installer look.
    *   Prefills everything from the previous release of the same assembly, and offers a
        **Previous release** picker to restore the settings of any earlier build.
    *   Invokes the CLI tool, waits for it and reports the real build result. **Open log** shows the
        NLog file.

3.  **QuickMsiBuilder.CLI**:
    *   A headless console application targeting `net48`.
    *   `net48` is required: `WixSharp.bin` only ships `net451`/`net462` assemblies.
    *   Accepts arguments for all metadata and file paths, validated by `MsiBuildOptions`.
    *   Generates the Revit `.addin` manifest (`AddinManifest`) with the element set Revit expects
        for the given add-in type.
    *   Builds the `.msi` through WixSharp: one `InstallDir` at the Revit `Addins` root with a child
        folder per selected release, the same layout the Add-in Manager installer itself uses.
    *   Logs to console and to a rolling NLog file (`BuildLog`).
    *   Exits with code `0` on success, `1` on failure.

4.  **QuickMsiBuilder.Tests**:
    *   xUnit coverage for argument validation, version/year normalisation, deterministic GUIDs,
        the generated manifest shape and the release history store.

## Product identity

The MSI `UpgradeCode` is derived from the assembly name alone, so rebuilding an add-in - even for a
different set of Revit years - upgrades the existing install instead of stacking a second product.
The manifest `ClientId` is derived from assembly name plus full class name, so it stays stable across
releases.

## Release history

Every successful build is appended by the CLI to
`%AppData%\RevitAddinManager\QuickMsiBuilder\build-history.xml` (`BuildHistoryStore`), keyed by the
target assembly path. The UI reads it back so the next release starts from what was entered last
time instead of from blank defaults. The store keeps the 10 most recent builds per assembly, writes
atomically, and treats a missing or corrupt file as empty history - it is a convenience and never
fails a build.

## Logging

`BuildLog` configures NLog in code (no `NLog.config` to deploy):

*   File: `%AppData%\RevitAddinManager\QuickMsiBuilder\logs\quickmsibuilder.log`, rolled at 1 MB,
    5 archives, `Debug` level and above.
*   Console: `Info` and above, plain messages, because the UI parses stdout.

## Deployment and the WiX toolset

WixSharp shells out to `candle.exe` / `light.exe` but does not ship them, and an end user who just
installed the add-in has neither a WiX Toolset nor a NuGet cache. The toolset is therefore bundled:

*   `QuickMsiBuilder.CLI.csproj` copies `WixSharp.wix.bin` binaries into a `wix` folder next to the
    executable (`BundleWixToolset` target).
*   `Program.TryLocateWixToolset` prefers that bundled folder, then falls back to whatever WiX the
    machine already has, and only then reports a plain "install WiX" message.

The payload is identical for every Revit release, so it is installed **once** rather than duplicated
per version. The add-in build stages it in `AddInManager/bin/AddInShared/QuickMsiBuilder`, the Nuke
installer picks up every folder under `AddInShared`, and the MSI lays it out as:

```
%AppData%\Autodesk\Revit\Addins\
    2019\ ... 2027\
        RevitAddinManager.addin
        RevitAddinManager\RevitAddinManager.dll
    QuickMsiBuilder\
        QuickMsiBuilder.UI.exe, QuickMsiBuilder.CLI.exe, WixSharp*.dll, NLog.dll
        wix\candle.exe, light.exe, ...
```

`AddInManagerViewModel.ResolveQuickMsiBuilder` looks next to the add-in assembly first (what a local
build produces) and then two levels up in `QuickMsiBuilder`, which is the installed layout.

The QuickMsiBuilder projects always build as `Release` regardless of the Revit configuration
(`SetConfiguration` on the `ProjectReference`), so the 14 MB toolset is staged once instead of once
per configuration.

## Workflow

1.  User selects an assembly in Revit Add-In Manager and clicks **Build MSI...**.
2.  `AddInManager` launches `QuickMsiBuilder.UI.exe` with the assembly path and add-in metadata.
3.  `QuickMsiBuilder.UI` extracts metadata, restores the previous release and displays it.
4.  User adjusts settings, ticks the Revit versions and clicks **Build MSI**.
5.  `QuickMsiBuilder.UI` launches `QuickMsiBuilder.CLI.exe` with all gathered parameters.
6.  `QuickMsiBuilder.CLI` generates the manifest and builds the MSI into an `InstallerOutput`
    folder next to the target assembly.

## Known limitation

Only the selected assembly and its manifest are packaged. If the add-in has its own dependencies,
the CLI prints and logs a warning listing the assemblies that were left out.
