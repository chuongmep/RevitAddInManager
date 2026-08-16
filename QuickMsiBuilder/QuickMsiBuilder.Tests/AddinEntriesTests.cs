using System.Collections.Generic;
using System.Linq;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class AddinEntriesTests
    {
        private static AssemblyDetails Details(params AddinCandidate[] candidates)
        {
            return new AssemblyDetails { Candidates = candidates.ToList() };
        }

        private static AddinCandidate Command(string name)
        {
            return new AddinCandidate(name, RevitAddinType.Command);
        }

        private static AddinCandidate Application(string name)
        {
            return new AddinCandidate(name, RevitAddinType.Application);
        }

        [Fact]
        public void ResolveEntries_ByDefault_TakesEveryCommand()
        {
            var details = Details(Command("A.One"), Command("A.Two"), Application("A.App"));

            var entries = MsiBuildOptions.ResolveEntries(null, RevitAddinType.Command, details, "A");

            Assert.Equal(new[] { "A.One", "A.Two" }, entries.Select(e => e.FullClassName).ToArray());
        }

        [Fact]
        public void ResolveEntries_WithOnlyApplications_TakesThemAll()
        {
            var details = Details(Application("A.App"), Application("A.Other"));

            var entries = MsiBuildOptions.ResolveEntries("", RevitAddinType.Command, details, "A");

            Assert.Equal(2, entries.Count);
            Assert.All(entries, entry => Assert.Equal(RevitAddinType.Application, entry.AddinType));
        }

        [Fact]
        public void ResolveEntries_WithNothingDeclared_FallsBackToAGuess()
        {
            var entries = MsiBuildOptions.ResolveEntries(null, RevitAddinType.Command, Details(), "Contoso");

            var only = Assert.Single(entries);
            Assert.Equal("Contoso.Command", only.FullClassName);
        }

        [Fact]
        public void ResolveEntries_WithNothingDeclaredAndApplicationRequested_GuessesAnApplication()
        {
            var entries = MsiBuildOptions.ResolveEntries(null, RevitAddinType.Application, Details(), "Contoso");

            Assert.Equal("Contoso.Application", Assert.Single(entries).FullClassName);
        }

        [Fact]
        public void ResolveEntries_WithAnExplicitList_KeepsThatSelection()
        {
            var details = Details(Command("A.One"), Command("A.Two"), Application("A.App"));

            var entries = MsiBuildOptions.ResolveEntries("A.Two, A.App", RevitAddinType.Command, details, "A");

            Assert.Equal(new[] { "A.Two", "A.App" }, entries.Select(e => e.FullClassName).ToArray());
            // The type comes from the assembly, not from the fallback.
            Assert.Equal(RevitAddinType.Application, entries[1].AddinType);
        }

        [Fact]
        public void ResolveEntries_WithAnUnknownName_UsesTheFallbackType()
        {
            var entries = MsiBuildOptions.ResolveEntries(
                "A.Typed", RevitAddinType.Application, Details(Command("A.One")), "A");

            var only = Assert.Single(entries);
            Assert.Equal("A.Typed", only.FullClassName);
            Assert.Equal(RevitAddinType.Application, only.AddinType);
        }

        [Fact]
        public void Create_WritesOneAddInPerEntry()
        {
            var entries = new List<AddinCandidate> { Command("A.One"), Command("A.Two"), Application("A.App") };

            var addins = AddinManifest.Create("A", entries, "Contoso", "desc").Root.Elements("AddIn").ToList();

            Assert.Equal(3, addins.Count);
            Assert.Equal(new[] { "A.One", "A.Two", "A.App" },
                addins.Select(a => a.Element("FullClassName").Value).ToArray());
            Assert.Equal(new[] { "Command", "Command", "Application" },
                addins.Select(a => a.Attribute("Type").Value).ToArray());
        }

        [Fact]
        public void Create_GivesEachEntryItsOwnClientId()
        {
            var entries = new List<AddinCandidate> { Command("A.One"), Command("A.Two") };

            var ids = AddinManifest.Create("A", entries, "Contoso", "desc")
                .Root.Elements("AddIn")
                .Select(a => a.Element("ClientId").Value)
                .ToList();

            Assert.Equal(ids.Count, ids.Distinct().Count());
        }

        [Fact]
        public void Create_WithNoEntries_StillProducesOneAddIn()
        {
            var addins = AddinManifest.Create("A", new List<AddinCandidate>(), "Contoso", "desc")
                .Root.Elements("AddIn").ToList();

            Assert.Equal("A.Command", Assert.Single(addins).Element("FullClassName").Value);
        }

        [Fact]
        public void Create_SkipsEntriesWithoutAClassName()
        {
            var entries = new List<AddinCandidate> { Command("A.One"), Command(""), null };

            var addins = AddinManifest.Create("A", entries, "Contoso", "desc").Root.Elements("AddIn").ToList();

            Assert.Equal("A.One", Assert.Single(addins).Element("FullClassName").Value);
        }
    }
}
