# Quick MSI Builder Integration Guide

This guide describes how the Quick MSI Builder was integrated into the Revit Add-In Manager project.

## 1. Project Setup
Two new projects were added to the solution under the `QuickMsiBuilder` directory:
*   `QuickMsiBuilder.CLI`: .NET 8 Console App.
*   `QuickMsiBuilder.UI`: .NET 8 WPF App.

## 2. UI Integration
In `AddInManager/View/FrmAddInManager.xaml`, a new `MenuItem` was added to the `ContextMenu` of `TreeViewCommand` and `TreeViewApp`:

```xml
<Separator />
<MenuItem Command="{Binding BuildMsiCommand}" Header="Build MSI..." />
```

## 3. ViewModel Logic
In `AddInManager/ViewModel/AddInManagerViewModel.cs`, the `BuildMsiCommand` was implemented to identify the selected assembly and launch the UI tool:

```csharp
private void BuildMsiClick()
{
    // ... logic to get filePath from selected item ...
    string uiPath = Path.Combine(assemblyDir, "QuickMsiBuilder.UI.exe");
    Process.Start(uiPath, $"\"{filePath}\"");
}
```

## 4. Deployment
To use this feature, ensure that `QuickMsiBuilder.UI.exe` and `QuickMsiBuilder.CLI.exe` are deployed in the same directory as the `RevitAddinManager.dll` (or adjusted via the fallback paths in the code).
