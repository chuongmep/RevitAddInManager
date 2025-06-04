# Changelog
- 2026-04-10 **1.5.8**
  - Support Revit 2026 Release
- 2025-03-23 **1.5.7**
  - Fix click help button in revit 2025
  - Change configuration package to open source maintainer
- 2024-06-29 **1.5.6**
  - Hotfix bug with Revit 2025: Unload assembly not done when user raise an execute exception.
- 2024-04-02 **1.5.4**
  - Add new way to execute command with Revit 2025 [#54](https://github.com/chuongmep/RevitAddInManager/pull/54)
- 2024-04-02 **1.5.3**
  - Support Revit 2025 Release
- 2023-09-20 **1.5.2**
    - Improvement search bar.
    - Fix Bug Popup resolve load assembly manual with same domain conflict with another add-in.
- 2023-09-05 **1.5.1**
    - Support basic compare element properties (Alpha).
    - Add new button compare properties.
    - Add new button help.
    - Improvement UI compare element parameter.
    - Supported Sort by color when click to color button.
    - Improvement performance compare element parameter.
    - Fix min size windows popup.
    - Fix some small bug.
    - Fix round corner windows 11 problem.
- 2023-09-04 **1.5.0**
    - Compare Element Parameter Support All Parameter.
    - Add 4 color for compare element parameter.
    - Add new column compare element parameter.
    - Add compare result similarity score.
    - Support export all information bip checker compare element parameter.
    - Fix issue compare with color highlight missing with some parameter.
    - Enhancement UI compare parameter.
- 2023-09-04 **1.4.9**
    - Add new tool Compare Element Parameter.
- 2023-09-03 **1.4.8**
    - Add support bip checker multiple element.
    - Add more column parameter bip checker (ElementId,Category).
    - Fix installer assembly bip checker overrider update.
    - Improvement UI bip checker.
- 2023-06-02 **1.4.7**
    - Add more column parameter unit bip checker.
- 2023-06-02 **1.4.6**
    - Supported Button Quick Bip Checker - [RevitElementBipChecker](https://github.com/chuongmep/RevitElementBipChecker)
- 2023-04-20 **1.4.5**
    - Support open add-in manager from modify tab in Revit
- 2023-03-12 **1.4.4**
    - Fix load assembly with specified name is already
      loaded [#45](https://github.com/chuongmep/RevitAddInManager/pull/45)
- 2023-02-14 **1.4.3**
    - Fix Window Stuck Off Screen [#43](https://github.com/chuongmep/RevitAddInManager/issues/43)
- 2022-12-23 **1.4.2**
    - Keep save window position when close and open again (useful for multi monitor).
- 2022-12-18 **1.4.1**
    - Fix problem relative path \\\\mac... in MACOS [#40](https://github.com/chuongmep/RevitAddInManager/issues/40)
- 2022-11-26 **1.4.0**
    - Support Revit 2024 Preview Release
- 2022-11-8 **1.3.9**
    - Fix problem with some user install msi with PC company have
      policy [#41](https://github.com/chuongmep/RevitAddInManager/issues/41).
- 2022-10-16 **1.3.8**
    - Support save keep last window size change.
- 2022-09-11 **1.3.7**
    - Show detail version installed in title
    - Improve drag window only in DataGrid tab Command and tab App
- 2022-08-29 **1.3.6**
    - Fix problem` Trace/Debug not show result when one Add-in Manager in first
      time [#34](https://github.com/chuongmep/RevitAddInManager/issues/34)
- 2022-06-30 **1.3.5**`
    - Allow Use Add-in Manager without loading a model [#28](https://github.com/chuongmep/RevitAddInManager/issues/28)
- 2022-06-23 **1.3.4**
    - Fix DockPanel startup with some version lower 2019 [#27](https://github.com/chuongmep/RevitAddInManager/issues/27)
    - Revert use search by index of.
- 2022-06-15 **1.3.3**
    - Fix issue color with dark theme [#26](https://github.com/chuongmep/RevitAddInManager/issues/26)
- 2022-06-11 **1.3.2**
    - Allow drag window use mouse every where position
- 2022-06-10 **1.3.1**
    - Improve search bar Style
    - Improve TreeViewItem show tree same window form
    - Improve speed up search item by Knuth-Morris-Pratt (KMP) Algorithm.
- 2022-06-10 **1.3.0**
    - Add Mutiple Theme Support : Default, Dark, Light.
    - Use <kbd>Alt +T</kbd> to change theme.
    - Fix some small bug.
    - Improve search bar margin.
- 2022-06-02 **1.2.9**
    - Support event ArrowKeyDown and ArrowKeyUp to move between items search and
      TreeView [#25](https://github.com/chuongmep/RevitAddInManager/pull/25)
    - Support press key Esc from keyboard to close Form.
- 2022-05-15 **1.2.8**
    - Support copy Content Debug/Trace Output Right click or Crt + C
- 2022-05-11 **1.2.7**
    - Fix small bug use trace/debug same time in
      logcontrol [#24](https://github.com/chuongmep/RevitAddInManager/issues/24)
- 2022-05-10 **1.2.6**
    - Add tab support show result trace/debug
    - Add DockPanel Support Show/Hide Debug/Trace result.
    - Support use Trace.Write() output in RevitAddinManager and DockPanel **Debug/Trace Output**
    - Support use Trace.WriteLine() output in RevitAddinManager and DockPanel **Debug/Trace Output**
    - Support use Debug.WriteLine() output in RevitAddinManager and DockPanel **Debug/Trace Output**
    - Support use Debug.WriteLine() output in RevitAddinManager and DockPanel **Debug/Trace Output**
    - Support show difference color type : Add,Modify,Warning,Error,Delete.
- 2022-05-07 **1.2.5**
    - Support show detail assembly referenced to load manually.
- 2022-04-21 **1.2.4**
    - Support `Crt + Mouse Wheel` to zoomIn ZoomOut command
- 2022-03-07 **1.2.3**
    - Fix problem when user double click mutiple to open AddinManager.
- 2022-03-05 **1.2.2**
    - Fix problem with close document still opening
      addin [#21](https://github.com/chuongmep/RevitAddInManager/issues/21)
- 2022-03-04 **1.2.1**
    - Add feature save as addin to new folder
- 2022-02-25 **1.2.0**
    - Change position button **Run**
    - Improved some small features
- 2022-02-11 **1.1.9**
    - Support F1 go to help from keyboard
    - Support F5 to reload all commands and application or reload addin choosing in process
- 2022-02-11 **1.1.8**
    - Fix reload when user want reload all
    - Support user press delete command addin or application addin from keyboard
- 2022-02-11 **1.1.7**
    - Support reload command by menu context right click
- 2022-02-03 **1.1.6**
    - Fix main process close form with Revit program

- 2022-02-03 **1.1.5**
    - Add feature right click context menu red color selected

- 2022-02-03 **1.1.4**
    - Fix Command Failure for External Command

- 2022-02-02 **1.1.3**
    - Fix load duplicate assembly

- 2022-02-02 **1.1.2**
    - Fix Open readonly mistake with faceless

- 2022-01-31 **1.1.1**
    - Support inteactive with revit project when openning Addin Manager

- 2022-01-31 **1.1.0**
    - Fix The component does not have a resource identified by the URI from assembly
      loader [#7](https://github.com/chuongmep/RevitAddInManager/issues/7)

- 2022-01-31 **1.0.9**
    - Fix problem menucontext show assembly location
    - Fix The component does not have a resource identified by the
      URI [#7](https://github.com/chuongmep/RevitAddInManager/issues/7)

- 2022-01-30 **1.0.8**
    - Support Revit 2023
    - Improve Some code base

- 2022-01-27 **1.0.7**
    - Improve multiple version config support from R14-R23
    - Improve INotifyPropertyChanged Correct use with Commands and Applications
    - Improve cached objects
    - Refactor code base project : Change to style var, remove redundant this
    - File-scoped namespace
    - Change format build release new version

- 2022-01-25 **2022.0.0.7**
    - Improve startup tab toggle add-in
    - Fix addin is readonly request admin to change
    - Stable version

- 2022-01-25 **2022.0.0.6**
    - Improve selection tools
    - Fix some small bug

- 2022-01-25 **2022.0.0.5**
    - Fix assembly not load [#5](https://github.com/chuongmep/RevitAddInManager/issues/5)
    - Fix uri load form WPF [#6](https://github.com/chuongmep/RevitAddInManager/issues/6)
    - Fix conflict between Add-in Manager use winform olf with Assembly Add-in manager WPF
    - Rename NameSpace repair for support **Autocad** and **Naviswork**

- 2022-01-25 **2022.0.0.4**
    - Fix input Description slow
    - Add Feature Right Context Menu Open Location Assembly

- 2022-01-24 **2022.0.0.3**
    - Update Remind Description
      Selected [#b62a4b8](https://github.com/chuongmep/RevitAddInManager/commit/04163a0ac977341a0d24df8dca99417325d2c0b6)
    - Fix Conflict button run with event exit form
      main [#8a3a5c3](https://github.com/chuongmep/RevitAddInManager/commit/8a3a5c330bdd20f81384c5d679d759d25c69c9bf)

- 2022-01-23 **2022.0.0.2**
    - Remove gdi32 import in image source
    - Fix Selected Tab null
    - Fix load app and remove app
    - Remove Complex Interactivity And Interactions Library WPF

- 2022-01-22 **2022.0.0.1**
    - Support installer all in one from Revit 2014 to Revit 2023
    - Fix ReinstallMode installer, see
      at [reinstallmode](https://docs.microsoft.com/en-us/windows/win32/msi/reinstallmode)

- 2022-01-21 **2022.0.0.0**
    - Rollback Product
    - Update Version
    - Remove VersionInformation
    - Add Package `Revit_All_Main_Versions_API_x64` Version 2015
    - Add `AssemblyInformationalVersion` on `AssemblyInfo.cs`
    - Add Build
    - First Release
    - Rollback Product
    - Update Version
    - Remove VersionInformation
    - Add Package Revit_All_Main_Versions_API_x64 Version 2015
    - Add AssemblyInformationalVersion on AssemblyInfo.cs
    - Add Build
    - First Release

