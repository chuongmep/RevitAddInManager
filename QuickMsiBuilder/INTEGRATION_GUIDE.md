# Quick MSI Builder Integration Guide

This guide describes how the Quick MSI Builder was integrated into the Revit Add-In Manager project.

## 1. Project Setup

Three new projects were added to the solution under the `QuickMsiBuilder` directory:

*   `QuickMsiBuilder.CLI`: console app (`net48`, WixSharp).
*   `QuickMsiBuilder.UI`: WPF app (`net48`).
*   `QuickMsiBuilder.Tests`: xUnit tests (`net48`).

They declare the same `Configurations` list as `RevitAddinManager`, so the solution keeps a single
set of configurations (`Debug R22` … `Release R27`, `Installer`) and no extra platforms are added.

## 2. UI Integration

In `AddInManager/View/FrmAddInManager.xaml`, one `MenuItem` was added at the bottom of the
`ContextMenu` of `TreeViewCommand` and `TreeViewApp`:

```xml
<Separator />
<MenuItem Command="{Binding BuildMsiCommand}" Header="Build MSI..." />
```

## 3. ViewModel Logic

In `AddInManager/ViewModel/AddInManagerViewModel.cs`, `BuildMsiClick` resolves the selected
assembly plus its full class name and add-in type, then launches the UI tool:

```csharp
Process.Start(new ProcessStartInfo
{
    FileName = uiPath,
    Arguments = ArgumentUtils.Quote(filePath, revitVersion, fullClassName, addinType.ToString()),
    UseShellExecute = true
});
```

`ArgumentUtils.Quote` escapes the values so quotes and trailing backslashes survive the command line.

## 3b. Shared theme

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

The builder is light theme only, so `DarkTheme.xaml` is not linked.

**The theme dictionaries must keep merging each other through relative URIs**
(`Source="../Base/Colors.xaml"`, not `Source="/RevitAddinManager;component/Themes/Base/Colors.xaml"`).
An assembly-qualified URI makes the builder crash on startup, because it is installed on its own and
`RevitAddinManager.dll` is not next to it.

## 4. Deployment

Quick MSI Builder is shared by every Revit release, so it is installed once next to the version
folders rather than copied into each of them:

1.  `ProjectReference` entries in `AddInManager/RevitAddinManager.csproj` use
    `ReferenceOutputAssembly=false` (build order only) plus `SetConfiguration=Configuration=Release`,
    so the tool is built once no matter which Revit configuration is active.
2.  The `CopyFiles` target stages the tool - including its bundled `wix` folder - into
    `AddInManager/bin/AddInShared/QuickMsiBuilder`. Debug builds also mirror it into
    `%AppData%\Autodesk\Revit\Addins\QuickMsiBuilder`, matching the installed layout.
3.  `build/Build.Installer.cs` (`GetSharedDirectories`) passes every folder under `AddInShared` to
    the installer alongside the per-version folders.
4.  `Installer/Installer.cs` keeps a folder's own name when the name carries no Revit year, so the
    payload becomes `Addins\QuickMsiBuilder` instead of a year folder.

`AddInManagerViewModel.ResolveQuickMsiBuilder` finds it either next to the add-in assembly (local
build) or two levels up in `QuickMsiBuilder` (installed).

## 5. Release history

`QuickMsiBuilder.CLI` records each successful build in
`%AppData%\RevitAddinManager\QuickMsiBuilder\build-history.xml`. `QuickMsiBuilder.UI` references the
CLI project for the shared `BuildHistoryStore` type, prefills the form from the latest release of the
selected assembly and exposes the older ones through the **Previous release** picker.

## 6. Tests

```bash
dotnet test QuickMsiBuilder/QuickMsiBuilder.Tests/QuickMsiBuilder.Tests.csproj
```
