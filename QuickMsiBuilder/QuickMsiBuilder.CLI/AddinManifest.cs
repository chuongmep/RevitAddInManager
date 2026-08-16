using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace QuickMsiBuilder.CLI
{
    /// <summary>
    /// Writes the .addin manifest that ships next to the assembly inside the MSI.
    /// The element set differs per add-in type: Revit expects Name for an Application entry and
    /// Text/Description/VisibilityMode for a Command entry.
    /// </summary>
    public static class AddinManifest
    {
        public static XDocument Create(MsiBuildOptions options)
        {
            if (options == null) throw new ArgumentNullException("options");

            return Create(options.AssemblyName, options.Entries, options.Author, options.Description);
        }

        /// <summary>
        /// One AddIn element per entry point, which is how a Revit manifest declares an assembly
        /// that exposes several commands.
        /// </summary>
        public static XDocument Create(
            string assemblyName,
            IEnumerable<AddinCandidate> entries,
            string author,
            string description)
        {
            if (string.IsNullOrEmpty(assemblyName)) throw new ArgumentNullException("assemblyName");

            var elements = (entries ?? new List<AddinCandidate>())
                .Where(entry => entry != null && !string.IsNullOrEmpty(entry.FullClassName))
                .Select(entry => CreateAddin(assemblyName, entry.AddinType, entry.FullClassName, author, description))
                .ToArray();

            if (elements.Length == 0)
            {
                elements = new[]
                {
                    CreateAddin(assemblyName, RevitAddinType.Command, assemblyName + ".Command", author, description)
                };
            }

            return new XDocument(
                new XDeclaration("1.0", "utf-8", null),
                new XElement("RevitAddIns", elements));
        }

        public static XDocument Create(
            string assemblyName,
            RevitAddinType addinType,
            string fullClassName,
            string author,
            string description)
        {
            return Create(
                assemblyName,
                new[] { new AddinCandidate(fullClassName, addinType) },
                author,
                description);
        }

        private static XElement CreateAddin(
            string assemblyName,
            RevitAddinType addinType,
            string fullClassName,
            string author,
            string description)
        {
            if (string.IsNullOrEmpty(fullClassName)) fullClassName = assemblyName + ".Command";

            var displayName = DisplayName(fullClassName);
            var clientId = CreateDeterministicGuid(assemblyName + "|" + fullClassName);

            var addin = new XElement("AddIn", new XAttribute("Type", addinType.ToString()));

            if (addinType == RevitAddinType.Application)
            {
                addin.Add(new XElement("Name", displayName));
            }
            else
            {
                addin.Add(new XElement("Text", displayName));
                if (!string.IsNullOrEmpty(description)) addin.Add(new XElement("Description", description));
            }

            addin.Add(
                // The payload lives in a subfolder named after the assembly, so it cannot collide
                // with other add-ins sharing the Revit Addins directory.
                new XElement("Assembly", AssemblyRelativePath(assemblyName)),
                new XElement("FullClassName", fullClassName),
                new XElement("ClientId", clientId.ToString()),
                new XElement("VendorId", VendorId(author)),
                new XElement("VendorDescription", string.IsNullOrEmpty(author) ? "Unknown Publisher" : author));

            if (addinType == RevitAddinType.Command)
            {
                addin.Add(new XElement("VisibilityMode", "AlwaysVisible"));
            }

            return addin;
        }

        /// <summary>
        /// Same input always yields the same GUID, so re-running the builder keeps the ClientId and
        /// the MSI UpgradeCode stable and installs upgrade in place instead of stacking up.
        /// SHA256 is used rather than MD5 because MD5 throws on FIPS-enforced machines.
        /// </summary>
        public static Guid CreateDeterministicGuid(string name)
        {
            if (name == null) throw new ArgumentNullException("name");

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(name));
                var bytes = new byte[16];
                Array.Copy(hash, bytes, 16);

                // Stamp RFC 4122 version 5 / variant bits so the value is a well formed GUID.
                bytes[7] = (byte)((bytes[7] & 0x0F) | 0x50);
                bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
                return new Guid(bytes);
            }
        }

        /// <summary>
        /// Path Revit resolves relative to the .addin file: "MyAddin\MyAddin.dll".
        /// </summary>
        public static string AssemblyRelativePath(string assemblyName)
        {
            return assemblyName + "\\" + assemblyName + ".dll";
        }

        private static string DisplayName(string fullClassName)
        {
            var index = fullClassName.LastIndexOf('.');
            return index < 0 ? fullClassName : fullClassName.Substring(index + 1);
        }

        /// <summary>
        /// "ADSK" is reserved for Autodesk content, so derive a short vendor id from the author.
        /// </summary>
        private static string VendorId(string author)
        {
            if (string.IsNullOrEmpty(author)) return "AIMB";

            var builder = new StringBuilder();
            foreach (var c in author)
            {
                if (char.IsLetterOrDigit(c)) builder.Append(char.ToUpperInvariant(c));
                if (builder.Length == 8) break;
            }

            return builder.Length == 0 ? "AIMB" : builder.ToString();
        }
    }
}
