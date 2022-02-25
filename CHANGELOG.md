# Changelog

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
  - Fix The component does not have a resource identified by the URI from assembly loader [#7](https://github.com/chuongmep/RevitAddInManager/issues/7)

- 2022-01-31 **1.0.9**
  - Fix problem menucontext show assembly location
  - Fix The component does not have a resource identified by the URI [#7](https://github.com/chuongmep/RevitAddInManager/issues/7)

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
  - Update Remind Description Selected [#b62a4b8](https://github.com/chuongmep/RevitAddInManager/commit/04163a0ac977341a0d24df8dca99417325d2c0b6)
  - Fix Conflict button run with event exit form main [#8a3a5c3](https://github.com/chuongmep/RevitAddInManager/commit/8a3a5c330bdd20f81384c5d679d759d25c69c9bf)

- 2022-01-23 **2022.0.0.2** 
  - Remove gdi32 import in image source
  - Fix Selected Tab null
  - Fix load app and remove app
  - Remove Complex Interactivity And Interactions Library WPF

- 2022-01-22 **2022.0.0.1** 
  - Support installer all in one from Revit 2014 to Revit 2023
  - Fix ReinstallMode installer, see at [reinstallmode](https://docs.microsoft.com/en-us/windows/win32/msi/reinstallmode)

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

