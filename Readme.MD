
# Revit Add-in Manager
![Revit API](https://img.shields.io/badge/Revit%20API%202023-blue.svg) ![Platform](https://img.shields.io/badge/platform-Windows-lightgray.svg) [![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

![ReSharper](https://img.shields.io/badge/ReSharper-2021.3.3-yellow) ![Rider](https://img.shields.io/badge/Rider-2021.3.3-yellow) ![Visual Studio 2022](https://img.shields.io/badge/Visual_Studio_2022_Preview_2.0-17.1.0-yellow) ![.NET Framework](https://img.shields.io/badge/.NET_6.0-yellow)

[![Publish](../../actions/workflows/Workflow.yml/badge.svg)](../../actions)
<a href="https://twitter.com/intent/follow?screen_name=chuongmep">
<img src="https://img.shields.io/twitter/follow/chuongmep?style=social&logo=twitter"
alt="follow on Twitter"></a>

## Summary

![](pic/Addin.png)

Usually, when developing and debugging an addin with RevitAPI, user has to close & re-open Revit each time he/she modifies the addin code and wants to re-compile. But with Add-In Manager, user can modify and run the addin directly without closing & re-opening Revit again and again.


The product will be expanded in the future in process to support mutiple platform : 

- [x] [Revit Add-In Manager](https://github.com/chuongmep/RevitAddInManager)

- [x] [Navis Add-In Manager](https://github.com/chuongmep/NavisAddInManager)

- [x] [Cad Add-In Manager](https://github.com/chuongmep/CadAddInManager)

- [x] More...


### Some feature include:

- Add-in tester(use Debug and Trace to test your add-in)
- Add-in manager
    - AddInManagerFaceless
    - AddInManagerManual
    - AddInManagerReadOnly
    - Application
- Startup manager
- **Debug / Trace** : Support Show result use [Debug](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.debug?view=net-6.0) or [Trace](https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.trace?view=net-6.0) to show result in Revit
- Quick create `.addin` and load to `Revit`
- Clear Cache saved in temp folder
- Quick remove an Add-in not used in working process
- No depend on any library from outside **RevitAPI**.
- A newbie can create new Add-in in a few minutes and easily explore Revit API.

![](pic/LearningCurve.png)

- <kbd>F5</kbd> - Load fresh plugin don't need load again.
- <kbd>F1</kbd> - Go link open source report some error,bug or feature request.
- <kbd>Delete</kbd> - Quick remove by use right click or use from keyboard.
- <kbd>Crt + MouseWheel</kbd> - Zoom in/out by use mouse wheel in command plugin.
## Add-In Manager

![](pic/AddinManager.png)

### Command

- [x] AddInManagerManual
- [x] AddInManagerFaceless
- [x] AddInManagerReadOnly

### Application

- [x] Preview
- [x] Save And Load Application .Addin

### Startup

- [x] Show info all add-in installed in computer include all Add-in from Autodesk.
- [x] Enable or Disable Add-in.

![](pic/AddinManagerStartup.png)

### Trace/Debug

- [x] Show/Hide result inside **Dockpanel Revit**.
- [x] Show Result history inside **Revit Addin Manager**.
- [x] Clear Result.
- [X] Show Result by color.

![](pic/debug-trace.png)


---

## Installation

Please follow last release at section [Release](https://github.com/chuongmep/RevitAddInManager/releases/latest)

---

## Guideline

- Quick Example to Execute a Command : 
``` cs
[Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Code Something 
            return Result.Succeeded;
        }
    }
```
- Quick Example to Debug or Trace result output :

```cs
[Transaction(TransactionMode.Manual)]
    public class TraceWriteLine : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Trace or Debug Something
            Debug.Write($"Message output");
            Debug.WriteLine($"Message output");
            Trace.Write($"Message output");
            Trace.WriteLine($"Message output");
            return Result.Succeeded;
        }
    }
```
You can visit to wiki and read document or access by this [link](https://github.com/chuongmep/RevitAddInManager/wiki).

Some topic common:
- [How to install Add In Manager](https://github.com/chuongmep/RevitAddInManager/wiki/How-to-install-AddinManager)
- [How to Uninstall Add In Manager](https://github.com/chuongmep/RevitAddInManager/wiki/How-to-Uninstall-AddinManager)
- [How To Use Add In Manager](https://github.com/chuongmep/RevitAddInManager/wiki/How-To-Use-Add-In-Manager)
- [Fix Problem With Installer AddinManager](https://github.com/chuongmep/RevitAddInManager/wiki/Fix-Problem-With-Installer-AddinManager)
- [How To Use Debug Trace Show Output](https://github.com/chuongmep/RevitAddInManager/wiki/How-To-Use-Debug-Trace-Show-Output)

---

## Author

Originally implemented by Autodesk with platform **Windows Forms** and now it maintain by [ChuongHo](https://github.com/chuongmep) with platform **WPF** and add more feature to fit with the progressive development of modern technology. His focus is to work smarter and achieve the most effective practices to enhance data utilization and digital collaboration among AEC parties.

---

## License

This sample is licensed under the terms of the [MIT License](http://opensource.org/licenses/MIT). Please see the [License](License.md) file for full details.

Credit to [icons8.com](https://icons8.com) for the Revit Add-in Manager icons.

© 2010 Autodesk, Inc.  All rights reserved. All use of this Software is subject to the terms and conditions of the Autodesk End User License Agreement accepted upon installation of this Software and/or packaged with the Software.

---

## Contribute

**Add-In Manager** is an open-source project and would be nothing without its community. You can make suggestions or track and submit bugs via Github [issues](https://docs.github.com/en/issues/tracking-your-work-with-issues/creating-an-issue). You can submit your own code to the **Add-In Manager** project via a Github [pull request](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests).

Many Thanks all contributors for this repository. Feel free to contribute!
Please refer to the [CONTRIBUTING](CONTRIBUTING.md) for details.

<a href = "https://github.com/chuongmep/RevitAddInManager/graphs/contributors">
  <img src = "https://contrib.rocks/image?repo=chuongmep/RevitAddInManager"/>
</a>

---

## Sponsors

![](pic/jetbrains.png)

Thanks for providing a free All product IDE for this project.

---

## Learning Resources

- [Revit Developer Center](https://www.autodesk.com/developer-network/platform-technologies/revit)
- [Revit SDK Sample](https://github.com/jeremytammik/RevitSdkSamples)
- [Revit API Docs](https://www.revitapidocs.com/)
- [The Building Coder](https://thebuildingcoder.typepad.com/)

## Open Resource Recommend

I suggest you some of these projects, hope you will like it.

- [RevitLookup](https://github.com/jeremytammik/RevitLookup)

- [RevitLookupWpf](https://github.com/weianweigan/RevitLookupWpf)

- [Revit.ScriptCS](https://github.com/sridharbaldava/Revit.ScriptCS)

- [Revit.TestRunner](https://github.com/geberit/Revit.TestRunner)

- [Revit.Async](https://github.com/KennanChan/Revit.Async)

- [pyRevit](https://github.com/eirannejad/pyRevit)

