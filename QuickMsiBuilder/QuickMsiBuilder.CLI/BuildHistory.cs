using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace QuickMsiBuilder.CLI
{
    /// <summary>
    /// One recorded release: everything needed to reproduce the same MSI later.
    /// </summary>
    public class BuildHistoryEntry
    {
        public string AssemblyPath { get; set; }
        public string AssemblyName { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public string Description { get; set; }
        public string FullClassName { get; set; }
        public string AddinType { get; set; }
        public string IconPath { get; set; }
        public string BackgroundImagePath { get; set; }
        /// <summary>Comma separated list, e.g. "2024, 2026".</summary>
        public string RevitYears { get; set; }

        public string MsiPath { get; set; }
        public DateTime BuiltUtc { get; set; }

        /// <summary>Label shown in the history picker, e.g. "1.2.0 - Revit 2024, 2026 - 2026-08-15 09:41".</summary>
        [XmlIgnore]
        public string DisplayName
        {
            get
            {
                return string.Format("{0} - Revit {1} - {2:yyyy-MM-dd HH:mm}",
                    Version, RevitYears, BuiltUtc.ToLocalTime());
            }
        }
    }

    [XmlRoot("QuickMsiBuildHistory")]
    public class BuildHistory
    {
        [XmlArray("Entries")]
        [XmlArrayItem("Entry")]
        public List<BuildHistoryEntry> Entries { get; set; }

        public BuildHistory()
        {
            Entries = new List<BuildHistoryEntry>();
        }
    }
}
