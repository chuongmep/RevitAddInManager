# Quick MSI Builder Integration Guide

This guide describes how the Quick MSI Builder is wired into the Revit Add-In Manager project.

## 1. Project setup

Three projects live under the `QuickMsiBuilder` directory:

*   `QuickMsiBuilder.CLI`: console app (`net48`, WixSharp, Mono.Cecil, NLog).
*   `QuickMsiBuilder.UI`: WPF app (`net48`).
*   `QuickMsiBuilder.Tests`: xUnit tests (`net48`).

They build as plain `Debug`/`Release`. The solution maps every Revit configuration to `Release` for
them, and `RevitAddinManager` references them with `SetConfiguration=Configuration=Release`, so the
tool is built once no matter which Revit configuration is active.

## 2. UI integration

`AddInManager/View/FrmAddInManager.xaml` exposes the feature twice, both bound to `BuildMsiCommand`:

*   a **Build MSI** button in the right hand button column, and
*   a **Build MSI...** item at the bottom of the context menu of both trees.

The old **Startup** button was replaced by **Build MSI**; selecting the Startup tab now refreshes its
list itself (`IsTabStartSelected`), which is all that button used to add.

## 3. ViewModel logic

`AddInManagerViewModel.BuildMsiClick` resolves the assembly from whatever is selected and launches
the builder:

```csharp
Process.Start(new ProcessStartInfo
{
    FileName = uiPath,
    Arguments = ArgumentUtils.Quote(filePath, revitVersion),
    UseShellExecute = true
});
```

`ArgumentUtils.Quote` escapes the values so quotes and trailing backslashes survive the command line.
No class name is passed: the builder detects the entry points and defaults to every command.

## 4. Shared theme

`QuickMsiBuilder.UI.csproj` links (does not copy) the Add-in Manager theme assets, so the two windows
cannot drift apart:

```xml
<Page Include="..\..\AddInManager\Themes\Styles\LightTheme.xaml">
  <Link>Themes\Styles\LightTheme.xaml</Link>
</Page>
<Page Include="..\..\AddInManager\Themes\Base\*.xaml">
  <Link>Themes\Base\%(Filename)%(Extension)</Link>
</Page>
```

`ToggleSwitch.cs`, `TreeViewMarginConverter.cs` and `Resources\dev.ico` are linked the same way. The
dictionaries need `PresentationFramework.Aero2` and implicit usings, both enabled in the project file.

**The theme dictionaries must keep merging each other through relative URIs**
(`Source="../Base/Colors.xaml"`, not `Source="/RevitAddinManager;component/Themes/Base/Colors.xaml"`).
An assembly-qualified URI makes the builder crash on startup, because it is installed on its own and
`RevitAddinManager.dll` is not next to it.

## 5. Deployment

Quick MSI Builder is shared by every Revit release, so it is installed once next to the version
folders rather than copied into each of them:

1.  `ProjectReference` entries in `AddInManager/RevitAddinManager.csproj` use
    `ReferenceOutputAssembly=false` (build order only) plus `SetConfiguration=Configuration=Release`.
2.  The `CopyFiles` target stages the tool - including its bundled `wix` folder - into
    `AddInManager/bin/AddInShared/QuickMsiBuilder`. Debug builds also mirror it into
    `%AppData%\Autodesk\Revit\Addins\QuickMsiBuilder`, matching the installed layout.
3.  `build/Build.Installer.cs` (`GetSharedDirectories`) passes every folder under `AddInShared` to
    the installer alongside the per-version folders.
4.  `Installer/Installer.cs` keeps a folder's own name when the name carries no Revit year, so the
    payload becomes `Addins\QuickMsiBuilder` instead of a year folder.

`AddInManagerViewModel.ResolveQuickMsiBuilder` finds it next to the add-in assembly (local build),
two levels up in `QuickMsiBuilder` (installed), or in `bin\AddInShared\QuickMsiBuilder` (running a
Release build straight out of `bin`).

## 6. Release history

`QuickMsiBuilder.CLI` records each successful build in
`%AppData%\RevitAddinManager\QuickMsiBuilder\build-history.xml`. `QuickMsiBuilder.UI` references the
CLI project for the shared `BuildHistoryStore` type, prefills the form from the latest release of the
selected assembly and exposes the older ones through the **Previous release** picker.

## 7. Tests

```bash
dotnet test QuickMsiBuilder/QuickMsiBuilder.Tests/QuickMsiBuilder.Tests.csproj
```

`RevitAddinFixtures.cs` declares stand-in `Autodesk.Revit.UI` interfaces so entry point detection can
be tested without referencing the Revit API - `AssemblyInspector` matches interfaces by full name
straight out of the IL.
