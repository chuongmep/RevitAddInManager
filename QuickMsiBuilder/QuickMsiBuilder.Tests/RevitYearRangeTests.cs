using System;
using System.Linq;
using QuickMsiBuilder.CLI;
using Xunit;

namespace QuickMsiBuilder.Tests
{
    public class RevitYearRangeTests
    {
        [Fact]
        public void Newest_IsNextCalendarYear()
        {
            // Autodesk ships e.g. Revit 2027 during 2026.
            Assert.Equal(DateTime.Now.Year + 1, RevitYearRange.Newest);
        }

        [Fact]
        public void Oldest_IsFourYearsBeforeNewest()
        {
            Assert.Equal(RevitYearRange.Newest - 4, RevitYearRange.Oldest);
        }

        [Fact]
        public void Supported_ListsFiveConsecutiveYearsOldestFirst()
        {
            var years = RevitYearRange.Supported;

            Assert.Equal(RevitYearRange.SupportedCount, years.Count);
            Assert.Equal(RevitYearRange.Oldest.ToString(), years.First());
            Assert.Equal(RevitYearRange.Newest.ToString(), years.Last());

            var numbers = years.Select(int.Parse).ToList();
            for (var i = 1; i < numbers.Count; i++) Assert.Equal(numbers[i - 1] + 1, numbers[i]);
        }

        [Fact]
        public void Supported_ContainsNoDuplicates()
        {
            var years = RevitYearRange.Supported;

            Assert.Equal(years.Count, years.Distinct().Count());
        }

        [Fact]
        public void Default_IsTheCurrentCalendarYear()
        {
            Assert.Equal(DateTime.Now.Year.ToString(), RevitYearRange.Default);
        }

        [Fact]
        public void Default_IsOneOfTheSupportedYears()
        {
            Assert.Contains(RevitYearRange.Default, RevitYearRange.Supported);
        }

        [Fact]
        public void Supported_IsAcceptedByTheOptionParser()
        {
            System.Collections.Generic.List<string> normalized;
            string error;

            Assert.True(MsiBuildOptions.TryNormalizeRevitYears(
                string.Join(",", RevitYearRange.Supported.ToArray()), out normalized, out error));
            Assert.Equal(RevitYearRange.Supported, normalized);
        }
    }
}
