using System;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class ProgramOutputTests
    {
        [Fact]
        public void ParseResultPath_FindsTheBuiltPackage()
        {
            var output = "Packaging 3 file(s)\r\n" + Program.ResultPrefix + @"C:\out\Contoso-1.0.0-R2027.msi" + "\r\n";

            Assert.Equal(@"C:\out\Contoso-1.0.0-R2027.msi", Program.ParseResultPath(output));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("Packaging 3 file(s)\r\nsomething else")]
        public void ParseResultPath_WithoutAResultLine_ReturnsNull(string output)
        {
            Assert.Null(Program.ParseResultPath(output));
        }

        [Fact]
        public void ParseResultPath_IgnoresAnEmptyResultLine()
        {
            Assert.Null(Program.ParseResultPath(Program.ResultPrefix + "   "));
        }

        [Fact]
        public void StripResultLines_RemovesTheMachineReadableLineOnly()
        {
            var output = "Building MSI\r\n" + Program.ResultPrefix + @"C:\out\x.msi" + "\r\nDone";

            var stripped = Program.StripResultLines(output);

            Assert.DoesNotContain(Program.ResultPrefix, stripped);
            Assert.Contains("Building MSI", stripped);
            Assert.Contains("Done", stripped);
        }

        [Fact]
        public void StripResultLines_WithNoOutput_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, Program.StripResultLines(null));
            Assert.Equal(string.Empty, Program.StripResultLines(""));
        }
    }
}
