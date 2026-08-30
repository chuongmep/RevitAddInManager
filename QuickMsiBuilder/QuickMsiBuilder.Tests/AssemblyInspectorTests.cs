using System;
using System.IO;
using System.Linq;
using System.Reflection;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class AssemblyInspectorTests
    {
        // This test assembly carries the fixtures in RevitAddinFixtures.cs.
        private static string SelfPath
        {
            get { return new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath; }
        }

        private static AssemblyDetails InspectSelf()
        {
            return AssemblyInspector.Inspect(SelfPath);
        }

        [Fact]
        public void Inspect_FindsCommandsThatInheritTheInterfaceFromABaseClass()
        {
            var names = InspectSelf().Candidates.Select(c => c.FullClassName).ToList();

            // The interface is on BaseCommand; every derived command still has to be found.
            Assert.Contains("QuickMsiBuilder.Tests.Fixtures.Inherited.DerivedCommand", names);
            Assert.Contains("QuickMsiBuilder.Tests.Fixtures.Inherited.DeeplyDerivedCommand", names);
            Assert.Contains("QuickMsiBuilder.Tests.Fixtures.Inherited.DerivedApplication", names);
            Assert.DoesNotContain("QuickMsiBuilder.Tests.Fixtures.Inherited.BaseCommand", names);
        }

        [Fact]
        public void Inspect_FindsCommandsThroughAnInheritedInterface()
        {
            var candidates = InspectSelf().Candidates;
            var match = candidates.Single(c => c.FullClassName.EndsWith("CommandViaInterfaceChain"));

            Assert.Equal(RevitAddinType.Command, match.AddinType);
        }

        [Fact]
        public void Inspect_FindsPublicCommandsAndApplications()
        {
            var names = InspectSelf().Candidates.Select(c => c.FullClassName).ToList();

            Assert.Contains("QuickMsiBuilder.Tests.Fixtures.SampleCommand", names);
            Assert.Contains("QuickMsiBuilder.Tests.Fixtures.AnotherCommand", names);
            Assert.Contains("QuickMsiBuilder.Tests.Fixtures.SampleApplication", names);
        }

        [Fact]
        public void Inspect_FindsNestedEntryPointsWithTheirDeclaringType()
        {
            var names = InspectSelf().Candidates.Select(c => c.FullClassName).ToList();

            // '+' is what Revit's Assembly.GetType expects, not Cecil's '/'.
            Assert.Contains("QuickMsiBuilder.Tests.Fixtures.Outer+NestedCommand", names);
            Assert.DoesNotContain(names, name => name.Contains("/"));
        }

        [Fact]
        public void Inspect_SkipsAbstractInternalAndUnrelatedTypes()
        {
            var names = InspectSelf().Candidates.Select(c => c.FullClassName).ToList();

            Assert.DoesNotContain("QuickMsiBuilder.Tests.Fixtures.AbstractCommand", names);
            Assert.DoesNotContain("QuickMsiBuilder.Tests.Fixtures.InternalCommand", names);
            Assert.DoesNotContain("QuickMsiBuilder.Tests.Fixtures.NotAnAddin", names);
        }

        [Fact]
        public void Inspect_TagsEachCandidateWithItsAddinType()
        {
            var candidates = InspectSelf().Candidates;

            Assert.Equal(RevitAddinType.Command,
                candidates.Single(c => c.FullClassName.EndsWith("SampleCommand")).AddinType);
            Assert.Equal(RevitAddinType.Application,
                candidates.Single(c => c.FullClassName.EndsWith("SampleApplication")).AddinType);
        }

        [Fact]
        public void Inspect_ListsCommandsBeforeApplications()
        {
            var candidates = InspectSelf().Candidates;
            var lastCommand = candidates.FindLastIndex(c => c.AddinType == RevitAddinType.Command);
            var firstApplication = candidates.FindIndex(c => c.AddinType == RevitAddinType.Application);

            Assert.True(lastCommand < firstApplication);
        }

        [Fact]
        public void Inspect_ReadsTheAssemblyVersion()
        {
            var expected = Assembly.GetExecutingAssembly().GetName().Version;

            Assert.Equal(
                string.Format("{0}.{1}.{2}", expected.Major, expected.Minor, expected.Build),
                InspectSelf().Version);
        }

        [Fact]
        public void Inspect_IgnoresAnAssemblyVersionThatIsNotMsiCompatible()
        {
            // Revit add-ins routinely stamp the Revit year as their assembly version. 2027 exceeds
            // the MSI major field limit of 255, so it must not reach the package.
            var path = WriteAssembly("Contoso.RevitVersioned", new Version(2027, 0, 0));
            try
            {
                Assert.Null(AssemblyInspector.Inspect(path).Version);

                MsiBuildOptions options;
                string error;
                Assert.True(MsiBuildOptions.TryParse(new[] { path }, out options, out error));
                Assert.Equal("1.0.0", options.Version);
            }
            finally
            {
                try { Directory.Delete(Path.GetDirectoryName(path), true); } catch { /* best effort */ }
            }
        }

        [Fact]
        public void Inspect_KeepsAnMsiCompatibleAssemblyVersion()
        {
            var path = WriteAssembly("Contoso.Normal", new Version(3, 2, 1));
            try
            {
                Assert.Equal("3.2.1", AssemblyInspector.Inspect(path).Version);
            }
            finally
            {
                try { Directory.Delete(Path.GetDirectoryName(path), true); } catch { /* best effort */ }
            }
        }

        private static string WriteAssembly(string name, Version version)
        {
            var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, name + ".dll");

            using (var assembly = Mono.Cecil.AssemblyDefinition.CreateAssembly(
                       new Mono.Cecil.AssemblyNameDefinition(name, version), name, Mono.Cecil.ModuleKind.Dll))
            {
                assembly.Write(path);
            }

            return path;
        }

        [Fact]
        public void Inspect_OnMissingFile_ReturnsEmptyDetails()
        {
            var details = AssemblyInspector.Inspect(Path.Combine(Path.GetTempPath(), "does-not-exist.dll"));

            Assert.NotNull(details);
            Assert.Empty(details.Candidates);
            Assert.Null(details.Version);
        }

        [Fact]
        public void Inspect_OnNonManagedFile_DoesNotThrow()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".dll");
            File.WriteAllText(path, "definitely not an assembly");
            try
            {
                Assert.Empty(AssemblyInspector.Inspect(path).Candidates);
            }
            finally
            {
                try { File.Delete(path); } catch { /* best effort */ }
            }
        }

        [Fact]
        public void TryParse_WithoutVersion_UsesTheAssemblyVersion()
        {
            MsiBuildOptions options;
            string error;

            Assert.True(MsiBuildOptions.TryParse(new[] { SelfPath }, out options, out error));

            var expected = Assembly.GetExecutingAssembly().GetName().Version;
            Assert.Equal(
                string.Format("{0}.{1}.{2}", expected.Major, expected.Minor, expected.Build),
                options.Version);
        }

        [Fact]
        public void TryParse_WithAnExplicitVersion_KeepsIt()
        {
            MsiBuildOptions options;
            string error;

            Assert.True(MsiBuildOptions.TryParse(new[] { SelfPath, "9.8.7" }, out options, out error));
            Assert.Equal("9.8.7", options.Version);
        }

        [Fact]
        public void TryParse_WithoutClassName_UsesTheDetectedEntryPoint()
        {
            MsiBuildOptions options;
            string error;

            Assert.True(MsiBuildOptions.TryParse(new[] { SelfPath }, out options, out error));
            Assert.StartsWith("QuickMsiBuilder.Tests.Fixtures.", options.EntriesText);
        }

        [Fact]
        public void TryParse_WithoutClassName_OnAnAssemblyWithNoEntryPoint_FallsBackToAGuess()
        {
            var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "Contoso.Plain.dll");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "not an assembly");
            try
            {
                MsiBuildOptions options;
                string error;

                Assert.True(MsiBuildOptions.TryParse(new[] { path }, out options, out error));
                Assert.Equal("Contoso.Plain.Command", options.EntriesText);
            }
            finally
            {
                try { Directory.Delete(Path.GetDirectoryName(path), true); } catch { /* best effort */ }
            }
        }
    }
}
