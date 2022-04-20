## Contributing

#### Contributions are more than welcome! Please work in the dev branch to do so:

- Create or update your own fork of AddinManager under your GitHub account.
- Checkout to the ``dev`` branch.
- In the dev branch, implement and test you changes specific to the feature.
- Build the project and make sure everything works.
- Create well-documented commits of your changes.
- Submit a pull request to the origin:dev branch.
#### Build

Debugging:

- Run **Debug Profile** in Visual Studio or **Run Configuration** in JetBrains Rider. The required files have been added. All project files will be automatically copied to the Revit plugins folder.

Creating a package:

- Open the terminal of your IDE.
- Install Nuke global tools `dotnet tool install Nuke.GlobalTool --global`.
- Run `nuke` command.
- The generated package will be in the **output** folder.

For more information on building, see the [**RevitTemplates**](https://github.com/Nice3point/RevitTemplates) Wiki page.

**Note:** The project currently supports the version Revit from **2014** to **2023**.

Please refer to the [CHANGELOG](CHANGELOG.md) for details.

---
#### Please avoid:

- Lots of unrelated changes in one commit.
- Modifying files that are not directly related to the feature you implement.
