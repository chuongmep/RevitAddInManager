using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace QuickMsiBuilder.CLI
{
    /// <summary>
    /// Persists what the user typed for each assembly so the next release starts from the previous
    /// one instead of from blank defaults. Written by the CLI after a successful build and read back
    /// by the UI. Every failure is swallowed: history is a convenience, never a reason to fail a build.
    /// </summary>
    public class BuildHistoryStore
    {
        public const int MaxEntriesPerAssembly = 10;
        public const int MaxEntries = 200;

        private static readonly XmlSerializer Serializer = new XmlSerializer(typeof(BuildHistory));

        private readonly string _filePath;

        public BuildHistoryStore() : this(DefaultFilePath)
        {
        }

        public BuildHistoryStore(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            _filePath = filePath;
        }

        public string FilePath
        {
            get { return _filePath; }
        }

        public static string DefaultFilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RevitAddinManager", "QuickMsiBuilder", "build-history.xml");
            }
        }

        public BuildHistory Load()
        {
            try
            {
                if (!File.Exists(_filePath)) return new BuildHistory();
                using (var stream = File.OpenRead(_filePath))
                {
                    var history = (BuildHistory)Serializer.Deserialize(stream);
                    if (history == null) return new BuildHistory();
                    if (history.Entries == null) history.Entries = new List<BuildHistoryEntry>();
                    return history;
                }
            }
            catch
            {
                // A corrupt or partially written file must not break the builder.
                return new BuildHistory();
            }
        }

        public bool Save(BuildHistory history)
        {
            if (history == null) throw new ArgumentNullException("history");

            try
            {
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

                // Write to a temp file first so a crash mid-write cannot destroy existing history.
                var tempPath = _filePath + ".tmp";
                using (var stream = File.Create(tempPath))
                {
                    Serializer.Serialize(stream, history);
                }

                if (File.Exists(_filePath)) File.Delete(_filePath);
                File.Move(tempPath, _filePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a release to the front of the history and trims older ones.
        /// </summary>
        public BuildHistoryEntry Record(MsiBuildOptions options, string msiPath, DateTime builtUtc)
        {
            if (options == null) throw new ArgumentNullException("options");

            var entry = new BuildHistoryEntry
            {
                AssemblyPath = options.DllPath,
                AssemblyName = options.AssemblyName,
                Version = options.Version,
                Author = options.Author,
                Description = options.Description,
                FullClassName = options.EntriesText,
                AddinType = options.AddinType.ToString(),
                IconPath = options.IconPath,
                BackgroundImagePath = options.BackgroundImagePath,
                RevitYears = options.RevitYearsText,
                MsiPath = msiPath,
                BuiltUtc = builtUtc
            };

            var history = Load();
            history.Entries.Insert(0, entry);

            var kept = new List<BuildHistoryEntry>();
            var perAssembly = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var candidate in history.Entries)
            {
                var key = candidate.AssemblyPath ?? string.Empty;
                int count;
                perAssembly.TryGetValue(key, out count);
                if (count >= MaxEntriesPerAssembly) continue;

                perAssembly[key] = count + 1;
                kept.Add(candidate);
                if (kept.Count == MaxEntries) break;
            }

            history.Entries = kept;
            Save(history);
            return entry;
        }

        /// <summary>
        /// Releases previously built from the given assembly, newest first.
        /// </summary>
        public List<BuildHistoryEntry> GetFor(string assemblyPath)
        {
            if (string.IsNullOrEmpty(assemblyPath)) return new List<BuildHistoryEntry>();

            var normalized = Normalize(assemblyPath);
            return Load().Entries
                .Where(entry => string.Equals(Normalize(entry.AssemblyPath), normalized, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Callers hand over paths in whatever shape they have them (forward slashes, relative
        /// segments), so both sides of the lookup go through the same normalisation.
        /// </summary>
        private static string Normalize(string path)
        {
            if (string.IsNullOrEmpty(path)) return string.Empty;

            try
            {
                return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar);
            }
            catch
            {
                return path;
            }
        }
    }
}
