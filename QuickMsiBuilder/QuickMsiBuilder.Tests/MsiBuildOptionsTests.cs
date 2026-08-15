using System;
using System.Collections.Generic;
using System.IO;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class MsiBuildOptionsTests : IDisposable
    {
        private readonly string _tempDir;
        private readonly string _dllPath;

        public MsiBuildOptionsTests()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "QuickMsiBuilderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _dllPath = Path.Combine(_tempDir, "Contoso.Revit.dll");
            File.WriteAllText(_dllPath, "not a real assembly");
        }

        public void Dispose()
        {
            try { Directory.Delete(_tempDir, true); } catch { /* best effort */ }
        }

        [Fact]
        public void TryParse_WithNoArguments_ReturnsUsage()
        {
            MsiBuildOptions options;
            string error;

            Assert.False(MsiBuildOptions.TryParse(new string[0], out options, out error));
            Assert.Null(options);
            Assert.Equal(MsiBuildOptions.Usage, error);
        }

        [Fact]
        public void TryParse_WithMissingDll_Fails()
        {
            MsiBuildOptions options;
            string error;

            var missing = Path.Combine(_tempDir, "Nope.dll");
            Assert.False(MsiBuildOptions.TryParse(new[] { missing }, out options, out error));
            Assert.Contains("not found", error);
        }

        [Fact]
        public void TryParse_WithOnlyDll_AppliesDefaults()
        {
            MsiBuildOptions options;
            string error;

            Assert.True(MsiBuildOptions.TryParse(new[] { _dllPath }, out options, out error));
            Assert.Null(error);
            Assert.Equal("Contoso.Revit", options.AssemblyName);
            Assert.Equal("1.0.0", options.Version);
            Assert.Equal(new[] { RevitYearRange.Default }, options.RevitYears.ToArray());
            Assert.Equal(MsiBuildOptions.DefaultAuthor, options.Author);
            Assert.Equal(RevitAddinType.Command, options.AddinType);
            Assert.Equal("Contoso.Revit.Command", options.FullClassName);
        }

        [Fact]
        public void TryParse_ReadsEveryPositionalArgument()
        {
            MsiBuildOptions options;
            string error;

            var args = new[]
            {
                _dllPath, "2.5.1", "Contoso Ltd", "My add-in", "", "", "2026", "Contoso.App", "Application"
            };

            Assert.True(MsiBuildOptions.TryParse(args, out options, out error));
            Assert.Equal("2.5.1", options.Version);
            Assert.Equal("Contoso Ltd", options.Author);
            Assert.Equal("My add-in", options.Description);
            Assert.Equal(new[] { "2026" }, options.RevitYears.ToArray());
            Assert.Equal("Contoso.App", options.FullClassName);
            Assert.Equal(RevitAddinType.Application, options.AddinType);
        }

        [Fact]
        public void TryParse_WithApplicationTypeAndNoClassName_UsesApplicationSuffix()
        {
            MsiBuildOptions options;
            string error;

            var args = new[] { _dllPath, "", "", "", "", "", "", "", "Application" };

            Assert.True(MsiBuildOptions.TryParse(args, out options, out error));
            Assert.Equal("Contoso.Revit.Application", options.FullClassName);
        }

        [Theory]
        [InlineData("", "1.0.0")]
        [InlineData("2.3", "2.3.0")]
        [InlineData("2.3.4", "2.3.4")]
        [InlineData("2.3.4.5", "2.3.4")]
        public void TryNormalizeVersion_AcceptsMsiCompatibleValues(string input, string expected)
        {
            string normalized;
            string error;

            Assert.True(MsiBuildOptions.TryNormalizeVersion(input, out normalized, out error));
            Assert.Equal(expected, normalized);
        }

        [Theory]
        [InlineData("1.0.0-beta")]
        [InlineData("not-a-version")]
        [InlineData("256.0.0")]
        [InlineData("1.256.0")]
        [InlineData("1.0.65536")]
        public void TryNormalizeVersion_RejectsInvalidValues(string input)
        {
            string normalized;
            string error;

            Assert.False(MsiBuildOptions.TryNormalizeVersion(input, out normalized, out error));
            Assert.Null(normalized);
            Assert.False(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void TryNormalizeRevitYears_WithNoValue_UsesTheCurrentYear()
        {
            List<string> normalized;
            string error;

            Assert.True(MsiBuildOptions.TryNormalizeRevitYears("", out normalized, out error));
            Assert.Equal(new[] { DateTime.Now.Year.ToString() }, normalized.ToArray());
        }

        [Theory]
        [InlineData("2026", "2026")]
        [InlineData(" 2027 ", "2027")]
        [InlineData("2025.1", "2025")]
        [InlineData("2024,2026", "2024|2026")]
        [InlineData("2026;2024", "2024|2026")]
        [InlineData("2026 2024 2025", "2024|2025|2026")]
        [InlineData("2024,2024,2024", "2024")]
        [InlineData("2024, 2026,", "2024|2026")]
        public void TryNormalizeRevitYears_AcceptsKnownShapes(string input, string expected)
        {
            List<string> normalized;
            string error;

            Assert.True(MsiBuildOptions.TryNormalizeRevitYears(input, out normalized, out error));
            Assert.Equal(expected, string.Join("|", normalized.ToArray()));
        }

        [Theory]
        [InlineData("R2024")]
        [InlineData("nineteen")]
        [InlineData("1999")]
        [InlineData("2024,nineteen")]
        public void TryNormalizeRevitYears_RejectsGarbage(string input)
        {
            List<string> normalized;
            string error;

            Assert.False(MsiBuildOptions.TryNormalizeRevitYears(input, out normalized, out error));
            Assert.False(string.IsNullOrEmpty(error));
        }

        [Fact]
        public void TryParse_WithSeveralYears_KeepsThemAllSortedAndDeduped()
        {
            MsiBuildOptions options;
            string error;

            var args = new[] { _dllPath, "1.0.0", "", "", "", "", "2026,2024,2026", "", "" };

            Assert.True(MsiBuildOptions.TryParse(args, out options, out error));
            Assert.Equal(new[] { "2024", "2026" }, options.RevitYears.ToArray());
            Assert.Equal("2024, 2026", options.RevitYearsText);
        }

        [Fact]
        public void OutputName_ListsEveryTargetedYear()
        {
            MsiBuildOptions options;
            string error;

            Assert.True(MsiBuildOptions.TryParse(
                new[] { _dllPath, "1.4.0", "", "", "", "", "2024,2026" }, out options, out error));

            Assert.Equal("Contoso.Revit-1.4.0-R2024-2026", options.OutputName);
        }

        [Fact]
        public void DefaultAuthor_UsesTheWindowsAccountName()
        {
            Assert.Equal(Environment.UserName, MsiBuildOptions.DefaultAuthor);
        }

        [Theory]
        [InlineData("Application", RevitAddinType.Application)]
        [InlineData("application", RevitAddinType.Application)]
        [InlineData("Command", RevitAddinType.Command)]
        [InlineData("", RevitAddinType.Command)]
        [InlineData("nonsense", RevitAddinType.Command)]
        public void ParseAddinType_DefaultsToCommand(string input, RevitAddinType expected)
        {
            Assert.Equal(expected, MsiBuildOptions.ParseAddinType(input));
        }
    }
}
