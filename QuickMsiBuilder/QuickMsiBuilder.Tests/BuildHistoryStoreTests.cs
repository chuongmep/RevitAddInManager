using System;
using System.IO;
using System.Linq;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class BuildHistoryStoreTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _historyPath;
        private readonly BuildHistoryStore _store;

        public BuildHistoryStoreTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "QuickMsiBuilderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _historyPath = Path.Combine(_tempDir, "nested", "build-history.xml");
            _store = new BuildHistoryStore(_historyPath);
        }

        public void Dispose()
        {
            try { Directory.Delete(_tempDir, true); } catch { /* best effort */ }
        }

        private MsiBuildOptions CreateOptions(string assemblyName, string version, string revitYear = "2024")
        {
            var dllPath = Path.Combine(_tempDir, assemblyName + ".dll");
            if (!File.Exists(dllPath)) File.WriteAllText(dllPath, "not a real assembly");

            MsiBuildOptions options;
            string error;
            var args = new[] { dllPath, version, "Contoso Ltd", "My add-in", "", "", revitYear, "Contoso.MyCommand", "Command" };
            Assert.True(MsiBuildOptions.TryParse(args, out options, out error), error);
            return options;
        }

        [Fact]
        public void Load_WhenFileMissing_ReturnsEmptyHistory()
        {
            var history = _store.Load();

            Assert.NotNull(history);
            Assert.Empty(history.Entries);
        }

        [Fact]
        public void GetFor_WithUnknownAssembly_ReturnsEmpty()
        {
            _store.Record(CreateOptions("Contoso", "1.0.0"), "out.msi", DateTime.UtcNow);

            Assert.Empty(_store.GetFor(Path.Combine(_tempDir, "Other.dll")));
            Assert.Empty(_store.GetFor(null));
        }

        [Fact]
        public void Record_CreatesMissingDirectoryAndRoundTrips()
        {
            var options = CreateOptions("Contoso", "1.2.3", "2026");
            var builtUtc = new DateTime(2026, 8, 15, 10, 30, 0, DateTimeKind.Utc);

            _store.Record(options, @"C:\out\Contoso-1.2.3.msi", builtUtc);

            Assert.True(File.Exists(_historyPath));

            var entry = Assert.Single(new BuildHistoryStore(_historyPath).GetFor(options.DllPath));
            Assert.Equal("1.2.3", entry.Version);
            Assert.Equal("Contoso Ltd", entry.Author);
            Assert.Equal("My add-in", entry.Description);
            Assert.Equal("Contoso.MyCommand", entry.FullClassName);
            Assert.Equal("Command", entry.AddinType);
            Assert.Equal("2026", entry.RevitYears);
            Assert.Equal(@"C:\out\Contoso-1.2.3.msi", entry.MsiPath);
            Assert.Equal(builtUtc, entry.BuiltUtc);
        }

        [Fact]
        public void Record_PutsNewestFirst()
        {
            var options = CreateOptions("Contoso", "1.0.0");
            _store.Record(options, "a.msi", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            _store.Record(CreateOptions("Contoso", "2.0.0"), "b.msi", new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc));

            var entries = _store.GetFor(options.DllPath);

            Assert.Equal(new[] { "2.0.0", "1.0.0" }, entries.Select(e => e.Version).ToArray());
        }

        [Fact]
        public void GetFor_IsCaseInsensitiveOnPath()
        {
            var options = CreateOptions("Contoso", "1.0.0");
            _store.Record(options, "a.msi", DateTime.UtcNow);

            Assert.Single(_store.GetFor(options.DllPath.ToUpperInvariant()));
        }

        [Fact]
        public void GetFor_MatchesPathsWrittenWithForwardSlashes()
        {
            var options = CreateOptions("Contoso", "1.0.0");
            _store.Record(options, "a.msi", DateTime.UtcNow);

            // This is the shape the UI receives on its command line.
            Assert.Single(_store.GetFor(options.DllPath.Replace('\\', '/')));
        }

        [Fact]
        public void GetFor_MatchesPathsContainingRelativeSegments()
        {
            var options = CreateOptions("Contoso", "1.0.0");
            _store.Record(options, "a.msi", DateTime.UtcNow);

            var directory = Path.GetDirectoryName(options.DllPath);
            var indirect = Path.Combine(directory, ".", "Contoso.dll");

            Assert.Single(_store.GetFor(indirect));
        }

        [Fact]
        public void Record_KeepsHistoryOfEachAssemblySeparate()
        {
            var first = CreateOptions("Contoso", "1.0.0");
            var second = CreateOptions("Fabrikam", "9.9.9");
            _store.Record(first, "a.msi", DateTime.UtcNow);
            _store.Record(second, "b.msi", DateTime.UtcNow);

            Assert.Equal("1.0.0", Assert.Single(_store.GetFor(first.DllPath)).Version);
            Assert.Equal("9.9.9", Assert.Single(_store.GetFor(second.DllPath)).Version);
        }

        [Fact]
        public void Record_TrimsToMaxEntriesPerAssembly()
        {
            var options = CreateOptions("Contoso", "1.0.0");
            for (var i = 0; i < BuildHistoryStore.MaxEntriesPerAssembly + 5; i++)
            {
                _store.Record(CreateOptions("Contoso", "1.0." + i), "out.msi", DateTime.UtcNow);
            }

            var entries = _store.GetFor(options.DllPath);

            Assert.Equal(BuildHistoryStore.MaxEntriesPerAssembly, entries.Count);
            // The oldest builds are the ones dropped.
            Assert.Equal("1.0." + (BuildHistoryStore.MaxEntriesPerAssembly + 4), entries[0].Version);
        }

        [Fact]
        public void Load_WithCorruptFile_ReturnsEmptyInsteadOfThrowing()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_historyPath));
            File.WriteAllText(_historyPath, "<not valid xml");

            Assert.Empty(_store.Load().Entries);
        }

        [Fact]
        public void Record_OverCorruptFile_StillPersists()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_historyPath));
            File.WriteAllText(_historyPath, "<not valid xml");

            var options = CreateOptions("Contoso", "1.0.0");
            _store.Record(options, "a.msi", DateTime.UtcNow);

            Assert.Single(_store.GetFor(options.DllPath));
        }

        [Fact]
        public void DisplayName_ShowsVersionYearAndDate()
        {
            var entry = new BuildHistoryEntry
            {
                Version = "1.2.3",
                RevitYears = "2026",
                BuiltUtc = new DateTime(2026, 8, 15, 10, 30, 0, DateTimeKind.Utc)
            };

            Assert.StartsWith("1.2.3 - Revit 2026 - ", entry.DisplayName);
        }

        [Fact]
        public void DefaultFilePath_LivesUnderApplicationData()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            Assert.StartsWith(appData, BuildHistoryStore.DefaultFilePath);
            Assert.EndsWith("build-history.xml", BuildHistoryStore.DefaultFilePath);
        }
    }
}
