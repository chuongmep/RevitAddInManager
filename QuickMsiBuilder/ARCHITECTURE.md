# Quick MSI Builder Architecture

The Quick MSI Builder is a decoupled, lightweight tool designed to automate the creation of MSI installers for Revit Add-ins.

## Components

1.  **AddInManager Integration**:
    *   Reachable from the **Build MSI** button and from the tree context menu.
    *   Launches `QuickMsiBuilder.UI` with the selected assembly and the running Revit version.
        Any node of the tree resolves to the same assembly - parent or child, either tab - so the
        user never has to hunt for the "right" node. The builder detects everything else itself.

2.  **QuickMsiBuilder.UI**:
    *   A standalone WPF application targeting `net48`.
    *   Uses the Add-in Manager's light theme dictionary (linked, not copied), so the two windows
        share one look. Light only - there is no theme switch here. `Esc` closes the window.
    *   Prefills Version, Author and Description from the assembly itself.
    *   **Add-in entry points** is a tick list of every command and application found in the
        assembly. All commands are ticked by default. A class name can be typed by hand only when
        the assembly declares no entry point at all.
    *   **Revit versions** is a tick list: one MSI can install into several Revit releases.
    *   Icon and background are optional, each with a **Clear** button; empty means the default
        installer look.
    *   Prefills from the previous release of the same assembly and offers a **Previous release**
        picker to restore any earlier build. The Revit release the builder was launched from always
        stays ticked, so an old history entry cannot silently drop the version being worked on.
    *   Validates before spending a build: the version against the MSI limits, at least one entry
        point, at least one Revit version, and a confirmation when an icon or background is missing.
    *   Runs the CLI, reports the real result in a short dialog with **Open folder** / **Open log**.

3.  **QuickMsiBuilder.CLI**:
    *   A headless console application targeting `net48`.
    *   `net48` is required: `WixSharp.bin` only ships `net451`/`net462` assemblies.
    *   Accepts arguments for all metadata and file paths, validated by `MsiBuildOptions`.
    *   Reads entry points and metadata out of the target's IL with Mono.Cecil
        (`AssemblyInspector`). Reflection is not an option: the target references the Revit API and
        cannot be loaded into this process.
    *   Generates the Revit `.addin` manifest (`AddinManifest`) with one `AddIn` element per entry
        point and the element set Revit expects for each type.
    *   Builds the `.msi` through WixSharp: one `InstallDir` at the Revit `Addins` root with a child
        folder per selected release, the same layout the Add-in Manager installer itself uses.
    *   Logs to console and to a rolling NLog file (`BuildLog`).
    *   Exits with code `0` on success, `1` on failure, and prints `MSI_PATH=<path>` so the UI can
        pick up the produced file without parsing prose.

4.  **QuickMsiBuilder.Tests**:
    *   xUnit coverage for argument validation, version/year normalisation, entry point detection
        and resolution, deterministic GUIDs, the generated manifest and the release history store.

## What goes into the package

Everything sitting next to the target assembly, subfolders included, so an add-in with its own
dependencies installs complete. The `InstallerOutput` folder the builder writes into is excluded -
packaging it would swallow the previous MSI and grow the package on every run.

The payload is installed into a subfolder named after the assembly so it cannot collide with other
add-ins sharing the Revit `Addins` directory:

```
%AppData%\Autodesk\Revit\Addins\<year>\MyAddin.addin
%AppData%\Autodesk\Revit\Addins\<year>\MyAddin\MyAddin.dll, dependencies, subfolders...
```

## Defaults

| Field | Default |
| --- | --- |
| Version | assembly version, or `1.0.0` when it is missing or not MSI-compatible |
| Author | assembly `Company`, otherwise the Windows account name |
| Description | assembly description, otherwise `Revit Add-in` |
| Entry points | every `IExternalCommand` in the assembly (every `IExternalApplication` if it has no commands) |
| Revit versions | the running Revit release; the picker offers the current year + 1 back to four years earlier |

## Product identity

The MSI `UpgradeCode` is derived from the assembly name alone, so rebuilding an add-in - even for a
different set of Revit years - upgrades the existing install instead of stacking a second product.
Each manifest `ClientId` is derived from assembly name plus class name, so it stays stable across
releases and unique per entry point.

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
*   Console: `Info` and above, plain messages, because the UI reads stdout.

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
        QuickMsiBuilder.UI.exe, QuickMsiBuilder.CLI.exe, WixSharp*.dll, Mono.Cecil*.dll, NLog.dll
        wix\candle.exe, light.exe, ...
```

`AddInManagerViewModel.ResolveQuickMsiBuilder` looks next to the add-in assembly first (what a local
build produces) and then two levels up in `QuickMsiBuilder`, which is the installed layout.

The QuickMsiBuilder projects always build as `Release` regardless of the Revit configuration
(`SetConfiguration` on the `ProjectReference`), so the 14 MB toolset is staged once instead of once
per configuration.

## Workflow

1.  User selects an add-in in Revit Add-In Manager and clicks **Build MSI**.
2.  `AddInManager` launches `QuickMsiBuilder.UI.exe` with the assembly and the Revit version.
3.  The UI reads the assembly, restores the previous release and displays everything ready to go.
4.  User adjusts what they want and clicks **Build MSI**.
5.  The UI runs `QuickMsiBuilder.CLI.exe` and waits for it.
6.  The CLI generates the manifest and builds the MSI into an `InstallerOutput` folder next to the
    target assembly.
