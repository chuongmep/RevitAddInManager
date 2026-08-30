using System;
using System.IO;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class PayloadRulesTests : IDisposable
    {
        private readonly string _dir;

        public PayloadRulesTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), "QuickMsiBuilderTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_dir);
        }

        public void Dispose()
        {
            try { Directory.Delete(_dir, true); } catch { /* best effort */ }
        }

        private void Write(string relativePath)
        {
            var path = Path.Combine(_dir, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, "x");
        }

        [Theory]
        [InlineData("RevitAPI.dll")]
        [InlineData("revitapiui.dll")]
        [InlineData("AdWindows.dll")]
        public void IsRevitProvided_RecognisesRevitAssemblies(string name)
        {
            Assert.True(PayloadRules.IsRevitProvided(name));
        }

        [Theory]
        [InlineData("Contoso.dll")]
        [InlineData("Newtonsoft.Json.dll")]
        [InlineData("")]
        [InlineData(null)]
        public void IsRevitProvided_LeavesEverythingElseAlone(string name)
        {
            Assert.False(PayloadRules.IsRevitProvided(name));
        }

        [Fact]
        public void FindRevitAssemblies_ReportsThemSortedAndDeduped()
        {
            Write("RevitAPIUI.dll");
            Write("RevitAPI.dll");
            Write(Path.Combine("sub", "RevitAPI.dll"));
            Write("Contoso.dll");

            Assert.Equal(new[] { "RevitAPI.dll", "RevitAPIUI.dll" }, PayloadRules.FindRevitAssemblies(_dir).ToArray());
        }

        [Fact]
        public void FindRevitAssemblies_OnACleanFolder_ReturnsNothing()
        {
            Write("Contoso.dll");
            Write("Contoso.dll.config");

            Assert.Empty(PayloadRules.FindRevitAssemblies(_dir));
        }

        [Fact]
        public void FindRevitAssemblies_OnAMissingFolder_ReturnsNothing()
        {
            Assert.Empty(PayloadRules.FindRevitAssemblies(Path.Combine(_dir, "nope")));
            Assert.Empty(PayloadRules.FindRevitAssemblies(null));
        }
    }
}
