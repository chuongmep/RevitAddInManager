using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.Revit.Attributes;

[Transaction(TransactionMode.Manual)]
public class ExecuteDynamicCodeCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        // The code to be executed dynamically as a string
        string code = @"
        using Autodesk.Revit.UI;
        using Autodesk.Revit.DB;
        using System.Linq;
        using System.Collections.Generic;

        public class DynamicWallCounter
        {
            public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
            {
                Document doc = commandData.Application.ActiveUIDocument.Document;

                // Get all walls
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(Wall));
                var walls = collector.ToElements().ToList();

                // Display the wall count in the message
                message = ""Total number of walls: "" + walls.Count.ToString();
                TaskDialog.Show(""Wall Counter"", message);
                return Result.Succeeded;
            }
        }";

        // Compile the code using Roslyn
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        // Get currently loaded assemblies in Revit (including Revit API)
        var references = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName.Contains("Autodesk.Revit"))
            .Select(r => MetadataReference.CreateFromFile(r.Location))
            .ToList();

        // Add necessary .NET runtime assemblies
        references.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location)); // System.dll
        references.Add(MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location)); // System.Core.dll
        references.Add(MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.TaskAwaiter).Assembly.Location)); // System.Runtime.dll
        references.Add(MetadataReference.CreateFromFile(typeof(System.Runtime.ExceptionServices.ExceptionDispatchInfo).Assembly.Location)); // System.Runtime
        references.Add(MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location)); // System.Runtime

        // Add System.Runtime explicitly
        string systemRuntimePath = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.13\System.Runtime.dll";// Modify path if necessary
        string systemCollections = @"C:\Program Files\dotnet\shared\Microsoft.NETCore.App\8.0.13\System.Collections.dll";// Modify path if necessary
        references.Add(MetadataReference.CreateFromFile(systemRuntimePath));
        references.Add(MetadataReference.CreateFromFile(systemCollections));
        

        // Add Revit API assemblies explicitly
        string revitApiPath = @"C:\Program Files\Autodesk\Revit 2026\RevitAPI.dll"; // Modify the path if needed
        string revitUiPath = @"C:\Program Files\Autodesk\Revit 2026\RevitAPIUI.dll"; // Modify the path if needed

        references.Add(MetadataReference.CreateFromFile(revitApiPath));
        references.Add(MetadataReference.CreateFromFile(revitUiPath));

        // Create the compilation
        var compilation = CSharpCompilation.Create("DynamicWallCounterAssembly")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(references)
            .AddSyntaxTrees(syntaxTree);

        // Emit the compiled assembly to a memory stream
        using (var memoryStream = new MemoryStream())
        {
            var result = compilation.Emit(memoryStream);

            if (!result.Success)
            {
                message = "Compilation failed:\n" + string.Join("\n", result.Diagnostics.Select(d => d.ToString()));
                return Result.Failed;
            }

            // Load the compiled assembly from the memory stream
            memoryStream.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(memoryStream.ToArray());

            // Create an instance of the class
            object instance = assembly.CreateInstance("DynamicWallCounter");

            if (instance == null)
            {
                message = "Failed to create an instance of the class";
                return Result.Failed;
            }

            // Invoke the Execute method dynamically
            var executeMethod = instance.GetType().GetMethod("Execute");
            if (executeMethod == null)
            {
                message = "Failed to find the 'Execute' method in the dynamic code.";
                return Result.Failed;
            }

            try
            {
                executeMethod.Invoke(instance, new object[] { commandData, message, elements });
            }
            catch (Exception ex)
            {
                message = ex.ToString();
                return Result.Failed;
            }
        }

        return Result.Succeeded;
    }
}
