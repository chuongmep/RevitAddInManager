using System;
using System.Linq;
using System.Xml.Linq;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class AddinManifestTests
    {
        private static XElement CreateAddin(RevitAddinType type, string fullClassName = "Contoso.Revit.MyCommand")
        {
            var document = AddinManifest.Create("Contoso.Revit", type, fullClassName, "Contoso Ltd", "My add-in");
            return document.Root.Element("AddIn");
        }

        [Fact]
        public void Create_UsesRevitAddInsRootWithoutNamespace()
        {
            var document = AddinManifest.Create("Contoso.Revit", RevitAddinType.Command, "Contoso.Revit.MyCommand", "Contoso Ltd", "d");

            Assert.Equal("RevitAddIns", document.Root.Name.LocalName);
            // Revit's own manifests carry no default namespace; adding one makes the file unreadable.
            Assert.Equal(XNamespace.None, document.Root.Name.Namespace);
        }

        [Fact]
        public void Create_ForCommand_WritesTextAndVisibilityMode()
        {
            var addin = CreateAddin(RevitAddinType.Command);

            Assert.Equal("Command", addin.Attribute("Type").Value);
            Assert.Equal("MyCommand", addin.Element("Text").Value);
            Assert.Equal("My add-in", addin.Element("Description").Value);
            Assert.Equal("AlwaysVisible", addin.Element("VisibilityMode").Value);
            Assert.Null(addin.Element("Name"));
        }

        [Fact]
        public void Create_ForApplication_WritesNameAndNoCommandOnlyElements()
        {
            var addin = CreateAddin(RevitAddinType.Application, "Contoso.Revit.MyApp");

            Assert.Equal("Application", addin.Attribute("Type").Value);
            Assert.Equal("MyApp", addin.Element("Name").Value);
            Assert.Null(addin.Element("Text"));
            Assert.Null(addin.Element("VisibilityMode"));
        }

        [Fact]
        public void Create_PointsAtTheAssemblySubfolderNotAFullPath()
        {
            var addin = CreateAddin(RevitAddinType.Command);

            // The whole source folder is packaged into a subfolder named after the assembly, so the
            // manifest has to reach into it rather than sitting next to a loose dll.
            Assert.Equal(@"Contoso.Revit\Contoso.Revit.dll", addin.Element("Assembly").Value);
            Assert.Equal("Contoso.Revit.MyCommand", addin.Element("FullClassName").Value);
        }

        [Fact]
        public void AssemblyRelativePath_IsResolvedFromTheAddinFile()
        {
            Assert.Equal(@"MyAddin\MyAddin.dll", AddinManifest.AssemblyRelativePath("MyAddin"));
        }

        [Fact]
        public void Create_DoesNotClaimAutodeskVendorId()
        {
            var addin = CreateAddin(RevitAddinType.Command);

            Assert.NotEqual("ADSK", addin.Element("VendorId").Value);
            Assert.Equal("CONTOSOL", addin.Element("VendorId").Value);
            Assert.Equal("Contoso Ltd", addin.Element("VendorDescription").Value);
        }

        [Fact]
        public void Create_ClientIdIsStableAcrossRuns()
        {
            var first = CreateAddin(RevitAddinType.Command).Element("ClientId").Value;
            var second = CreateAddin(RevitAddinType.Command).Element("ClientId").Value;

            Assert.Equal(first, second);
            Assert.NotEqual(Guid.Empty, Guid.Parse(first));
        }

        [Fact]
        public void Create_ClientIdDiffersPerClass()
        {
            var first = CreateAddin(RevitAddinType.Command, "Contoso.Revit.CommandA").Element("ClientId").Value;
            var second = CreateAddin(RevitAddinType.Command, "Contoso.Revit.CommandB").Element("ClientId").Value;

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void CreateDeterministicGuid_ProducesRfc4122Shape()
        {
            var guid = AddinManifest.CreateDeterministicGuid("Contoso.Revit");
            var bytes = guid.ToByteArray();

            Assert.Equal(0x50, bytes[7] & 0xF0);
            Assert.Equal(0x80, bytes[8] & 0xC0);
        }

        [Fact]
        public void Create_WithEmptyClassName_FallsBackToAssemblyCommand()
        {
            var document = AddinManifest.Create("Contoso.Revit", RevitAddinType.Command, "", "Contoso Ltd", "d");

            Assert.Equal("Contoso.Revit.Command", document.Root.Element("AddIn").Element("FullClassName").Value);
        }

        [Fact]
        public void Create_ElementOrderMatchesRevitSchema()
        {
            var addin = CreateAddin(RevitAddinType.Command);
            var names = addin.Elements().Select(e => e.Name.LocalName).ToArray();

            Assert.Equal(
                new[] { "Text", "Description", "Assembly", "FullClassName", "ClientId", "VendorId", "VendorDescription", "VisibilityMode" },
                names);
        }
    }
}
